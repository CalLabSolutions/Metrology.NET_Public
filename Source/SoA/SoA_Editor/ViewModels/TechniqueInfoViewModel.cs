using Caliburn.Micro;
using SOA_DataAccessLib;
using System;
using System.Collections.Generic;
using System.Windows;
using static SoA_Editor.ViewModels.Helper;

namespace SoA_Editor.ViewModels
{
    internal class TechniqueInfoViewModel : Screen
    {
        /**
         * To add a new Technique we will need to add the following objects
         * Unc_Technique
         * Mtc_Technique
         * Unc_CMC object with the new techniqe
         * Category and DUTs
         *
         * From here we can later add a Unc_CMCFunctions object
         */

        private string category = "";
        private string dut = "";
        private string name = "";
        private string taxonName = "";
        private string functionName = "";

        
        private Unc_Technique uncTechnique;
        private Unc_CMC cmc;

        // private vars for saving a new technique
        private Unc_Taxon uncTaxon;
        private Unc_CMCs uncCMCs;

        // private vars for editing an existing technique;
        private Unc_Template template;
        

        private MessageDialog dialog = new MessageDialog();

        public TechniqueInfoViewModel(Unc_Taxon taxon, Unc_CMCs cmcs)
        {
            taxonName = taxon.name;
            uncTaxon = taxon;
            uncCMCs = cmcs;
            dialog.Button = MessageBoxButton.OK;
            dialog.Image = MessageBoxImage.Exclamation;
            dialog.Title = "Validation Error";
            EditMode = false;
        }

        public TechniqueInfoViewModel(Unc_Technique technique, Unc_Template template, Unc_CMC cmc)
        {
            // We are in edit mode, populate the fields as needed
            EditMode = true;
            Name = technique.Name;
            FunctionName = technique.Technique.CMCUncertainties[0].function_name;
            int roleIndex = technique.Technique.RequiredEquipment.Roles.Count();
            foreach (Mtc_Role role in technique.Technique.RequiredEquipment.Roles)
            {
                if (role.Name.ToLower() == "source")
                {
                    Dut += string.Join(",S\n", role.DeviceTypes);
                }
                else
                {
                    Dut += string.Join(",M\n", role.DeviceTypes);
                }
            }
            // make sure the last device has the correct identifer
            var lastRole = technique.Technique.RequiredEquipment.Roles[roleIndex - 1];
            if (lastRole.Name.ToLower() == "source")
            {
                Dut += ",S";
            }
            else
            {
                Dut += ",M";
            }
            Category = cmc.Category.Name;
            this.template = template;
            this.cmc = cmc;
            this.uncTechnique = technique;
        }

        public void Edit()
        {
            if (Validate())
            {
                // Update technique and functions names where needed
                template.TemplateTechnique.Name = Name;
                template.CMCUncertaintyFunctions[0].name = FunctionName;
                uncTechnique.Technique.CMCUncertainties[0].function_name = FunctionName;
                uncTechnique.Name = Name;
                uncTechnique.Technique.Name = Name;                
                
                // cmc category
                cmc.Category.Name = Category == null ? "" : Category;

                // Clear and update device types
                cmc.DUT.DeviceTypes.Clear();
                uncTechnique.Technique.RequiredEquipment.Roles.Clear();
                UpdateDeviceTypes();
                TreeViewTechnique = uncTechnique;
                base.TryCloseAsync(null);
            }
        }

