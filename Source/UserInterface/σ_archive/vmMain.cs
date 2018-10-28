using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using soa_1_03.models;

#if false
namespace soa_1_03.viewModels
{
    public class vmMain : Inpc
    {
#region Private Fields
        private mTaxonomy _activeTaxonomy;
#endregion

#region Constructor

#endregion

#region Singleton
        private static readonly vmMain instance = null;
        static vmMain() { instance = new vmMain(); }
        public static vmMain GetInstance() { return instance; }
#endregion

#region Public Properties

        public mTaxonomy ActiveTaxonomy
        {
            get { return _activeTaxonomy; }
            set
            {
                if (value != _activeTaxonomy)
                {
                    _activeTaxonomy = value;
                    OnPropertyChanged("ActiveTaxonomy");
                }
            }
        }

#endregion

#region Methods

#endregion
}
}
#endif