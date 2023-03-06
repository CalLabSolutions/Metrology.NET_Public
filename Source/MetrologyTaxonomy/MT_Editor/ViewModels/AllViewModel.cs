using Caliburn.Micro;
using MT_DataAccessLib;
using System.Collections.Generic;
using System.Windows;

namespace MT_Editor.ViewModels
{
    public class AllViewModel : Screen
    {
        
        public string Title
        {
            get { return "Taxonomy"; }
        }

        private IEnumerable<Taxon> taxonomy;        

        public IEnumerable<Taxon> Taxonomy
        {
            get {
                foreach (Taxon taxon in taxonomy)
                {
                    string name = taxon.Name;
                    if (name.Contains("TestProcess."))
                    {
                        taxon.Name = name.Replace("TestProcess.", "");
                    }
                }
                return taxonomy; }
            set
            {
                taxonomy = value;
                NotifyOfPropertyChange(() => Taxonomy);
            }
        }

        private Taxon selectedTaxon = null;

        public Taxon SelectedTaxon
        {
            get { return selectedTaxon; }
            set
            {
                selectedTaxon = value;
                NotifyOfPropertyChange(() => SelectedTaxon);
                if (value != null)
                {
                    Helper.SelectedTaxon = selectedTaxon;
                    Helper.UnselectMenuItems();
                    Helper.Shell.ActivateItemAsync(new DetailsViewModel(SelectedTaxon));
                }
            }   
        }

        private string queryText;

        public string QueryText
        {
            get { return queryText; }
            set
            {
                queryText = value;
                NotifyOfPropertyChange(() => QueryText);
                OnSerachInputChange();
            }
        }

        // TODO: Add Filters as ENUMs with Converter for the UI
        private readonly string[] filters = { "All", "Measure", "Source", "Parameters", "Results", "Deprecated" };

        public string[] Filters
        {
            get { return filters; }
        }

        private string selectedFilter = "All";

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                NotifyOfPropertyChange(() => SelectedFilter);
                OnSerachInputChange();
            }
        }

        TaxonomyFactory factory = new();

        public AllViewModel()
        {
            if (factory.Count() == 0)
            {
                Taxonomy = new List<Taxon>();
                // TODO: Make Contetnt Dialog Class
                Message("Notice", "There are either no Taxons on file, or there was an error loading the data.  Please try adding a Taxon.");
            }
            else
            {
                Taxonomy = factory.GetAllTaxons();
            }

        }

        private void OnSerachInputChange()
        {
            SelectedTaxon = null;
            Taxonomy = factory.GetByName(QueryText, SelectedFilter);
        }

        private void Message(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
