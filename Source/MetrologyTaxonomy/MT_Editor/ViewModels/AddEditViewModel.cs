using Caliburn.Micro;
using MT_DataAccessLib;
using MT_Editor.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static MT_Editor.Helper;
using Parameter = MT_DataAccessLib.Parameter;

namespace MT_Editor.ViewModels
{
    internal class AddEditViewModel : Screen
    {
        private bool edit;
        private bool parsing = false;
        private TaxonomyFactory factory = null;
        private MessageDialog dialog = null;

        public AddEditViewModel()
        {
        }

        public AddEditViewModel(bool edit)
        {
            // Edit or add flag
            this.edit = edit;

            // Factory and Dialog
            factory = new();
            dialog = new();

            // Set up Details Tab
            Definition = TaxonToSave.Definition;
            quantitiesDic = UomDataSource.getQuantities();

            // parse out taxon name
            parsing = true;
            if (TaxonToSave.Name.Length > 0)
                ParseTaxon(TaxonToSave);
            else
                TaxonToSave.Name = "TestProcess.";

            // Set up Parameter Tab
            if (TaxonToSave.Parameters == null)
            {
                TaxonToSave.Parameters = new List<Parameter>();
            }
            Parameters = new ObservableCollection<Parameter>(TaxonToSave.Parameters);

            // Set up validation message
            parsing = false;
            dialog.Title = "Validation Error";
            dialog.Button = MessageBoxButton.OK;
            dialog.Image = MessageBoxImage.Error;
        }

        //========================================================================
        // Add/Edit Methods                                                      |
        //========================================================================

        #region Add Edit Methods

        public void SaveTaxon()
        {
            if (ValidateDetails())
            {
                if (edit)
                {
                    var taxonomy = factory.Edit(TaxonToSave, SelectedTaxon.Name);
                    factory.Save(taxonomy, SaveLocal);
                    Navigate(MenuItem.ALL);
                }
                else
                {
                    var taxonomy = factory.Add(TaxonToSave);
                    factory.Save(taxonomy, SaveLocal);
                    Navigate(MenuItem.ALL);
                }
            }
            else
            {
                dialog.Show();
                base.Refresh();
            }
        }

        private bool ValidateDetails()
        {
            // Make sure we have a well Helperated Taxon name and that is does not already exist
            // We need to check all the taxon for its elements
            if (TypeStr == "" || TypeStr == null)
            {
                dialog.Message = "The Taxon must have a Type";
                return false;
            }
            if (QuantityStr == "" || QuantityStr == null)
            {
                dialog.Message = "Please select a Quantity";
                return false;
            }
            if (Process == "" || Process == null)
            {
                dialog.Message = "Please enter a Process";
                return false;
            }

            // make sure it does not already exist
            if (!edit && factory.GetAllTaxons().Where(w => w.Name.ToLower().Equals(TaxonToSave.Name.ToLower())).ToList().Count > 0)
            {
                dialog.Message = string.Format("Taxon \"{0}\" already exists.", TaxonToSave.Name);
                return false;
            }

            // Make sure we have at least 1 Parameter and 1 Result
            if (TaxonToSave.Parameters == null)
            {
                dialog.Message = "You must have at least 1 Parameter";
                return false;
            }
            if (TaxonToSave.Parameters.Count == 0)
            {
                dialog.Message = "You must have at least 1 Parameter";
                return false;
            }
            if (TaxonToSave.Results == null)
            {
                dialog.Message = "You must have at least 1 Result";
                return false;
            }
            if (TaxonToSave.Results.Count == 0)
            {
                dialog.Message = "You must have at least 1 Result";
                return false;
            }

            return true;
        }

        #endregion Add Edit Methods

        //========================================================================
        // Details Tab Properties and Methods                                    |
        //========================================================================

        #region Deatils Properties

        private string quantityStr;

        public string QuantityStr
        {
            get { return quantityStr; }
            set
            {
                quantityStr = value;
                BuildTaxonName();
                NotifyOfPropertyChange(() => QuantityStr);
            }
        }

        private string process;

        public string Process
        {
            get { return process; }
            set
            {
                process = value;
                BuildTaxonName();
                NotifyOfPropertyChange(() => Process);
            }
        }

        private string definition;

        public string Definition
        {
            get { return definition; }
            set
            {
                definition = value;
                TaxonToSave.Definition = definition;
                NotifyOfPropertyChange(() => Definition);
            }
        }

        private Types types;

        public Types Types
        {
            get { return types; }
            set
            {
                types = value;
                TypeStr = value.ToString() + ".";
                NotifyOfPropertyChange(() => Types);
            }
        }

        private string typeStr;

        public string TypeStr
        {
            get { return typeStr; }
            set
            {
                typeStr = value;
                BuildTaxonName();
                NotifyOfPropertyChange(() => TypeStr);
            }
        }

        private Dictionary<string, UomDataSource.Quantity> quantitiesDic;

        private ObservableCollection<Quantity> quantities = new ObservableCollection<Quantity>();

        public ObservableCollection<Quantity> Quantities
        {
            get
            {
                if (quantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantitiesDic)
                    {
                        // Helperat for Display
                        if (entry.Value != null)
                            quantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }
                return quantities;
            }
        }

        private Quantity selectedQuantity;

        public Quantity SelectedQuantity
        {
            get { return selectedQuantity; }
            set
            {
                if (value == null) return;
                selectedQuantity = value;
                QuantityStr = value.QuantitiyName + ".";
                NotifyOfPropertyChange(() => SelectedQuantity);
            }
        }

        #endregion Deatils Properties

        #region Details Tools

