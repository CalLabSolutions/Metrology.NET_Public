using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.Model;
using SoA_Editor.Models;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace SoA_Editor.ViewModels
{
    internal class RangeInfoViewModel : Screen
    {
        private List<Mtc_Range> vars;
        private List<Assertion> assertions;
        private string[] constants;
        private string functionName;
        private Unc_Template template;
        private Unc_Technique technique;
        private Helper.MessageDialog dialog;
        private NumberStyles styles = NumberStyles.Number | NumberStyles.AllowExponent;
        private CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

        public RangeInfoViewModel(List<Mtc_Range> infQtys, List<Mtc_Range> vars, string[] constants, List<Assertion> assertions, string functionName, Unc_Template template)
        {
            dialog = new();
            dialog.Title = "Validation Error";
            dialog.Button = System.Windows.MessageBoxButton.OK;
            dialog.Image = System.Windows.MessageBoxImage.Warning;

            this.vars = vars;
            this.constants = constants;
            this.assertions = assertions;
            this.functionName = functionName;
            this.template = template;
            UncRanges = new();

            // Get existing cases, vars, constants, and vars
            InfQtyRange = new Range_Influence_Quantity(infQtys);

            RangeGrid = new();
            RangeGrid.Columns.Add("Minimum");
            RangeGrid.Columns.Add("Maximum");

            for (int i = 0; i < constants.Length; i++)
            {
                RangeGrid.Columns.Add(constants[i]);
            }

            if (Vars.Count == 0)
            {
                dialog.Message = "You must configure your Technique's variables before adding Uncertaitny Ranges.";
                dialog.Show();
            }
        }

        #region properties;

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

        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; NotifyOfPropertyChange(() => FunctionName); }
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

        private List<Models.Unc_Range> uncRanges;

        public List<Models.Unc_Range> UncRanges
        {
            get { return uncRanges; }
            set { uncRanges = value; }
        }

        public List<Assertion> Assertions
        {
            get { return assertions; }
            set { assertions = value; NotifyOfPropertyChange(() => Assertions); }
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

        public void Save()
        {
            // Make sure we have what we need before trying to save a case

            // Check our assertions
            if (Assertions.Count > 0)
            {
                foreach (Assertion a in Assertions)
                {
                    if (a.SelectedValue == null || a.SelectedValue == "")
                    {
                        dialog.Message = "Please select or enter an Assertion Value for \"" + a.Name + "\"";
                        dialog.Show();
                        return;
                    }
                }
            }

            // Check if an Infuance quantity was selected
            double test;            
            if (InfQtyRange.SelectedQty != null)
            {
                if (InfQtyRange.SelectedQty.name != "" || InfQtyRange.Min == "" || InfQtyRange.Max == "")
                {
                    // Check that the min max are numbers

                    if (!double.TryParse(infQtyRange.Min, styles, culture, out test) || !double.TryParse(infQtyRange.Max, styles, culture, out test))
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
                        if (double.Parse(InfQtyRange.Min, styles) < double.Parse(min, styles) || double.Parse(InfQtyRange.Max, styles) > double.Parse(max, styles))
                        {
                            dialog.Message = "Your Influance Quantity min and max is outside of the assinged range";
                            dialog.Show();
                            return;
                        }
                    }
                } else
                {
                    InfQtyRange.SelectedQty = null;
                }
            }

            if (SelectedVar == null || SelectedVar.name == "")
            {
                dialog.Message = "You must select a Parameter";
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
            UncRanges.Clear();
            for (int i = 0; i < rowData.Count; i++)
            {
                var row = rowData[i];
                var uncRange = new Models.Unc_Range();
                uncRange.Min = row["Minimum"];
                uncRange.Max = row["Maximum"];

                for (int j = 0; j < constants.Length; j++)
                {
                    uncRange.Constants.Add(new Range_Constant()
                    {
                        ConstName = constants[j],
                        Value = row[constants[j]]
                    });
                }
                uncRanges.Add(uncRange);
            }

            // Now check that we have the values needed
            if (uncRanges.Count == 0)
            {
                dialog.Message = "You did not set any Uncertainty Ranges.";
                dialog.Show();
                return;
            }

            // Check Constants
            foreach (Models.Unc_Range range in UncRanges)
            {
                if (constants.Length > 0 && range.Constants.Count == 0)
                {
                    dialog.Message = "One of the Uncertainty Ranges has Constants that must be entered.";
                    dialog.Show();
                    return;
                }

                foreach (Range_Constant constant in range.Constants)
                {
                    if (!double.TryParse(constant.Value, styles, culture, out test))
                    {
                        dialog.Message = "One of your Constant's values must be a number";
                        dialog.Show();
                        return;
                    }
                }
            }

            foreach (Models.Unc_Range range in UncRanges)
            {
                if (range.Min == null || range.Max == null || range.Min == "" || range.Max == "")
                {
                    dialog.Message = "You Uncertainty Range with no Min or Max set.";
                    dialog.Show();
                    return;
                }
                if (!double.TryParse(range.Min, styles, culture, out test) || !double.TryParse(range.Max, styles, culture, out test))
                {
                    dialog.Message = "You have an Uncertianty Range's min and max that are not numbers";
                    dialog.Show();
                    return;
                }
                // Make sure the range they chose a value that falls withing the parameter range
                if (SelectedVar.Start.ValueString != "")
                {
                    if (double.Parse(range.Min, styles, culture) < double.Parse(SelectedVar.Start.ValueString, styles, culture) ||
                        double.Parse(range.Max, styles, culture) > double.Parse(SelectedVar.End.ValueString, styles, culture))
                    {
                        dialog.Message = "One of your Uncertainty Ranges is outside the Parameter's assigned Range";
                        dialog.Show();
                        return;
                    }
                }
            }

            // see if we have a case already
            List<string> values = new();
            foreach (Assertion a in Assertions)
            {
                values.Add(a.SelectedValue);
            }

            var exsitingCase = template.getCasesByAssertionValues(functionName, values.ToArray());
            // if we have an existing case add the unc range to it
            if (exsitingCase.Count > 0)
            {
                if (exsitingCase[0].Ranges == null) exsitingCase[0].Ranges = new();
                AddRanges(exsitingCase[0]);
                Helper.TreeViewCase = exsitingCase[0];
            }
            else // We need to add a new case and ranges
            {
                // Get our assertions together
                Unc_Assertions unc_assertions = new Unc_Assertions();
                foreach (Assertion a in Assertions)
                {
                    Unc_Assertion unc_a = new Unc_Assertion()
                    {
                        Name = a.Name,
                        Value = a.SelectedValue,
                        type = ""
                    };
                    unc_assertions.Add(unc_a);
                }
                Unc_Case newCase = new Unc_Case(template, unc_assertions);
                if (newCase.Ranges == null) newCase.Ranges = new();
                AddRanges(newCase);
                Helper.TreeViewCase = newCase;
                template.CMCUncertaintyFunctions[0].Cases.Add(newCase);
            }
            base.TryCloseAsync(null);
        }

        private void AddRanges(Unc_Case Case)
        {
            // We need to see if an existing range is being edited.  The user might just be updating a Constant value.
            // First check the Influence Quantity
            SOA_DataAccessLib.Unc_Range startingRange = null;
            bool newRanges = true;
            var ranges = Case.Ranges.RangesByVarType("influence_quantity");
            NumberStyles styles = NumberStyles.Number | NumberStyles.AllowExponent;
            foreach (SOA_DataAccessLib.Unc_Range range in ranges)
            {
                if (range.Variable_name == InfQtyRange.SelectedQty.name)
                {
                    // now see if we are adding a new Qty via the ranges or a new set
                    if (range.Start.Value == decimal.Parse(InfQtyRange.Min, styles) && range.End.Value == decimal.Parse(InfQtyRange.Max, styles))
                    {
                        startingRange = range;
                        newRanges = false;
                        break;
                    }
                }
            }

            if (startingRange == null) // See if we have any parameter ranges that exist if they were not nested in an Influence Quantity
            {
                ranges = Case.Ranges.RangesByVarType("parameter");
                foreach (SOA_DataAccessLib.Unc_Range range in ranges)
                {
                    if (range.Variable_name == SelectedVar.name)
                    {
                        // now see if we are adding a new Qty via the ranges or a new set
                        if (range.Start.Value == SelectedVar.Start.Value && range.End.Value == SelectedVar.End.Value)
                        {
                            startingRange = range;
                            newRanges = false;
                            break;
                        }
                    }
                }
            }

            if (startingRange == null)
            {
                if (InfQtyRange.SelectedQty != null)
                {
                    // Start and stop ranges for the influence qty
                    startingRange = new SOA_DataAccessLib.Unc_Range(template);
                    startingRange.Start = SetStart("at", InfQtyRange.Min, InfQtyRange.SelectedQty.Start.Quantity);
                    startingRange.End = SetEnd("at", InfQtyRange.Max, InfQtyRange.SelectedQty.End.Quantity);
                    startingRange.Variable_name = InfQtyRange.SelectedQty.name;
                    startingRange.Variable_type = "influence_quantity";

                    // See if we need to set the Case Ranges var and type
                    if (Case.Ranges.variable_name == "")
                    {
                        Case.Ranges.variable_name = InfQtyRange.SelectedQty.name;
                        Case.Ranges.variable_type = "influence_quantity";
                    }
                }
                else
                {
                    // See if we need to set the Case Ranges var and type
                    if (Case.Ranges.variable_name == "")
                    {
                        Case.Ranges.variable_name = SelectedVar.name;
                        Case.Ranges.variable_type = "parameter";
                    }
                }
            }

            // Add our variable ranges and constants
            int count = 1;
            if (infQtyRange.SelectedQty != null)
            {
                startingRange.Ranges.variable_name = SelectedVar.name;
                startingRange.Ranges.variable_type = "parameter";
            }
            
            foreach (Models.Unc_Range range in UncRanges)
            {
                var paramRange = new SOA_DataAccessLib.Unc_Range(template);
                paramRange.ConstantValues = new();
                paramRange.Variable_name = SelectedVar.name;
                paramRange.Variable_type = "parameter";
                if (count > 1)
                    paramRange.Start = SetStart("at", range.Min, SelectedVar.Start.Quantity);
                else
                    paramRange.Start = SetStart("at", range.Min, SelectedVar.Start.Quantity);
                paramRange.End = SetEnd("at", range.Max, SelectedVar.Start.Quantity);
                foreach (Range_Constant constant in range.Constants)
                {
                    paramRange.ConstantValues.Add(new Unc_ConstantValue()
                    {
                        const_parameter_name = constant.ConstName,
                        Value = decimal.Parse(constant.Value, styles),
                        Quantity = SelectedVar.Start.Quantity,
                        symbol = UomDataSource.getQuantity(SelectedVar.Start.Quantity).UoM.symbol
                    });
                }
                count++;
                if (infQtyRange.SelectedQty != null)
                {
                    startingRange.Ranges.Add(paramRange);
                }
                else
                {
                    Case.Ranges.Add(paramRange);   
                }
            }

            if (newRanges && infQtyRange.SelectedQty != null) Case.Ranges.Add(startingRange);
        }

        private Unc_Range_Start SetStart(string test, string min, string quantity)
        {
            Unc_Range_Start start = new Unc_Range_Start();
            var qty = UomDataSource.getQuantity(quantity);
            start.test = test;
            start.Value = decimal.Parse(min, styles);
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
            end.Value = decimal.Parse(max, styles);
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