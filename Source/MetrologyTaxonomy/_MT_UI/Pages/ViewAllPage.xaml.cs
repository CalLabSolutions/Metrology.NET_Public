using Windows.UI.Xaml.Controls;
using MT_UI.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewAllPage : Page
    {
        public ViewAllPage()
        {
            InitializeComponent();
            DataContext = new ViewAllPageViewModel();
        }
    }
}
