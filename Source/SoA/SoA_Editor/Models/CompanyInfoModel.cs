using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Models
{
    public class CompanyInfoModel
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

        public string Name { get => _name; set => _name = value; }
        public string AccrBody { get => _accrBody; set => _accrBody = value; }
        public string AccrLogo { get => _accrLogo; set => _accrLogo = value; }
        public string ScopeID { get => _scopeID; set => _scopeID = value; }
        public string Criteria { get => _criteria; set => _criteria = value; }
        public string EffectiveDate { get => _effectiveDate; set => _effectiveDate = value; }
        public string ExpirDate { get => _expirDate; set => _expirDate = value; }
        public string Statement { get => _statement; set => _statement = value; }

        public string LocID { get => _locID; set => _locID = value; }
        public string State { get => _state; set => _state = value; }
        public string Street { get => _street; set => _street = value; }
        public string City { get => _city; set => _city = value; }
        public string Zip { get => _zip; set => _zip = value; }
        public string ContactName { get => _contactName; set => _contactName = value; }
        public string PhoneNo { get => _phoneNo; set => _phoneNo = value; }
        public string Emails { get => _emails; set => _emails = value; }
        public string Urls { get => _urls; set => _urls = value; }
    }
}
