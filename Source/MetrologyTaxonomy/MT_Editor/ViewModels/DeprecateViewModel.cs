using Caliburn.Micro;
using MT_DataAccessLib;
using System.Collections.ObjectModel;

namespace MT_Editor.ViewModels
{
    internal class DeprecateViewModel : Screen
    {
        private TaxonomyFactory factory;

        public DeprecateViewModel()
        {
            taxon = Helper.SelectedTaxon;
            factory = new();
            foreach (Taxon taxon in factory.GetAllNonDeprecated())
            {
                if (Taxon.Name != taxon.Name)
                    taxons.Add(taxon);
            }
        }

        #region Properties

        private Taxon taxon;

        public Taxon Taxon
        {
            get { return taxon; }
            set
            {
                taxon = value;
                NotifyOfPropertyChange(() => Taxon);
            }
        }

        private Taxon selectedTaxon;

        public Taxon SelectedTaxon
        {
            get { return selectedTaxon; }
            set
            {
                selectedTaxon = value;
                NotifyOfPropertyChange(() => SelectedTaxon);
            }
        }

        private ObservableCollection<Taxon> taxons = new ObservableCollection<Taxon>();

        public ObservableCollection<Taxon> Taxons
        {
            get { return taxons; }
            set { taxons = value; NotifyOfPropertyChange(() => Taxons); }
        }

        #endregion Properties

        #region Commands

        public void Deprecate()
        {
            if (SelectedTaxon == null) return;
            Taxon.Replacement = SelectedTaxon.Name;
            Taxon.Deprecated = true;
            factory.Save(factory.Edit(taxon, taxon.Name), Helper.SaveLocal);
            Helper.Navigate(Helper.MenuItem.ALL);
        }

        #endregion Commands
    }
}