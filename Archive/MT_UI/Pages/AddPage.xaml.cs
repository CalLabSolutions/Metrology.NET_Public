using MT_DataAccessLib;
using MT_UI.Pages.Forms;
using MT_UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MT_UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        public AddPage()
        {
            this.InitializeComponent();
            MT_Data.SelectedTaxon = null;
            Form.TaxonToSave = new Taxon();
            Form.Frame = FormContent;
            FormContent.Navigate(typeof(FormDetailsPage));
            DataContext = new AddEditPageViewModel();            
        }
    }

    public static class Form
    {
        public static Frame Frame;
        public static Taxon TaxonToSave;
    }
}
