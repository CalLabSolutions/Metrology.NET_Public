using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using soa_1_03.classes;

namespace soa_1_03
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Prelims and Event Handler Hooks
        public static MainWindow AppWindow;
        utilities util;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            util = utilities.GetInstance();
        }

        #region Commands

        private void GoToPageExecuteHandler(object sender, ExecutedRoutedEventArgs e)
        {
            frameMain.NavigationService.Navigate(new Uri((string)e.Parameter, UriKind.Relative));
        }

        private void GoToPageCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e) /*Edit command uses this method also*/
        {
            util.editSelected = (bool)e.Parameter;

            pages.editSoa page = GetCurrentPageInstance();
            if (page != null)
            {
                page.SaveIfEdited();
            }
            if (util.canceled != true)
            {
                frameMain.Content = null;
                pages.cmcEditor newPg = new pages.cmcEditor();
                frameMain.Content = newPg;
            }
            UncheckMenuToggleButtons();
            util.editSelected = false;
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            pages.editSoa page = GetCurrentPageInstance();
            if (page != null)
            {
                vmTaxonomy vm = page.ViewmodelProvider();
                utilities.SaveFile(vm, util.activeFilePath, utilities.fileType.xml);
            }
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //check for a util.saveFileName value. If != null, then save using saveFileName
            if (util.activeFilePath != null)
            {
                e.CanExecute = true;
            }
            else { e.CanExecute = false; }
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            pages.editSoa page = GetCurrentPageInstance();
            if (page != null)
            {
                vmTaxonomy vm = page.ViewmodelProvider();
                utilities.SaveFileAs(vm);
            }
        }

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentPageCheck("editSoa");
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //check for save
            frameMain.Content = null;
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentPageCheck("editSoa");
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        #endregion

        #region ButtonClick Events

        private async void UncheckMenuToggleButtons()
        {
            await DelayButtonUncheck();
            foreach (var control in spMenu.Children)
            {
                if (control is RadioButton)
                {
                    RadioButton rb = (RadioButton)control;
                    rb.IsChecked = false;
                }
            }
        }

        async Task DelayButtonUncheck()
        {
            await Task.Delay(1200);
        }

        private pages.editSoa GetCurrentPageInstance()
        {
            //TODO: Check if save required before page unloaded
            pages.editSoa page = null;
            if (frameMain.Content != null)
            {
                //page = (pages.editSoa)frameMain.Content;

                //Dictionary<string, vmTaxonomy> vmTaxonomies = page.ViewmodelProvider();
                //List<string> s = GetAddressFields(vmTaxonomies);
            }
            else { page = null; }
            return page;
        }

        private List<string> GetAddressFields(Dictionary<string, vmTaxonomy> vmTaxonomies)
        {
            List<string> ls = new List<string>();
            foreach (KeyValuePair<string, vmTaxonomy> kvp in vmTaxonomies)
            {
                vmTaxonomy vm = kvp.Value;
                ls.Add(vm.vmClient.streetAddress01);
            }
            return ls;
        }
        private bool CurrentPageCheck(string pageName)
        {
            bool result = false;
            if (frameMain.Content != null)
            {
                string s = frameMain.Content.GetType().ToString();
                if (s.Contains(pageName))
                {
                    result = true;
                }
            }
            return result;
        }

        private void OpenEditSoa()
        {
            pages.editSoa page = null;
            if (frameMain.Content != null)
            {
                page = (pages.editSoa)frameMain.Content;
                //page.ResetPage();
            }
            else
            {
                page = new pages.editSoa();
                frameMain.Content = page;
            }
        }
        #endregion


        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (frameMain.Content != null)
            {
                pages.editSoa page = GetCurrentPageInstance();
                if (page != null)
                {
                    page.SaveIfEdited();
                }
            }
            if (util.canceled == true)
            {
                e.Cancel = true;
            }
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand Edit = new RoutedUICommand ( "Edit", "Edit", typeof(CustomCommands), 
            new InputGestureCollection() { new KeyGesture(Key.E, ModifierKeys.Control) });

        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.X, ModifierKeys.Control) });
    }
}
