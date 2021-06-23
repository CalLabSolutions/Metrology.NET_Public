using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class CompanyModel
    {
        private CompanyInfoModel _companyInfo;
        private TaxonomyInfoModel _taxonomyInfo;

        public CompanyModel(CompanyInfoModel companyInfo, TaxonomyInfoModel taxonomyInfo)
        {
            CompanyInfo = companyInfo;
            TaxonomyInfo = taxonomyInfo;
        }

        public CompanyInfoModel CompanyInfo
        {
            get { return _companyInfo; }
            set { _companyInfo = value; }
        }

        public TaxonomyInfoModel TaxonomyInfo
        {
            get { return _taxonomyInfo; }
            set { _taxonomyInfo = value; }
        }
    }
}
