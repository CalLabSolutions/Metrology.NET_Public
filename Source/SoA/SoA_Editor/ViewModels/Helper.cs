using MT_DataAccessLib;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using System;
using System.Windows;

namespace SoA_Editor.ViewModels
{
    public static class Helper
    {
        public static void LoadCompanyInfoToSoaObjectToSave(Soa SampleSoA, CompanyInfoViewModel companyInfoVM, bool newFile = false)
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
            SampleSoA.CapabilityScope.MeasuringEntity = companyInfoVM.CompanyName;
            SampleSoA.CapabilityScope.Locations[0].id = companyInfoVM.LocID;
            SampleSoA.CapabilityScope.Locations[0].ContactName = companyInfoVM.ContactName;

            //need to first remove existing phone numners, then add new one
            SampleSoA.CapabilityScope.Locations[0].ContactInfo.PhoneNumbers.removePhoneNumber("");
            SampleSoA.CapabilityScope.Locations[0].ContactInfo.EmailAccounts.removeEmail("");
            SampleSoA.CapabilityScope.Locations[0].ContactInfo.Urls.removeUrl("");
            SampleSoA.CapabilityScope.Locations[0].ContactInfo.PhoneNumbers.addPhoneNumber(companyInfoVM.PhoneNo);
            string[] temp = companyInfoVM.PhoneNo != null ? companyInfoVM.PhoneNo.Split(',') : new string[] {""};
            foreach (string number in temp)
            {
                SampleSoA.CapabilityScope.Locations[0].ContactInfo.PhoneNumbers.addPhoneNumber(number);
            }

            temp = companyInfoVM.Emails.Split(',');
            foreach (string email in temp)
            {
                SampleSoA.CapabilityScope.Locations[0].ContactInfo.EmailAccounts.addEmail(email);
            }
            temp = companyInfoVM.Urls.Split(',');
            foreach (string url in temp)
            {
                SampleSoA.CapabilityScope.Locations[0].ContactInfo.Urls.addUrl(url);
            }
        }

        public static void LoadCompanyInfoFromSoaObjectToOpen(Soa SampleSoA, CompanyModel CompanyM)
        {
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
            if (CompanyM.CompanyInfo.Emails == null) CompanyM.CompanyInfo.Emails = "";
            if (CompanyM.CompanyInfo.Urls == null) CompanyM.CompanyInfo.Urls = "";
            if (CompanyM.CompanyInfo.PhoneNo == null) CompanyM.CompanyInfo.PhoneNo = "";

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

        private static Taxon _treeViewSelectedTaxon = null;

        public static Taxon TreeViewSelectedTaxon
        {
            get { return _treeViewSelectedTaxon; }
            set { _treeViewSelectedTaxon = value; }
        }

        private static Unc_Technique treeViewTechnique;
        public static Unc_Technique TreeViewTechnique
        {
            get { return treeViewTechnique; }
            set { treeViewTechnique = value; }
        }

        private static Unc_Case treeViewCase;
        public static Unc_Case TreeViewCase
        {
            get { return treeViewCase; }
            set { treeViewCase = value; }
        }

        // Class to help with Message boxes
        internal class MessageDialog
        {
            public string Title = "";
            public string Message = "";
            public MessageBoxButton Button;
            public MessageBoxImage Image;

            public void Show()
            {
                MessageBox.Show(Message, Title, Button, Image);
            }
        }
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

        public static Quantity FormatUomQuantity(SOA_DataAccessLib.UomDataSource.Quantity quantity)
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
                FormatedName = qname,
                QuantitiyName = quantity.name
            };
        }
    }

    public static class ToEngineeringFormat
    {
        // Used in adding the units to the return string
        private static string[] prefix_const = { " y", " z", " a", " f", " p", " n", " u", " m", " ", " k", " M", " G", " T" };

        // number: The number to convert.
        // 
        // significant_digits: The number of significant digits to return. digits should be a minimum of 3 for 
        // Engineering notation this routine will work with any number from 1 to 15
        // But when significant_digits is less than 3 the output may be in scientific notation
        //
        // units: what the number is a measure of, like "Hz", "Farads", "Tesla", etc.
        public static string Convert(double value)
        {
            int exp = (int)(Math.Floor(Math.Log10(value) / 3.0) * 3.0);
            double newValue = value * Math.Pow(10.0, -exp);
            if (newValue >= 1000.0)
            {
                newValue = newValue / 1000.0;
                exp = exp + 3;
            }
            return string.Format("{0:##0}e{1}", newValue, exp);
        }
    }
}