using MT_DataAccessLib;
using MT_UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT_UI.ViewModels
{
    class MainPageViewModel : Notifier
    {
        public MainPageViewModel()
        {
            // Get our Taxonomy XML file read in
            var facotry = new TaxonomyFactory();
            if (facotry.Count() == 0 && !facotry.LoadedFromServer)
            {
                facotry.Reload(true); // reload from server
            }

            // load our Uom Data source
            XMLDataSource datasource = new XMLDataSource();
            datasource.load(UomDataSource.DatabasePath);
            
            // Set our locked status
            Locked = MT_Data.Locked;
        }

        private bool locked;
        public bool Locked
        {
            get { return locked; }
            set
            {
                locked = value;
                OnPropertyChanged("Locked");
            }
        }
    }
}
