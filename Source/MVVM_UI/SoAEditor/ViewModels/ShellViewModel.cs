using Caliburn.Micro;
using System.Windows;
using SoAEditor.Models;
using System;
using System.Windows.Forms;
using System.Xml.Linq;
using SOA_DataAccessLibrary;
using MessageBox = System.Windows.MessageBox;
using System.Collections.ObjectModel;

namespace SoAEditor.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        //this is for treeview binding=====================================
        private ObservableCollection<Taxonomy> _taxonomies;
        private ObservableCollection<Range> _ranges;

        public ObservableCollection<Taxonomy> Taxonomies
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

        public ObservableCollection<Range> Ranges
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

        public ShellViewModel()
        {
            ActivateItem(new WelcomeViewModel());
            //CompanyInfoM = new CompanyInfoModel();
            //TaxonomyInfoM = new TaxonomyInfoModel();
            //CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            //CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);
            //TaxonomyInfoVM = new TaxonomyInfoViewModel();

            //SampleSOA = new Soa();
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
            loadRangeViewModelObj(lbl.Content.ToString());
            //ActivateItem(RangeVM);
        }

        public void LoadCompanyInfo()
        {
            ActivateItem(CompanyInfoVM);
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
            ActivateItem(TechniqueVM);
        }

        //show the corresponding range pane on the right-hand side based on the selected item from the menu
        private void loadRangeViewModelObj(string lbl)
        {
            RangeVM = new RangeViewModel();
            ActivateItem(RangeVM);
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
            Taxonomies = new ObservableCollection<Taxonomy>();
            for (int processTypeIndex = 0; processTypeIndex < SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count(); processTypeIndex++)
            {
                Taxonomy tax = new Taxonomy(SampleSOA.CapabilityScope.Activities[0].ProcessTypes[processTypeIndex].name);
                Taxonomies.Add(tax);

                for (int techniqueIndex = 0; techniqueIndex < SampleSOA.CapabilityScope.Activities[0].Techniques.Count(); techniqueIndex++)
                {
                    Technique tech = new Technique(SampleSOA.CapabilityScope.Activities[0].Techniques[techniqueIndex].name);
                    tax.Techniques.Add(tech);

                    //fine the number of cases
                    int caseCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count();
                                      
                    for (int caseIndex = 0; caseIndex < caseCount; caseIndex++)
                    {
                        int assertionCount = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions.Count();
                        String rangeStr = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[0].Value;

                        for (int assertionIndex = 1; assertionIndex < assertionCount; assertionIndex++)
                        {
                            String rangeHeader = rangeStr + " " + SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[caseIndex].Assertions[assertionIndex].Value;

                            Range range = new Range(rangeHeader);
                            tech.Ranges.Add(range);
                        }
                    }                       
                }
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

        public String lblCompanyInfoName
        {
            get { return _lblCompanyInfoName; }
            set { _lblCompanyInfoName = value; NotifyOfPropertyChange(() => lblCompanyInfoName); }
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
