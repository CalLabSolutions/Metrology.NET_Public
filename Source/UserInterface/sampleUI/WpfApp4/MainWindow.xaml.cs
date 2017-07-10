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
        XmlDocument db = new XmlDocument();
        SOA_DataAccess dao = new SOA_DataAccess();
        SOA_DataAccess dao2 = new SOA_DataAccess();
        OpResult op;
        public MainWindow()
        {
            InitializeComponent();
            db.Load("MetrologyNET_Taxonomy_v2.xml");
            dao2.load("MetrologyNET_Taxonomy_v2.xml");

            int process_count = db.GetElementsByTagName("mtc:ProcessType").Count;
            //MessageBox.Show(db.GetElementsByTagName("mtc:ProcessType").Count.ToString());
            ComboBoxItem comboitem2 = null;
            comboitem2 = new ComboBoxItem();
            for (int i = 0; i < process_count; i++)
            {
                comboitem2 = new ComboBoxItem();
                comboitem2.Uid = i.ToString();
                comboitem2.Content = db.GetElementsByTagName("mtc:ProcessType")[i].Attributes["name"].Value;
                combo2.Items.Add(comboitem2);
                //comboitem2.IsSelected = true;
            }

            //MessageBox.Show(db.GetElementsByTagName("mtc:ProcessType")[0].ChildNodes.Count.ToString());
            //XmlNode x= db.GetElementsByTagName("mtc:ProcessType")[1].ChildNodes[0].Attributes["name"].Value;

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
            //MessageBox.Show(process_name1.ToString());
            //MessageBox.Show(XMLdoc.GetElementsByTagName("mtc:ProcessType")[0].Attributes["name"].Value);

            //id_box.Text =  +", "+ +", " +  + ", " + 
            name.Text = SampleSOA.CapabilityScope.MeasuringEntity.ToString();
            street.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.Street;
            city.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.City;
            state.Text = SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.State;
            zip.Text= SampleSOA.CapabilityScope.Locations[0].OrganizationAddress.Zip;
            process_name.Text = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].Name;

            //load the exsiting process name into combobox
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
                //childItem.IsSelected = true;

                //MessageBox.Show(childItem.Name);
            }

            
        }
        private void DoSomething(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MessageBox.Show("Do Something!");
            
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
            ComboBox c_item = new ComboBox();
            ComboBoxItem cb_item = new ComboBoxItem();
            string h =  combo2.Text + "." + fill.Text;
            source_voltage.Text = h;

            int s_index = combo2.SelectedIndex;
            XmlNode s_node = db.GetElementsByTagName("mtc:ProcessType")[s_index];
            //MessageBox.Show("rslt cnt:" + db.GetElementsByTagName("mtc:ProcessType")[1].Name);
            if(s_node.HasChildNodes)
            {
                listview1.Items.Clear();
                listview2.Items.Clear();
                int chld_cnt = s_node.ChildNodes.Count;//GetType.ToString();// Count;
                //MessageBox.Show("rslt cnt:" + chld_cnt.ToString());
                for (int i = 0; i < chld_cnt; i++)
                {
                    
                    if (s_node.ChildNodes[i].Name == "mtc:Result")
                    {

                        //MyData data = getDataItem(index);
                        c_item = new ComboBox();
                        if (s_node.ChildNodes[i].Attributes["name"] != null)
                            c_item.Name = s_node.ChildNodes[i].Attributes["name"].Value;
                        else
                            c_item.Name = "";
                        //c_item.Name = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        cb_item = new ComboBoxItem();
                        cb_item.Content = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        c_item.Items.Add(cb_item);
                        cb_item.IsSelected = true;
                        listview1.Items.Add(c_item);
                    }
                    else if (s_node.ChildNodes[i].Name == "mtc:Parameter")
                    {

                        //MyData data = getDataItem(index);
                        c_item = new ComboBox();
                        c_item.Name = s_node.ChildNodes[i].Attributes["name"].Value;
                        //c_item.Name = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        cb_item = new ComboBoxItem();
                        cb_item.Content = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        c_item.Items.Add(cb_item);
                        //c_item.Height = 23;
                        cb_item.IsSelected = true;
                        listview2.Items.Add(c_item);

                    }
                }
                d_box.Header = s_node.LastChild.FirstChild.FirstChild.FirstChild.InnerText;
                int x = s_node.LastChild.FirstChild.LastChild.ChildNodes.Count;
                d_text.Text = "";
                for (int i = 0; i < x; i++)
                {
                    d_text.Text += s_node.LastChild.FirstChild.LastChild.ChildNodes[i].InnerText + "\n";
                }
            }
            
            // MessageBox.Show("rslt cnt:"+result_cnt.ToString());
            //ComboBoxItem childItem = e as ComboBoxItem;
        }

    }
}
