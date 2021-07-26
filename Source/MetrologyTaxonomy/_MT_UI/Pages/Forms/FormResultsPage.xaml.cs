using MT_UI.ViewModels.ViewModelForms;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages.Forms
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FormResultsPage : Page
    {
        public FormResultsPage()
        {
            this.InitializeComponent();
            DataContext = new FormResultsPageViewModel();
        }
    }
}
