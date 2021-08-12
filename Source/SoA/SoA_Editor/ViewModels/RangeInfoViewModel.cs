using Caliburn.Micro;
using SOA_DataAccessLib;
using System.Collections.Generic;

namespace SoA_Editor.ViewModels
{
    internal class RangeInfoViewModel : Screen
    {
        private string[] avaiableInfQty;
        private string[] vars;
        private Unc_Template template;
        private Helper.MessageDialog dialog;
        private bool firstCase;

        public RangeInfoViewModel(string[] avaiableInfQty, string[] vars, Unc_Template template, bool firstCase = false)
        {
            this.avaiableInfQty = avaiableInfQty;
            this.vars = vars;
            this.firstCase = firstCase;
            this.template = template;

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
                    Assertion1.Name = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Name;
                    Assertion2.Name = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[1].Name;
                    if (!firstCase)
                    {
                        Assertion1.Value = "";
                        Assertion2.Value = "";
                    }
                    else
                    {
                        Assertion1.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Value;
                        Assertion2.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[1].Value;
                    }
                }
                else
                {
                    Assertion1 = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0];
                    Assertion2 = null;
                    if (!firstCase)
                    {
                        Assertion1.Value = "";
                    }
                    else
                    {
                        Assertion1.Value = this.template.CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Value;
                    }
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

        #endregion Methods
    }
}