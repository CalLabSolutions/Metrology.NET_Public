using Caliburn.Micro;
using SoA_Editor.ViewModels;
using System.Windows;

namespace SoA_Editor
{
    public class Bootstrapper : BootstrapperBase // using Caliburn.Micro // must be added
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>(); // we want this code to run on startup // using WPFUI.ViewModels; must be added
        }

    }
}
