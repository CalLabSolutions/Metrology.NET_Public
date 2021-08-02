using SoA_Editor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void myShellWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.IsSaveAs = false;
                viewModel.SaveXML();
            }

            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.OpenXMLFile();
            }
        }
    }
}
