﻿using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace SoA_Editor.ViewModels
{
    public class RangeViewModel : Screen
    {
        // public vars needed for the calculation
        public Unc_Template template;

        public string functionName;
        public Mtc_CMCUncertainty uncertainty;
        public List<string> assertionNodeValues;

        // private vars needed for the calculation
        private Unc_Range range = null;
        private Helper.MessageDialog dialog;

        private string orginalFormula;
        private string calculatedResult;

        public RangeViewModel()
        {
            assertionNodeValues = new();
            dialog = new();
            dialog.Button = System.Windows.MessageBoxButton.OK;
            dialog.Image = System.Windows.MessageBoxImage.Error;
            dialog.Title = "Range Error";
        }

        #region Properties

        private string _CalculatedValue;

        public string CalculatedValue
        {
            get { return _CalculatedValue; }
            set { Set(ref _CalculatedValue, value); }
        }

        private DataTable _RangeGrid;

        public DataTable RangeGrid
        {
            get
            {
                return _RangeGrid;
            }
            set
            {
                Set(ref _RangeGrid, value);
            }
        }

        private string _activeHierarchy;

        public string activeHierarchy
        {
            get { return _activeHierarchy; }
            set { Set(ref _activeHierarchy, value); }
        }

        private DataRowView row;

        public DataRowView Row
        {
            get { return row; }
            set
            {
                if (value == null) return;

                row = value;
                UpdateFormula(row.Row);
                NotifyOfPropertyChange(() => Row);
            }
        }

        private string _Formula;

        public string Formula
        {
            get
            {
                return _Formula;
            }
            set
            {
                if (orginalFormula == null) orginalFormula = value;
                Set(ref _Formula, value);
            }
        }

        private ObservableCollection<ExpressionVariable> _ExprVars;

        public ObservableCollection<ExpressionVariable> ExprVars
        {
            get
            {
                return _ExprVars;
            }
            set
            {
                Set(ref _ExprVars, value);
            }
        }

        #endregion Properties

        // Calculate selected row
        public void calcButton()
        {
            updateValues();
            CalculatedValue =  calculatedResult;
        }

        private void updateValues()
        {
            // No range selected, don't bother evaluating
            if (range == null) return;

            // set the evaluation variables
            foreach (ExpressionVariable variable in ExprVars)
            {
                // See if we have a value and that it is numeric
                double test;
                if (variable.Value == "" || variable.Value == null) return;
                if (double.TryParse(variable.Value, out test))
                {
                    template.setCMCFunctionSymbol(functionName, variable.Name, double.Parse(variable.Value));
                }
                else
                {
                    dialog.Message = "The values you entered must a numeric value";
                    dialog.Show();
                    return;
                }

                Formula = Formula.Replace(variable.Name, variable.Value);
            }

            // Check that ExprVars are within range
            if (!CheckWithinRange())
            {
                dialog.Message = "The values you entered are not within the select Range";
                dialog.Show();
                return;
            }

            // set the constant vars
            foreach (var constant in range.ConstantValues)
            {
                template.setCMCFunctionSymbol(functionName, constant.const_parameter_name, (double)constant.BaseValue);
            }
            double result = (double)template.evaluateCMCFunction(functionName);
            calculatedResult = ToEngineeringFormat.Convert(result);
        }

        private void UpdateFormula(DataRow row)
        {
            Formula = orginalFormula;
            var data = row.ItemArray;
            var table = row.Table;

            // build our column to value list
            Dictionary<string, string> rowData = new();
            int index = 0;
            foreach (DataColumn col in table.Columns)
            {
                rowData.Add(col.ColumnName.ToLower(), data[index].ToString());
                index++;
            }

            // See if we have values we need to pair up with their assertionName
            List<string> searchNames = new();
            List<string> values = new();
            if (assertionNodeValues.Count > 0)
            {
                foreach (string searchValue in assertionNodeValues)
                {
                    searchNames = template.getCMCFunctionAssertionNames(functionName).ToList();
                    foreach (string name in searchNames)
                    {
                        values = template.getCMCFunctionAssertionValues(functionName, name).ToList();
                        foreach (string value in values)
                        {
                            if (value == searchValue)
                            {
                                rowData.Add(name.ToLower(), value);
                            }
                        }
                    }
                }
            }

            // lets find the right case now
            searchNames = template.getCMCFunctionAssertionNames(functionName).ToList();
            values.Clear();
            foreach (string name in searchNames)
            {
                values.Add(rowData[name.ToLower()]);
            }
            var Case = template.getCaseByAssertionValues(functionName, values.ToArray());

            // find our influence quantity it will not be apart of our variables or constats
            List<string> rangeVars = template.getCMCFunctionRangeVariables(functionName).ToList();
            var symbols = template.getCMCUncertaintyFunctionSymbols(functionName);
            string influenc_qty = rangeVars.Where(r => !symbols.Contains(r)).ToList()[0].ToLower();
            string param = rangeVars.Where(r => symbols.Contains(r)).ToList()[0].ToLower();

            // get our possible ranges
            var ranges = Case.Ranges[influenc_qty];

            // get our infuence_quantity value and param
            string[] qtys = rowData[influenc_qty].Split(" to ");
            string[] _params = rowData[param].Split(" to ");

            // The ranges are not in the correct format back out and let the user know
            if (qtys.Length != 2 || _params.Length != 2)
            {
               
                dialog.Message = "The Selected Range is not in the correct format, the calculation will not work.";
                dialog.Show();
            }

            // search for the right range
            bool foundRange = false;
            foreach (Unc_Range range in ranges.getRanges())
            {
                if (foundRange) break;
                if (range.Start.ValueString == qtys[0] && range.End.ValueString == qtys[1])
                {
                    foreach (Unc_Range _range in range.Ranges.getRanges())
                    {
                        if (_range.Start.ValueString == _params[0] && _range.End.ValueString == _params[1])
                        {
                            this.range = _range;
                            foundRange = true;
                            break;
                        }
                    }
                }
            }

            // lets update the fomula with the constant values
            string expression = Formula;
            foreach (Unc_ConstantValue constant in this.range.ConstantValues)
            {
                expression = expression.Replace(constant.const_parameter_name, constant.Value.ToString());
            }
            Formula = expression;
        }

        // Check that it is with the range Start and Stop
        private bool CheckWithinRange()
        {
            bool checkStartAt = false;
            bool checkStartAfter = false;
            bool checkEndAt = false;
            bool checkEndBefore = false;
            bool withinRange = false;

            if (range.Start.test == "at")
            {
                checkStartAt = true;
            }
            else if (range.Start.test == "after")
            {
                checkStartAfter = true;
            }

            if (range.End.test == "at")
            {
                checkEndAt = true;
            }
            else if (range.End.test == "before")
            {
                checkEndBefore = true;
            }

            foreach (ExpressionVariable exp in ExprVars)
            {
                decimal value = decimal.Parse(exp.Value);

                if (checkStartAt && checkEndAt)
                {
                    if (value >= range.Start.Value && value <= range.End.Value)
                        withinRange = true;
                }

                if (checkStartAfter && checkEndAt)
                {
                    if (value > range.Start.Value && value <= range.End.Value)
                        withinRange = true;
                }

                if (checkStartAt && checkEndBefore)
                {
                    if (value >= range.Start.Value && value < range.End.Value)
                        withinRange = true;
                }

                if (checkStartAfter && checkEndBefore)
                {
                    if (value > range.Start.Value && value < range.End.Value)
                        withinRange = true;
                }
                if (!withinRange) break;
            }

            return withinRange;
        }
    }
}