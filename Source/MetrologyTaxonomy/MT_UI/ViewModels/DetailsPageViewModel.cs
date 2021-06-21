using GalaSoft.MvvmLight.Command;
using MT_DataAccessLib;
using MT_UI.Services;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MT_UI.ViewModels
{
    class DetailsPageViewModel : Notifier
    {
        private TaxonomyFactory factory;
        
        public DetailsPageViewModel()
        {
            Taxon = MT_Data.SelectedTaxon;
            factory = new TaxonomyFactory();
            Deprecated = Taxon.Deprecated;
        }

        #region commands

        private ICommand restore;
        public ICommand Restore
        {
            get
            {
                if (restore == null)
                {
                    restore = new RelayCommand(() =>
                    {
                        taxon.Replacement = "";
                        taxon.Deprecated = false;
                        var taxonomy = factory.Edit(taxon,taxon.Name);
                        factory.Save(taxonomy, MT_Data.SaveLocal);
                        Deprecated = false;
                    });
                }
                return restore;
            }
            set
            {
                restore = value;
                OnPropertyChanged("Restore");
            }
        }

        private ICommand exportHTML;
        public ICommand ExportHTML
        {
            get
            {
                if (exportHTML == null)
                {
                    exportHTML = new RelayCommand(async () =>
                    {
                        var html = factory.ExportHTML(taxon);
                        var filePicker = new FileSavePicker
                        {
                            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                            SuggestedFileName = "HTML - " + taxon.Name
                        };
                        filePicker.FileTypeChoices.Add("HTML", new List<string>() { ".html" });
                        StorageFile file = await filePicker.PickSaveFileAsync();
                        if (file != null)
                        {
                            CachedFileManager.DeferUpdates(file);
                            await FileIO.WriteTextAsync(file, html);
                        }
                    });
                }
                return exportHTML;
            }
            set
            {
                exportHTML = value;
                OnPropertyChanged("ExportHTML");
            }
        }

        #endregion

        #region properties

        private Taxon taxon;

        public Taxon Taxon
        {
            get { return taxon;  }
            set
            {
                taxon = value;
                OnPropertyChanged("Taxon");
            }
        }

        private bool deprecated;
        public bool Deprecated
        {
            get { return deprecated; }
            set
            {
                deprecated = value;
                OnPropertyChanged("Deprecated");
            }
        }

        #endregion
    }
}
