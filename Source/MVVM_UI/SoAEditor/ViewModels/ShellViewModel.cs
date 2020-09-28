using Caliburn.Micro;
using System.Windows;
using SoAEditor.Models;
using System;
using System.Windows.Forms;
using System.Xml.Linq;
using SOA_DataAccessLibrary;
using MessageBox = System.Windows.MessageBox;
using System.Collections.ObjectModel;
using System.Data;
using System.Collections.Generic;
using Screen = Caliburn.Micro.Screen;
using System.Linq;

namespace SoAEditor.ViewModels
{    
    public class ShellViewModel : Conductor<object>
    {
        //this is for treeview binding=====================================
        private ObservableCollection<TreeView_Taxonomy> _taxonomies;
        private ObservableCollection<TreeView_Range> _ranges;

        public ObservableCollection<TreeView_Taxonomy> Taxonomies
        {
            get
            {
                return _taxonomies;
            }
            set
            {
                Set(ref _taxonomies, value);
                //_taxonomies = value;
                //OnPropertyChanged("Departments");
            }
        }

        public ObservableCollection<TreeView_Range> Ranges
        {
            get
            {
                return _ranges;
            }
            set
            {
                Set(ref _ranges, value);
            }
        }

        
        //==================================================================

        
        private CompanyModel _companyM;
        private CompanyInfoModel _companyInfoM;
        private TaxonomyInfoModel _taxonomyInfoM;

        private WelcomeViewModel _WelcomeVM = null;
        private CompanyInfoViewModel _companyInfoVM = null;
        private CreateTaxonomyViewModel _taxonomyInfoVM = null;
        private TaxonomyViewModel _taxonomyVM = null;
        private TechniqueViewModel _techiqueVM = null;
        private RangeViewModel _rangeVM = null;

        private String _lblCompanyInfoName = null;

        private string _fullPath = "";
        private bool _isSaveAs = false;

        XDocument doc;
        Soa SampleSOA;
        SOA_DataAccess dao;

        //the global var to keep row elements as they're added in the recursive function
        List<string> row;

        //===================================================================
        //methods                                                           |
        //===================================================================

        public ShellViewModel()
        {
            WelcomeVM = new WelcomeViewModel();
            ActivateItem(WelcomeVM);

            mRootNodes = new ObservableCollection<Node>();


            //CompanyInfoM = new CompanyInfoModel();
            //TaxonomyInfoM = new TaxonomyInfoModel();
            //CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            //CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);
            //TaxonomyInfoVM = new TaxonomyInfoViewModel();

            //SampleSOA = new Soa();

            //DataTable temp = new DataTable();
            //temp.Columns.Add();
            //temp.Columns.Add();

            //DataRow row = temp.NewRow();
            //row[0] = "1";
            //row[1] = "2";

            //temp.Rows.Add(row);

        }

        public void showTaxonomyView(System.Windows.Controls.Label lbl)
        {
            loadTaxonomyViewModelObj(lbl.Content.ToString());
        }

        public void showTechniqueView(System.Windows.Controls.Label lbl)
        {

            loadTechniqueViewModelObj(lbl.Content.ToString());
            //ActivateItem(TechniqueVM);
        }

        public void showRangeView(System.Windows.Controls.Label lbl)
        {
            //loadRangeViewModelObj(lbl.Content.ToString());
            //ActivateItem(RangeVM);
        }

        public void LoadCompanyInfo()
        {
            ActivateItem(CompanyInfoVM);
        }

        public void showWelcomeScreen()
        {
            ActivateItem(WelcomeVM);            
        }

        public void showTestScreen()
        {
            ActivateItem(new TestViewModel());
        }

        /*
        public void LoadTaxonomyInfo(string lbl)
        {
            TaxonomyVM = new TaxonomyViewModel();
            
            TaxonomyVM.ResultQuant = "me1";
            ActivateItem(TaxonomyVM);
        }
        */

        //public void runme(object sender)
        //{
        //    //if (!(sender is Label lbl)) return;
        //    MessageBox.Show($"i'm in");

        //}

