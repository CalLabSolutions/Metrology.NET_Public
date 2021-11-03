using SoA_Editor.Models;
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

        // if would be easier to handle tree view clicks for nested ranges here
        private void NestedRangeNodeSelected(object sender, RoutedEventArgs e)
        {
            var node = TaxonomyTreeView.SelectedItem as Node;
            if (node.Type != NodeType.Range) return;
            if (node.Type == NodeType.Range)
            {
                if (node.Parent.Type == NodeType.Technique)
                {
                    return;
                }
            }
            var viewModel = (ShellViewModel)this.DataContext;
            viewModel.RangeNodeClick((RangeNode)node);
        }

        private void myShellWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!SaveXML.IsEnabled) return;
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.IsSaveAs = false;
                viewModel.SaveXML();
            }

            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.OpenXMLFile();
            }

            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.NewXML();
            }

            if (e.Key == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!CloseXMLFile.IsEnabled) return;
                var viewModel = (ShellViewModel)this.DataContext;
                viewModel.NewXML();
            }
        }
    }
}
