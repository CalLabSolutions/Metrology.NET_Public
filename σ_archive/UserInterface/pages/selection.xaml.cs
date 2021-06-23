using System.Windows;
using System.Windows.Controls;

namespace soa_1_03.pages
{
    //This is test code. It provides a launcher activated through the Taxonomies menu button to display the old-style SOA builder pages. I'm leaving 
    //for now, just in case we revert to older approach.


    /// <summary>
    /// Interaction logic for selection.xaml
    /// </summary>
    public partial class selection : Page
    {
        public selection()
        {
            InitializeComponent();
        }

        private void btnNewSoa_Click(object sender, RoutedEventArgs e)
        {
            editSoa newEditPage = new editSoa();
            ((MainWindow)Application.Current.MainWindow).frameMain.Navigate(newEditPage);
        }
    }
}
