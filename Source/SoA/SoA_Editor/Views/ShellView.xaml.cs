using System.Windows;
using System.Windows.Controls;

namespace SoA_Editor.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            // do not set to handled or nested controls will not scroll
            // I will wait and see if this causes a problem
            //e.Handled = true;
        }
    }
}
