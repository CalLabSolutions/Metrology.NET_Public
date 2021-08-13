using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.Model;
using SoA_Editor.Models;
using System.Collections.ObjectModel;

namespace SoA_Editor.ViewModels
{
    internal class RangeInfoViewModel : Screen
    {
        private string[] avaiableInfQty;
        private string[] vars;
        private string[] constants;
        private string functionName;
        private Unc_Template template;
        private Helper.MessageDialog dialog;
        private bool firstCase;

        public RangeInfoViewModel(string[] avaiableInfQty, string[] vars, string[] constants, string functionName, Unc_Template template, bool firstCase = false)
        {
            this.avaiableInfQty = avaiableInfQty;
            this.vars = vars;
            this.constants = constants;
            this.functionName = functionName;
            this.firstCase = firstCase;
            this.template = template;
            var assertionNames = this.template.getCMCFunctionAssertionNames(functionName);

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
                InfQtyRange = new Range_Influence_Quantity(avaiableInfQty);
                Constants = new();
                for (int i = 0; i < constants.Length; i++)
                {
                    Constants.Add(new Range_Constant(this.constants[i], ""));
                }
                Vars = new();
                for (int i = 0; i < vars.Length; i++)
                {
                    Vars.Add(new ExpressionVariable(this.vars[i]));
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

        private ObservableCollection<ExpressionVariable> _vars;

        public ObservableCollection<ExpressionVariable> Vars
        {
            get { return _vars; }
            set
            {
                _vars = value;
                NotifyOfPropertyChange(() => Vars);
            }
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
            // Make sue we have what we need before trying to save a case
            bool error = false;

            // Check our assertions
            if (Assertion1.Value == "")
            {
                dialog.Message = "Please Select or enter the first Assertion Value";
                error = true;
            }
            if (Assertion2 != null && Assertion2.Value == "")
            {
                dialog.Message = "Please Select or enter the first Assertion Value";
                error = true;
            }

            // Check our Infuance quantity
            if ((InfQtyRange.InfQtyName == null || InfQtyRange.InfQtyName == "") || (InfQtyRange.Min == "" || InfQtyRange.Max == ""))
            {
                dialog.Message = "You must select and Inflence Quantity, Minimum, and Maximum values";
                error = true;
            }

            // Check that the min max are numbers
            double test;
            if (!double.TryParse(infQtyRange.Min, out test) || !double.TryParse(infQtyRange.Max, out test))
            {
                dialog.Message = "Your Influance Quantity min and max must be a number";
                error = true;
            }

            // Check Variables
            foreach (ExpressionVariable _var in Vars)
            {
                if (_var.Enable)
                {
                    if (_var.Min == null || _var.Max == null || _var.Min == "" || _var.Max == "")
                    {
                        dialog.Message = "You have an enabled Variable with no Min or Max set.";
                        error = true;
                        break;
                    }
                    if (!double.TryParse(_var.Min, out test) || !double.TryParse(_var.Max, out test))
                    {
                        dialog.Message = "Your Variables Range's min and max must be a number";
                        error = true;
                        break;
                    }
                }
            }

            // Check Constants
            foreach (Range_Constant constant in Constants)
            {
                if (!double.TryParse(constant.Value, out test))
                {
                    dialog.Message = "Your Constant values must be a number";
                    error = true;
                }
            }

            // If error show the user
            if (error)
            {
                dialog.Show();
                return;
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
            }
            else // We need to add a new case and ranges
            {
                // Get our assertions together
                Unc_Assertions assertions = new Unc_Assertions();
                assertions.Add(Assertion1);
                if (Assertion2 != null) assertions.Add(Assertion2);
                Unc_Case newCase = new Unc_Case(template, assertions);
                AddRanges(newCase);
            }
        }

        private void AddRanges(Unc_Case Case)
        {
            // Influence Quantity
            Case.Ranges.variable_name = InfQtyRange.InfQtyName;
            Case.Ranges.variable_type = "influence_quantity";

            // Start and stop ranges for the influence qty
            Unc_Range infQtyRange = new Unc_Range();
            infQtyRange.Start = SetStart("at", InfQtyRange.Min);
            infQtyRange.End = SetEnd("at", InfQtyRange.Max);

            // Add our variable ranges and constants
            int count = 1;
            foreach (ExpressionVariable _var in Vars)
            {
                if (!_var.Enable) continue;
                Unc_Range paramRange = new Unc_Range();
                paramRange.Variable_name = _var.Name;
                paramRange.Variable_type = "parameter";
                if (count > 1)
                    paramRange.Start = SetStart("after", _var.Min);
                else
                    paramRange.Start = SetStart("at", _var.Min);
                paramRange.End = SetEnd("at", _var.Max);
                foreach (Range_Constant constant in Constants)
                {
                    paramRange.ConstantValues.Add(new Unc_ConstantValue()
                    {
                        const_parameter_name = constant.ConstName,
                        Value = decimal.Parse(constant.Value)
                    });
                }
                count++;
                infQtyRange.Ranges.Add(paramRange);
            }

            Case.Ranges.Add(infQtyRange);
        }

        private Unc_Range_Start SetStart(string test, string min)
        {
            Unc_Range_Start start = new Unc_Range_Start();

            start.test = test;
            start.Value = decimal.Parse(min);
            start.ValueString = min;
            start.Quantity = "";
            start.symbol = "";
            return start;
        }

        private Unc_Range_End SetEnd(string test, string max)
        {
            Unc_Range_End end = new Unc_Range_End();

            end.test = test;
            end.Value = decimal.Parse(max);
            end.ValueString = max;
            end.Quantity = "";
            end.symbol = "";
            return end;
        }

        #endregion Methods
    }
}