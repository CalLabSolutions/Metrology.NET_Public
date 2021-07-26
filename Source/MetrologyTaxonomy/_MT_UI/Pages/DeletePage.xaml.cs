using Windows.UI.Xaml.Controls;
using MT_DataAccessLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeletePage : Page
    {
        public Taxon taxon;
        private TaxonomyFactory factory;
        
        public DeletePage()
        {
            this.InitializeComponent();
            taxon = MT_Data.SelectedTaxon;
            factory = new TaxonomyFactory();
        }

        private void No_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MT_Data.ViewAll.IsSelected = true;
        }

        private void Yes_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var taxonomy = factory.Delete(taxon);
            factory.Save(taxonomy, MT_Data.SaveLocal);            
            MT_Data.ViewAll.IsSelected = true;
        }
    }
}