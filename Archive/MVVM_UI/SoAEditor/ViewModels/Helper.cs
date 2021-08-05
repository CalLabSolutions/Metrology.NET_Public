using SOA_DataAccessLibrary;
using SoAEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.ViewModels
{
    public static class Helper
    {

        public static void LoadCompanyInfoToSoaObjectToSave(Soa SampleSoA, CompanyInfoViewModel companyInfoVM)
        {
            SampleSoA.CapabilityScope.Locations[0].Address.Street = companyInfoVM.Street;
            SampleSoA.CapabilityScope.Locations[0].Address.City = companyInfoVM.City;
            SampleSoA.CapabilityScope.Locations[0].Address.State = companyInfoVM.State;
            SampleSoA.CapabilityScope.Locations[0].Address.Zip = companyInfoVM.Zip;

            SampleSoA.Ab_ID = companyInfoVM.AccrBody;
            SampleSoA.Ab_Logo_Signature = companyInfoVM.AccrLogo;
            SampleSoA.Scope_ID_Number = companyInfoVM.ScopeID;
            SampleSoA.Criteria = companyInfoVM.Criteria;
            SampleSoA.EffectiveDate = companyInfoVM.EffectiveDate;
            SampleSoA.ExpirationDate = companyInfoVM.ExpirDate;
            SampleSoA.Statement = companyInfoVM.Statement;
            SampleSoA.CapabilityScope.MeasuringEntity = companyInfoVM.Name;
            SampleSoA.CapabilityScope.Locations[0].id = companyInfoVM.LocID;
            SampleSoA.CapabilityScope.Locations[0].ContactName = companyInfoVM.ContactName;

            //need to first remove existing phone numners, then add new one
            SampleSoA.CapabilityScope.Locations[0].ContactInfo.PhoneNumbers.addPhoneNumber(companyInfoVM.PhoneNo);
        }

        public static void LoadCompanyInfoFromSoaObjectToOpen(Soa SampleSoA, CompanyModel CompanyM)
        {
            //SampleSoA = aSampleSoA;
            
            CompanyM.CompanyInfo.AccrBody = SampleSoA.Ab_ID;
            CompanyM.CompanyInfo.AccrLogo = SampleSoA.Ab_Logo_Signature;
            CompanyM.CompanyInfo.ScopeID = SampleSoA.Scope_ID_Number;
            CompanyM.CompanyInfo.Criteria = SampleSoA.Criteria;
            CompanyM.CompanyInfo.EffectiveDate = SampleSoA.EffectiveDate;
            CompanyM.CompanyInfo.ExpirDate = SampleSoA.ExpirationDate;
            CompanyM.CompanyInfo.Statement = SampleSoA.Statement;
            CompanyM.CompanyInfo.Name = SampleSoA.CapabilityScope.MeasuringEntity.ToString();
            CompanyM.CompanyInfo.LocID = SampleSoA.CapabilityScope.Locations[0].id;            
            CompanyM.CompanyInfo.ContactName = SampleSoA.CapabilityScope.Locations[0].ContactName;
            CompanyM.CompanyInfo.Emails = string.Join(",", SampleSoA.CapabilityScope.Locations[0].ContactInfo.EmailAccounts);
            CompanyM.CompanyInfo.Urls = string.Join(",", SampleSoA.CapabilityScope.Locations[0].ContactInfo.Urls);
            CompanyM.CompanyInfo.PhoneNo = string.Join(",", SampleSoA.CapabilityScope.Locations[0].ContactInfo.PhoneNumbers);

            CompanyM.CompanyInfo.Street = SampleSoA.CapabilityScope.Locations[0].Address.Street;
            CompanyM.CompanyInfo.City = SampleSoA.CapabilityScope.Locations[0].Address.City;
            CompanyM.CompanyInfo.State = SampleSoA.CapabilityScope.Locations[0].Address.State;
            CompanyM.CompanyInfo.Zip = SampleSoA.CapabilityScope.Locations[0].Address.Zip;
        }

        //global variable for name of the created node on the tree
        private static string _treeViewNewNodeName;
        public static string treeViewNewNodeName
        {
            get { return _treeViewNewNodeName; }
            set { _treeViewNewNodeName = value; }
        }

    }
}
