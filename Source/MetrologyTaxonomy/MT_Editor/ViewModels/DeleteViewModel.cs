using Caliburn.Micro;
using MT_DataAccessLib;
using static MT_Editor.Helper;

namespace MT_Editor.ViewModels
{
    internal class DeleteViewModel : Screen
    {
        private TaxonomyFactory factory;

        public DeleteViewModel()
        {
            Taxon = Helper.SelectedTaxon;
            factory = new();
        }

        #region Properties

        private Taxon taxon;

        public Taxon Taxon
        {
            get { return taxon; }
            set { taxon = value; NotifyOfPropertyChange(() => Taxon); }
        }

        #endregion Properties

        #region Commands

        public void No()
        {
            Navigate(MenuItem.ALL);
        }

        public void Yes()
        {
            var taxonomy = factory.Delete(taxon);
            factory.Save(taxonomy, Helper.SaveLocal);
            Navigate(MenuItem.ALL);
        }

        #endregion Commands
    }
}