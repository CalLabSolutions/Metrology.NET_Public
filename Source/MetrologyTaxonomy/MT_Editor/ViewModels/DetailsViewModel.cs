using Caliburn.Micro;
using Microsoft.Win32;
using MT_DataAccessLib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MT_Editor.ViewModels
{
    internal class DetailsViewModel : Screen
    {
        public DetailsViewModel(Taxon selectedTaxon)
        {
            Taxon = selectedTaxon;
            factory = new TaxonomyFactory();
            Deprecated = Taxon.Deprecated;
        }

        #region Properties

        private TaxonomyFactory factory;

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

        private bool deprecated;

        public bool Deprecated
        {
            get { return deprecated; }
            set
            {
                deprecated = value;
                NotifyOfPropertyChange(() => Deprecated);
            }
        }

        #endregion Properties

        #region Commands

        public void Link()
        {
            try
            {
                Helper.OpenBrowser(Taxon.ExternalReference.Url);
            }
            catch (Exception)
            {
                MessageBox.Show("Error", "Error occured while opening the browser.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefLink(string url)
        {
            try
            {
                Helper.OpenBrowser(url);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error", "Browser Error " + e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Restore()
        {
            if (!Taxon.Deprecated)
            {
                return;
            }
            Taxon.Replacement = "";
            Taxon.Deprecated = false;
            var taxonomy = factory.Edit(Taxon, Taxon.Name);
            factory.Save(taxonomy, Helper.SaveLocal);
            Deprecated = false;
        }

        public void ExportHTML()
        {
            var html = factory.ExportHTML(Taxon);
            var saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = "html",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FileName = "HTML - " + Taxon.Name
            };
            saveFileDialog.Filter = "HTML Files (*.html)|*.html";
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult.HasValue || dialogResult.Value)
            {
                File.WriteAllText(saveFileDialog.FileName, html);
            }
        }

        public void GoBack()
        {
            Helper.Shell.ActivateItemAsync(new AllViewModel());
        }

        #endregion Commands
    }
}