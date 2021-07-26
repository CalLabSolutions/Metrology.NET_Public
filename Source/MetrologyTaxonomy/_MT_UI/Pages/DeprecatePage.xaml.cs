using MT_DataAccessLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeprecatePage : Page
    {
        public Taxon taxon;
        public string name;
        ObservableCollection<string> taxons = new ObservableCollection<string>();
        private TaxonomyFactory factory;

        public DeprecatePage()
        {
            this.InitializeComponent();
            taxon = MT_Data.SelectedTaxon;
            factory = new TaxonomyFactory();
            foreach (Taxon t in factory.GetAllTaxons())
            {
                if (taxon.Name != t.Name && t.Deprecated == false)
                    taxons.Add(t.Name);
            }
        }

        private void DeprecateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (taxonCombo.SelectedItem == null) return;
            string replacement = taxonCombo.SelectedItem.ToString();
            taxon.Deprecated = true;
            taxon.Replacement = replacement;
            var taxonomy = factory.Edit(taxon, taxon.Name);
            factory.Save(taxonomy, MT_Data.SaveLocal);
            MT_Data.ViewAll.IsSelected = true;
        }

        private async void DisplayAlreadyDeprecatedDialog()
        {
            ContentDialog selectTaxonDialog = new ContentDialog
            {
                Title = "Notice",
                Content = string.Format("\"{0}\" is already deprecated", taxon.Name),
                CloseButtonText = "Ok"
            };
            _ = await selectTaxonDialog.ShowAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MT_Data.SelectedTaxon.Deprecated)
            {
                DisplayAlreadyDeprecatedDialog();
                MT_Data.ViewAll.IsSelected = true;
            }
            
        }
    }
}
