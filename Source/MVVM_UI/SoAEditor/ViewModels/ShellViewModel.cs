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

        //==================================================================


        private CompanyModel _companyM;
        private CompanyInfoModel _companyInfoM;
        private TaxonomyInfoModel _taxonomyInfoM;
        

        private CompanyInfoViewModel _companyInfoVM = null;
        private TaxonomyInfoViewModel _taxonomyInfoVM = null;
        private TechniqueViewModel _techiqueVM = null;

        private string _fullPath = "";
        private bool _isSaveAs = false;

        XDocument doc;
        Soa SampleSOA;
        SOA_DataAccess dao;

        public ShellViewModel()
        {

            //CompanyInfoM = new CompanyInfoModel();
            //TaxonomyInfoM = new TaxonomyInfoModel();
            //CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            //CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);
            //TaxonomyInfoVM = new TaxonomyInfoViewModel();

            //SampleSOA = new Soa();
        }

        public void showTaxonomyView(Taxonomy sender)
        {
            LoadTaxonomyInfo();
        }

        public void showTechniqueView(Taxonomy sender) {
            ActivateItem(TechniqueVM);
        }

        public void LoadCompanyInfo()
        {
            ActivateItem(CompanyInfoVM);
        }

        public void LoadTaxonomyInfo()
        {
            ActivateItem(TaxonomyInfoVM);
        }

        //public void runme(object sender)
        //{
        //    //if (!(sender is Label lbl)) return;
        //    MessageBox.Show($"i'm in");

        //}

        public void OpenXMLFile()
        {
            CompanyInfoM = new CompanyInfoModel();
            TaxonomyInfoM = new TaxonomyInfoModel();
            CompanyM = new CompanyModel(CompanyInfoM, TaxonomyInfoM);
            CompanyInfoVM = new CompanyInfoViewModel(CompanyInfoM);
            TaxonomyInfoVM = new TaxonomyInfoViewModel();
            TechniqueVM = new TechniqueViewModel();


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
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("The data file is invalid!");
                return;
            }


            //fill in treeview
            Taxonomies = new ObservableCollection<Taxonomy>();
            for (int i = 0; i < SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count(); i++)
            {
                Taxonomy tax = new Taxonomy(SampleSOA.CapabilityScope.Activities[0].ProcessTypes[i].name);
                Taxonomies.Add(tax);

                for (int j = 0; j < SampleSOA.CapabilityScope.Activities[0].Techniques.Count(); j++)
                {
                    Technique tech = new Technique(SampleSOA.CapabilityScope.Activities[0].Techniques[j].name);
                    tax.Techniques.Add(tech);

                    //for (int k = 0; k < SampleSOA.CapabilityScope.Activities[0].Techniques[j].; k++)
                    //{

                    //    SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Ranges[0]
                    //}
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
           //System.Windows.Forms.Application.Current.Shutdown();
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

        public TechniqueViewModel TechniqueVM
        {
            get { return _techiqueVM; }
            set { _techiqueVM = value; }
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
