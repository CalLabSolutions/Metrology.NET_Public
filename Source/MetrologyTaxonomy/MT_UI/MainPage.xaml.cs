using MT_DataAccessLib;
using MT_UI.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MT_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static bool Locked;
        public MainPage()
        {                        

            this.InitializeComponent();
            DataContext = new MainPageViewModel();
            
            // We need some control over these navigation while inside our ViewModels
            MT_Data.ContentFrame = ContentFrame;
            MT_Data.ViewAll = ViewAll;
            MT_Data.RootFrame = RootFrame;
        }

        #region navigation

        // Keep the last page visited in memeory
        private NavigationViewItem _lastItem;

        // Event for Navigation Menu
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (!(args.InvokedItemContainer is NavigationViewItem item) || item == _lastItem)
                return;
            var clickedPage = item.Tag?.ToString();

            // see if we are going to settings
            if (clickedPage == null && item.Content.ToString().ToLower() == "settings")
                clickedPage = "SettingsPage";
            
            // Make sure we have a selected taxon before going to edit, delete, or deprecate
            if ((clickedPage == "EditPage" && MT_Data.SelectedTaxon == null) ||
                (clickedPage == "DeletePage"&& MT_Data.SelectedTaxon == null) ||
                (clickedPage == "DeprecatePage" && MT_Data.SelectedTaxon == null))
            {
                NavigateToPage("ViewAllPage");
                SetSelectedItem();
                DisplaySelectTaxonDialog(clickedPage.Replace("Page", ""));
                return;
            }

            // remove selected taxon if needed
            if ((clickedPage != "EditPage" && clickedPage != "DeletePage" && clickedPage != "DeprecatePage") && (MT_Data.SelectedTaxon != null))
            {
                MT_Data.SelectedTaxon = null;
            }

            // navigate to page
            if (!NavigateToPage(clickedPage)) return;
            _lastItem = item; // save last item
        }

        // Get the Page type and navigate to it.
        private bool NavigateToPage(string clickedView)
        {
            var page = Assembly.GetExecutingAssembly()
                .GetType($"MT_UI.Pages.{clickedView}");

            if (string.IsNullOrWhiteSpace(clickedView) || page == null)
            {
                return false;
            }

            ContentFrame.Navigate(page, null, new EntranceNavigationTransitionInfo());
            return true;
        }

        // Go Back
        private void NavView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                try
                {
                    ContentFrame.GoBack();
                    SetSelectedItem();
                }
                catch (Exception)
                {
                    NavigateToPage("ViewAllPage");
                    ViewAll.IsSelected = true;
                }                
            }

        }

        // Not sure why I need this, but all examples implemented it.  Go to View All Page
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            ContentFrame.BackStack.Clear();
            NavigateToPage("ViewAllPage");
            ViewAll.IsSelected = true;
        }

        // Make sure Home is Selected 
        private void Home_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Home.IsSelected = true;
        }

        private void SetSelectedItem()
        {
            var pageName = ContentFrame.Content.GetType().Name;
            try
            {
                var navItem = NavView.MenuItems
                     .OfType<NavigationViewItem>()
                     .Where(i => i.Tag.ToString() == pageName)
                     .First();
                NavView.SelectedItem = navItem;
                _lastItem = navItem;
            }
            catch(Exception)
            {
                if (_lastItem != null)
                {
                    NavView.SelectedItem = _lastItem;
                }
            }
            
        }

        private async void DisplaySelectTaxonDialog(String action)
        {
            ContentDialog selectTaxonDialog = new ContentDialog
            {
                Title = "Notice",
                Content = "Please Select a Taxon from the View All Page to " + action,
                CloseButtonText = "Ok"
            };
            _ = await selectTaxonDialog.ShowAsync();
        }

        #endregion

    }




    #region Globals
    public static class MT_Data
    {
        // Selected Taxon
        public static Taxon SelectedTaxon = null;
        
        // Navigation
        public static Frame ContentFrame; // used for navigation from within a view model
        public static Page RootFrame;
        public static NavigationViewItem ViewAll;

        // Save local or in memory
        public static bool SaveLocal = true;

        // Track where we loaded data from
        public static bool LoadedFromServer = true;

        // Default mode is locked
#if DEBUG
        public static bool Locked = false;
#else
        public static bool Locked = true;
#endif
    }
#endregion
}