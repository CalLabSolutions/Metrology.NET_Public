using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using MaterialDesignColors;
using System.IO;
using SOA_DataAccessLibrary;

namespace WpfApp4
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        private object tv;
        XmlDocument XMLdoc = new XmlDocument();
        SOA_DataAccess dao = new SOA_DataAccess();
        OpResult op;
        public MainWindow()
        {

            XmlDocument xmldoc;
            InitializeComponent();
        }
        private void BrowseXmlFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "XML Files (*.xml)|*.xml|All Files(*.*)|*.*";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() != true) { return; }

            
            try
            {
                XMLdoc.Load(dlg.FileName);
                dao.load(dlg.FileName);
            }
            catch (XmlException)
            {
                MessageBox.Show("The XML file is invalid");

                return;
            }
            TreeViewItem treeItem = null;
            treeItem = new TreeViewItem();
            treeItem.Header = XMLdoc.GetElementsByTagName("unc:ProcessType")[0].Attributes["name"].Value;
            treeItem.Items.Add(new TreeViewItem() { Header = XMLdoc.GetElementsByTagName("unc:Technique")[0].Attributes["name"].Value });
            tvMain.Items.Add(treeItem);
            Soa SampleSOA = dao.SOADataMaster;
            var process_name1 = SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count();//
            //MessageBox.Show(process_name1);
            MessageBox.Show(process_name1.ToString());
            //MessageBox.Show(XMLdoc.GetElementsByTagName("mtc:ProcessType")[0].Attributes["name"].Value);

            //id_box.Text =  +", "+ +", " +  + ", " + 
            name.Text = SampleSOA.CapabilityScope.MeasuringEntity.ToString();
            street.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.Street;
            city.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.City;
            state.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.State;
            zip.Text= SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.Zip;
            process_name.Text = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].Name;

            ComboBoxItem comboitem = null;
            comboitem = new ComboBoxItem();
            comboitem.Content = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Name;
            combo2.Items.Add(comboitem);
            comboitem.IsSelected = true;


        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            //freeArea.Text = XMLdoc.GetElementsByTagName("soa:AB_ID")[0].Value;
            //txtFilePath.Text = dlg.FileName;
            //vXMLViwer.xmlDocument = XMLdoc;
            //xmldata = XMLdoc;
            //.InnerText= "0102030405";
            //XMLdoc.Save("denemesave.xml");
            Microsoft.Win32.SaveFileDialog dlg2 = new Microsoft.Win32.SaveFileDialog();
            dlg2.FileName = "denemesave"; // Default file name
            dlg2.DefaultExt = ".xml"; // Default file extension
            dlg2.Filter = "XML Files (*.xml)|*.xml|All Files(*.*)|*.*"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg2.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                //string filename = dlg2.FileName;
                XMLdoc.Save(dlg2.FileName);
            }
        }
        private void DooSomething(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show("Do Something in TreeView!");
            //expander1.Header = "fff";
            //MessageBox.Show(((TreeViewItem)e.NewValue).Header.ToString());
            

            TreeViewItem childItem = e.NewValue as TreeViewItem;

            
            //treeItem.Header = XMLdoc.GetElementsByTagName("soa:City")[0].InnerText;
            //"North America";
            //treeItem.Items.Add(new TreeViewItem() { Header = "USA" });
            //treeItem.Items.Add(new TreeViewItem() { Header = "Canada" });
            //treeItem.Items.Add(new TreeViewItem() { Header = "Mexico" });
            

            if (childItem != null)
            {
                //childItem.Visibility = Visibility.Collapsed;
                process_name.Text = childItem.Header.ToString();
                //MessageBox.Show(childItem.Header.ToString()); // or MessageBox.Show(childItem.toString);
                childItem.IsSelected = true;

                //MessageBox.Show(childItem.Name);
            }
            
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string h =  combo2.Text + "." + fill.Text;
                source_voltage.Text = h;
            }
        }
        private void comboselection(object sender, SelectionChangedEventArgs e)
        {
            string h =  combo2.Text + "." + fill.Text;
            source_voltage.Text = h;
        }

    }
}
