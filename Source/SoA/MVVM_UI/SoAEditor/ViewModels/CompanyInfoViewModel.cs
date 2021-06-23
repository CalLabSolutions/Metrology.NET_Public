using Caliburn.Micro;
using SoAEditor.Models;

namespace SoAEditor.ViewModels
{
    public class CompanyInfoViewModel : Screen
    {
        private string _name;
        private string _accrBody;
        private string _accrLogo;
        private string _scopeID;
        private string _criteria;
        private string _effectiveDate;
        private string _expirDate;
        private string _statement;

        private string _locID;
        private string _state;
        private string _street;
        private string _city;
        private string _zip;
        private string _contactName;
        private string _phoneNo;
        private string _emails;
        private string _urls;

        public CompanyInfoModel companyInfoM;

        //XDocument doc;
        //Soa SampleSoA;

        public CompanyInfoViewModel(CompanyInfoModel companyInfoModel)
        {
            companyInfoM = companyInfoModel;

        }

        public void SaveCompanyInfo()
        {

            companyInfoM.Name= Name;
            companyInfoM.AccrBody= AccrBody;
            companyInfoM.AccrLogo = AccrLogo;
            companyInfoM.ScopeID = ScopeID;
            companyInfoM.Criteria = Criteria;
            companyInfoM.EffectiveDate = EffectiveDate;
            companyInfoM.ExpirDate = ExpirDate;
            companyInfoM.Statement = Statement;

            companyInfoM.LocID = LocID;
            companyInfoM.State = State;
            companyInfoM.Street = Street;
            companyInfoM.City = City;
            companyInfoM.Zip = Zip;
            companyInfoM.ContactName = ContactName;
            companyInfoM.PhoneNo = PhoneNo;
            companyInfoM.Emails = Emails;
            companyInfoM.Urls = Urls;
                       


        //System.Windows.Forms.MessageBox.Show("Company Info Saved!");

        }

        public void LoadCompanyInfo()
        {
            Name = companyInfoM.Name;
            AccrBody = companyInfoM.AccrBody;
            AccrLogo = companyInfoM.AccrLogo;
            ScopeID = companyInfoM.ScopeID;
            Criteria = companyInfoM.Criteria;
            EffectiveDate = companyInfoM.EffectiveDate;
            ExpirDate = companyInfoM.ExpirDate;
            Statement = companyInfoM.Statement;

            LocID = companyInfoM.LocID;
            State = companyInfoM.State;
            Street = companyInfoM.Street;
            City = companyInfoM.City;
            Zip = companyInfoM.Zip;
            ContactName = companyInfoM.ContactName;
            PhoneNo = companyInfoM.PhoneNo;
            Emails = companyInfoM.Emails;
            Urls = companyInfoM.Urls;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public string AccrBody
        {
            get
            {
                return _accrBody;
            }
            set
            {
                _accrBody = value;
                NotifyOfPropertyChange(() => AccrBody);
            }
        }
        public string AccrLogo
        {
            get
            {
                return _accrLogo;
            }
            set
            {
                _accrLogo = value;
                NotifyOfPropertyChange(() => AccrLogo);
            }
        }
        public string ScopeID
        {
            get
            {
                return _scopeID;
            }
            set
            {
                _scopeID = value;
                NotifyOfPropertyChange(() => ScopeID);
            }
        }
        public string Criteria
        {
            get
            {
                return _criteria;
            }
            set
            {
                _criteria = value;
                NotifyOfPropertyChange(() => Criteria);
            }
        }
        public string EffectiveDate
        {
            get
            {
                return _effectiveDate;
            }
            set
            {
                _effectiveDate = value;
                NotifyOfPropertyChange(() => EffectiveDate);
            }
        }
        public string ExpirDate
        {
            get
            {
                return _expirDate;
            }
            set
            {
                _expirDate = value;
                NotifyOfPropertyChange(() => ExpirDate);
            }
        }
        public string Statement
        {
            get
            {
                return _statement;
            }
            set
            {
                _statement = value;
                NotifyOfPropertyChange(() => Statement);
            }
        }
        public string LocID
        {
            get
            {
                return _locID;
            }
            set
            {
                _locID = value;
                NotifyOfPropertyChange(() => LocID);
            }
        }
        public string State
        {
            get { return _state; }
            set
            {
                _state = value;
                NotifyOfPropertyChange(() => State);
            }
        }
        public string Street
        {
            get { return _street; }
            set
            {
                _street = value;
                NotifyOfPropertyChange(() => Street);
            }
        }
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
                NotifyOfPropertyChange(() => City);    
            }
        }
        public string Zip
        {
            get { return _zip; }
            set
            {
                _zip = value;
                NotifyOfPropertyChange(() => Zip);
            }
        }

        public string ContactName
        {
            get { return _contactName; }
            set
            {
                _contactName = value;
                NotifyOfPropertyChange(() => ContactName);
            }
        }
        public string PhoneNo
        {
            get { return _phoneNo; }
            set
            {
                _phoneNo = value;
                NotifyOfPropertyChange(() => PhoneNo);
            }
        }
        public string Emails
        {
            get { return _emails; }
            set
            {
                _emails = value;
                NotifyOfPropertyChange(() => Emails);
            }
        }
        public string Urls
        {
            get { return _urls; }
            set
            {
                _urls = value;
                NotifyOfPropertyChange(() => Urls);
            }
        }

    }
}