        //show the corresponding taxonomy pane on the right-hand side based on the selected item from the menu
        private void loadTaxonomyViewModelObj(string lbl)
        {
            TaxonomyVM = new TaxonomyViewModel();
            //set result type
            TaxonomyVM.ResultQuant = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.ResultTypes[0];

            //set input params
            TaxonomyInputParam inputParam;

            for (int i = 0; i < SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Parameters.Count(); i++)
            {
                string param = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Parameters[i].name;
                string quantity = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Parameters[i].Quantity.name;
                string optional = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Parameters[i].optional.ToString();

                inputParam = new TaxonomyInputParam(param, quantity, optional);

                TaxonomyVM.InputParams.Add(inputParam);
            }

            /*
            //set external URL
            if (SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].Uri != null)
            {
                TaxonomyVM.ExternalURL = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].Uri;
            }
            */

            //set documentation
            if (SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Documentation != null)
            {
                TaxonomyVM.EmbeddedDoc = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Documentation.Document.ToString();
            }

            ActivateItem(TaxonomyVM);
        }

        //show the corresponding technique pane on the right-hand side based on the selected item from the menu
        private void loadTechniqueViewModelObj(string lbl)
        {
            TechniqueVM = new TechniqueViewModel();
            TechniqueVM.TaxonomyName = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ProcessTypeName;
            TechniqueVM.TechniqueName = lbl;
            TechniqueVM.Formula = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].Expression;


            for (int i = 0; i < SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ParameterRanges.Count(); i++)
            {
                string paramRangeName = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ParameterRanges[i].name;
                string paramMin = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ParameterRanges[i].Start.Value.ToString();
                string paramMax = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ParameterRanges[i].End.Value.ToString();

                TechniqueVM.InputParameterRanges.Add(new Technique_InputParameterRange(paramRangeName, paramMin, paramMax));
            }


            for (int i = 0; i < SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters.Count(); i++)
            {

                string inputParamName = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters[i].name;
                string inputParamQty;
                if (SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters[i].Quantity != null)
                {
                    inputParamQty = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters[i].Quantity.name;
                }
                else inputParamQty = "";

                TechniqueVM.InputParameters.Add(new Technique_InputParameter(inputParamName, inputParamQty));
                
            }


            string resultMin = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ResultRanges[0].Start.Value.ToString();
            string resultMax = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ResultRanges[0].End.Value.ToString();

            TechniqueVM.Outputs.Add(new Technique_Output("result", resultMin, resultMax));


            for (int i = 0; i < SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].ExpressionSymbols.Count; i++)
            {
                string varName = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].ExpressionSymbols[i].ToString();

