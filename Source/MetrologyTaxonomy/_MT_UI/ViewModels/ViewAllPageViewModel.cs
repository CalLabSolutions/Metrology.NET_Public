using GalaSoft.MvvmLight.Command;
using MT_DataAccessLib;
using MT_UI.Services;
using MT_UI.Pages;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls;

namespace MT_UI.ViewModels
{
    class ViewAllPageViewModel : Notifier
    {
        #region properties

        public string Title
        {
            get { return "Taxonomy"; }
        }

        private IEnumerable<Taxon> taxonomy;
        public IEnumerable<Taxon> Taxonomy
        {
            get { return taxonomy; }
            set
            { 
                taxonomy = value;
                OnPropertyChanged("Taxonomy");
            }
        }

        private Taxon selectedTaxon = null;

        public Taxon SelectedTaxon
        {
            get { return selectedTaxon;  }
            set
            {
                selectedTaxon = value;
                MT_Data.SelectedTaxon = selectedTaxon;
                OnPropertyChanged("SelectedTaxon");
                if (value != null)
                    MT_Data.ContentFrame.Navigate(typeof(DetailsPage), null, new EntranceNavigationTransitionInfo());
            }
        }

        private string queryText;

        public string QueryText
        {
            get { return queryText; }
            set
            {
                queryText = value;
                OnPropertyChanged("QueryText");
                OnSerachInputChange();
            }
        }

        // TODO: Add Filters as ENUMs with Converter for the UI
        private readonly string[] filters = { "All", "Measure", "Source", "Parameters", "Results", "Deprecated"};

        public string[] Filters
        {
            get { return filters; }
        }

        private string selectedFilter = "All";

        public string SelectedFilter
        {
            get { return selectedFilter;  }
            set
            {
                selectedFilter = value;
                OnPropertyChanged("SelectedFilter");
                OnSerachInputChange();
            }
        }

        #endregion

        TaxonomyFactory factory = new TaxonomyFactory();

        public ViewAllPageViewModel()
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

        private async void Message(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Content = message,
                Title = title,
                CloseButtonText = "Ok"
            };
            _ = await dialog.ShowAsync();
        }

    }
}