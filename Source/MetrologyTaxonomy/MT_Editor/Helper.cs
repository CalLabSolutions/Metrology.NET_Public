using Caliburn.Micro;
using MT_DataAccessLib;
using MT_Editor.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace MT_Editor
{
    public class Helper
    {
        // In case we need access to the Shell from other screens
        public static Conductor<object> Shell = null;

        // Navigation Helper
        public static ListView Menu = null;

        public static void Navigate(MenuItem item)
        {
            Menu.SelectedIndex = (int)item;
        }

        public enum MenuItem
        {
            HOME, ALL, ADD, EDIT, DELETE, DEPRACATE, SETTINGS
        }

        // Track our selected Taxon
        public static Taxon SelectedTaxon = null;

        // Track our Taxon to Add or Edit
        public static Taxon TaxonToSave = null;

        // Save local or in memory
        public static bool SaveLocal = true;

        // Track if the application is locked or not
#if DEBUG
        public static bool Locked = false;
#else
        public static bool Locked = true;
#endif

        // Open Link in the browser
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        internal class MessageDialog
        {
            public string Title = "";
            public string Message = "";
            public MessageBoxButton Button;
            public MessageBoxImage Image;

            public void Show()
            {
                MessageBox.Show(Message, Title, Button, Image);
            }
        }
    }
}