using MT_DataAccessLib;
using MT_UI.Pages.Forms;
using MT_UI.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            this.InitializeComponent();
            Form.Frame = FormContent;
            Form.TaxonToSave = (Taxon)MT_Data.SelectedTaxon.Clone();
            FormContent.Navigate(typeof(FormDetailsPage));
            DataContext = new AddEditPageViewModel();
        }
    }
}
