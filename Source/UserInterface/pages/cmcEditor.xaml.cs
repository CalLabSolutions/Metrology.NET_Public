using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using soa_1_03.classes;
using soa_1_03.viewModels;
using System.IO;
using soa_1_03.models;
using System.ComponentModel;

namespace soa_1_03.pages
{
    /// <summary>
    /// Interaction logic for editSoa.xaml
    /// </summary>
    public partial class cmcEditor : Page
    {
        //TODO: enable Add Taxonomy button when both combo boxes have selection
        vmCmc vm;
        utilities util = utilities.GetInstance();


        public cmcEditor()
        {
            InitializeComponent();

            this.DataContext = null;
            string s = File.ReadAllText(util.taxonomiesPath);
            vm = utilities.GetTaxonomyMasterFromXml();
            CreateFilteredTaxonomy();
            SetCvUserTaxonomyGroupings();
            this.DataContext = vm;

            //Uncomment once data entry views are complete. Code will need to be edited to accommodate new viewmodel
            //Application.Current.MainWindow.Closing += CheckForEdits;
        }

        private void SetCvUserTaxonomyGroupings()
        {
            vm.cvUserTaxonomy = (CollectionView)CollectionViewSource.GetDefaultView(vm.userTaxonomy);
            PropertyGroupDescription cvUserTaxGroupAction = new PropertyGroupDescription("action");
            //PropertyGroupDescription cvUserTaxGroupFullTaxonomy = new PropertyGroupDescription("fullTaxonomy");
            vm.cvUserTaxonomy.GroupDescriptions.Add(cvUserTaxGroupAction);
            SortDescription cvUserTaxSortAction = new SortDescription("action", ListSortDirection.Ascending);
            SortDescription cvUserTaxSortFullTaxonomy = new SortDescription("fullTaxonomy", ListSortDirection.Ascending);
            vm.cvUserTaxonomy.SortDescriptions.Add(cvUserTaxSortAction);
            vm.cvUserTaxonomy.SortDescriptions.Add(cvUserTaxSortFullTaxonomy);
        }

        private void CreateFilteredTaxonomy()
        {
            vm.currentFilterTaxonomy = (CollectionView)CollectionViewSource.GetDefaultView(vm.masterTaxonomy);
            vm.currentFilterTaxonomy.Filter = SetFilteredTaxonomy;
        }

        private bool SetFilteredTaxonomy(object item)
        {
            bool contains = false;
            mTaxonomy t = item as mTaxonomy;
            string filter = vm.currentTaxonomy.action;
            if (filter == null || t.action == filter)
            {
                contains = true;
            }
            return contains;
        }

        private void cbActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.currentFilterTaxonomy.Refresh();
        }

        private void btnAddTaxonomy_Click(object sender, RoutedEventArgs e)
        {
            vm.userTaxonomy.Add(vm.currentTaxonomy);
            vm.cvUserTaxonomy.Refresh();
            GridView gv = lvViewer.View as GridView;
            if (gv != null)
            {
                foreach (GridViewColumn gvc in gv.Columns)
                {
                    gvc.Width = gvc.ActualWidth;
                    gvc.Width = double.NaN;
                }
            }
        }

        private void lvViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgDyRanges.DataContext = vm.rngTable.DefaultView;
        }

        #region Leave for now - will delete if not used
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (tabTaxonomies.IsSelected)
            //{
            //    ExpandCompleteTreeView();
            //}
        }

        public void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //bool UpdateOriginalViewModel = utilities.CheckForEdits(originalViewModel, viewModel, utilities.soaIsModified);
            //if (UpdateOriginalViewModel == true) { originalViewModel = viewModel; }
            //SaveIfEdited();

        }

        #endregion

        #region Check for Save stuff
        //public void LeavePageTasks()
        //{
        //    //utilities.executeFlag result = utilities.executeFlag.halt;
        //    //find if changes to original viewmodel
        //    bool IsModified = utilities.CompareObjects(viewModel);

        //    //show msg dialog for user decision to save / disregard / cancel
        //    if (IsModified == false)
        //    {
        //        vmTaxonomy newVm = new vmTaxonomy();
        //        ResetPage(newVm, true);
        //        return;
        //    }
        //    string msg = string.Format("{0}{1}{2}", "This page contains unsaved changes.", Environment.NewLine, "Click 'Yes' to save changes, 'No' to discard unsaved changes, or 'Cancel' to return to previous document.");
        //    string title = "Unsaved Changes";
        //    MessageBoxResult mbr = utilities.ShowMessageDialog(msg, title);
        //    if (mbr == MessageBoxResult.Cancel) { return; }
        //    if (mbr == MessageBoxResult.No)
        //    {
        //        //ResetPage();
        //        //return result = utilities.executeFlag.execute;
        //    }

        //    //display save file dialog
        //    //if (util.activeFilePath == null)
        //    //{
        //    //save as dialog
        //    string path = util.userPath;
        //    if (util.activeFilePath != null) { path = util.activeFilePath; }
        //    util.dictDlgResults = utilities.ShowSaveDialog(path);
        //    string dlgResult = util.dictDlgResults["result"];
        //    string newPath = util.dictDlgResults["path"];
        //    if (dlgResult == "False") { /*return result = utilities.executeFlag.halt;*/ }
        //    if (dlgResult == "True")
        //    {
        //        utilities.SaveFile(viewModel, newPath, utilities.fileType.xml);
        //        util.activeFilePath = newPath;
        //        //result = utilities.executeFlag.execute;
        //        //ResetPage();
        //    }
        //}

        //else
        //{
        //    //save w/o dialog
        //    utilities.SaveFile(viewModel, util.activeFilePath, utilities.fileType.xml);
        //    result = MessageBoxResult.Yes;
        //}
        //originalViewModel = viewModel;
        //    return;
        //}

        //public void ResetPage(vmTaxonomy newVm, bool resetOriginal)
        //{
        //    viewModel = null;
        //    viewModel = newVm;
        //    originalViewModel = viewModel;
        //    if (util.editSelected == true)
        //    {
        //        //viewModel = utilities.OpenSoa(viewModel);
        //    }
        //    if (resetOriginal == true) { originalViewModel = viewModel; }
        //    this.DataContext = viewModel;
        //}
        #endregion

    }
}
