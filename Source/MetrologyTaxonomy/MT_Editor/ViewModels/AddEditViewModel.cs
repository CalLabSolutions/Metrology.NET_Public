using Caliburn.Micro;
using MT_DataAccessLib;
using MT_Editor.Converters;
using MT_Editor.Extensions;
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

            if (this.edit)
            {
                AddEdit = "Edit Taxon";
            }
            else
            {
                AddEdit = "Add Taxon";
            }

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

            // Set up Results Tab
            if (TaxonToSave.Results == null)
            {
                TaxonToSave.Results = new List<Result>();
            }
            Results = new ObservableCollection<Result>(TaxonToSave.Results);

            // Set up Discipline Tab
            if (TaxonToSave.Discipline == null)
            {
                TaxonToSave.Discipline = new Discipline
                {
                    SubDisciplines = new List<string>()
                };
            }
            else if (TaxonToSave.Discipline != null && TaxonToSave.Discipline.SubDisciplines == null)
            {
                TaxonToSave.Discipline.SubDisciplines = new List<string>();
            }
            DisciplineName = TaxonToSave.Discipline.Name;
            SubDisciplines = new ObservableCollection<string>(TaxonToSave.Discipline.SubDisciplines);

            // Set up Ext. Reference Tab
            if (TaxonToSave.ExternalReference == null)
            {
                TaxonToSave.ExternalReference = new ExternalReference()
                {
                    CategoryTags = new List<CategoryTag>()
                };
            }
            else if (TaxonToSave.ExternalReference != null && TaxonToSave.ExternalReference.CategoryTags == null)
            {
                TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>();
            }

            RefName = TaxonToSave.ExternalReference.Name;
            Url = TaxonToSave.ExternalReference.Url;
            CategoryTags = new ObservableCollection<CategoryTag>(TaxonToSave.ExternalReference.CategoryTags);

            // Set up validation message
            parsing = false;
            dialog.Title = "Validation Error";
            dialog.Button = MessageBoxButton.OK;
            dialog.Image = MessageBoxImage.Error;
        }

        private string addEdit;

        public string AddEdit
        {
            get { return addEdit; }
            set
            {
                addEdit = value;
                NotifyOfPropertyChange(() => AddEdit);
            }
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

        private string quantityStr = "";

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

        private string process = "";

        public string Process
        {
            get { return process; }
            set
            {
                process = value;
                if (process.Length > 0)
                {
                    if (SelectedQuantity != null)
                        QuantityStr = SelectedQuantity.QuantitiyName + ".";
                    else
                        QuantityStr = ".";
                }   
                else
                {
                    if (SelectedQuantity != null)
                        QuantityStr = SelectedQuantity.QuantitiyName;
                    else
                        QuantityStr = "";
                }
                BuildTaxonName();
                NotifyOfPropertyChange(() => Process);
            }
        }

        private string definition = "";

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
                if (types == Types.MeasureRatio)
                {
                    TypeStr = "Measure.Ratio.";
                }
                else if (types == Types.SourceRatio)
                {
                    TypeStr = "Source.Ratio.";
                }
                else
                {
                    TypeStr = value.ToString() + ".";
                }
                
                NotifyOfPropertyChange(() => Types);
            }
        }

        private string typeStr = "";

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
                if (Process.Length > 0)
                    QuantityStr = value.QuantitiyName + ".";
                else
                    QuantityStr = value.QuantitiyName;
                NotifyOfPropertyChange(() => SelectedQuantity);
            }
        }

        #endregion Deatils Properties

        #region Details Tools

        // Get the elements of the taxon from the name and set the xaml elmements up accordingly
        private void ParseTaxon(Taxon taxon)
        {
            // set types
            if (taxon.Name.ToLower().Contains("measure.ratio"))
            {
                Types = Types.MeasureRatio;
            }
            else if (taxon.Name.ToLower().Contains("source.ratio"))
            {
                Types = Types.SourceRatio;
            }
            else if (taxon.Name.ToLower().Contains("measure"))
            {
                Types = Types.Measure;
            }
            else if (taxon.Name.ToLower().Contains("source"))
            {
                Types = Types.Source;
            }

            try
            {
                int firstDot = taxon.Name.IndexOf(".", 1) + 1;
                int secondDot = taxon.Name.IndexOf(".", firstDot) + 1;
                if (Types == Types.SourceRatio || Types == Types.MeasureRatio) // We have another dot to worry about now
                {
                    secondDot = taxon.Name.IndexOf(".", secondDot) + 1;
                }
                int thirdDot = taxon.Name.IndexOf(".", secondDot) + 1;
                string qname = "";
                if (thirdDot > 0)
                {
                    qname = taxon.Name.Substring(secondDot, (thirdDot - secondDot) - 1);
                }
                else
                {
                    qname = taxon.Name.Substring(secondDot);
                }

                

                qname = qname.Replace(" ", "-").ToLower();
                var UomQuantity = UomDataSource.getQuantity(qname);
                if (UomQuantity != null) SelectedQuantity = Quantity.FormatUomQuantity(UomQuantity);

                if (thirdDot > 0)
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
                if (paramQuantities.Count == 0)
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

            if (SelectedParamQuantity == null)
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
                    Name = SelectedParamQuantity.QuantitiyName
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

        public void DeleteParam(string name)
        {
            Parameters.RemoveAll(p => p.Name.ToLower().Equals(name.ToLower()));
            TaxonToSave.Parameters = new List<Parameter>(Parameters);
        }

        #endregion Parameters Methods

        //========================================================================
        // Results Tab Properties and Methods                                    |
        //========================================================================

        #region Results Properties

        private string resultName;

        public string ResultName
        {
            get { return resultName; }
            set
            {
                resultName = value;
                NotifyOfPropertyChange(() => ResultName);
            }
        }

        private ObservableCollection<Result> results;

        public ObservableCollection<Result> Results
        {
            get { return results; }
            set
            {
                if (value == null) return;
                results = value;
                NotifyOfPropertyChange(() => Results);
            }
        }

        private Quantity selectedResultQuantity;

        public Quantity SelectedResultQuantity
        {
            get { return selectedResultQuantity; }
            set
            {
                if (value == null) return;
                selectedResultQuantity = value;
                NotifyOfPropertyChange(() => SelectedResultQuantity);
            }
        }

        private List<Quantity> resultQuantities = new List<Quantity>();

        public List<Quantity> ResultQuantities
        {
            get
            {
                if (resultQuantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantitiesDic)
                    {
                        // Format for Display
                        if (entry.Value != null)
                            resultQuantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }
                return resultQuantities;
            }
        }

        #endregion Results Properties

        #region Results Methods

        private bool ValidateResults()
        {
            // verify required inputs
            if (ResultName == null || ResultName == "")
            {
                dialog.Message = "A Result must have a name";
                return false;
            }

            if (SelectedResultQuantity == null)
            {
                dialog.Message = "A Result must have a Quantity";
                return false;
            }

            // make sure the name is not already in use
            if (Results.Where(r => r.Name.ToLower().Equals(ResultName)).ToList().Count > 0)
            {
                dialog.Message = "That Result Name already exists";
                return false;
            }

            return true;
        }

        public void AddResult()
        {
            if (ValidateResults())
            {
                var q = new MT_DataAccessLib.Quantity()
                {
                    Name = SelectedResultQuantity.QuantitiyName
                };
                Result result = new Result()
                {
                    Name = ResultName,
                    Quantity = q
                };
                Results.Add(result);
                TaxonToSave.Results = new List<Result>(Results);
            }
            else
            {
                dialog.Show();
            }
        }

        public void DeleteResult(string name)
        {
            Results.RemoveAll(p => p.Name.ToLower().Equals(name.ToLower()));
            TaxonToSave.Results = new List<Result>(Results);
        }

        #endregion Results Methods

        //========================================================================
        // Discipline Tab Properties and Methods                                 |
        //========================================================================

        #region Discipline Properties

        private string disciplineName;

        public string DisciplineName
        {
            get { return disciplineName; }
            set
            {
                disciplineName = value;
                TaxonToSave.Discipline.Name = value;
                NotifyOfPropertyChange(() => DisciplineName);
            }
        }

        private string subDisciplineName;

        public string SubDisciplineName
        {
            get { return subDisciplineName; }
            set
            {
                subDisciplineName = value;
                NotifyOfPropertyChange(() => SubDisciplineName);
            }
        }

        private ObservableCollection<string> subDisciplines;

        public ObservableCollection<string> SubDisciplines
        {
            get { return subDisciplines; }
            set
            {
                subDisciplines = value;
                NotifyOfPropertyChange(() => SubDisciplines);
            }
        }

        #endregion Discipline Properties

        #region Discipline Methods

        public void AddSub(string subName)
        {
            // make sure the name is not already in use
            if (SubDisciplines.Where(s => s.ToLower().Equals(subName)).ToList().Count > 0)
            {
                dialog.Message = "That Sub Discipline already exists";
                dialog.Show();
                return;
            }
            SubDisciplines.Add(subName);
            TaxonToSave.Discipline.SubDisciplines = new List<string>(SubDisciplines);
        }

        public void DeleteSub(string subName)
        {
            SubDisciplines.RemoveAll(s => s.ToLower().Equals(subName.ToLower()));
            TaxonToSave.Discipline.SubDisciplines = new List<string>(SubDisciplines);
        }

        #endregion Discipline Methods

        //========================================================================
        // Ext. reference Tab Properties and Methods                             |
        //========================================================================

        #region Ext. Reference Properties

        private string refName;

        public string RefName
        {
            get { return refName; }
            set
            {
                refName = value;
                TaxonToSave.ExternalReference.Name = value;
                NotifyOfPropertyChange(() => RefName);
            }
        }

        private string url;

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                TaxonToSave.ExternalReference.Url = value;
                NotifyOfPropertyChange(() => Url);
            }
        }

        private CategoryTag categoryTag = new CategoryTag();

        public CategoryTag CategoryTag
        {
            get { return categoryTag; }
            set
            {
                categoryTag = value;
                NotifyOfPropertyChange(() => CategoryTag);
            }
        }

        private ObservableCollection<CategoryTag> categoryTags;

        public ObservableCollection<CategoryTag> CategoryTags
        {
            get { return categoryTags; }
            set
            {
                categoryTags = value;
                NotifyOfPropertyChange(() => CategoryTags);
            }
        }

        #endregion Ext. Reference Properties

        #region Ext. Reference Methods

        public void AddCat(CategoryTag catTag)
        {
            if (catTag.Name == null) catTag.Name = "";
            if (catTag.Name != "" && CategoryTags.Where(c => c.Name.ToLower().Equals(catTag.Name)).ToList().Count > 0)
            {
                dialog.Message = "That Category Name already exists";
                dialog.Show();
                return;
            }
            if (catTag.Value == null)
            {
                dialog.Message = "Category Tag must at least have a Value";
                dialog.Show();
                return;
            }
            CategoryTags.Add(catTag);
            TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>(CategoryTags);
            CategoryTag = new CategoryTag();
        }

        public void DeleteCat(CategoryTag catTag)
        {
            CategoryTags.RemoveAll(c => c.Value.ToLower().Equals(catTag.Value.ToLower()));
            TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>(CategoryTags);
        }

        #endregion Ext. Reference Methods
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

        private string formatedName;

        public string FormatedName
        {
            get { return formatedName; }
            set { formatedName = value; }
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
                QuantitiyName = qname,
                FormatedName = string.Format("{0}  |  Base: {1}", qname, bname)
            };
        }
    }
}