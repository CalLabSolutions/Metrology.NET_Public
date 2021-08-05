using GalaSoft.MvvmLight.Command;
using MT_DataAccessLib;
using MT_UI.Pages;
using MT_UI.Services;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MT_UI.ViewModels
{
    class SettingsPageViewModel : Notifier
    {

        private TaxonomyFactory factory;
        public SettingsPageViewModel()
        {
            factory = new TaxonomyFactory();
            Locked = MT_Data.Locked;
        }

        #region commnands

        private ICommand loadFromServer;
        public ICommand LoadFromServer
        {
            get
            {
                if (loadFromServer == null)
                {
                    loadFromServer = new RelayCommand(() =>
                    {
                        factory.Reload(true);
                    });
                }
                return loadFromServer;
            }
            set
            {
                LoadFromServer = value;
                OnPropertyChanged("LoadFromServer");
            }
        }

        private ICommand exportXML;

        public ICommand ExportXML
        {
            get
            {
                if (exportXML == null)
                {
                    exportXML = new RelayCommand(async () =>
                    {
                        var filePicker = new FileSavePicker
                        {
                            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                            SuggestedFileName = "Local Metrology Taxonomy"
                        };
                        filePicker.FileTypeChoices.Add("XML", new List<string>() { ".xml" });
                        StorageFile file = await filePicker.PickSaveFileAsync();
                        if (file != null)
                        {
                            CachedFileManager.DeferUpdates(file);
                            await FileIO.WriteTextAsync(file, factory.Save(factory.GetAllTaxons()));
                        }
                    });
                }
                return exportXML;
            }
            set
            {
                exportXML = value;
                OnPropertyChanged("ExportXML");
            }
        }

        private ICommand exportXMLwXSLT;

        public ICommand ExportXMLwXSLT
        {
            get
            {
                if (exportXMLwXSLT == null)
                {
                    exportXMLwXSLT = new RelayCommand(async () =>
                    {
                        var folderPicker = new FolderPicker();
                        folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                        folderPicker.FileTypeFilter.Add("*");
                        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                        if (folder != null)
                        {
                            await factory.SaveWithXSLT(folder);
                        }
                    });
                }
                return exportXMLwXSLT;
            }
            set
            {
                exportXMLwXSLT = value;
                OnPropertyChanged("ExportXMLwXSLT");
            }
        }

        private ICommand loadLocalXML;

        public ICommand LoadLocalXML
        {
            get
            {
                if (loadLocalXML == null)
                {
                    loadLocalXML = new RelayCommand(async () =>
                    {
                        var filePicker = new FileOpenPicker
                        {
                            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                        };
                        filePicker.FileTypeFilter.Add(".xml");
                        StorageFile file = await filePicker.PickSingleFileAsync();
                        if (file != null)
                        {
                            string xml = await FileIO.ReadTextAsync(file);
                            factory.ReplaceLocal(xml);
                        }
                    });
                }
                return loadLocalXML;
            }
            set
            {
                loadLocalXML = value;
                OnPropertyChanged("LoadLocalXML");
            }
        }

        private ICommand unlock;
        public ICommand Unlock
        {
            get
            {
                if (unlock == null)
                {
                    unlock = new RelayCommand(() =>
                    {
                        if (Password == "MII_Taxonomy_1998")
                        {
                            MT_Data.Locked = false;
                            Locked = false;
                            MT_Data.RootFrame.DataContext = new MainPageViewModel();
                            Settings.PasswordBox.Password = "";
                        }
                        else
                        {
                            MT_Data.Locked = true;
                            Locked = true;
                        }
                    });
                }
                return unlock;
            }
            set
            {
                unlock = value;
                OnPropertyChanged("Unlock");
            }
        }

        private ICommand lockIt;
        public ICommand LockIt
        {
            get
            {
                if (lockIt == null)
                {
                    lockIt = new RelayCommand(() =>
                    {
                        MT_Data.Locked = true;
                        Locked = true;
                        MT_Data.RootFrame.DataContext = new MainPageViewModel();
                    });
                }
                return lockIt;
            }
            set
            {
                lockIt = value;
                OnPropertyChanged("LockIt");
            }
        }

        #endregion

        #region properties

        public string Password { private get; set; }

        private bool saveLocal = true;
        public bool SaveLocal
        {
            get { return saveLocal; }
            set
            {
                saveLocal = value;
                MT_Data.SaveLocal = saveLocal;
                OnPropertyChanged("SaveLocal");
            }
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

        #endregion
    }
}
