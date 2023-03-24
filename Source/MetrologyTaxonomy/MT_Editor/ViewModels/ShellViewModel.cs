using Caliburn.Micro;
using System;
using System.Windows;
using System.Windows.Controls;
using static MT_Editor.Views.ShellView;

namespace MT_Editor.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            LoadHomeView();
            Helper.Shell = this;
        }

        #region Properites

        private ListViewItem selectedMenuItem;

        public ListViewItem SelectedMenuItem
        {
            get { return selectedMenuItem; }
            set
            {
                selectedMenuItem = value;
                if (value == null) return;
                string name = selectedMenuItem.Name.ToLower();
                NotifyOfPropertyChange(() => SelectedMenuItem);
                LoadMenuItem(name);
            }
        }

        public bool UnLocked
        {
            get { return !Helper.Locked; }
        }

        #endregion Properites

        #region Navigation Methods

        public void LoadHomeView()
        {
            Helper.SelectedTaxon = null;
            _ = ActivateItemAsync(new HomeViewModel());
        }

        public void LoadAllView()
        {
            Helper.SelectedTaxon = null;
            _ = ActivateItemAsync(new AllViewModel());
        }

        public void LoadDetailsView(MT_DataAccessLib.Taxon taxon)
        {
            Helper.SelectedTaxon = null;
            _ = ActivateItemAsync(new DetailsViewModel(taxon));
        }

        public void LoadAddEditView(bool edit)
        {
            if (edit)
            {
                if (Helper.SelectedTaxon == null) return;
                Helper.TaxonToSave = (MT_DataAccessLib.Taxon)Helper.SelectedTaxon.Clone();
                _ = ActivateItemAsync(new AddEditViewModel(true));
            }
            else
            {
                Helper.SelectedTaxon = null;
                Helper.TaxonToSave = new MT_DataAccessLib.Taxon();
                _ = ActivateItemAsync(new AddEditViewModel(false));
            }
        }

        public void LoadSettingsView()
        {
            _ = ActivateItemAsync(new SettingsViewModel());
        }

        public void LoadDeprecatedView()
        {
            _ = ActivateItemAsync(new DeprecateViewModel());
        }

        public void LoadDeleteView()
        {
            _ = ActivateItemAsync(new DeleteViewModel());
        }

        public void LoadMenuItem(string name)
        {
            // see if we have a menu item that requires an selected Taxon
            if ((name == "edit" || name == "deprecate" || name == "delete") && Helper.SelectedTaxon == null)
            {
                Helper.Navigate(Helper.MenuItem.ALL);             
                MessageBox.Show("You must select a Taxon from the \"View All\" Page Before can " + name + ".", "Notice",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (name == "deprecate" && Helper.SelectedTaxon != null && Helper.SelectedTaxon.Deprecated.Equals(true))
            {                           
                MessageBox.Show(Helper.SelectedTaxon.Name + "Is already Deprecated.", "Notice",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (name)
            {
                case "home":
                    LoadHomeView();
                    break;

                case "all":
                    LoadAllView();
                    break;

                case "settings":
                    LoadSettingsView();
                    break;

                case "add":
                    LoadAddEditView(false);
                    break;

                case "edit":
                    LoadAddEditView(true);
                    break;

                case "delete":
                    LoadDeleteView();
                    break;

                case "deprecate":
                    LoadDeprecatedView();
                    break;

                default:
                    LoadHomeView();
                    break;
            }
        }

        #endregion Navigation Methods
    }
}