        // Get the elements of the taxon from the name and set the xaml elmements up accordingly
        private void ParseTaxon(Taxon taxon)
        {
            // set types
            if (taxon.Name.ToLower().Contains("measure"))
            {
                Types = Types.Measure;
            }

            if (taxon.Name.ToLower().Contains("source"))
            {
                Types = Types.Source;
            }

            try
            {
                int firstDot = taxon.Name.IndexOf(".", 1) + 1;
                int secondDot = taxon.Name.IndexOf(".", firstDot) + 1;
                int thirdDot = taxon.Name.IndexOf(".", secondDot) + 1;

                var qname = taxon.Name.Substring(secondDot, (thirdDot - secondDot) - 1);

                qname = qname.Replace(" ", "-").ToLower();
                var UomQuantity = UomDataSource.getQuantity(qname);
                if (UomQuantity != null) SelectedQuantity = Quantity.FormatUomQuantity(UomQuantity);

                Process = taxon.Name.Substring(thirdDot);
            }
            catch { }
        }

        private void BuildTaxonName()
        {
            if (!parsing)
            {
                string name = "TestProcess.";
                if (TypeStr != "" && TypeStr != null)
                {
                    name += TypeStr;
                }
                if (QuantityStr != "" && QuantityStr != null)
                {
                    name += QuantityStr;
                }
                if (Process != "" && Process != null)
                {
                    name += Process;
                }
                TaxonToSave.Name = name;
            }
        }

        #endregion Details Tools

        //========================================================================
        // Parameters Tab Properties and Methods                                 |
        //========================================================================

        #region Parameters Properties

        private string paramName;

        public string ParamName
        {
            get { return paramName; }
            set
            {
                paramName = value;
                NotifyOfPropertyChange(() => ParamName);
            }
        }

        private string paramDefinition;

        public string ParamDefinition
        {
            get { return paramDefinition; }
            set
            {
                paramDefinition = value;
                NotifyOfPropertyChange(() => ParamDefinition);
            }
        }

        private bool optional = false;

        public bool Optional
        {
            get { return optional; }
            set
            {
                optional = value;
                NotifyOfPropertyChange(() => Optional);
            }
        }

        private Quantity selectedParamQuantity;

        public Quantity SelectedParamQuantity
        {
            get { return selectedParamQuantity; }
            set
            {
                selectedParamQuantity = value;
                NotifyOfPropertyChange(() => SelectedParamQuantity);
            }
        }

        private ObservableCollection<Parameter> parameters;

        public ObservableCollection<Parameter> Parameters
        {
            get { return parameters; }
            set
            {
                if (value == null) return;
                parameters = value;
                NotifyOfPropertyChange(() => Parameters);
            }
        }

        private List<Quantity> paramQuantities = new List<Quantity>();

        public List<Quantity> ParamQuantities
        {
            get
            {
                if (quantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantitiesDic)
                    {
                        // Format for Display
                        if (entry.Value != null)
                            paramQuantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }
                return paramQuantities;
            }
        }

        #endregion Parameters Properties

        #region Parameters Methods

        private bool ValidateParam()
        {
            // verify required inputs
            if (ParamName == null || ParamName == "")
            {
                dialog.Message = "A Parameter must have a name";
                return false;
            }

            if (SelectedQuantity == null)
            {
                dialog.Message = "A Parameter must have a Quantity";
                return false;
            }

            // make sure the name is not already in use
            if (Parameters.Where(p => p.Name.ToLower().Equals(ParamName)).ToList().Count > 0)
            {
                dialog.Message = "That Parameter name already exists";
                return false;
            }

            return true;
        }

        public void AddParam()
        {
            if (ValidateParam())
            {
                var q = new MT_DataAccessLib.Quantity()
                {
                    Name = SelectedQuantity.QuantitiyName
                };
                Parameter param = new Parameter()
                {
                    Name = ParamName,
                    Definition = ParamDefinition,
                    Optional = Optional,
                    Quantity = q
                };
                Parameters.Add(param);
                TaxonToSave.Parameters = new List<Parameter>(Parameters);
            }
            else
            {
                dialog.Show();
            }
        }

        #endregion Parameters Methods
    }

    // Quantity Object for the view
    public class Quantity
    {
        private string baseName;

        public string BaseName
        {
            get { return baseName; }
            set { baseName = value; }
        }

        private string quantitiyName;

        public string QuantitiyName
        {
            get { return quantitiyName; }
            set { quantitiyName = value; }
        }

        public static Quantity FormatUomQuantity(UomDataSource.Quantity quantity)
        {
            var bname = quantity.UoM.name;
            var bnameArr = bname.Split("-");
            if (bnameArr.Length > 0)
            {
                for (int i = 0; i < bnameArr.Length; i++)
                {
                    bnameArr[i] = bnameArr[i][0].ToString().ToUpper() + bnameArr[i].Substring(1);
                }
                bname = string.Join(" ", bnameArr);
            }
            else
            {
                bname = bname[0].ToString().ToUpper() + bname.Substring(1);
            }

            var qname = quantity.name;
            var qnameArr = qname.Split("-");
            if (qnameArr.Length > 0)
            {
                for (int i = 0; i < qnameArr.Length; i++)
                {
                    qnameArr[i] = qnameArr[i][0].ToString().ToUpper() + qnameArr[i].Substring(1);
                }
                qname = string.Join("-", qnameArr);
            }
            else
            {
                qname = qname[0].ToString().ToUpper() + qname.Substring(1);
            }

            return new Quantity()
            {
                BaseName = bname,
                QuantitiyName = qname
            };
        }
    }
}