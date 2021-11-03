using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using SoA_Editor.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Windows;

namespace SoA_Editor.ViewModels
{
    public class RangeViewModel : Screen
    {
        // public vars needed for the calculation
        public Unc_Template template;
        public Unc_Technique technique;
        public string functionName;
        public Mtc_CMCUncertainty uncertainty;
        public List<string> assertionNodeValues;

        // private vars needed for the calculation
        private SOA_DataAccessLib.Unc_Range range = null;
        private Unc_Case Case = null;
        private Helper.MessageDialog dialog;
        private EditSingleRangeDialog editSingleRangeDialog;

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
            CalculatedValue = calculatedResult;
        }

        // Edit Range
        public async void EditRange(DataRowView row)
        {
            // No range selected, don't bother evaluating
            if (range == null) return;

            if (!DialogHost.IsDialogOpen("RootDialog"))
            {
                SOA_DataAccessLib.Unc_Range r = new();
                r.ConstantValues = new();
                r.Start = new();
                r.End = new();
                foreach (var constant in range.ConstantValues)
                {
                    r.ConstantValues.Add(new Unc_ConstantValue()
                    {
                        const_parameter_name = constant.const_parameter_name,
                        ValueString = constant.ValueString
                    });
                }
                r.Variable_name = range.Variable_name;
                r.Start.ValueString = range.Start.ValueString;
                r.End.ValueString = range.End.ValueString;
                var pr = technique.Technique.ParameterRanges[range.Variable_name];
                string minMax = "";
                if (pr != null) minMax = string.Format("Overall Paramter Range: {0} to {1}", pr.Start.ValueString, pr.End.ValueString);
                editSingleRangeDialog = new EditSingleRangeDialog()
                {
                    DataContext = new EditSingleRangeDialogViewModel(r, minMax, pr.Start.Value, pr.End.Value)
                };
                var viewModel = (EditSingleRangeDialogViewModel)editSingleRangeDialog.DataContext;
                
                object result = await DialogHost.Show(editSingleRangeDialog, "RootDialog", ClosingEventHandlerRange);

                if ((bool)result)
                {
                    
                    // update range object
                    for (int i = 0; i < range.ConstantValues.Count(); i++)
                    {
                        range.ConstantValues[i].const_parameter_name = r.ConstantValues[i].const_parameter_name;
                        range.ConstantValues[i].ValueString = r.ConstantValues[i].ValueString;
                        range.ConstantValues[i].Value = decimal.Parse(r.ConstantValues[i].ValueString);
                    }
                    range.Variable_name = r.Variable_name;
                    range.Start.ValueString = r.Start.ValueString;
                    range.Start.Value = decimal.Parse(r.Start.ValueString);
                    range.End.ValueString = r.End.ValueString;
                    range.End.Value = decimal.Parse(r.End.ValueString);
                }
                // update row data
                row.Row[range.Variable_name] = range.Start.ValueString + " to " + range.End.ValueString;
                List<string> constantsCol = new();
                foreach (var constant in range.ConstantValues)
                {
                    constantsCol.Add(string.Format("{0} = {1}", constant.const_parameter_name, constant.ValueString));
                }
                row.Row["Constants"] = string.Join("\n", constantsCol);
            }
            range = null;
        }

        // Edit Range Validation
        private void ClosingEventHandlerRange(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                var viewModel = (EditSingleRangeDialogViewModel)editSingleRangeDialog.DataContext;
                // validate min and max fields
                var rangeMin = viewModel.Range.Start.ValueString;
                var rangeMax = viewModel.Range.End.ValueString;
                decimal test;
                if (!decimal.TryParse(rangeMin, out test))
                {
                    viewModel.Error = "The Minimum Value is not a number";
                    eventArgs.Cancel();
                    return;
                }
                if (!decimal.TryParse(rangeMax, out test))
                {
                    viewModel.Error = "The Maximum Value is not a number";
                    eventArgs.Cancel();
                    return;
                }
                var min = decimal.Parse(rangeMin);
                var max = decimal.Parse(rangeMax);
                if (min >= max)
                {
                    viewModel.Error = "Your Minimum Range Value must be below the Maximum Value";
                    eventArgs.Cancel();
                    return;
                }
                if (viewModel.ParameterRange != "" && min < viewModel.Min)
                {
                    viewModel.Error = "The Minimum Value can not below the Parameter's Overall Range";
                    eventArgs.Cancel();
                    return;
                }
                if (viewModel.ParameterRange != "" && max > viewModel.Max)
                {
                    viewModel.Error = "The Maximum Value can not above the Parameter's Overall Range";
                    eventArgs.Cancel();
                    return;
                }

                // Constant values are checked in the SoA Lib and only accepted if they are valid numbers.
                // If not it will maintain the orginal value
                
            }
        }

            // Delete Range
            public async void DeleteRange(DataRowView row)
        {
            // No range selected, don't bother evaluating
            if (range == null) return;

            if (!DialogHost.IsDialogOpen("RootDialog"))
            {
                DeleteDialogView view = new DeleteDialogView()
                {
                    DataContext = new DeleteDialogViewModel()
                };
                var viewModel = (DeleteDialogViewModel)view.DataContext;
                viewModel.Message = "Are you sure your want to delete this Uncertainty Range";

                object result = await DialogHost.Show(view, "RootDialog");

                if ((bool)result)
                {
                    this.Case.Ranges.Remove(range);
                    int index = RangeGrid.AsDataView().Table.Rows.IndexOf(row.Row);
                    RangeGrid.Rows.RemoveAt(index);
                }
            }

            range = null;
        }

        // Update values for the formula
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
                rowData.Add(col.ColumnName, data[index].ToString());
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
                                rowData.Add(name, value);
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
                values.Add(rowData[name]);
            }
            var _case = template.getCasesByAssertionValues(functionName, values.ToArray());
            if (_case.Count > 0) Case = _case[0];
            // find our influence quantity it will not be apart of our variables or constats
            List<string> rangeVars = template.getCMCFunctionRangeVariables(functionName).ToList();
            var symbols = template.getCMCUncertaintyFunctionSymbols(functionName);
            string influenc_qty = rangeVars.Where(r => !symbols.Contains(r)).ToList()[0];
            string param = rangeVars.Where(r => symbols.Contains(r)).ToList()[0];

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
            foreach (SOA_DataAccessLib.Unc_Range range in ranges.getRanges())
            {
                if (foundRange) break;
                if (range.Start.ValueString == qtys[0] && range.End.ValueString == qtys[1])
                {
                    foreach (SOA_DataAccessLib.Unc_Range _range in range.Ranges.getRanges())
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