using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.Model;
using SoA_Editor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace SoA_Editor.ViewModels
{
    internal class RangeInfoViewModel : Screen
    {
        private List<Mtc_Range> infQtys;
        private List<Mtc_Range> vars;
        private string[] constants;
        private string functionName;
        private Unc_Template template;
        private Helper.MessageDialog dialog;
        private bool firstCase;

        public RangeInfoViewModel(List<Mtc_Range> infQtys, List<Mtc_Range> vars, string[] constants, string functionName, Unc_Template template, bool firstCase = false)
        {
            this.infQtys = infQtys;
            this.vars = vars;
            this.constants = constants;
            this.functionName = functionName;
            this.firstCase = firstCase;
            this.template = template;
            var assertionNames = this.template.getCMCFunctionAssertionNames(functionName);
            MinMax = new();

            // if we do not have any cases to add Ranges to we need to make our assertions first
            if (this.template.CMCUncertaintyFunctions[0].Cases.Count() == 0)
            {
                Assertion1 = new();
                Assertion2 = new();
            }
            else // we are adding Unc Range values set up influence and vars
            {
                // see how many assertions the first case has, the rest will follow suit
                if (this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions.Count() == 2)
                {
                    Assertion1 = new();
                    Assertion2 = new();
                    AssertionsValues1 = new();
                    AssertionsValues2 = new();
                    Assertion1.Name = assertionNames[0];
                    Assertion2.Name = assertionNames[1];
                    if (!firstCase)
                    {
                        Assertion1.Value = "";
                        Assertion2.Value = "";
                        foreach (string value in template.getCMCFunctionAssertionValues(functionName, assertionNames[0]))
                        {
                            AssertionsValues1.Add(value);
                        }
                        foreach (string value in template.getCMCFunctionAssertionValues(functionName, assertionNames[1]))
                        {
                            AssertionsValues2.Add(value);
                        }
                    }
                    else
                    {
                        Assertion1.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Value;
                        Assertion2.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[1].Value;
                    }
                }
                else
                {
                    Assertion1.Name = assertionNames[0];
                    Assertion2 = null;
                    if (!firstCase)
                    {
                        Assertion1.Value = "";
                    }
                    else
                    {
                        Assertion1.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Value;
                    }
                    foreach (string value in template.getCMCFunctionAssertionValues(functionName, assertionNames[0]))
                    {
                        AssertionsValues1.Add(value);
                    }
                    AssertionsValues2 = null;
                }
                // Get existing cases, vars, constants, and vars
                InfQtyRange = new Range_Influence_Quantity(infQtys);

                Vars = new();
                Vars = vars;

                RangeGrid = new();
                RangeGrid.Columns.Add("Minimum");
                RangeGrid.Columns.Add("Maximum");

                Constants = new();
                for (int i = 0; i < constants.Length; i++)
                {
                    RangeGrid.Columns.Add(constants[i]);
                }
            }

            dialog = new();
            dialog.Title = "Validation Error";
            dialog.Button = System.Windows.MessageBoxButton.OK;
            dialog.Image = System.Windows.MessageBoxImage.Warning;
        }

        #region properties;

        public bool NoCases
        {
            get { return this.template.CMCUncertaintyFunctions[0].Cases.Count() == 0 ? true : false; }
        }

        public bool FirstCase
        {
            get { return firstCase; }
        }

        private Unc_Assertion assertion1;

        public Unc_Assertion Assertion1
        {
            get { return assertion1; }
            set
            {
                assertion1 = value;
                NotifyOfPropertyChange(() => Assertion1);
            }
        }

        private Unc_Assertion assertion2;

        public Unc_Assertion Assertion2
        {
            get { return assertion2; }
            set
            {
                assertion2 = value;
                NotifyOfPropertyChange(() => Assertion2);
            }
        }

        private Range_Influence_Quantity infQtyRange;

        public Range_Influence_Quantity InfQtyRange
        {
            get { return infQtyRange; }
            set
            {
                infQtyRange = value;
                NotifyOfPropertyChange(() => InfQtyRange);
            }
        }

        private ObservableCollection<Range_Constant> _constants;

        public ObservableCollection<Range_Constant> Constants
        {
            get { return _constants; }
            set
            {
                _constants = value;
                NotifyOfPropertyChange(() => Constants);
            }
        }

        private Mtc_Range selectedVar;

        public Mtc_Range SelectedVar
        {
            get { return selectedVar; }
            set
            {
                selectedVar = value;
                var range = selectedVar.Start.ValueString;
                ParameterRange = "";
                if (range != null || range != "")
                {
                    ParameterRange = string.Format("{0} to {1}", selectedVar.Start.ValueString, selectedVar.End.ValueString);
                }
                NotifyOfPropertyChange(() => SelectedVar);
            }
        }

        private string parameterRange;

        public string ParameterRange
        {
            get { return parameterRange; }
            set { parameterRange = value; NotifyOfPropertyChange(() => ParameterRange); }
        }

        public List<Mtc_Range> Vars
        {
            get { return vars; }
            set
            {
                vars = value;
                NotifyOfPropertyChange(() => Vars);
            }
        }

        private ObservableCollection<Unc_Range_Min_Max> minMax;

        public ObservableCollection<Unc_Range_Min_Max> MinMax
        {
            get { return minMax; }
            set { minMax = value; NotifyOfPropertyChange(() => MinMax); }
        }

        private ObservableCollection<string> assertionsValues1;

        public ObservableCollection<string> AssertionsValues1
        {
            get { return assertionsValues1; }
            set
            {
                assertionsValues1 = value;
                NotifyOfPropertyChange(() => AssertionsValues1);
            }
        }

        private ObservableCollection<string> assertionsValues2;

        public ObservableCollection<string> AssertionsValues2
        {
            get { return assertionsValues2; }
            set
            {
                assertionsValues2 = value;
                NotifyOfPropertyChange(() => AssertionsValues2);
            }
        }

        private DataTable rangeGrid;

        public DataTable RangeGrid
        {
            get
            {
                return rangeGrid;
            }
            set
            {
                Set(ref rangeGrid, value);
            }
        }

        #endregion properties;

        #region Methods

        public void SaveFirstCase()
        {
            if (Assertion1.Name == "" || Assertion1.Value == "")
            {
                dialog.Message = "You must have at least one Assertion";
                dialog.Show();
                return;
            }
            if (Assertion1.Name != "" && Assertion1.Value == "")
            {
                dialog.Message = "You must enter a value for the first Assertion";
                dialog.Show();
                return;
            }
            if (Assertion2.Name != "" && Assertion2.Value == "")
            {
                dialog.Message = "You must enter a value for the second Assertion";
                dialog.Show();
                return;
            }

            var assertions = new Unc_Assertions();
            assertions.Add(Assertion1);
            assertions.Add(Assertion2);
            Helper.TreeViewCase = new Unc_Case(template, assertions);
            base.TryCloseAsync(null);
        }

        public void SaveNewCase()
        {
            // Make sure we have what we need before trying to save a case

            // Check our assertions
            if (Assertion1.Value == "")
            {
                dialog.Message = "Please Select or enter the first Assertion Value";
                dialog.Show();
                return;
            }
            if (Assertion2 != null && Assertion2.Value == "")
            {
                dialog.Message = "Please Select or enter the first Assertion Value";
                dialog.Show();
                return;
            }

            // Check our Infuance quantity
            if (InfQtyRange.SelectedQty.name == null || InfQtyRange.Min == "" || InfQtyRange.Max == "")
            {
                dialog.Message = "You must select and Inflence Quantity, Minimum, and Maximum values";
                dialog.Show();
                return;
            }

            // Check that the min max are numbers
            double test;
            if (!double.TryParse(infQtyRange.Min, out test) || !double.TryParse(infQtyRange.Max, out test))
            {
                dialog.Message = "Your Influance Quantity min and max must be a number";
                dialog.Show();
                return;
            }

            // that the min and max are with in range if they have a parameter range
            var min = InfQtyRange.SelectedQty.Start.ValueString;
            var max = InfQtyRange.SelectedQty.End.ValueString;
            if (min != "" || min != null)
            {
                if (double.Parse(InfQtyRange.Min) < double.Parse(min) || double.Parse(InfQtyRange.Max) > double.Parse(max))
                {
                    dialog.Message = "Your Influance Quantity min and max is outside of the assinged range";
                    dialog.Show();
                    return;
                }
            }

            if (SelectedVar.name == null || SelectedVar.name == "")
            {
                dialog.Message = "You must selected a Parameter";
                dialog.Show();
                return;
            }

            // Check Variables
            DataRowCollection rows = RangeGrid.Rows;
            DataColumnCollection cols = RangeGrid.Columns;
            Dictionary<int, Dictionary<string, string>> rowData = new Dictionary<int, Dictionary<string, string>>();
            int rowCount = 0;
            int colCount;
            foreach (DataRow row in rows)
            {
                var items = row.ItemArray;
                colCount = 0;
                var newRowData = new Dictionary<string, string>();
                foreach (DataColumn col in cols)
                {
                    newRowData.Add(col.ColumnName, items[colCount].ToString());
                    colCount++;
                }
                rowData.Add(rowCount, newRowData);
                rowCount++;
            }

            // lets populate our min max for our variable parameter and our constant values
            for (int i = 0; i < rowData.Count; i++)
            {
                var row = rowData[i];
                MinMax.Add(new Unc_Range_Min_Max()
                {
                    Min = row["Minimum"],
                    Max = row["Maximum"]
                });
                for (int j = 0; j < constants.Length; j++)
                {
                    Constants.Add(new Range_Constant()
                    {
                        ConstName = constants[j],
                        Value = row[constants[j]]
                    }); ;
                }
            }

            // Now check that we have the values needed
            if (MinMax.Count == 0)
            {
                dialog.Message = "You did not set any Uncertainty Ranges.";
                dialog.Show();
                return;
            }

            if (constants.Length > 0 && Constants.Count == 0)
            {
                dialog.Message = "This Uncertainty has Constants that must be entered.";
                dialog.Show();
                return;
            }

            foreach (Unc_Range_Min_Max minMax in MinMax)
            {
                if (minMax.Min == null || minMax.Max == null || minMax.Min == "" || minMax.Max == "")
                {
                    dialog.Message = "You Uncertainty Range with no Min or Max set.";
                    dialog.Show();
                    return;
                }
                if (!double.TryParse(minMax.Min, out test) || !double.TryParse(minMax.Max, out test))
                {
                    dialog.Message = "You have an Uncertianty Range's min and max that are not numbers";
                    dialog.Show();
                    return;
                }
                // Make sure the range they chose a value that falls withing the parameter range
                if (SelectedVar.Start.ValueString != "")
                {
                    if (double.Parse(minMax.Min) < double.Parse(SelectedVar.Start.ValueString) ||
                        double.Parse(minMax.Max) > double.Parse(SelectedVar.End.ValueString))
                    {
                        dialog.Message = "One of your Uncertainty Ranges is outside the Parameter's assigned Range";
                        dialog.Show();
                        return;
                    }
                }
            }

            // Check Constants
            foreach (Range_Constant constant in Constants)
            {
                if (!double.TryParse(constant.Value, out test))
                {
                    dialog.Message = "Your Constant values must be a number";
                    dialog.Show();
                    return;
                }
            }

            // see if we have a case already
            int size = 2;
            if (Assertion2 == null) size = 1;
            string[] values = new string[size];
            values[0] = Assertion1.Value;
            if (size == 2) values[1] = Assertion2.Value;
            var exsitingCase = template.getCaseByAssertionValues(functionName, values);
            // if we have an existing case add the unc range to it
            if (exsitingCase != null)
            {
                AddRanges(exsitingCase);
                Helper.TreeViewCase = exsitingCase;
            }
            else // We need to add a new case and ranges
            {
                // Get our assertions together
                Unc_Assertions assertions = new Unc_Assertions();
                assertions.Add(Assertion1);
                if (Assertion2 != null) assertions.Add(Assertion2);
                Unc_Case newCase = new Unc_Case(template, assertions);
                AddRanges(newCase);
                Helper.TreeViewCase = newCase;
                template.CMCUncertaintyFunctions[0].Cases.Add(newCase);
            }
            base.TryCloseAsync(null);
        }

        private void AddRanges(Unc_Case Case)
        {
            // Influence Quantity, we need to check if we already have an influence qty type
            Unc_Range infQtyRange = null;
            bool newRanges = true;
            foreach (Unc_Range range in Case.Ranges.getRanges())
            {
                if (range.Variable_name == InfQtyRange.SelectedQty.name)
                {
                    // now see if we are adding a new Qty via the ranges or a new set
                    if (range.Start.Value == decimal.Parse(InfQtyRange.Min) && range.End.Value == decimal.Parse(InfQtyRange.Max))
                    {
                        infQtyRange = range;
                        newRanges = false;
                        break;
                    }
                }
            }

            if (infQtyRange == null)
            {
                Case.Ranges.variable_name = InfQtyRange.SelectedQty.name;
                Case.Ranges.variable_type = "influence_quantity";

                // Start and stop ranges for the influence qty
                infQtyRange = new Unc_Range(template);
                infQtyRange.Start = SetStart("at", InfQtyRange.Min, InfQtyRange.SelectedQty.Start.Quantity);
                infQtyRange.End = SetEnd("at", InfQtyRange.Max, InfQtyRange.SelectedQty.Start.Quantity);
                infQtyRange.Variable_name = InfQtyRange.SelectedQty.name;
                infQtyRange.Variable_type = "influence_quantity";
            }

            // Add our variable ranges and constants
            int count = 1;
            Unc_Range paramRange = new Unc_Range(template);
            paramRange.ConstantValues = new();
            paramRange.Variable_name = SelectedVar.name;
            paramRange.Variable_type = "parameter";
            infQtyRange.Ranges.variable_name = SelectedVar.name;
            infQtyRange.Ranges.variable_type = "parameter";
            foreach (Unc_Range_Min_Max minMax in MinMax)
            {
                if (count > 1)
                    paramRange.Start = SetStart("after", minMax.Min, SelectedVar.Start.Quantity);
                else
                    paramRange.Start = SetStart("at", minMax.Min, SelectedVar.Start.Quantity);
                paramRange.End = SetEnd("at", minMax.Max, SelectedVar.Start.Quantity);
                foreach (Range_Constant constant in Constants)
                {
                    paramRange.ConstantValues.Add(new Unc_ConstantValue()
                    {
                        const_parameter_name = constant.ConstName,
                        Value = decimal.Parse(constant.Value),
                        Quantity = SelectedVar.Start.Quantity,
                        symbol = UomDataSource.getQuantity(SelectedVar.Start.Quantity).UoM.symbol
                });
                }
                count++;
                infQtyRange.Ranges.Add(paramRange);
            }

            if (newRanges) Case.Ranges.Add(infQtyRange);
        }

        private Unc_Range_Start SetStart(string test, string min, string quantity)
        {
            Unc_Range_Start start = new Unc_Range_Start();
            var qty = UomDataSource.getQuantity(quantity);
            start.test = test;
            start.Value = decimal.Parse(min);
            start.ValueString = min;
            if (qty != null)
            {
                start.Quantity = qty.name;
                start.symbol = qty.UoM.symbol;
            }
            return start;
        }

        private Unc_Range_End SetEnd(string test, string max, string quantity)
        {
            Unc_Range_End end = new Unc_Range_End();
            var qty = UomDataSource.getQuantity(quantity);
            end.test = test;
            end.Value = decimal.Parse(max);
            end.ValueString = max;           
            if (qty != null)
            {
                end.Quantity = qty.name;
                end.symbol = qty.UoM.symbol;
            }            
            return end;
        }

        #endregion Methods
    }
}