                TechniqueVM.Variables.Add(new Technique_Variable(varName));
            }

            ActivateItem(TechniqueVM);
        }

        //show the corresponding range pane on the right-hand side based on the selected item from the menu
        private void loadRangeViewModelObj(string nodeName, String treeNodeName)
        {
            RangeVM = new RangeViewModel();
            //RangeVM.RangeName = "test";
            //RangeVM.CalculatedValue = "";

            //RangeVM.SubRanges.Add("4 wire");
            //RangeVM.SubRanges.Add("2 wire");

            //RangeVM.FreqSubRanges.Add("f1");
            //RangeVM.FreqSubRanges.Add("f2");

            //RangeVM.NominalSubRanges.Add("f1");
            //RangeVM.NominalSubRanges.Add("f1");

            //RangeVM.ConstSubRanges.Add("c1");
            //RangeVM.ConstSubRanges.Add("c2");

            //RangeVM.Ranges.Add(new Range_Range("1", "1", "1"));
            //RangeVM.Ranges.Add(new Range_Range("2", "2", "2"));

            //RangeVM.Constants.Add(new Range_Constant("1", "1"));
            //RangeVM.Constants.Add(new Range_Constant("2", "2"));

            //RangeVM.Formulas.Add(new Range_Formula("1", "1"));
            //RangeVM.Formulas.Add(new Range_Formula("2", "2"));



            //SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions[0].Name


            RangeVM.RangeGrid = new DataTable();

            /*
            RangeVM.RangeGrid.Columns.Add("Connection", typeof(string));
            RangeVM.RangeGrid.Columns.Add("Frequency (start)", typeof(string));
            RangeVM.RangeGrid.Columns.Add("Frequency (stop)", typeof(string));
            RangeVM.RangeGrid.Columns.Add("Nominal (start)", typeof(string));
            RangeVM.RangeGrid.Columns.Add("Nominal (stop)", typeof(string));
            RangeVM.RangeGrid.Columns.Add("Constants", typeof(string));


            // Step 3: here we add rows.
            RangeVM.RangeGrid.Rows.Add("4 Wire", "60", "60", "0", "11", "0.0001 , \n0.0002");
            RangeVM.RangeGrid.Rows.Add("4 Wire", "60", "60", "11", "110", "0.0001 , \n0.0002");

            RangeVM.RangeGrid.Rows.Add("4 Wire", "60", "60", "0", "11", "0.0001 , \n0.0003");
            RangeVM.RangeGrid.Rows.Add("4 Wire", "60", "60", "11", "110", "0.0001 , \n0.0004");

            RangeVM.RangeGrid.Rows.Add("4 Wire", "400", "400", "0", "11", "0.0001 , \n0.00015");
            RangeVM.RangeGrid.Rows.Add("4 Wire", "400", "400", "11", "110", "0.0001 , \n0.00025");

            RangeVM.RangeGrid.Rows.Add("4 Wire", "400", "400", "0", "11", "0.0001 , \n0.00035");
            RangeVM.RangeGrid.Rows.Add("4 Wire", "400", "400", "11", "110", "0.0001 , \n0.00045");
            */


            /*
             * psudocode for adding columns and rows of the dataTable in a recursive way
                ---------------------------
                adding columns:

                look at first case

                for each Assertion add a new column

                if there is Ranges -> call recursive function with Ranges

                recursiveFunctionForColumn
                {
	                get the variable_name and add as a column
	                under the first Range, if there is Ranges -> call recursive function with Ranges
	                else, under the first Range, if there are ConstantValues, add another column (Constants)
                }

                ----------------------------
                adding rows:

                for each case repeat this
                {
	                for each Assertion add a new cell to the row

	                if there is Ranges -> call recursive function with Ranges

	                recursiveFunctionForRow
	                {
		                For each Range repeat this
		                {
			                add a new cell with "start/end" values

			                if there is Ranges -> call recursive function with Ranges
			                else add all values under ConstantValues as a new cell
		                }
	                }
                }
             */

            //to insert first columns into the grid, only add assetion columns after the selected node (assertion node in the subtree)
            //first find the index of the selected assertion (node in the subtree) in the assertion list
            int foundAssertIndex = TreeView_helper.getAssertionNodeIndex(nodeName, SampleSOA);
            

            //adding columns:

            //for each Assertion add a new column, starting from the index of the selected assertion node in the subtree
            int assertCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions.Count();
            for (int i = foundAssertIndex+1; i < assertCount; i++)
            {
                RangeVM.RangeGrid.Columns.Add(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions[i].Name , typeof(string));
            }

            //if there is Ranges -> call addColumnToDataTable function with Ranges
            if (SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Ranges != null)
            {
                addColumnToDataTable(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Ranges);
            }

            //row = new List<string>();
            //row.Add("1");
            //row.Add("2");
            //row.Add("3");
            //row.Add("4");
            //row.Add("5");

            //DataRow myrow = RangeVM.RangeGrid.NewRow();
            //myrow["Resolution"] = "2";


            //RangeVM.RangeGrid.Rows.Add(myrow);


            //adding rows:

            //for each case repeat this
            int caseCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count();
            for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
            {
                //fist create an appendable empty string array for row
                row = new List<string>();

                //here do the filtering based on the item selected from the left-side menu
                //if the value does not match continue with the next item in the case list
                if (SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[foundAssertIndex].Value != nodeName)
                    continue;

                //for each Assertion add a new cell to the row
                assertCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions.Count();
                for (int assertIndex = foundAssertIndex+1; assertIndex < assertCount; assertIndex++)
                {
                    row.Add(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[assertIndex].Value);
                }

                //if there is Ranges -> call addRowsToDataTable function with Ranges
                if (SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Ranges != null)
                {
                    addRowsToDataTable(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Ranges);
                }                
            }


            RangeVM.Formula = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].Expression;
            

            //int switchCase_count = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count();

            //for (int i = 0; i < switchCase_count; i++)
            //{
            //    //since the genric template for all docs is not known know,
            //    //for now the 2nd assertion is considered (connection). if this could be different in other docs, then aother solution could be found
            //    string assertValue = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[i].Assertions[1].Name;
            //    RangeVM.RangeGrid.Columns.Add(assertValue, typeof(string));
            //}

            ActivateItem(RangeVM);
        }
        //recursive function to add columns to the dataTable
        private void addColumnToDataTable(SOA_DataAccessLibrary.Unc_Ranges ranges)
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
        private void addRowsToDataTable(SOA_DataAccessLibrary.Unc_Ranges ranges)
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

                    for(int index = 0; index < RangeVM.RangeGrid.Columns.Count; index++)
                    {
                        myrow[RangeVM.RangeGrid.Columns[index].ColumnName] = row[index];
                    }               

                    RangeVM.RangeGrid.Rows.Add(myrow);

                    row.RemoveAt(row.Count - 1);
                }

                row.RemoveAt(row.Count - 1);
            }

            //row.RemoveAt(row.Count - 1);
        }




        public void OpenXMLFile()
        {     
            
            CompanyInfoM = new CompanyInfoModel();
            TaxonomyInfoM = new TaxonomyInfoModel();
            CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);
            //TaxonomyInfoVM = new CreateTaxonomyViewModel();
            
            //TaxonomyVM.ResultQuant = "test";
            //TechniqueVM = new TechniqueViewModel();
            //RangeVM = new RangeViewModel();


            SampleSOA = new Soa();

            dao = new SOA_DataAccess();

            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "XML Files (*.xml)|*.xml|All Files(*.*)|*.*";
            dlg.Multiselect = false;
            //string path = dlg.FileName;
            try
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                FullPath = dlg.FileName;
                dao.load(dlg.FileName);
                SampleSOA = dao.SOADataMaster;
                Helper.LoadCompanyInfoFromSoaObjectToOpen(SampleSOA, CompanyM); // assigns info extracted from XML to the CompanyM object
                CompanyInfoVM.LoadCompanyInfo(); // copies info into local parameters to be shown in the view
                ActivateItem(CompanyInfoVM);

                //set name label
                lblCompanyInfoName = CompanyInfoVM.Name;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("The data file is invalid!");
                return;
            }


            //fill in treeview            

            /*
            // Test data for example purposes
            Node root = new Node() { Name = "Root" };

            Node a = new Node(root) { Name = "Node A" };

            root.Children.Add(a);

            Node b = new Node(root) { Name = "Node B" };
            root.Children.Add(b);

            Node c = new Node(b) { Name = "Node C" };
            b.Children.Add(c);

            Node d = new Node(b) { Name = "Node D" };
            b.Children.Add(d);

            Node e = new Node(root) { Name = "Node E" };
            root.Children.Add(e);

            Node f = new Node(e) { Name = "Node F" };
            e.Children.Add(f);

            mRootNodes.Add(root);

            Node root2 = new Node() { Name = "Root2" };
            mRootNodes.Add(root2);
            */

            Taxonomies = new ObservableCollection<TreeView_Taxonomy>();
            for (int processTypeIndex = 0; processTypeIndex < SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count(); processTypeIndex++)
            {
                //add nodes in the Taxonomy level to the tree
                string taxonomyName = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[processTypeIndex].name;
                Node taxonom = new Node() { Name = taxonomyName };
                mRootNodes.Add(taxonom);

                for (int techniqueIndex = 0; techniqueIndex < SampleSOA.CapabilityScope.Activities[0].Techniques.Count(); techniqueIndex++)
                {
                    //add nodes in the Technique level to the tree
                    string techniqueName = SampleSOA.CapabilityScope.Activities[0].Techniques[techniqueIndex].name;
                    Node techn = new Node(taxonom) { Name = techniqueName };
                    taxonom.Children.Add(techn);


                    //in two steps add assertions as subtrees
                    //1) first build a matrix with assertions as rows and cases as columns
                    //2) then, go through the matrix column by column and build the tree accordingly

                    //-------------------------------
                    //first step -> build a matrix  |
                    //-------------------------------
                    //find the number of cases
                    DataTable assertionMatrix = new DataTable();
                    //number of cases in the switch
                    int caseCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count();
                    //number of assertions in a case. assuming it's the same for all cases, we look at the first case
                    int assertionCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions.Count();

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
                            string assertStr = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[assertIndex].Value;
                            row[caseIndex] = assertStr;                           
                        }
                        assertionMatrix.Rows.Add(row);
                    }

                    //-----------------------------------
                    //second step -> build the subtree  |
                    //-----------------------------------

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
                                parent = new Node() { Name = name, Children = new ObservableCollection<Node>() };
                                current = techn.Children.Where(x => x.Name == parent.Name).SingleOrDefault();
                                if (current == null)
                                    techn.Children.Add(parent);
                                else
                                    parent = current;
                            }
                            else
                            {
                                current = new Node() { Name = name, Children = new ObservableCollection<Node>() };
                                if (parent.Children.Where(x => x.Name == current.Name).SingleOrDefault() == null)
                                    parent.Children.Add(current);
                                else
                                    current = parent.Children.Where(x => x.Name == current.Name).SingleOrDefault();
                                parent = current;
                            }
                        }
                    }



                    /*
                    //first create a node named All to put all other nodes under it
                    Node root = new Node() { Name = "All" };

                    for (int assertIndex = 0; assertIndex < assertionCount; assertIndex++)
                    {
                        Node currentParent = root;

                        for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
                        {
                            string cellValue = assertionMatrix.Rows[assertIndex][caseIndex].ToString();
                            Node newNode = new Node(currentParent) { Name = cellValue };

                            currentParent.Children.Add(newNode);

                        }
                        
                    }
                    */

                }
            }

                
                                          
                /*
                //fill in treeview (old version)
                Taxonomies = new ObservableCollection<TreeView_Taxonomy>();
                for (int processTypeIndex = 0; processTypeIndex < SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count(); processTypeIndex++)
                {
                    TreeView_Taxonomy tax = new TreeView_Taxonomy(SampleSOA.CapabilityScope.Activities[0].ProcessTypes[processTypeIndex].name);
                    Taxonomies.Add(tax);

                    for (int techniqueIndex = 0; techniqueIndex < SampleSOA.CapabilityScope.Activities[0].Techniques.Count(); techniqueIndex++)
                    {
                        TreeView_Technique tech = new TreeView_Technique(SampleSOA.CapabilityScope.Activities[0].Techniques[techniqueIndex].name);
                        tax.Techniques.Add(tech);

                        //find the number of cases
                        int caseCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count();

                        for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
                        {
                            int assertionCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions.Count();
                            String rangeStr = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[0].Value;

                            //for (int assertionIndex = 1; assertionIndex < assertionCount; assertionIndex++)
                            //{
                            //    String rangeHeader = rangeStr; // + " " + SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[assertionIndex].Value;

                            //    TreeView_Range range = new TreeView_Range(rangeHeader);
                            //    tech.Ranges.Add(range);
                            //}

                            //this is an ugly way not to have duplicates in the list.
                            bool found = false;
                            foreach (TreeView_Range item in tech.Ranges)
                            {
                                if (item.RangeName == rangeStr) found = true;
                            }
                            if (!found)
                            {
                                TreeView_Range range = new TreeView_Range(rangeStr);
                                tech.Ranges.Add(range);                            
                            }               

                        }                       
                    }
                }
                */

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

        public void treeviewNodeClick(Node node)
        {
            //System.Windows.Forms.MessageBox.Show("node clicked");
                        
            String returnedType = TreeView_helper.getNodeType(node.Name, SampleSOA);
            switch (returnedType)
            {
                case "taxonomy":
                    loadTaxonomyViewModelObj(node.Name);
                    break;

                case "technique":
                    loadTechniqueViewModelObj(node.Name);
                    break;

                case "range":
                    loadRangeViewModelObj(node.Name, node.Name);
                    break;

                default:
                    break;
            }

        }

        public void SaveXML()
        {

            doc = new XDocument();

            Helper.LoadCompanyInfoToSoaObjectToSave(SampleSOA, CompanyInfoVM);


            SampleSOA.writeTo(doc, SampleSOA);

            if (FullPath.Length == 0 || IsSaveAs == true) // Saving a new file or Save as...
            {
                System.Windows.Forms.SaveFileDialog saveFileDlg = new System.Windows.Forms.SaveFileDialog();

                saveFileDlg.Filter = "XML Files (*.xml)|*.xml|All Files(*.*)|*.*";
                saveFileDlg.FilterIndex = 2;
                saveFileDlg.RestoreDirectory = true;

                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    // Code to write the stream goes here.
                    FullPath = saveFileDlg.FileName;
                    doc.Save(FullPath);
                    System.Windows.Forms.MessageBox.Show("File saved!");
                    return;
                }
            }
            else if (FullPath.Length != 0)
            {
                doc.Save(FullPath);
                System.Windows.Forms.MessageBox.Show("File saved!");
            }
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

        //===================================================================
        //parameters                                                        |
        //===================================================================


        private ObservableCollection<Node> mRootNodes;

        public IEnumerable<Node> RootNodes {
            get { return mRootNodes; }
            //set { Set(ref mRootNodes, (ObservableCollection < Node >)value); NotifyOfPropertyChange(() => mRootNodes); }
        }

        
        public String lblCompanyInfoName
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

        public CreateTaxonomyViewModel TaxonomyInfoVM
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

        public  RangeViewModel RangeVM
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

    }
}

