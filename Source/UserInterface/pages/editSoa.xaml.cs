using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using soa_1_03.classes;

namespace soa_1_03.pages
{
    /// <summary>
    /// Interaction logic for editSoa.xaml
    /// </summary>
    public partial class editSoa : Page
    {
        //This window is unused for now. I'm leaving it in the code though, in case we go back to this approach. 

        vmTaxonomy viewModel;
        utilities util = utilities.GetInstance();


        public editSoa()
        {
            InitializeComponent();

            this.DataContext = null;
            viewModel = new vmTaxonomy();
            if (util.editSelected == true)
            {
                viewModel = utilities.OpenSoa(viewModel);
            }
            if (util.editSelected == false)
            {
                util.activeFilePath = null;
            }
            utilities.CreateCleanJson(viewModel);
            this.DataContext = viewModel;

            //Application.Current.MainWindow.Closing += CheckForEdits;
        }

        public void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAll(childControl, true);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if(item != null) { item.IsExpanded = true; }
            }
        }

        public static class VisualTreeHelperExtensions
        {
            public static T FindAncestor<T>(DependencyObject dependencyObject)
                where T : class
            {
                DependencyObject target = dependencyObject;
                do
                {
                    target = VisualTreeHelper.GetParent(target);
                }
                while (target != null && !(target is T));
                return target as T;
            }
        }

        private void tvComplete_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (object item in this.tvComplete.Items)
            {
                TreeViewItem treeItem = tvComplete.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null) { ExpandAll(treeItem, true); }
                treeItem.IsExpanded = true;
            }
        }

        private void tvComplete_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var trv = sender as TreeView;
            if (trv == null) return;
                TreeViewItem treeItem = trv.ItemContainerGenerator.ContainerFromItem(trv.SelectedItem) as TreeViewItem;
                if (treeItem != null)
                {
                    treeItem.IsSelected = false;
                }
                
            TreeViewItem treeItemCurrent = trv.ItemContainerGenerator.ContainerFromIndex(trv.Items.CurrentPosition) as TreeViewItem;
            if (treeItemCurrent != null) { treeItemCurrent.IsSelected = false; }
        }

        private void HierarchicalDataTemplate_Selected(object sender, RoutedEventArgs e)
        {
            TreeView trv = sender as TreeView;
        }

        private void dgTaxonomies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dgSender = sender as DataGrid;
            mSoaTaxonomy tempTaxonomy = dgSender.SelectedItem as mSoaTaxonomy;
            if (dgSender.SelectedItem != null)
            {
                Binding myBinding = new Binding("tempTaxonomy.soaTechniqueDescriptors");
                dgTechniques.DataContext = tempTaxonomy;
            }
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //bool UpdateOriginalViewModel = utilities.CheckForEdits(originalViewModel, viewModel, utilities.soaIsModified);
            //if (UpdateOriginalViewModel == true) { originalViewModel = viewModel; }
            //SaveIfEdited();

        }

        public void SoaPageUnloadHandler(object sender, EventArgs e)
        {
            SaveIfEdited();
        }

        public void SaveIfEdited()
        {
            utilities.CheckForEdits(viewModel, util.soaIsModified);
        }

        private void CheckForEdits(object sender, EventArgs e)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/80e9d0fc-02be-4dc4-8bcb-096ed42892e1/unloaded-event-of-page?forum=wpf
            SaveIfEdited();
        }

        public vmTaxonomy ViewmodelProvider()
        {
            vmTaxonomy vm = viewModel;
            return vm;
        }
    }
}
