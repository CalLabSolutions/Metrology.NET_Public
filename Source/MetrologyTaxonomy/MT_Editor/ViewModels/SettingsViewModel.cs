using Caliburn.Micro;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MT_DataAccessLib;
using System;
using System.IO;
using static MT_Editor.Views.SettingsView;

namespace MT_Editor.ViewModels
{
    internal class SettingsViewModel : Screen
    {
        private TaxonomyFactory factory;

        public SettingsViewModel()
        {
            factory = new TaxonomyFactory();
            Locked = Helper.Locked;
        }

        #region Commands

        public void LoadFromServer()
        {
            factory.Reload(true);
        }

        public void ExportXML()
        {
            SaveFileDialog saveFileDialog = new()
            {
                DefaultExt = "xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "XML Files (*.xml)|*.xml"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                File.WriteAllText(saveFileDialog.FileName, factory.Save(factory.GetAllTaxons()));
            }
        }

        public void ExportXMLwXSLT()
        {
            CommonOpenFileDialog openFolderDialog = new()
            {
                Title = "Select Folder",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                IsFolderPicker = true,
            };

            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _ = factory.SaveWithXSLT(openFolderDialog.FileName + "\\");
            }
        }

        public void LoadLocalXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                DefaultExt = "xml",
                Filter = "XML Files (*.xml)|*.xml"
            };
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult.HasValue || dialogResult.Value)
            {
                string xml = File.ReadAllText(openFileDialog.FileName);
                factory.ReplaceLocal(xml);
            }
        }

        public void Unlock()
        {
            if (Password == "MII_Taxonomy_1998")
            {
                Helper.Locked = false;
                Locked = false;
                Helper.Shell.Refresh();
                Settings.PasswordBox.Password = "";
            }
            else
            {
                Helper.Locked = true;
                Locked = true;
            }
        }

        public void LockIt()
        {
            Helper.Locked = true;
            Locked = true;
            Helper.Shell.Refresh();
        }

        #endregion Commands

        #region Properties

        public string Password { private get; set; }

        private bool saveLocal = true;

        public bool SaveLocal
        {
            get { return saveLocal; }
            set
            {
                saveLocal = value;
                Helper.SaveLocal = saveLocal;
                NotifyOfPropertyChange(() => SaveLocal);
            }
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set
            {
                locked = value;
                NotifyOfPropertyChange(() => Locked);
            }
        }

        #endregion Properties
    }
}