        public void Save()
        {
            try
            {
                if (Validate())
                {
                    // Add new CMC object
                    cmc = uncCMCs.CMCs.Add(uncCMCs);
                    cmc.Category.Name = Category == null ? "" : Category;

                    // we need to get the newest template
                    Unc_Template template = cmc.Templates[cmc.Templates.Count() - 1];
                    template.MtcTaxon = uncTaxon.Taxon;
                    template.TemplateTechnique = new Unc_TemplateTechnique(template)
                    {
                        Name = Name
                    };

                    // new Technique object data
                    uncTechnique = new Unc_Technique(uncCMCs)
                    {
                        Name = Name,
                        Taxon = uncTaxon.name,
                        //Uri = Url
                    };
                    uncTechnique.Technique.Taxon = uncTaxon.Taxon;
                    uncTechnique.Technique.TaxonName = uncTaxon.name;
                    uncTechnique.Technique.Name = Name;

                    // Parameters and Result Ranges
                    foreach (Mtc_Parameter param in uncTaxon.Taxon.Parameters)
                    {
                        if (!param.optional)
                        {
                            // Input Params
                            uncTechnique.Technique.Parameters.Add(new Mtc_Parameter(param.name, param.Quantity.name, param.optional));

                            // Input Params Ranges Default to 0 and 10 for now using the "at" test
                            uncTechnique.Technique.ParameterRanges.Add(new Mtc_Range()
                            {
                                name = param.name,
                                Start = getDefaultStart(param.Quantity),
                                End = getDefaultEnd(param.Quantity)
                            }); ;
                        }
                    }

                    // Result Ranges
                    foreach (Mtc_Result result in uncTaxon.Taxon.Results)
                    {
                        // Default to 0 and 10 for now using the "at" test
                        uncTechnique.Technique.ResultRanges.Add(new Mtc_Range()
                        {
                            name = result.Name,
                            Start = getDefaultStart(result.Quantity),
                            End = getDefaultEnd(result.Quantity)
                        });
                    }

                    // Device Types
                    UpdateDeviceTypes();

                    // Uncertainty/Function
                    Mtc_CMCUncertainty mtcUncertainty = new Mtc_CMCUncertainty()
                    {
                        function_name = FunctionName,
                        TechniqueName = Name
                    };
                    Unc_CMCFunction uncFunction = new Unc_CMCFunction()
                    {
                        name = FunctionName,
                        TechniqueName = Name,
                    };
                    uncTechnique.Technique.CMCUncertainties.Add(mtcUncertainty);
                    template.CMCUncertaintyFunctions.Add(uncFunction);
                    template.CMCUncertaintyFunctions[0].Cases = new Unc_Cases(template, new List<Unc_Case>());

                    // Add the technique to our template
                    template.MtcTechnique = uncTechnique.Technique;

                    // Add to our global value
                    TreeViewTechnique = uncTechnique;

                    // Close Window
                    base.TryCloseAsync(null);
                }
                else
                {
                    dialog.Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                dialog.Title = "Error";
                dialog.Message = "Sorry There was an error adding the Technique.";
                dialog.Show();
            }
        }

        public bool Validate()
        {
            bool validated = true;

            if (Name == null || Name == "")
            {
                validated = false;
                dialog.Message = "Please enter a Name for the Technique.";
            }

            if (FunctionName == null || functionName == "")
            {
                validated = false;
                dialog.Message = "Please enter an Uncertainty Name for the Technique";
            }
           
            return validated;
        }

        public string Category
        {
            get { return category; }
            set
            {
                category = value.Trim();
                NotifyOfPropertyChange(() => Category);
            }
        }

        public string Dut
        {
            get { return dut; }
            set
            {
                dut = value.Trim();
                NotifyOfPropertyChange(() => Dut);
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value.Trim();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string TaxonName
        {
            get { return taxonName; }
            set
            {
                taxonName = value;
                NotifyOfPropertyChange(() => TaxonName);
            }
        }

        public string FunctionName
        {
            get { return functionName; }
            set
            {
                functionName = value.Trim();
                NotifyOfPropertyChange(() => FunctionName);
            }
        }

        private bool editMode;
        
        public bool EditMode
        {
            get { return editMode; }
            set
            {
                editMode = value;
                NotifyOfPropertyChange(() => EditMode);
            }
        }

        private Mtc_Range_Start getDefaultStart(UomDataSource.Quantity quantity)
        {
            Mtc_Range_Start start = new Mtc_Range_Start();
            start.Quantity = quantity.name;
            start.symbol = quantity.UoM.symbol;
            start.Value = new decimal(0.0);
            start.ValueString = start.Value.ToString();
            start.format = start.ValueString;
            start.test = "at";
            return start;
        }

        private Mtc_Range_End getDefaultEnd(UomDataSource.Quantity quantity)
        {
            Mtc_Range_End end = new Mtc_Range_End();
            end.Quantity = quantity.name;
            end.symbol = quantity.UoM.symbol;
            end.Value = new decimal(10.0);
            end.ValueString = end.Value.ToString();
            end.format = end.ValueString;
            end.test = "at";
            return end;
        }

        // Add/Update device types
        public void UpdateDeviceTypes()
        {
            if (Dut == null || Dut == "") return;
            string deviceName;
            char type = 's';
            string[] duts;
            Mtc_Role source = new Mtc_Role() { Name = "source" };
            Mtc_Role measure = new Mtc_Role() { Name = "measure" };
            Mtc_Roles roles = new Mtc_Roles();
            foreach (string dt in Dut.Split("\n"))
            {
                duts = dt.Split(",");
                deviceName = duts[0].Trim();
                if (duts.Length > 1)
                {
                    type = duts[1][0];
                    type = char.ToLower(type);
                    type = type == 's' || type == 'm' ? type : 's';
                }
                if (type == 's')
                {
                    source.DeviceTypes.Add(deviceName);
                }
                else
                {
                    measure.DeviceTypes.Add(deviceName);
                }

                cmc.DUT.DeviceTypes.Add(deviceName);
            }
            if (measure.DeviceTypes.Count > 0)
            {
                roles.Add(measure);
            }

            if (source.DeviceTypes.Count > 0)
            {
                roles.Add(source);
            }

            uncTechnique.Technique.RequiredEquipment.Roles = roles;
        }
    }
}