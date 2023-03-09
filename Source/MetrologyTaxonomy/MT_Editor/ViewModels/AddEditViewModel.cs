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

            ShowAddReference = !edit;
            ShowRefs = edit;
            // Remove this and associated single reference code
            ShowRef = false;

            // Factory and Dialog
            factory = new();
            dialog = new();

            // Set up Details Tab
            Definition = TaxonToSave.Definition;
            quantitiesDic = UomDataSource.getQuantities();

            // parse out taxon name
            parsing = true;
            if (TaxonToSave.Name.Length > 0)
            {
                if (TaxonToSave.Name.Contains("TestProcess", System.StringComparison.CurrentCulture))
                {
                    TaxonToSave.Name.Replace("TestProcess.", "");
                }
                ParseTaxon(TaxonToSave);
            }
            else
                TaxonToSave.Name = "";

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

            CategoryTags = new ObservableCollection<CategoryTag>();

            // Set up validation message
            parsing = false;
            dialog.Title = "Validation Error";
            dialog.Button = MessageBoxButton.OK;
            dialog.Image = MessageBoxImage.Error;

            // Setup External References - Test Code
            if (TaxonToSave.ExternalReferences == null)
            {
                TaxonToSave.ExternalReferences = new ExternalReferences()
                {
                    References = new List<Reference>()
                };
            }
            else if (TaxonToSave.ExternalReferences != null && TaxonToSave.ExternalReferences.References == null)
            {
                TaxonToSave.ExternalReferences.References = new List<Reference>();
            }
            References = new ObservableCollection<Reference>(TaxonToSave.ExternalReferences.References);
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
                    {
                        QuantityStr = SelectedQuantity.QuantitiyName + ".";
                    }
                    else
                    {
                        QuantityStr = ".";
                    }
                }   
                else
                {
                    if (SelectedQuantity != null)
                    {
                        QuantityStr = SelectedQuantity.QuantitiyName;
                    }
                    else
                    {
                        QuantityStr = "";
                    }
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
                        {
                            quantities.Add(Quantity.FormatUomQuantity(entry.Value));
                        }
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
                {
                    QuantityStr = value.QuantitiyName + ".";
                }
                else
                {
                    QuantityStr = value.QuantitiyName;
                }
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
                if (taxon.Name.IndexOf("TestProcess.") > -1)
                {
                    taxon.Name = taxon.Name.Replace("TestProcess.", "");
                }
                int firstDot = taxon.Name.IndexOf(".", 1) + 1;
                int secondDot = taxon.Name.IndexOf(".", firstDot) + 1;
                string qname = "";
                if (secondDot > 0)
                {
                    qname = taxon.Name.Substring(firstDot, (secondDot - secondDot) - 1);
                }
                else
                {
                    qname = taxon.Name.Substring(firstDot);
                }

                

                qname = qname.Replace(" ", "-").ToLower();
                var UomQuantity = UomDataSource.getQuantity(qname);
                if (UomQuantity != null)
                {
                    SelectedQuantity = Quantity.FormatUomQuantity(UomQuantity);
                }

                if (secondDot > 0)
                {
                    Process = taxon.Name.Substring(secondDot);
                }
            }
            catch { }
        }

        private void BuildTaxonName()
        {
            if (!parsing)
            {
                string name = "";
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
                if (value == null)
                {
                    return;
                }
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
                        {
                            paramQuantities.Add(Quantity.FormatUomQuantity(entry.Value));
                        }
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
                MT_DataAccessLib.Quantity q = null;
                if (SelectedParamQuantity != null)
                {
                    q = new MT_DataAccessLib.Quantity()
                    {
                        Name = SelectedParamQuantity.BaseName
                    };
                }
                
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
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
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
                if (value == null)
                {
                    return;
                }
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
                if (value == null)
                {
                    return;
                }
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
                        {
                            resultQuantities.Add(Quantity.FormatUomQuantity(entry.Value));
                        }
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
                    Name = SelectedResultQuantity.BaseName
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
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
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
            if (string.IsNullOrWhiteSpace(subName))
            {
                return;
            }
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
            if (string.IsNullOrWhiteSpace(subName))
            {
                return;
            }
            SubDisciplines.RemoveAll(s => s.ToLower().Equals(subName.ToLower()));
            TaxonToSave.Discipline.SubDisciplines = new List<string>(SubDisciplines);
        }

        #endregion Discipline Methods

        //========================================================================
        // Ext. references Tab Properties and Methods                             |
        //========================================================================

        #region Ext. References Properties

        #region code to turn on or off reference styles 
        private bool showRef = true;

        // Disables existing External Reference
        public bool ShowRef
        {
            get { return showRef; }
            set
            {
                showRef = value;
                NotifyOfPropertyChange(() => ShowRef);
            }
        }

        private bool showRefs = true;

        // Enable new multiple reference tab
        public bool ShowRefs
        {
            get { return showRefs; }
            set
            {
                showRefs = value;
                NotifyOfPropertyChange(() => ShowRefs);
            }
        }
        #endregion code to turn on or off reference styles

        private bool showReferences = false;

        /// <summary>
        /// True to show expand details of selected external reference
        /// </summary>
        public bool ShowReferences
        {
            get { return showReferences; }
            set
            {
                showReferences = value;
                NotifyOfPropertyChange(() => ShowReferences);
            }
        }

        private string showHideContentText = "Show Details";

        /// <summary>
        /// Text to apply to button to show or hide details content
        /// </summary>
        public string ShowHideContentText
        {
            get { return showHideContentText; }
            set
            {
                showHideContentText = value;
                NotifyOfPropertyChange(() => ShowHideContentText);
            }
        }

        /// <summary>
        /// Index of selected reference to show details for
        /// </summary>
        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                NotifyOfPropertyChange(() => SelectedIndex);
            }
        }

        private string selectedName = string.Empty;

        /// <summary>
        /// Name of selected reference shown in details
        /// </summary>
        public string SelectedName
        {
            get { return selectedName; }
            set
            {
                selectedName = value;
                NotifyOfPropertyChange(() => SelectedName);
            }
        }

        private string selectedUrlValue = string.Empty;

        /// <summary>
        /// Url for selected reference in details
        /// </summary>
        public string SelectedUrl
        {
            get { return selectedUrlValue; }
            set
            {
                selectedUrlValue = value;
                NotifyOfPropertyChange(() => SelectedUrl);
            }
        }

        private ObservableCollection<CategoryTag> selectCategoryTags = new ObservableCollection<CategoryTag>();

        /// <summary>
        /// Category tags for selected reference in details
        /// </summary>
        public ObservableCollection<CategoryTag> SelectedCategoryTags
        {
            get { return selectCategoryTags; }
            set
            {
                selectCategoryTags = value;
                NotifyOfPropertyChange(() => SelectedCategoryTags);
            }
        }

        private ObservableCollection<Reference> references;

        /// <summary>
        /// Collection of references for a taxon
        /// </summary>
        public ObservableCollection<Reference> References
        {
            get { return references; }
            set
            {
                references = value;
                List<Reference> updateList = new List<Reference>();
                foreach (Reference item in references)
                {
                    updateList.Add(item);
                }
                TaxonToSave.ExternalReferences.References = updateList;
                NotifyOfPropertyChange(() => References);
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


        private Reference reference;

        /// <summary>
        /// Single reference for a taxon to add to external references
        /// </summary>
        public Reference Reference
        {
            get { return reference; }
            set
            {
                reference = value;
                NotifyOfPropertyChange(() => Reference);
            }
        }

        private ReferenceUrl referenceUrl;

        /// <summary>
        /// This contains the reference URL and name if added
        /// </summary>
        public ReferenceUrl ReferenceUrl
        {
            get { return referenceUrl; }
            set { referenceUrl = value; }
        }


        #endregion Ext. References Properties

        #region Ext. References Methods

        /// <summary>
        /// Shows the add reference tab
        /// </summary>
        public void EnableAddTab()
        {
            ShowExtRefTab = false;
            ShowAddReference = true;
            SelectAdd = true;
            AddRefName = string.Empty;
            AddRefUrl = string.Empty;
            CategoryList = new ObservableCollection<CategoryTag>();
        }

        public void DeleteReference(Reference reference)
        {
            if (reference != null &&
                References != null &&
                References.Where(c => c.ReferenceUrl.UrlValue.ToLower().Equals(reference.ReferenceUrl.UrlValue)).ToList().Count > 0)
            {
                var referenceToRemove = References.Where(c => c.ReferenceUrl.UrlValue.ToLower().Equals(reference.ReferenceUrl.UrlValue)).ToList();
                if (referenceToRemove != null)
                {
                    int index = References.IndexOf(referenceToRemove.FirstOrDefault<Reference>());
                    References.Remove(referenceToRemove.FirstOrDefault<Reference>());
                }
                if (References.Count == 0)
                {
                    References = new ObservableCollection<Reference>();
                    ShowRefs = false;
                    ShowReferences = false;
                    ShowAddReference = true;
                    SelectAdd = true;
                }
                TaxonToSave.ExternalReferences.References = new List<Reference>(References);
            }
        }

        /// <summary>
        /// Show more information about an existing resource
        /// </summary>
        /// <param name="index"></param>
        public void ShowMore(int index)
        {
            if (index == -1)
            {
                return;
            }
            if (ShowHideContentText.IndexOf("Show Details") > -1)
            {
                // if only 1 reference and user does not click on it set it as active
                if (index == -1 && References.Count > 0)
                {
                    SelectedIndex = 0;
                    index = 0;
                }
                ShowHideContentText = "Hide Details";
                var reference = References[index];
                SelectedName = reference.ReferenceUrl.UrlName;
                SelectedUrl = reference.ReferenceUrl.UrlValue;
                SelectedCategoryTags = new ObservableCollection<CategoryTag>(reference.CategoryTagList);
                ShowReferences = true;
            }
            else
            {
                ShowReferences = false;
                ShowHideContentText = "Show Details";
            }
        }

        /// <summary>
        /// This will add a new category to an existing reference
        /// </summary>
        /// <param name="catTag"></param>
        public void AddNewCat(CategoryTag catTag)
        {
            if (SelectedCategoryTags == null)
            {
                SelectedCategoryTags = new ObservableCollection<CategoryTag>();
            }
            if (catTag.Name == null)
            {
                catTag.Name = "";
            }
            if (catTag.Name != "" && SelectedCategoryTags.Where(c => c.Name.ToLower().Equals(catTag.Name)).ToList().Count > 0)
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
            SelectedCategoryTags.Add(catTag);
            References[SelectedIndex].CategoryTagList = new List<CategoryTag>(SelectedCategoryTags);
            TaxonToSave.ExternalReferences.References = new List<Reference>(References);
            CategoryTag = new CategoryTag();

        }

        /// <summary>
        /// Delete a category tag external references tab 
        /// </summary>
        /// <param name="catTag"></param>
        public void DeleteSelectCat(CategoryTag catTag)
        {
            if (catTag == null || string.IsNullOrWhiteSpace(catTag.Value))
            {
                return;
            }
            SelectedCategoryTags.RemoveAll(c => c.Value.ToLower().Equals(catTag.Value.ToLower()));
            References[SelectedIndex].CategoryTagList = new List<CategoryTag>(SelectedCategoryTags);
            TaxonToSave.ExternalReferences.References = new List<Reference>(References);
            CategoryTag = new CategoryTag();
        }

        #endregion Ext.References Methods

        #region Add Reference Properties
        private bool showExtRefsTab = false;

        /// <summary>
        /// This allows the external references tab to show after closing add reference
        /// </summary>
        public bool ShowExtRefTab
        {
            get { return showExtRefsTab; }
            set
            {
                showExtRefsTab = value;
                NotifyOfPropertyChange(() => ShowExtRefTab);
            }
        }
        /// <summary>
        /// True to switch to the add reference tab
        /// </summary>
        private bool showAddReference = false;

        public bool ShowAddReference
        {
            get { return showAddReference; }
            set
            {
                showAddReference = value;
                NotifyOfPropertyChange(() => ShowAddReference);
            }
        }
        /// <summary>
        /// Set add tab focus
        /// </summary>
        private bool selectAdd = false;
        public bool SelectAdd
        {
            get { return selectAdd; }
            set 
            { 
                selectAdd = value;
                NotifyOfPropertyChange(() => SelectAdd);
            }
        }

        private string refUrlName;

        /// <summary>
        /// This is the reference URL title if wanted
        /// </summary>
        public string AddRefName
        {
            get { return refUrlName; }
            set
            {
                refUrlName = value;
                NotifyOfPropertyChange(() => AddRefName);
            }
        }

        private string urlValue;

        /// <summary>
        /// This is a specific reference URL for a taxon
        /// </summary>
        public string AddRefUrl
        {
            get { return urlValue; }
            set
            {
                urlValue = value;
                NotifyOfPropertyChange(() => AddRefUrl);
            }
        }

        private ObservableCollection<CategoryTag> categoryList;

        /// <summary>
        /// Collection of category tags for a single reference
        /// </summary>
        public ObservableCollection<CategoryTag> CategoryList
        {
            get { return categoryList; }
            set
            {
                categoryList = value;
                NotifyOfPropertyChange(() => CategoryList);
            }
        }

        #endregion Add Reference Properties

        #region Add Reference methods

        /// <summary>
        /// This will cancel adding a reference and reload the external references tab
        /// </summary>
        public void CancelAddRef()
        {
            if (edit || References.Count  > 0)
            {
                ShowAddReference = false;
                SelectAdd = false;
                ShowExtRefTab = true;
            }
            AddRefName = string.Empty;
            AddRefUrl = string.Empty;
            CategoryList = new ObservableCollection<CategoryTag>();
        }

        /// <summary>
        /// Add a new category tag to an existing reference
        /// </summary>
        /// <param name="catTag"></param>
        public void AddRefCat(CategoryTag catTag)
        {
            if (catTag.Name == null)
            {
                catTag.Name = "";
            }
            if (!string.IsNullOrWhiteSpace(catTag.Value))
            {
                if (CategoryList == null)
                {
                    CategoryList = new ObservableCollection<CategoryTag>();
                }
                if (catTag.Name != "" && CategoryList.Where(c => c.Name.ToLower().Equals(catTag.Name)).ToList().Count > 0)
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
                CategoryList.Add(catTag);
                CategoryTag = new CategoryTag();
            }
        }

        /// <summary>
        /// Delete a category on the add reference tab
        /// </summary>
        /// <param name="catTag"></param>
        /// <param name="referenceIndex"></param>
        public void DeleteRefCat(CategoryTag catTag)
        {
            if (catTag == null || string.IsNullOrWhiteSpace(catTag.Value))
            {
                return;
            }
            CategoryList.RemoveAll(c => c.Value.ToLower().Equals(catTag.Value.ToLower()));
            CategoryTag = new CategoryTag();
        }

        /// <summary>
        /// Save a new reference to the editing taxon
        /// </summary>
        public void AddNewReference()
        {
            if (AddRefUrl == null || AddRefUrl.Length == 0)
            {
                dialog.Message = "New reference must contain a URL";
                dialog.Show();
                return;
            }
            Reference reference = new Reference();
            ReferenceUrl referenceUrl = new ReferenceUrl();
            referenceUrl.UrlName = AddRefName;
            referenceUrl.UrlValue = AddRefUrl;
            reference.ReferenceUrl = referenceUrl;
            if (CategoryList != null)
            {
                reference.CategoryTagList = new List<CategoryTag>(CategoryList);
            }
            References.Add(reference);
            TaxonToSave.ExternalReferences.References = new List<Reference>(References);
            // Hide add reference tab
            ShowAddReference = false;
            SelectAdd = false;
            // show Extern ref tab
            ShowExtRefTab = true;
            // close expanded section on Ext Reference
            ShowReferences = false;
            ShowHideContentText = "Show Details";
            if (!ShowRefs && !edit)
            {
                ShowRefs = true;
            }
        }

        #endregion Add Reference Methods

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