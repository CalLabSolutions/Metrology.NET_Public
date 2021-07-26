using System.Windows;
using System.Windows.Controls;

namespace MT_Editor.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();            
            Settings.PasswordBox = pwdBox;
        }

        private void pwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password;
            }
        }

        public static class Settings
        {
            public static PasswordBox PasswordBox = null;
        }
    }
}
