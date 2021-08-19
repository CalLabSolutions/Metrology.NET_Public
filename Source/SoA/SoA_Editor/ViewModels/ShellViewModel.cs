using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using SoA_Editor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using MessageBox = System.Windows.MessageBox;

namespace SoA_Editor.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private CompanyModel _companyM;
        private CompanyInfoModel _companyInfoM;
        private TaxonomyInfoModel _taxonomyInfoM;

        private WelcomeViewModel _WelcomeVM = null;
        private CompanyInfoViewModel _companyInfoVM = null;
        private TaxonomyInfoViewModel _taxonomyInfoVM = null;
        private TaxonomyViewModel _taxonomyVM = null;
        private TechniqueViewModel _techiqueVM = null;
        private RangeViewModel _rangeVM = null;

        private string _lblCompanyInfoName = null;

        private string _fullPath = "";
        private bool _isSaveAs = false;

        private XDocument doc;
        private Soa soa;
        private SOA_DataAccess dao;

        //the global var to keep row elements as they're added in the recursive function
        private List<string> row;

        public ShellViewModel()
        {
            WelcomeVM = new WelcomeViewModel();
            _ = ActivateItemAsync(WelcomeVM);
        }

        //===================================================================
        //Methods to load views                                             |
        //===================================================================

        #region Load Views

        public void LoadCompanyViewModelObj()
        {
            if (soa != null)
            {
                Helper.LoadCompanyInfoFromSoaObjectToOpen(soa, CompanyM);
                _ = ActivateItemAsync(CompanyInfoVM);
            }
        }

        // show the welcome page
        public void LoadWelcomeViewModelObj()
        {
            _ = ActivateItemAsync(new WelcomeViewModel());
        }

        //show the corresponding taxonomy pane on the right-hand side based on the selected item from the menu
        private void LoadTaxonomyViewModelObj(string lbl)
        {
            TaxonomyVM = new TaxonomyViewModel();

            //get the correct taxon
            Mtc_Taxon taxon = soa.CapabilityScope.Activities[0].Taxons[lbl].Taxon;

            // title
            TaxonomyVM.TaxonName = taxon.Name;

            for (int i = 0; i < taxon.Results.Count(); i++)
            {
                TaxonomyVM.ResultQuant.Add(new TaxonomyResult()
                {
                    Name = taxon.Results[i].Name,
                    Quantity = taxon.Results[i].Quantity.name
                });
            }

            //set input params
            TaxonomyInputParam inputParam;

            for (int i = 0; i < taxon.Parameters.Count(); i++)
            {
                string param = taxon.Parameters[i].name;
                string quantity = taxon.Parameters[i].Quantity == null ? "" : taxon.Parameters[i].Quantity.name;
                string optional = taxon.Parameters[i].optional.ToString();

                inputParam = new TaxonomyInputParam(param, quantity, optional);

                TaxonomyVM.InputParams.Add(inputParam);
            }

            //set definition
            if (taxon.Definition != null)
            {
                TaxonomyVM.Definition = taxon.Definition.Value;
            }

            _ = ActivateItemAsync(TaxonomyVM);
        }

        //show the corresponding technique pane on the right-hand side based on the selected item from the menu
        private void LoadTechniqueViewModelObj(TechniqueNode node)
        {
            TechniqueVM = new TechniqueViewModel();
            var technique = soa.CapabilityScope.Activities[0].Techniques[node.Name];
            TechniqueVM.TaxonomyName = technique.Taxon;
            TechniqueVM.TechniqueName = technique.Name;

            var template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(technique.Name);
            var function = template.getCMCUncertaintyTechniqueName(technique.Name);
            TechniqueVM.Formula = function != null ? function[0].Expression : "";
            TechniqueVM.FunctionName = function != null ? function[0].function_name : "";
            TechniqueVM.Category = template.GetCMC_Category();
            technique.Technique.RequiredEquipment.Roles.ToList().ForEach(r =>
            {
                if (r.Name.ToLower() == "source")
                {
                    r.DeviceTypes.ToList().ForEach(d => TechniqueVM.SourceEquipment.Add(d));
                }
                else
                {
                    r.DeviceTypes.ToList().ForEach(d => TechniqueVM.MeasureEquipment.Add(d));
                }
            });

            for (int i = 0; i < technique.Technique.ParameterRanges.Count(); i++)
            {
                string paramRangeName = technique.Technique.ParameterRanges[i].name;
                string paramMin = technique.Technique.ParameterRanges[i].Start.Value.ToString();
                string paramMax = technique.Technique.ParameterRanges[i].End.Value.ToString();
                string testMin = technique.Technique.ParameterRanges[i].Start.test;
                string testMax = technique.Technique.ParameterRanges[i].End.test;

                TechniqueVM.InputParameterRanges.Add(new Technique_InputParameterRange(paramRangeName, paramMin, paramMax, testMin, testMax));
            }

            for (int i = 0; i < technique.Technique.Parameters.Count(); i++)
            {
                string inputParamName = technique.Technique.Parameters[i].name;
                bool inputParamOptional = technique.Technique.Parameters[i].optional;
                string inputParamQty;
                if (technique.Technique.Parameters[i].Quantity != null)
                {
                    inputParamQty = technique.Technique.Parameters[i].Quantity.name;
                }
                else inputParamQty = "";

                bool variable = false;
                string type = "";
                for (int j = 0; j < technique.Technique.CMCUncertainties[0].SymbolDefinitions.Count(); j++)
                {
                    if (technique.Technique.CMCUncertainties[0].SymbolDefinitions[j].parameter == inputParamName)
                    {
                        variable = true;
                        type = technique.Technique.CMCUncertainties[0].SymbolDefinitions[j].type;
                        break;
                    }
                }

                TechniqueVM.InputParameters.Add(new Technique_InputParameter(inputParamName, inputParamQty, inputParamOptional, variable, type));
            }

            for (int i = 0; i < technique.Technique.ResultRanges.Count(); i++)
            {
                string resultMin = technique.Technique.ResultRanges[i].Start.Value.ToString();
                string resultMax = technique.Technique.ResultRanges[i].End.Value.ToString();

                string name = technique.Technique.ResultRanges[i].name == "" ? "result" : technique.Technique.ResultRanges[i].name;
                string testMin = technique.Technique.ResultRanges[i].Start.test;
                string testMax = technique.Technique.ResultRanges[i].End.test;

                TechniqueVM.Outputs.Add(new Technique_Output(name, resultMin, resultMax, testMin, testMax));
            }

            for (int i = 0; i < technique.Technique.CMCUncertainties[0].SymbolDefinitions.Count(); i++)
            {
                string varName = technique.Technique.CMCUncertainties[0].SymbolDefinitions[i].parameter;
                string type = technique.Technique.CMCUncertainties[0].SymbolDefinitions[i].type;

                TechniqueVM.Variables.Add(new Technique_Variable(varName, type));
            }

            // We will need to update data
            TechniqueVM.Technique = technique;
            TechniqueVM.template = template;
            TechniqueVM.cmc = soa.CapabilityScope.Activities[0].Unc_CMCs.GetCMCByFunctionName(function[0].function_name);
            TechniqueVM.node = node;
            TechniqueViewModel.Instance = TechniqueVM;
            _ = ActivateItemAsync(TechniqueVM);
        }

        //show the corresponding range pane on the right-hand side based on the selected item from the menu
        private void LoadRangeViewModelObj(Node node)
        {
            // see if we are a child of the technique or assertion and get the required data
            string nodeName = node.Name;
            string techniqueName = "";
            Unc_Template template;
            Unc_Technique technique;
            Mtc_CMCUncertainty uncertainty;
            Unc_CMCFunction function;
            if (node.Parent.Type == NodeType.Technique)
            {
                techniqueName = node.Parent.Name;
            }
            else if (node.Parent.Type == NodeType.Assertion)
            {
                techniqueName = node.Parent.Parent.Name;
            }
            else
            {
                return;
            }
            technique = soa.CapabilityScope.Activities[0].Techniques[techniqueName];
            template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(technique.Name);
            uncertainty = technique.Technique.CMCUncertainties[0];
            function = template.CMCUncertaintyFunctions[uncertainty.function_name];
            RangeVM = new RangeViewModel();

            IList<String> formulaExpression = soa.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].Variables;

            RangeVM.ExprVars = new ObservableCollection<ExpressionVariable>();

            foreach (string var in formulaExpression)
            {
                RangeVM.ExprVars.Add(new ExpressionVariable(var));
            }

            RangeVM.RangeGrid = new DataTable();

            string activeHierarchylbl = "";
            Node tempNode = node;
            while (tempNode.Parent != null)
            {
                activeHierarchylbl = tempNode.Name + "\n" + activeHierarchylbl;
                if (tempNode.Name != techniqueName && tempNode.Name != "")
                {
                    RangeVM.assertionNodeValues.Add(tempNode.Name);
                }
                tempNode = tempNode.Parent;
            }

            RangeVM.activeHierarchy = activeHierarchylbl; //node.Parent.Name + "\n" + node.Name;

            //to insert first columns into the grid, only add assetion columns after the selected node (assertion node in the subtree)
            //first find the index of the selected assertion (node in the subtree) in the assertion list
            int foundAssertIndex = TreeView_helper.getAssertionNodeIndex(nodeName, function);

            //adding columns:

            //for each Assertion add a new column, starting from the index of the selected assertion node in the subtree
            int assertCount = function.Cases[0].Assertions.Count();
            for (int i = foundAssertIndex + 1; i < assertCount; i++)
            {
                RangeVM.RangeGrid.Columns.Add(function.Cases[0].Assertions[i].Name, typeof(string));
            }

            //if there is Ranges -> call addColumnToDataTable function with Ranges
            if (function.Cases[0].Ranges != null)
            {
                addColumnToDataTable(function.Cases[0].Ranges);
            }

            //adding rows:

            //for each case repeat this
            int caseCount = function.Cases.Count();
            for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
            {
                //fist create an appendable empty string array for row
                row = new List<string>();

                //here do the filtering based on the item selected from the left-side menu
                //if the value does not match continue with the next item in the case list
                if (!nodeName.ToUpper().Equals("ALL"))
                {
                    if (function.Cases[caseIndex].Assertions[foundAssertIndex].Value != nodeName)
                        continue;
                }

                //for each Assertion add a new cell to the row
                assertCount = function.Cases[caseIndex].Assertions.Count();
                for (int assertIndex = foundAssertIndex + 1; assertIndex < assertCount; assertIndex++)
                {
                    row.Add(function.Cases[caseIndex].Assertions[assertIndex].Value);
                }

                //if there is Ranges -> call addRowsToDataTable function with Ranges
                if (function.Cases[caseIndex].Ranges != null)
                {
                    addRowsToDataTable(function.Cases[caseIndex].Ranges);
                }
            }

            RangeVM.Formula = uncertainty.Expression;
            RangeVM.functionName = function.name;
            RangeVM.template = template;

            _ = ActivateItemAsync(RangeVM);
        }

        //recursive function to add columns to the dataTable
        private void addColumnToDataTable(Unc_Ranges ranges)
        {
            //get the variable_name and add as a column
            string varName = ranges.variable_name;
            RangeVM.RangeGrid.Columns.Add(varName, typeof(string));

            //under the first Range, if there is Ranges -> call recursive function with Ranges
            if (ranges.getRanges() != null && ranges.getRanges()[0].Ranges != null && ranges.getRanges()[0].Ranges.variable_name != "")
            {
                addColumnToDataTable(ranges.getRanges()[0].Ranges);
            }
            //else, under the first Range, if there are ConstantValues, add another column (Constants)
            else
            {
                if (ranges.getRanges()[0].ConstantValues != null)
                {
                    RangeVM.RangeGrid.Columns.Add("Constants", typeof(string));
                }
            }
        }

        //recursive function to add rows to the dataTable
        private void addRowsToDataTable(Unc_Ranges ranges)
        {
            //For each Range repeat this
            int rangeCount = ranges.getRanges().Count;
            for (int rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++)
            {
                //add a new cell with "start/end" values
                row.Add(ranges.getRanges()[rangeIndex].Start.Value + " to " + ranges.getRanges()[rangeIndex].End.Value);

                //if there is Ranges -> call recursive function with Ranges
                if (ranges.getRanges()[rangeIndex].Ranges != null && ranges.getRanges()[0].Ranges.variable_name != "")
                {
                    addRowsToDataTable(ranges.getRanges()[rangeIndex].Ranges);
                }
                //else add all values under ConstantValues as a new cell
                else
                {
                    string constVals = "";
                    int constValCount = ranges.getRanges()[rangeIndex].ConstantValues.Count();
                    for (int constValIndex = 0; constValIndex < constValCount; constValIndex++)
                    {
                        constVals += ranges.getRanges()[rangeIndex].ConstantValues[constValIndex].const_parameter_name + " = " + ranges.getRanges()[rangeIndex].ConstantValues[constValIndex].Value + "\n";
                    }

                    row.Add(constVals);

                    //now add the row
                    DataRow myrow = RangeVM.RangeGrid.NewRow();

                    for (int index = 0; index < RangeVM.RangeGrid.Columns.Count; index++)
                    {
                        myrow[RangeVM.RangeGrid.Columns[index].ColumnName] = row[index];
                    }

                    RangeVM.RangeGrid.Rows.Add(myrow);

                    row.RemoveAt(row.Count - 1);
                }

                row.RemoveAt(row.Count - 1);
            }
        }

        #endregion Load Views

        //===================================================================
        //Methods to Handle context menus                                   |
        //===================================================================

        #region Context Menus

        public void AddTaxon()
        {
            if (soa == null)
            {
                MessageBox.Show("Please open saved SoA file or start a new one", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 450;
            settings.Title = "Add new Taxon";

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(new TaxonomyInfoViewModel(soa.CapabilityScope.Activities[0].Taxons.Names()), null, settings);
            if (Helper.TreeViewSelectedTaxon != null)
            {
                // create our new node
                TaxonNode newNode = new TaxonNode() { Name = Helper.TreeViewSelectedTaxon.Name };
                //add the new node to the soa object
                soa.CapabilityScope.Activities[0].Taxons.Add(new Unc_Taxon(Helper.TreeViewSelectedTaxon));
                ReloadTree(newNode);
                Helper.TreeViewSelectedTaxon = null;
            }
        }

        public async void DeleteTaxon(TaxonNode node)
        {
            bool delete = await DeleteNode("Are you sure you want to delete the Taxon \"" + node.Name + "\"");

            if (delete)
            {
                // First remove children Techniques adn assoicated objects
                foreach (Node childNode in node.Children)
                {
                    var technique = soa.CapabilityScope.Activities[0].Techniques[childNode.Name];
                    soa.CapabilityScope.Activities[0].Techniques.Remove(technique);
                }
                // Remove the taxon
                var taxon = soa.CapabilityScope.Activities[0].Taxons[node.Name];
                soa.CapabilityScope.Activities[0].Taxons.Remove(taxon);
                ReloadTree();
            }
        }

        public void AddTechnique(TaxonNode node)
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 450;
            settings.Title = "Add new Technique";

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(new TechniqueInfoViewModel(soa.CapabilityScope.Activities[0].Taxons[node.Name],
                soa.CapabilityScope.Activities[0].Unc_CMCs), null, settings);
            if (Helper.TreeViewTechnique != null)
            {
                // create our new node
                TechniqueNode newNode = new TechniqueNode(node) { Name = Helper.TreeViewTechnique.Name };

                //add the new node to the soa object
                soa.CapabilityScope.Activities[0].Techniques.Add(Helper.TreeViewTechnique);
                ReloadTree(newNode);
                Helper.TreeViewTechnique = null;
            }
        }

        public async void DeleteTechnique(TechniqueNode node)
        {
            bool delete = await DeleteNode("Are you sure you want to delete the Technique \"" + node.Name + "\"");

            if (delete)
            {
                var technique = soa.CapabilityScope.Activities[0].Techniques[node.Name];
                // Remove technique
                soa.CapabilityScope.Activities[0].Techniques.Remove(technique);
                ReloadTree();
            }
        }

        public void AddRanges(TechniqueNode node)
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 450;

            IWindowManager manager = new WindowManager();

            Unc_Template template;
            Unc_Technique technique;
            string functionName;
            bool firstCase = false;

            // get the template and cases
            template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(node.Name);
            technique = soa.CapabilityScope.Activities[0].Techniques[node.Name];
            functionName = technique.Technique.CMCUncertainties[0].function_name;

            //Dictionary<string, Unc_Range_Min_Max> _vars = new();
            //Dictionary<string, Unc_Range_Min_Max> infQty = new();
            List<Mtc_Range> _vars = new();
            List<Mtc_Range> infQty = new();

            // see if we have added any cases yet because the same assertions must be used moving fwd
            if (template.CMCUncertaintyFunctions[0].Cases.Count() == 0)
            {
                node.Children.Add(new AssertionNode(node) { Name = "All" });
                settings.Title = "Add Assertion Names for new Ranges";
                manager.ShowDialogAsync(new RangeInfoViewModel(infQty, _vars, Array.Empty<string>(), functionName, template), null, settings);
                if (Helper.TreeViewCase != null)
                {
                    AssertionNode newNode = new AssertionNode(node) { Name = Helper.TreeViewCase.Assertions[0].Value };
                    node.Children.Add(newNode);

                    template.CMCUncertaintyFunctions[0].Cases.Add(Helper.TreeViewCase);
                    Helper.TreeViewCase = null;
                    firstCase = true; // we want to prefill and disable our assertion values if its the first case
                }
            }

            // Now let the user add a range because we should have assertions at this point
            settings.Title = "Add new Range";

            // We need our influence quantity and available expression symbols (variables)
            var expSymbols = technique.Technique.CMCUncertainties[0].ExpressionSymbols;
            var _params = technique.Technique.Parameters.Where(p => !expSymbols.Contains(p.name)).ToArray();
            string[] symbols = technique.Technique.CMCUncertainties[0].Variables.ToArray();
            var ranges = technique.Technique.ParameterRanges;

            for (int i = 0; i < _params.Length; i++)
            {
                var range = ranges[_params[i].name];
                if (range != null) infQty.Add(range);
            }

            for (int i = 0; i < symbols.Length; i++)
            {
                var range = ranges[symbols[i]];
                if (range != null) _vars.Add(range);
            }
            string[] constants = technique.Technique.CMCUncertainties[0].Constants.ToArray();
            manager.ShowDialogAsync(new RangeInfoViewModel(infQty, _vars, constants, functionName, template, firstCase), null, settings);
            // At this point it would be simplier to just reload the whole tree
            if (Helper.TreeViewCase != null)
            {
                ReloadTree(node);
            }
        }

        public async void DeleteRanges(Node node)
        {
            if (node.Name.ToLower() == "all") return;

            bool delete = false;

            if (node.Parent.Type == NodeType.Technique)
            {
                delete = await DeleteNode("Are you sure you want to delete the Ranges for Assertion \"" + node.Name + "\"");

                if (delete)
                {
                    // first lets get the technique
                    var technique = soa.CapabilityScope.Activities[0].Techniques[node.Parent.Name];

                    // get the template so we can remove the proper cases
                    var template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(technique.Name);

                    // see which cases have the selected assertion and remove them
                    List<Unc_Case> casesToRemove = new();
                    foreach (Unc_Case _case in template.CMCUncertaintyFunctions[0].Cases)
                    {
                        var assertions = _case.Assertions;

                        if (assertions.Where(a => a.Value == node.Name).Count() > 0)
                        {
                            casesToRemove.Add(_case);
                        }
                    }
                    if (casesToRemove.Count > 0)
                    {
                        foreach (Unc_Case _case in casesToRemove)
                        {
                            template.CMCUncertaintyFunctions[0].Cases.Remove(_case);
                        }
                    }
                }
            }
            else
            {
                // we need the parent node so that we are getting the exact case to remove
                string[] assertions = new string[2];
                assertions[0] = node.Parent.Name;
                assertions[1] = node.Name;

                delete = await DeleteNode("Are you sure you want to delete the Ranges for Assertions \"" + assertions[0] + "\" and \"" + assertions[1] + "\"");

                if (delete)
                {
                    // lets get the technique
                    var technique = soa.CapabilityScope.Activities[0].Techniques[node.Parent.Parent.Name];

                    // get the template so we can remove the proper cases
                    var template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(technique.Name);

                    var Case = template.getCaseByAssertionValues(technique.Technique.CMCUncertainties[0].function_name, assertions);
                    if (Case != null)
                    {
                        template.CMCUncertaintyFunctions[0].Cases.Remove(Case);
                    }
                }
            }

            if (delete) ReloadTree();
        }

        public void TaxonNodeClick(TaxonNode node)
        {
            LoadTaxonomyViewModelObj(node.Name);
        }

        public void TechniqueNodeClick(TechniqueNode node)
        {
            LoadTechniqueViewModelObj(node);
        }

        public void AssertionNodeClick(AssertionNode node)
        {
            LoadRangeViewModelObj(node);
        }

        public void RangeNodeClick(RangeNode node)
        {
            LoadRangeViewModelObj(node);
        }

        private async Task<bool> DeleteNode(string message)
        {
            DeleteDialogView view = new DeleteDialogView()
            {
                DataContext = new DeleteDialogViewModel()
            };
            var viewModel = (DeleteDialogViewModel)view.DataContext;
            viewModel.Message = message;

            object result = await DialogHost.Show(view, "RootDialog");

            return (bool)result;
        }

        #endregion Context Menus

        //===================================================================
        //File IO                                                           |
        //===================================================================

        #region File IO

        public void NewXML()
        {
            soa = new Soa();
            soa.CapabilityScope.Activities[0].Techniques.Clear();
            soa.CapabilityScope.Activities[0].Templates.Clear();
            soa.CapabilityScope.Activities[0].Taxons.Clear();
            soa.CapabilityScope.Activities[0].CMCs.Clear();
            soa.CapabilityScope.MeasuringEntity = "Company Name";
            RootNode = new();
            CompanyM = new(new CompanyInfoModel(), new TaxonomyInfoModel());
            CompanyInfoVM = new(new CompanyInfoModel());
            CompanyInfoVM.CompanyName = soa.CapabilityScope.MeasuringEntity;
            RootNode.Name = "soa";
            IsSaveAs = true;
            lblCompanyInfoName = "";
            SaveXML(true);
        }

        public void OpenXMLFile()
        {
            CompanyInfoM = new CompanyInfoModel();
            TaxonomyInfoM = new TaxonomyInfoModel();
            CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Multiselect = false;
            try
            {
                bool? dialogResult = dlg.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    soa = new Soa();
                    dao = new SOA_DataAccess();
                    FullPath = dlg.FileName;
                    fileAddr = "../" + dlg.SafeFileName;
                    dao.load(dlg.FileName);
                    soa = dao.SOADataMaster;
                    Helper.LoadCompanyInfoFromSoaObjectToOpen(soa, CompanyM); // assigns info extracted from XML to the CompanyM object
                    CompanyInfoVM.LoadCompanyInfo(); // copies info into local parameters to be shown in the view
                    _ = ActivateItemAsync(CompanyInfoVM);

                    //set name label
                    lblCompanyInfoName = CompanyInfoVM.CompanyName;
                }
                else
                {
                    return;
                }

                LoadTree();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                MessageBox.Show("The data file is invalid!");
                return;
            }
        }

        private void Reload()
        {
            soa = new Soa();
            dao = new SOA_DataAccess();
            dao.load(FullPath);
            soa = dao.SOADataMaster;
            Helper.LoadCompanyInfoFromSoaObjectToOpen(soa, CompanyM); // assigns info extracted from XML to the CompanyM object
            CompanyInfoVM.LoadCompanyInfo(); // copies info into local parameters to be shown in the view
            _ = ActivateItemAsync(CompanyInfoVM);

            //set name label
            lblCompanyInfoName = CompanyInfoVM.CompanyName;
        }

        private /*async*/ void LoadTree()
        {
            //var bar = await ShowProgressBar();
            
            //fill in treeview
            RootNode = new();
            RootNode.Name = "soa";

            for (int taxonIndex = 0; taxonIndex < soa.CapabilityScope.Activities[0].Taxons.Count(); taxonIndex++)
            {
                //add nodes in the Taxonomy level to the tree
                string taxonomyName = soa.CapabilityScope.Activities[0].Taxons[taxonIndex].name;
                TaxonNode taxonNode = new TaxonNode() { Name = taxonomyName };
                RootNode.Children.Add(taxonNode);
                var techniques = soa.CapabilityScope.Activities[0].Techniques.getByTaxonName(taxonomyName);

                for (int techniqueIndex = 0; techniqueIndex < techniques.Count; techniqueIndex++)
                {
                    //add nodes in the Technique level to the tree
                    string techniqueName = techniques[techniqueIndex].Name;
                    TechniqueNode techNode = new TechniqueNode(taxonNode) { Name = techniqueName };
                    taxonNode.Children.Add(techNode);

                    //in two steps add assertions as subtrees
                    //1) first build a matrix with assertions as rows and cases as columns
                    //2) then, go through the matrix column by column and build the tree accordingly

                    //--------------------------------
                    // first step -> build a matrix  |
                    //--------------------------------
                    //find the number of cases

                    DataTable assertionMatrix = new DataTable();
                    Unc_Template template = soa.CapabilityScope.Activities[0].GetTemplateByTemplateTechnique(techniqueName);

                    // loop throug our Unc Functions and add them to the technique
                    foreach (Unc_CMCFunction function in template.CMCUncertaintyFunctions)
                    {
                        //number of cases in the switch
                        int caseCount = function.Cases.Count();

                        if (caseCount == 0) return;

                        //number of assertions in a case. assuming it's the same for all cases, we look at the first case
                        int assertionCount = function.Cases[0].Assertions.Count();

                        //buid the matrix
                        //first create columns
                        for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
                        {
                            assertionMatrix.Columns.Add();
                        }

                        //now add the rows
                        for (int assertIndex = 0; assertIndex < assertionCount; assertIndex++)
                        {
                            DataRow row = assertionMatrix.NewRow();

                            for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
                            {
                                string assertStr = function.Cases[caseIndex].Assertions[assertIndex].Value;
                                row[caseIndex] = assertStr;
                            }
                            assertionMatrix.Rows.Add(row);
                        }

                        //------------------------------------
                        // second step -> build the subtree  |
                        //------------------------------------

                        //add a single node as "All" to show the whole assetion grid
                        AssertionNode assertAllNode = new AssertionNode(techNode) { Name = "All" };
                        techNode.Children.Add(assertAllNode);

                        //build the assertion subtree from the assertionMatrix
                        for (int c = 0; c < assertionMatrix.Columns.Count; c++)
                        {
                            Node parent = null;
                            Node current = null;
                            for (int r = 0; r < assertionMatrix.Rows.Count; r++)
                            {
                                DataRow dr = assertionMatrix.Rows[r];
                                string name = Convert.ToString(dr[c]);
                                if (parent == null)
                                {
                                    parent = new AssertionNode(techNode) { Name = name, Children = new ObservableCollection<Node>() };
                                    current = techNode.Children.Where(x => x.Name == parent.Name).SingleOrDefault();
                                    if (current == null)
                                        techNode.Children.Add((AssertionNode)parent);
                                    else
                                        parent = current;
                                }
                                else
                                {
                                    current = new RangeNode((AssertionNode)parent) { Name = name, Children = new ObservableCollection<Node>() };
                                    if (parent.Children.Where(x => x.Name == current.Name).SingleOrDefault() == null)
                                        parent.Children.Add((RangeNode)current);
                                    else
                                        current = parent.Children.Where(x => x.Name == current.Name).SingleOrDefault();
                                    parent = current;
                                }
                            }
                        }
                    }
                }
            }
            //if (bar != null) HideProgressBar();
        }

        private void ReloadTree(Node node = null)
        {
            // Auto Save
            SaveXML();
            // Reload to get some of the data populated correctly
            Reload();
            // Load the tree view
            LoadTree();
            // Expand out to the node we found
            if (node != null)
            {
                findNode(node, node.Name).IsExpanded = true;
            }
        }

        private async Task<ProgressBarView> ShowProgressBar()
        {
            if (!DialogHost.IsDialogOpen("RootDialog"))
            {
                // set up our dialog view
                ProgressBarView bar = new ProgressBarView()
                {
                    DataContext = new ProgressBarViewModel()
                };
                _ = await DialogHost.Show(bar, "RootDialog");
                return bar;
            }
            return null;
        }

        private void HideProgressBar()
        {
            DialogHost.Close("RootDialog");
        }

        public Node findNode(Node root, string findStr)
        {
            if (root.Name.Equals(findStr))
                return root;

            foreach (Node n in root.Children)
            {
                Node result = findNode(n, findStr);
                if (result != null)
                    return result;
            }

            return null;
        }

        public void SaveXML(bool newFile = false)
        {
            doc = new XDocument();

            if (!newFile)
            {
                Helper.LoadCompanyInfoToSoaObjectToSave(soa, CompanyInfoVM);
            }
            else
            {
                LoadCompanyViewModelObj();
            }

            soa.writeTo(doc);
            lblCompanyInfoName = soa.CapabilityScope.MeasuringEntity;

            if (FullPath.Length == 0 || IsSaveAs == true) // Saving a new file or Save as...
            {
                SaveFileDialog saveFileDlg = new SaveFileDialog();

                saveFileDlg.Filter = "XML Files (*.xml)|*.xml";
                saveFileDlg.RestoreDirectory = true;
                saveFileDlg.FileName = soa.CapabilityScope.MeasuringEntity + ".xml";

                bool? dialogResult = saveFileDlg.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    // Code to write the stream goes here.
                    FullPath = saveFileDlg.FileName;
                    doc.Save(FullPath);
                    IsSaveAs = false;
                    lblCompanyInfoName = soa.CapabilityScope.MeasuringEntity;
                    return;
                }
            }
            else if (FullPath.Length != 0)
            {
                doc.Save(FullPath);
            }
            IsSaveAs = false;
        }

        public void SaveAsXML()
        {
            IsSaveAs = true;
            SaveXML();
        }

        public void ExitApp()
        {
            Environment.Exit(0);
        }

        #endregion File IO

        //===================================================================
        // Properties                                                       |
        //===================================================================

        #region Properties

        private string _fileAddr;

        public string fileAddr
        {
            get { return _fileAddr; }
            set { Set(ref _fileAddr, value); }
        }

        private Node mRootNode;

        public Node RootNode
        {
            get { return mRootNode; }
            set { Set(ref mRootNode, value); }
        }

        public string lblCompanyInfoName
        {
            get { return _lblCompanyInfoName; }
            set { _lblCompanyInfoName = value; NotifyOfPropertyChange(() => lblCompanyInfoName); }
        }

        public WelcomeViewModel WelcomeVM
        {
            get { return _WelcomeVM; }
            set { _WelcomeVM = value; }
        }

        public CompanyModel CompanyM
        {
            get { return _companyM; }
            set { _companyM = value; }
        }

        public CompanyInfoModel CompanyInfoM
        {
            get { return _companyInfoM; }
            set { _companyInfoM = value; }
        }

        public TaxonomyInfoModel TaxonomyInfoM
        {
            get { return _taxonomyInfoM; }
            set { _taxonomyInfoM = value; }
        }

        public CompanyInfoViewModel CompanyInfoVM
        {
            get { return _companyInfoVM; }
            set { _companyInfoVM = value; }
        }

        public TaxonomyInfoViewModel TaxonomyInfoVM
        {
            get { return _taxonomyInfoVM; }
            set { _taxonomyInfoVM = value; }
        }

        public TaxonomyViewModel TaxonomyVM
        {
            get { return _taxonomyVM; }
            set { _taxonomyVM = value; }
        }

        public TechniqueViewModel TechniqueVM
        {
            get { return _techiqueVM; }
            set { _techiqueVM = value; }
        }

        public RangeViewModel RangeVM
        {
            get { return _rangeVM; }
            set { _rangeVM = value; }
        }

        public string FullPath
        {
            get { return _fullPath; }
            set { _fullPath = value; }
        }

        public bool IsSaveAs
        {
            get { return _isSaveAs; }
            set
            {
                _isSaveAs = value;
                NotifyOfPropertyChange(() => IsSaveAs);
            }
        }

        #endregion Properties
    }
}