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
using System.Xml.Linq;
using Kent.Boogaart.Converters;
using System.Data;
using System.Globalization;

namespace WpfApp4
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        XmlDocument XMLdoc = new XmlDocument();
        XmlDocument db = new XmlDocument();
        SOA_DataAccess dao = new SOA_DataAccess();
        SOA_DataAccess dao2 = new SOA_DataAccess();
        OpResult op;
        Soa SampleSOA;
        StringBuilder stringBuilder;
        List<TextBlock> textboxes = new List<TextBlock>();

        int fdataindex;
        string[] data1=new string[9];
        string[] data2 = new string[9];

        public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CloseableTabItem));
        private object tv;
        private int changed_no=0;
        private int casenumber = 1;
        TextBlock t = new TextBlock();

        public MainWindow()
        {
            InitializeComponent();
            var bc = new BrushConverter();
            Brush mycolor= (Brush)bc.ConvertFrom("#008b8b ");
            //load the process database
            db.Load("MetrologyNET_Taxonomy_v2.xml");
            //dao2.load("MetrologyNET_Taxonomy_v2.xml");
                //process counts in the database
            int process_count = db.GetElementsByTagName("mtc:ProcessType").Count;
                //put the processes into combobox(combo2)
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

            //cd0.Width = new GridLength(0, GridUnitType.Pixel);
            cd1.Width = new GridLength(0, GridUnitType.Pixel);
            
            /*
            split1.Width = 0;
            cv1.Visibility = Visibility.Hidden;
            cv2.Visibility = Visibility.Hidden;
            cv1tb.Visibility = Visibility.Hidden;
            cv2tb.Visibility = Visibility.Hidden;
            Separator ss = new Separator();
            ss.Height = 4;
            ss.Opacity = 0;
            sp1.Children.Add(ss);
            Button b1 = new Button();
            b1.Content = "Edit";
            b1.Height = 32;
            b1.Width=74;
            b1.Click +=Open_Click;
            sp1.Children.Add(b1);
            ss = new Separator();
            ss.Height = 4;
            ss.Opacity = 0;
            sp2.Children.Add(ss);
            ss = new Separator();
            ss.Height = 6;
            ss.Opacity = 0;
            sp2.Children.Add(ss);
            TextBlock b2 = new TextBlock();
            b2.Text = "Formula 1";
            sp2.Children.Add(b2);*/
            //MessageBox.Show(db.GetElementsByTagName("mtc:ProcessType")[0].ChildNodes.Count.ToString());
            //XmlNode x= db.GetElementsByTagName("mtc:ProcessType")[1].ChildNodes[0].Attributes["name"].Value;
            tabs.SelectedIndex=0;

        }
        
        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }
        public TreesViewModel ViewModel => DataContext as TreesViewModel;
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel == null) return;

            ViewModel.SelectedItem = e.NewValue;
        }
        private void AddProcessItem(object sender, RoutedEventArgs e)
        {
            int ic = tvMain.Items.Count;
            TreeViewItem treeItem = new TreeViewItem();
            treeItem = new TreeViewItem() { Header = "Process"+(ic-1).ToString() };
            tvMain.Items.Insert(ic-1, treeItem);
            //(tvMain.Items[ic - 1] as TreeViewItem).MouseLeftButtonUp += slct_prcss;


        }
        private void AddTechniqueItem(object sender, RoutedEventArgs e)
        {
            int ic = tvMain.Items.Count;
            int ind = tvMain.Items.IndexOf(tvMain.SelectedItem);
            int tc = (tvMain.Items[ind] as TreeViewItem).Items.Count;

            TreeViewItem treeItem = new TreeViewItem();
            if (ind > 0)
            {
                (tvMain.Items[ind] as TreeViewItem).Items.Add(new TreeViewItem() { Header = "Technique" + (tc+1).ToString()});
            }
        }
        private void RemoveTreeItem(object sender, RoutedEventArgs e)
        {
            if(tvMain.Items.IndexOf(tvMain.SelectedItem)>0)
            tvMain.Items.RemoveAt(tvMain.Items.IndexOf(tvMain.SelectedItem));
            else if(tvMain.Items.IndexOf(tvMain.SelectedItem)==tvMain.Items.Count)
            {
                MessageBox.Show("Please Choose Process or Technique to Delete!");
            }
            else
            {
                MessageBox.Show("Please Choose Process or Technique to Delete!");
            }
        }
        private void BrowseXmlFile(object sender, RoutedEventArgs e)
        {
            dao = new SOA_DataAccess();
            split1.Width = 5;
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
                //make empty main tree
            
                //resize column defitions of the tree&tab grid 
            //cd0.Width = new GridLength(0, GridUnitType.Auto);
            cd1.Width = new GridLength(220, GridUnitType.Pixel);
                
            SampleSOA = dao.SOADataMaster;
            set_company_info(SampleSOA);


            var process_name1 = SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count();//
            
            string techwithext= SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Name;
            string fwithext = SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].name;
            //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count().ToString());
            int cc;
            int pl = prcss.Header.ToString().Length;
            int tl = techwithext.Length;
            int fl = fwithext.Length;
            
            //SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Name = "jj";            //MessageBox.Show(pl.ToString());
            string t=techwithext.Substring(0, 3);
            tech_tree0.Header = techwithext.Substring(pl+1);
            f_tree.Header = fwithext.Substring(tl + 1);
            tec_name.Text = tech_tree0.Header.ToString();


            //tvMain.Items.RemoveAt(1);
            tvMain.Items.Clear();
            ToolTip tt = new ToolTip(); ToolTip tt2 = new ToolTip();
            int pc = SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count();
            int tc = SampleSOA.CapabilityScope.Activities[0].Techniques.Count();
            int fc = SampleSOA.CapabilityScope.Activities[0].Templates.Count();
            int q = 0;
            int p = 0;
            TreeViewItem infonode = new TreeViewItem();
            infonode.Header = "Company Info";
            infonode.Selected += slct_info;
            tvMain.Items.Add(infonode);
            TreeViewItem processnode = new TreeViewItem();
            TreeViewItem technode = new TreeViewItem();
            TreeViewItem funcnode = new TreeViewItem();
            TreeViewItem cnode = new TreeViewItem();
            for (int i=0;i<pc;i++)
            {
                
                string s = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[i].ProcessType.Name;
                tt.Content = s;
                int ic=tvMain.Items.Count;

                processnode.Header = s;// "Process"+(i+1).ToString();
                //processnode.ToolTip = tt;
                //tt = new ToolTip();
                processnode.Selected += slct_prcss;
                processnode.Name = "process" + (i).ToString();
                for (int j=0;j<tc;j++)
                {

                    if (s == SampleSOA.CapabilityScope.Activities[0].Techniques[j].Technique.ProcessTypeName)
                    {
                        string s2 = SampleSOA.CapabilityScope.Activities[0].Techniques[j].name;
                        tt.Content = s2;
                        //technode.ToolTip = tt;
                        //tt = new ToolTip();
                        technode.Header = s2;// "Technique"+(q+1).ToString();
                        technode.Name= "technique" + (j).ToString();
                        for (int k=0;k<fc;k++)
                        {
                            if(s2== SampleSOA.CapabilityScope.Activities[0].Templates[k].TemplateTechnique.name)
                            {
                                string s3 = SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].name;
                                tt.Content = s3;
                                //funcnode.ToolTip = k;
                                //tt = new ToolTip();
                                funcnode.Header = s3;// "Function" + (p + 1).ToString();
                                funcnode.Name = "f" + (k).ToString();
                                cc = SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].Cases.Count();
                                if(cc==1)
                                {
                                cnode.Name = "case" + casenumber.ToString();
                                cnode.Header = SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].Cases[0].Ranges[0].Variable_name+" function";
                                cnode.Selected += slct_case;
                                funcnode.Items.Add(cnode);
                                cnode = new TreeViewItem();
                                technode.Items.Add(funcnode);
                                funcnode = new TreeViewItem(); p++;
                                }
                                if (cc > 1)
                                {
                                    for (int x = 0; x < cc; x++)
                                    {
                                        string temp = "";
                                        cnode.Name = "case" + casenumber.ToString();
                                        int an = SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].Cases[x].Assertions.Count();
                                        cnode.Header = SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].Cases[x].Assertions[0].Value;
                                        for (int u = 1; u < an; u++)
                                        {
                                            cnode.Header = cnode.Header+" "+ SampleSOA.CapabilityScope.Activities[0].Templates[k].CMCUncertaintyFunctions[0].Cases[x].Assertions[u].Value;
                                            
                                        }

                                        cnode.Selected += slct_case;
                                        funcnode.Items.Add(cnode);
                                        cnode = new TreeViewItem();
                                    }
                                    technode.Items.Add(funcnode);
                                    funcnode = new TreeViewItem(); p++;
                                }
                                

                            }
                            
                        }
                        p = 0;
                        processnode.Items.Add(technode);
                        technode = new TreeViewItem();
                        q++;
                        
                    }
                    
                }
                q = 0;
                //tvMain.Items.Insert(ic - 1, processnode);
                tvMain.Items.Add( processnode);
                processnode = new TreeViewItem();
                
            }
            
            dlg = new Microsoft.Win32.OpenFileDialog();


        }
            //set company info page
        private void set_company_info(Soa s)
        {
            ab.Text = s.Ab_ID;
            ablogo.Text = s.Ab_Logo_Signature;
            scopeid.Text = s.Scope_ID_Number;
            crtr.Text = s.Criteria;
            eff.Text = s.EffectiveDate;
            exp.Text = s.ExpirationDate;
            sttmnt.Text = s.Statement;
            //m_entity.Text = s.CapabilityScope.MeasuringEntity;
            name.Text = s.CapabilityScope.MeasuringEntity.ToString();
            street.Text = s.CapabilityScope.Locations[0].Address.Street;
            city.Text = s.CapabilityScope.Locations[0].Address.City;
            state.Text = s.CapabilityScope.Locations[0].Address.State;
            zip.Text = s.CapabilityScope.Locations[0].Address.Zip;
            loc_id.Text = s.CapabilityScope.Locations[0].id;
            cname.Text = s.CapabilityScope.Locations[0].ContactName;
            cinfo.Text = s.CapabilityScope.Locations[0].ContactInfo.ToString();
        }
            //save the data
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].name = "selamd";
            SampleSOA.CapabilityScope.Locations[0].Address.Street = street.Text;
            SampleSOA.CapabilityScope.Locations[0].Address.City = city.Text;
            SampleSOA.CapabilityScope.Locations[0].Address.State = state.Text;
            SampleSOA.CapabilityScope.Locations[0].Address.Zip = zip.Text;
            SampleSOA.Ab_ID = ab.Text;
            SampleSOA.Ab_Logo_Signature = ablogo.Text;
            SampleSOA.Scope_ID_Number = scopeid.Text;
            SampleSOA.Criteria = crtr.Text;
            SampleSOA.EffectiveDate = eff.Text;
            SampleSOA.ExpirationDate = exp.Text;
            SampleSOA.Statement = sttmnt.Text;
            save_changed(1);
            /*
            SampleSOA.CapabilityScope.Locations[0].Address.Street=street.Text;
            
            
            */
            SampleSOA.CapabilityScope.Locations[0].id= loc_id.Text;
            SampleSOA.CapabilityScope.Locations[0].ContactName= cname.Text;
            //SampleSOA.CapabilityScope.Locations[0].ContactInfo=;
            

            XDocument savefile = new XDocument();
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
                SampleSOA.writeTo(savefile);
                savefile.Save(dlg2.FileName);
                //XMLdoc.Save(dlg2.FileName);
                //dao.SOADataMaster.Save("ser");
               // dao.Doc.Save("we");
            }

        }
            //save the changes
        private void save_changed(int x)
        {
            
            switch (x)
            {
                case 1:
                    SampleSOA.CapabilityScope.MeasuringEntity = "selam";
                    //MessageBox.Show("process saved");
                    break;
                default:
                    //MessageBox.Show("xxx");
                    break;
            }
        }
       
        private void slct_tech(object sender, RoutedEventArgs e)
        {
            //tabs.SelectedIndex = 3;
            //MessageBox.Show("Technique!");
            TreeViewItem childItem = e.Source as TreeViewItem;
            childItem.IsSelected = false;
            
            if (childItem.Name[0] == 't')
                tabs.SelectedIndex = 3;
            else
                tabs.SelectedIndex = 4;
            //MessageBox.Show(childItem.Header.ToString());

        }
        private void slct_prcss(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Process!");
            TreeViewItem childItem = e.Source as TreeViewItem;
            //childItem.IsSelected = false;
            //MessageBox.Show(childItem.Name);
            if (childItem.HasItems && childItem.Name[0] == 'p')
            {
                int i = tvMain.Items.IndexOf(childItem);
                set_process(i);
                tabs.SelectedIndex = 2;
            }
            
            else if (childItem.Name[0] == 't')
            {
                int tl = childItem.Name.Length;
                int t = (int)Char.GetNumericValue(childItem.Name[tl - 1]);
                TreeViewItem parent = childItem.Parent as TreeViewItem;
                int pl = parent.Name.Length;
                int p = (int)Char.GetNumericValue(parent.Name[pl - 1]);
                set_tech(p, t);
                tabs.SelectedIndex = 3;
                tabs.SelectedIndex = 6;
            }
                
            else if (childItem.Name[0] == 'c')
            {
                // int i = tvMain.Items.IndexOf(tvMain.SelectedItem); MessageBox.Show(i.ToString());
            }  
            else 
            {
                //set_func(6);
               // tabs.SelectedIndex = 5;
            }
                
        }
        private void set_process(int p)
        {
            //load the exsiting process name into combobox
            ComboBoxItem comboitem = null;
            comboitem = new ComboBoxItem();
            comboitem.Content = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p-1].ProcessType.Name;
            combo2.Items.Add(comboitem);
            comboitem.IsSelected = true;
            prcss.Header = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p-1].ProcessType.Name;
            //*
            Separator s = new Separator();
            ComboBox c = new ComboBox();
            ComboBoxItem i = new ComboBoxItem();
            TextBlock t = new TextBlock();
            CheckBox b = new CheckBox();

            int ic = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p-1].ProcessType.Parameters.Count();
            is1.Children.Clear();
            is2.Children.Clear();
            is3.Children.Clear();
            os1.Children.Clear();
            os2.Children.Clear();
            os3.Children.Clear();

            for (int x=0;x<ic;x++)
            {
                
                addsep(s, is1); s = new Separator();
                addsep(s, is1); s = new Separator();
                t.Text = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.Parameters[x].name;
                addtext(t, is1, 60); t = new TextBlock();
                addsep(s, is1); s = new Separator();
                if(SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.Parameters[x].Quantity!=null)
                {
                    i.Content = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.Parameters[x].Quantity.name;

                }
                c.Items.Add(i);
                i.IsSelected = true;
                i = new ComboBoxItem();
                addsep(s, is2); s = new Separator();
                addcombo(c, is2);c = new ComboBox();
                //addsep(s, is2); s = new Separator();
                addsep(s, is3); s = new Separator();
                addsep(s, is3); s = new Separator();
                addcheck(b, is3); b = new CheckBox();
                addsep(s, is3); s = new Separator(); 
            }
                //output for process
            addsep(s, os1); s = new Separator();
            addsep(s, os1); s = new Separator();
            t.Text = "Result:";
            addtext(t, os1, 60); t = new TextBlock();
            i.Content = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.ProcessResults[0].Quantity.name;
            c.Items.Add(i);
            i.IsSelected = true;
            i = new ComboBoxItem();
            addsep(s, os2); s = new Separator();
            addcombo(c, os2); c = new ComboBox();

            //documentation
            //if (SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.Documentation.Document!=null) 
            //d_text.Text= SampleSOA.CapabilityScope.Activities[0].ProcessTypes[p - 1].ProcessType.Documentation.Document;
        }
        private void set_tech(int p,int tt)
        {
            int globe = 0;
            //MessageBox.Show(p.ToString() + tt.ToString());
            Separator s = new Separator();
            ComboBox c = new ComboBox();
            ComboBoxItem i = new ComboBoxItem();
            TextBlock t = new TextBlock();
            TextBox t2 = new TextBox();
            CheckBox b = new CheckBox();
            Button bt = new Button();
           // textBlock11t.Text=
            int it = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters.Count();
            int ip = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges.Count();
            is4.Children.Clear();
            is5.Children.Clear();
            //is6.Children.Clear();
            os4.Children.Clear();
            os5.Children.Clear();
            os6.Children.Clear();
            addsep(s, is4); s = new Separator();
            addsep(s, is4); s = new Separator();
            addsep(s, is5); s = new Separator();
            addsep(s, is5); s = new Separator();
            //new tech
            is6t.Children.Clear();
            is7t.Children.Clear();
            is8t.Children.Clear();
            is9t.Children.Clear();
            int tgc = svgridt.Children.Count;
            svgridt.Children.RemoveRange(4, tgc - 3);
            addtexthwb(t, is6t, 30, 90); t = new TextBlock();
            addtexthwb(t, is7t, 30, 90); t = new TextBlock();
            addtexthwb(t, is8t, 30, 90); t = new TextBlock();
            textBlock11t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ProcessTypeName+"; \n"+SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Name;
            for (int x = 0; x < it; x++)
            {
                
                addsep(s, is4); s = new Separator();
                t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters[x].name;
                addtext(t, is4, 65); t = new TextBlock();
                //new text

                //t.Text = "MIN";

                t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters[x].name;
                addtexthw(t, is6t, 30, 90); t = new TextBlock();
                if (x < ip)
                {
                    addsep(s, is5); s = new Separator();
                    t.Text = "Start " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].Start.test +
                        ": " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].Start.Value.ToString() +
                        " End " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].End.test +
                        ": " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].End.Value.ToString();
                    //addtext(t, is5, 130); t = new TextBlock();//old tech
                    //new tech
                    t2.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].Start.Value.ToString();
                    addtextboxhwb(t2, is7t, 30, 90); t2 = new TextBox();
                    t2.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ParameterRanges[x].End.Value.ToString();
                    addtextboxhwb(t2, is8t, 30, 90); t2 = new TextBox();
                    if (x == ip-1)
                    {
                        addtexthwb(t, is6t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is7t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is8t, 30, 90); t = new TextBlock();
                    }
                }
                else
                {
                    if(SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters[x].Quantity!=null)
                    {
                        t.Text = "";
                        addtexthwb(t, is8t, 30, 90); t = new TextBlock();
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters[x].Quantity.name;
                        addtexthw(t, is7t, 30, 90); t = new TextBlock();
                    }
                    else
                    {
                        //t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Parameters[x].
                        // what about enumeration parmeter?
                        addtexthwb(t, is7t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is8t, 30, 90); t = new TextBlock();
                    }

                }

            }
            //output for technique
            addsep(s, os5); s = new Separator();
            addsep(s, os5); s = new Separator();
            addsep(s, os5); s = new Separator();
            t.Text = "Result:"; 
            addtext(t, os5, 60); t = new TextBlock();
            addsep(s, os6); s = new Separator();
            addsep(s, os6); s = new Separator();
            addsep(s, os6); s = new Separator();
            t.Text = "Start " +
                SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].Start.test +
                        ": " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].Start.Value.ToString() +
                        " End " +
                SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].End.test +
                        ": " +
                        SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].End.Value.ToString();
            addtext(t, os6, 135); t = new TextBlock();
            //new tech output
            addtexthwb(t, is6t, 30, 90); t = new TextBlock();
            addtexthwb(t, is7t, 30, 90); t = new TextBlock();
            addtexthwb(t, is8t, 30, 90); t = new TextBlock();
            t.Text = "Result"+ (SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges.Count()).ToString()+":";
            addtexthw(t, is6t, 30, 90); t = new TextBlock();
            t2.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].Start.Value.ToString();
            addtextboxhwb(t2, is7t, 30, 90); t2 = new TextBox();
            t2.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.ResultRanges[0].End.Value.ToString();
            addtextboxhwb(t2, is8t, 30, 90); t2 = new TextBox();
            //new tech formulas
            addtexthwb(t, is6t, 30, 90); t = new TextBlock();
            addtexthwb(t, is7t, 30, 90); t = new TextBlock();
            addtexthwb(t, is8t, 30, 90); t = new TextBlock();
            //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.CMCUncertainties[0].Variables[0]);
            int fc = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties.Count();
            int gcc = fgrid.Children.Count; 
            fgrid.Children.RemoveRange(3, gcc - 3);
            sp1.Children.Clear();
            sp2.Children.Clear();
            sp3.Children.Clear();
            //new tech
            globe += 5;
            globe += it;
            for (int u=0;u<fc;u++)
            {
                bt.Content = "Edit";
                addbuttonhw(bt, sp1, 30, 100);bt = new Button();
                t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[u].Expression;//"Function" +(u+1).ToString();
                //addtexthw(t, sp2, 30, 150);t = new TextBlock();//old tech
                addtexthwb(t, is6t, 30, 240); t = new TextBlock();
                int fcv = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[u].ExpressionSymbols.Count();
                globe += fcv;
                addsepgrid(s, svgridt, globe*46); s = new Separator();
                for (int y=0;y<fcv;y++)
                {
                    if (y == 0)
                    {
                        t.Text = "VARIABLES";
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[u].ExpressionSymbols[y];
                        addtexthw(t, is7t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is8t, 30, 90); t = new TextBlock();
                    }
                        
                    else
                    {
                        t.Text = "";
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[u].ExpressionSymbols[y];
                        addtexthw(t, is7t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is8t, 30, 90); t = new TextBlock();
                        addtexthwb(t, is6t, 30, 90); t = new TextBlock();
                    }
                        
                    
                    addtexthw(t, sp1, 30, 150);t.TextAlignment =TextAlignment.Left;
                    t.Margin = new Thickness(12, 16, 0, 0);
                    t = new TextBlock();
                    t.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[u].ExpressionSymbols[y];
                    addtexthw(t, sp2, 30, 150); t = new TextBlock();
                }
                addsepgrid(s, fgrid, 46 + 46 *u* (1 + fcv)); s = new Separator();
                addsepgrid(s, fgrid,46+46*u*(1+fcv)+fcv*46);s = new Separator();
            }
            formula_def.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.CMCUncertainties[0].Expression;
            techdoctext.Text = SampleSOA.CapabilityScope.Activities[0].Techniques[tt].Technique.Documentation.Document;
            //documentation

            //new tech
            bt.Content = " " +
                    "INPUT PARAMETER RANGES    " +
                    "                MIN                       MAX                                                         ";
            bt.HorizontalAlignment = HorizontalAlignment.Left;
            bt.IsHitTestVisible = false;
            addtextboxgrid(bt, svgridt, 0); bt = new Button();
            bt.Content = " " +
                    "INPUT PARAMETERS          " +
                    "            QUANTITY                                                                             ";
            bt.HorizontalAlignment = HorizontalAlignment.Left;
            bt.IsHitTestVisible = false;
            addtextboxgrid(bt, svgridt,46+ 46*ip+0); bt = new Button();
            bt.Content = "                " +
                    "OUTPUTS                    " +
                    "               MIN                       MAX                                                        ";
            bt.HorizontalAlignment = HorizontalAlignment.Left;
            bt.IsHitTestVisible = false;
            addtextboxgrid(bt, svgridt, 92 + 46 * it + 0); bt = new Button();
            bt.Content = "                " +
                    "FORMULAS                    " +
                    "           VARIABLES         " +
                    "                                                                           ";
            bt.HorizontalAlignment = HorizontalAlignment.Left;
            bt.IsHitTestVisible = false;
            addtextboxgrid(bt, svgridt,92+ 92 + 46 * it + 0); bt = new Button();
            bt.Content = "                " +
                    "DOCUMENTATION                    " +
                    "                    " +
                    "                                                                           ";
            bt.HorizontalAlignment = HorizontalAlignment.Left;
            bt.IsHitTestVisible = false;
            addtextboxgrid(bt, svgridt,globe*46); bt = new Button();
        }
        private void set_func(int f)
        {
            //MessageBox.Show("fre");
        }
        private void slct_case(object sender, RoutedEventArgs e)
        {

            tabs.SelectedIndex=5;
            TreeViewItem childItem = e.Source as TreeViewItem;
            ItemsControl parentitems = GetSelectedTreeViewItemParent(childItem);
            //TreeViewItem parent = parentitems as TreeViewItem;
            //int i = parent.Items.IndexOf(childItem); MessageBox.Show(i.ToString());
            TreeViewItem parent = childItem.Parent as TreeViewItem;
            int i = parent.Items.IndexOf(childItem);// MessageBox.Show(i.ToString());
            int fl = parent.Name.Length;
            int f =Convert.ToInt32(parent.Name.Remove(0,1));//            (int)String.GetNumericValue(parent.ToolTip.ToString());
            set_cases(f,i);//MessageBox.Show(f.ToString()+ i.ToString());
        }
        private void set_cases(int f,int c)
        {
            //MessageBox.Show(f.ToString()+c.ToString());
            var bc = new BrushConverter();

            Button b = new Button();
            Separator s = new Separator();
            TextBox t2=new TextBox();
            TextBlock t = new TextBlock(); 
            CheckBox cb = new CheckBox();
            ToolTip tt = new ToolTip();
            DataTable dt = new DataTable();
            int fc=SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions.Count();
            int an= SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Assertions.Count();
            
            //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[0].Variable_name);
            range_tree.Items.Clear();
            TreeViewItem ft = new TreeViewItem();
            TreeViewItem ft2 = new TreeViewItem();
            int rn = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges.Count();
            //MessageBox.Show(rn.ToString());
            is6.Children.Clear(); int hgt = 0;
            is7.Children.Clear();
            is8.Children.Clear();
            is9.Children.Clear();
            int gcc=svgrid.Children.Count; //MessageBox.Show(gcc.ToString());
            
            svgrid.Children.RemoveRange(4, gcc-3);
            textBlock11.Text = "";
          //addtext(t, is6, 60); t = new TextBlock();
            if (an>1)
            {
                
                for (int u = 0; u < an; u++)
                {
                    textBlock11.Text = textBlock11.Text+ SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Assertions[u].Name +
                    " " + SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Assertions[u].Value + " ";


                }
                int ip = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[f].Variables.Count();// .Techniques[tt].Technique.ParameterRanges.Count();

                textboxes.Clear();
                for (int i = 0; i < rn; i++)
                {
                    
                    int rn2 = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges.Count();//inner ranges count
                    ft.Header = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Variable_name;
                    ft.Name = "f" + f.ToString() + c.ToString() + i.ToString();
                    
                    ft.Selected += set_upperrange_click;
                    for (int j = 0; j < rn2; j++)
                    {
                        t.Text = "MIN";
                        addtexthwb(t, is7, 30, 90); t = new TextBlock();
                        t.Text = "MAX"; 
                        addtexthwb(t, is8, 30, 90); t = new TextBlock();
                        t.Text = "RANGES";
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        addtexthw(t, is9, 30, 90); t = new TextBlock();
                        int cc = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues.Count();
                        data1 = new string[rn * rn2*ip];
                        if (j == 0 && i == 0)
                        {
                            int temp = is7.Children.Count;
                            hgt = Convert.ToInt32(temp * 30);
                        }
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Variable_type;
                        addtexthwb(t, is9, 30, 90); t = new TextBlock();
                        t.Text = "";
                        addtexthw(t, is9, 30, 90); t = new TextBlock();
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Variable_name;
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].Variable_name;
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        
                        b.Content = "   " +
                    "RANGES                         " +
                    "              MIN                     MAX                                             ";
                        b.HorizontalAlignment = HorizontalAlignment.Left;
                        b.IsHitTestVisible = false;
                        addtextboxgrid(b, svgrid, (46 * cc + 322 - 46 + 46 * ip) * (j + i * rn2) + 0); b = new Button();
                        
                        t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Start.ValueString;
                        addtextboxhw(t2, is7, 30, 60); t2 = new TextBox();
                        t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].End.ValueString;
                        addtextboxhw(t2, is8, 30, 60); t2 = new TextBox();
                        t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].Start.ValueString;
                        addtextboxhw(t2, is7, 30, 60); t2 = new TextBox();
                        t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].End.ValueString;
                        addtextboxhw(t2, is8, 30, 60); t2 = new TextBox();
                        
                        
                        b.Content = "             " +
                    "CONSTANTS                                                " +
                    "                                                                               ";
                        b.HorizontalAlignment = HorizontalAlignment.Left;
                        b.IsHitTestVisible = false;
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        addtexthw(t, is7, 30, 90); t = new TextBlock();
                        addtexthw(t, is8, 30, 90); t = new TextBlock();
                        addtexthw(t, is9, 30, 90); t = new TextBlock();
                        addtextboxgrid(b, svgrid, (46 * cc + 322 - 46 + 46 * ip) * (j + i * rn2) + 138); b = new Button();
                        for (int x = 0; x < cc; x++)
                        {
                            t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[x].const_parameter_name;
                            addtexthw(t, is6, 30, 90); t = new TextBlock();
                            if (x == 0)
                                t.Text = "CONTANTS";
                            else
                                t.Text = "";
                            addtexthwb(t, is9, 30, 90); t = new TextBlock();

                        }
                        for (int x = 0; x < cc; x++)
                        {
                            t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[x].ValueString;
                            addtextboxhw(t2, is7, 30, 60); t2 = new TextBox();
                            t2.Visibility = Visibility.Hidden;
                            addtextboxhw(t2, is8, 30, 60); t2 = new TextBox();

                        }
                        b.Content = "                " +
                    "FORMULA                    " +
                    "           VARIABLES         " +
                    "                                                                           ";
                        b.HorizontalAlignment = HorizontalAlignment.Left;
                        b.IsHitTestVisible = false;
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        addtexthw(t, is7, 30, 90); t = new TextBlock();
                        addtexthw(t, is8, 30, 90); t = new TextBlock();
                        addtexthw(t, is9, 30, 90); t = new TextBlock();
                        addtextboxgrid(b, svgrid, (46 * cc + 322 - 46 + 46 * ip) * (j + i * rn2) + 46 *cc+184); b = new Button();
                        for (int a = 0; a < ip; a++)
                        {

                            t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[a];
                            addtexthw(t, is7, 30, 90); t = new TextBlock();
                            t2.TextChanged += setdata1;// MessageBox.Show((a + j * ip + i * rn2 * ip).ToString());
                            t2.Name = "d" + (a + j * ip + i * rn2 * ip).ToString();
                            addtextboxhw(t2, is8, 30, 90); t2 = new TextBox();
                            if(a==0)
                            {
                                t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Expression;
                                
                            }
                            addtexthw(t, is6, 30, 241); t = new TextBlock();

                        }
                        
                        //addtextboxgrid(b, svgrid, (46 * cc + 322 + 46 * ip) *(j + i * rn2)+ 46 * cc + 230+46*ip); b = new Button();
                        t.Text = "FORMULA";
                        addtexthwb(t, is9, 30, 90); t = new TextBlock();
                        t.Text = "Result:";
                        addtexthwb(t, is7, 30, 90); t = new TextBlock();
                         
                        b.Name = "a"+f.ToString()+ "a" +c.ToString() + "a" + i.ToString() + "a" + j.ToString()+"a"+rn2.ToString();
                        b.Content = "CALCULATE";
                        b.Background = (Brush)bc.ConvertFrom("#d6fff6 ");
                        b.Foreground = (Brush)bc.ConvertFrom("#008b8b ");
                        
                        b.Click += calculateboxes;
                        addbuttonhw(b, is6, 30, 120); b = new Button();
                        textboxes.Add(t);
                        addtexthw(t, is8, 30, 100); t = new TextBlock();
                        
                         //ft and ft2 are related with function tab
                         ft2.Header = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].Variable_name;
                         ft.Items.Add(ft2);
                         ft2.Name = "f" + f.ToString() + c.ToString() + i.ToString() + j.ToString();
                         ft2 = new TreeViewItem();
                        //
                        addsepgrid(s, svgrid, (46 * cc + 322 - 46 + 46 * ip) * (j + i * rn2) + (5+ip+cc)* 46); s = new Separator();//put separators
                        /*addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j+i * rn2) + 92 + 46 * cc + 46); s = new Separator();
                        addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j+i * rn2) + 92 + ip * 46 + 46 * cc + 46); s = new Separator();
                        addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j + i * rn2) + 92 + ip * 46 + 46 + 46 * cc + 46); s = new Separator();
                        addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j+i * rn2) + 92 + ip * 46 +46+ 46 * cc+1 + 46); s = new Separator();
                        addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j + i * rn2) + 92 + ip * 46 + 46 + 46 * cc + 2 + 46); s = new Separator();
                        addsepgrid(s, svgrid, (92 + ip * 46 + 46 + 46 * cc + 46) * (j + i * rn2) + 92 + ip * 46 + 46 + 46 * cc+3 + 46); s = new Separator();*/
                    }
                    range_tree.Items.Add(ft);
                     ft = new TreeViewItem();
                }

            }
            else
            {
                textBlock11.Text = "";
                int rn2 = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges.Count();//inner ranges count
                textboxes.Clear();
                int tf=0;
                while(SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].name!= SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[tf].function_name)
                {
                    tf++;
                }
                int ip = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[tf].Variables.Count();// .Techniques[tt].Technique.ParameterRanges.Count();

                for (int j = 0; j < rn2; j++)
                {
                    t.Text = "MIN";
                    addtexthwb(t, is7, 30, 90); t = new TextBlock();
                    t.Text = "MAX";
                    addtexthwb(t, is8, 30, 90); t = new TextBlock();
                    addtexthw(t, is6, 30, 90); t = new TextBlock();
                    addtexthw(t, is9, 30, 90); t = new TextBlock();
                    int cc = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues.Count();
                    data1 = new string[ rn2];
                    if (j == 0 )
                    {
                        int temp = is7.Children.Count;
                        hgt = Convert.ToInt32(temp * 30);
                    }
                    t.Text = "RANGES";
                    addtexthwb(t, is9, 30, 90); t = new TextBlock();
                    t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].Variable_name;
                    addtexthw(t, is6, 30, 90); t = new TextBlock();
                    b.Content = "   " +
                    "RANGES                         " +
                    "              MIN                     MAX                                             ";
                    b.HorizontalAlignment = HorizontalAlignment.Left;
                    b.IsHitTestVisible = false;
                    addtextboxgrid(b, svgrid, (46 * cc + 230 + 46 * ip) * (j ) + 0); b = new Button();
                    t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].Start.ValueString;
                    addtextboxhw(t2, is7, 30, 60); t2 = new TextBox();
                    t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].End.ValueString;
                    addtextboxhw(t2, is8, 30, 60); t2 = new TextBox();
                    b.Content = "             " +
                    "CONSTANTS                                                " +
                    "                                                                               ";
                    b.HorizontalAlignment = HorizontalAlignment.Left;
                    b.IsHitTestVisible = false;
                    addtexthw(t, is6, 30, 90); t = new TextBlock();
                    addtexthw(t, is7, 30, 90); t = new TextBlock();
                    addtexthw(t, is8, 30, 90); t = new TextBlock();
                    addtexthw(t, is9, 30, 90); t = new TextBlock();
                    addtextboxgrid(b, svgrid, (46 * cc + 230 + 46 * ip) * (j ) + 92); b = new Button();
                    for (int x=0;x<cc;x++)
                    {
                        t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues[x].const_parameter_name;
                        addtexthw(t, is6, 30, 90); t = new TextBlock();
                        if (x == 0)
                            t.Text = "CONTANTS";
                        else
                            t.Text = "";
                        addtexthwb(t, is9, 30, 90); t = new TextBlock();
                    }
                   


                    
                    
                    for (int x = 0; x < cc; x++)
                    {
                        t2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues[x].ValueString;
                        addtextboxhw(t2, is7, 30, 60); t2 = new TextBox();
                        t2.Visibility = Visibility.Hidden;
                        addtextboxhw(t2, is8, 30, 60); t2 = new TextBox();
                        
                    }
                    b.Content = "                " +
                    "FORMULA                    " +
                    "           VARIABLES         " +
                    "                                                                           ";
                    b.HorizontalAlignment = HorizontalAlignment.Left;
                    b.IsHitTestVisible = false;
                    addtexthw(t, is6, 30, 90); t = new TextBlock();
                    addtexthw(t, is7, 30, 90); t = new TextBlock();
                    addtexthw(t, is8, 30, 90); t = new TextBlock();
                    addtexthw(t, is9, 30, 90); t = new TextBlock();
                    addtextboxgrid(b, svgrid, (46 * cc + 230 + 46 * ip) * (j ) + 46 * cc + 138); b = new Button();
                    for (int a = 0; a < ip; a++)
                    {

                        t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[tf].Variables[a];
                        addtexthw(t, is7, 30, 90); t = new TextBlock();
                        t2.TextChanged += setdata1;
                        t2.Name = "d" + (a + j * ip + 0 * rn2 * ip).ToString();
                        addtextboxhw(t2, is9, 30, 90); t2 = new TextBox();
                        addtexthw(t, is8, 30, 241); t = new TextBlock();
                        if (a == 0)
                        {
                            t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Expression;

                        }
                        addtexthw(t, is6, 30, 241); t = new TextBlock();
                    }
                    t.Text = "FORMULA";
                    addtexthwb(t, is9, 30, 90); t = new TextBlock();
                    t.Text = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[tf].Expression;
                    t.Margin = new Thickness(-23, 0, 0, 0);
                    addtexthw(t, is6, 30, 241); t = new TextBlock();
                    
                    b.Name = "a" + f.ToString() + "a" + c.ToString() + "a" + j.ToString() + "a" + rn2.ToString()+"a"+tf.ToString();

                    b.Content = "CALCULATE";
                    b.Background = (Brush)bc.ConvertFrom("#d6fff6 ");
                    b.Foreground = (Brush)bc.ConvertFrom("#008b8b ");

                    b.Click += calculateboxes2;
                    addbuttonhw(b, is7, 30, 120); b = new Button();
                    textboxes.Add(t);
                    addtexthw(t, is8, 30, 100); t = new TextBlock();
                    
                    
                    addsepgrid(s, svgrid, (46 * cc + 230 + 46 * ip) * (j) + 46 * cc + 230); s = new Separator();//put separators
                                                                                                             /* addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 46 + 46 * cc + 46); s = new Separator();
                                                                                                              addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 46 + ip * 46 + 46 * cc + 46); s = new Separator();
                                                                                                              addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 92 + ip * 46 + 46 * cc + 46); s = new Separator();

                                                                                                              addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 92 + ip * 46 + 46 * cc+1 + 46); s = new Separator();
                                                                                                              addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 92 + ip * 46 + 46 * cc+2 + 46); s = new Separator();
                                                                                                              addsepgrid(s, svgrid, (92 + ip * 46 + 46 * cc + 46) * (j) + 92 + ip * 46 + 46 * cc+3 + 46); s = new Separator();*/
                }
            }
                        /* */
                        //f_tree.Items.Add
                        //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[0]);
                        //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[1]);
        }
        private void set_upperrange_click(object sender, RoutedEventArgs e)
        {
            TreeViewItem childItem = e.Source as TreeViewItem;
            int o = (int)Char.GetNumericValue(childItem.Name[3]);
            int c = (int)Char.GetNumericValue(childItem.Name[2]);
            int f = (int)Char.GetNumericValue(childItem.Name[1]);
            //MessageBox.Show(f.ToString() + c.ToString() + o.ToString());
            if (childItem.HasItems)
            {
                set_upperrange(f, c, o);
                cv1.Visibility = Visibility.Hidden;
                cv2.Visibility = Visibility.Hidden;
                cv1tb.Visibility = Visibility.Hidden;
                cv2tb.Visibility = Visibility.Hidden;
            }
               
            else
            {
                int o2 = (int)Char.GetNumericValue(childItem.Name[4]);
                set_lowerrange(f, c, o, o2);
                cv1.Visibility = Visibility.Visible;
                cv2.Visibility = Visibility.Visible;
                cv1tb.Visibility = Visibility.Visible;
                cv2tb.Visibility = Visibility.Visible;
            }
  
        }
        private void set_upperrange(int t,int c,int o)
        {
            starttb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Start.ValueString;
            endtb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].End.ValueString;
            
        }
        private void set_lowerrange(int t, int c, int o, int o2)
        {

            starttb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].Start.ValueString;
            endtb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].End.ValueString;
            cv1.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].ConstantValues[0].const_parameter_name;
            cv2.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].ConstantValues[1].const_parameter_name;
            cv1tb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].ConstantValues[0].ValueString;
            cv2tb.Text = SampleSOA.CapabilityScope.Activities[0].Templates[t].CMCUncertaintyFunctions[0].Cases[c].Ranges[o].Ranges[o2].ConstantValues[1].ValueString;
        }
        
        private void setdata2(object sender, RoutedEventArgs e)
        {
            TextBox x = e.Source as TextBox;
            stringBuilder.Replace("{Company}", x.Text);
            data2[1] = x.Text;
        }
        private void setdata1(object sender, RoutedEventArgs e)
        {
            TextBox x = e.Source as TextBox;
            string parse = x.Name.Remove(0, 1);
            int a = Int32.Parse(parse);
            data1[a] = x.Text;
        }
        private void calculateboxes(object sender, RoutedEventArgs e)
        {
            double num;
            int candidate = 1;
            Button x = e.Source as Button;
            string parse = x.Name.Remove(0, 1);
            string[] tokens = parse.Split('a');
            int f = Int32.Parse(tokens[0]);
            int c = Int32.Parse(tokens[1]);
            int i = Int32.Parse(tokens[2]);
            int j = Int32.Parse(tokens[3]);
            int rn2 = Int32.Parse(tokens[4]);
            string temp = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Expression;
            int cc = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues.Count();
            int ic = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Constants.Count();
            int ip = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables.Count();
            for (int q = 0; q < ic; q++)
            {
                for(int p=0;p<cc;p++)
                {
                    //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Constants[q]);
                    //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[p].ValueString);
                    if (SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Constants[q]== SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[p].const_parameter_name)
                    {
                        
                        temp = temp.Replace(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Constants[q],
                SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[p].ValueString);
                    }
                }
                

            }
            for (int q=0;q<ip;q++)
            {
                //MessageBox.Show((q + j * ip + i * rn2 * ip).ToString());
                //MessageBox.Show(data1[q + j * ip + i * rn2 * ip]);
                if (double.TryParse(data1[q + j * ip + i * rn2 * ip],NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out num))
                {
                    // It's a number!
                    //if(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[q])
                    temp = temp.Replace(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[q],
                        data1[q + j * ip + i * rn2 * ip]);
                }
                else
                {
                    MessageBox.Show("Please enter a valid "+ SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Variables[q] + " number!");
                    candidate = 0;
                }
            }
            
            DataTable dt = new DataTable();
            if (candidate == 1)
            {
                textboxes[i * rn2 + j].Text = dt.Compute(temp, "").ToString();
                textboxes[i * rn2 + j].FontWeight = FontWeights.Bold;
            }
                
            else
            {
                x.Content = "CALCULATE";
            }
                
        }
        private void calculateboxes2(object sender, RoutedEventArgs e)
        {
            double num;
            int candidate = 1;
            Button x = e.Source as Button;
            string parse = x.Name.Remove(0, 1);
            string[] tokens = parse.Split('a');
            int f = Int32.Parse(tokens[0]);
            int c = Int32.Parse(tokens[1]);
            int j = Int32.Parse(tokens[2]);
            int rn2 = Int32.Parse(tokens[3]);
            int ft = Int32.Parse(tokens[4]);
            string temp = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Expression;
            int cc = SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues.Count();
            int ic = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Constants.Count();
            int ip = SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Variables.Count();
            for (int q = 0; q < ic; q++)
            {
                for (int p = 0; p < cc; p++)
                {
                    //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[0].Constants[q]);
                    //MessageBox.Show(SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[i].Ranges[j].ConstantValues[p].ValueString);
                    if (SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Constants[q] == SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues[p].const_parameter_name)
                    {

                        temp = temp.Replace(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Constants[q],
                SampleSOA.CapabilityScope.Activities[0].Templates[f].CMCUncertaintyFunctions[0].Cases[c].Ranges[j].ConstantValues[p].ValueString);
                    }
                }


            }
            for (int q = 0; q < ip; q++)
            {
                if (double.TryParse(data1[q + j * ip ], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out num))
                {
                    // It's a number!
                    temp = temp.Replace(SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Variables[q],
                        data1[q + j * ip ]);
                }
                else
                {
                    MessageBox.Show("Please enter a valid " + SampleSOA.CapabilityScope.Activities[0].Templates[f].MtcTechnique.CMCUncertainties[ft].Variables[q] + " number!");
                    candidate = 0;
                }
            }

            DataTable dt = new DataTable();
            if (candidate == 1)
            {
                textboxes[j].Text = dt.Compute(temp, "").ToString();
                textboxes[j].FontWeight = FontWeights.Bold;
            }
                
            else
            {
                
            }

        }
        private void addsepgrid(Separator s, Grid g, int m)
        {
            var bc = new BrushConverter();
            s.Width = 700;
            s.Height = 5;
            s.Background = (Brush)bc.ConvertFrom("#008b8b ");
            s.VerticalAlignment = VerticalAlignment.Top;
            s.HorizontalAlignment = HorizontalAlignment.Left;
            s.Margin = new Thickness(0, m, 0, 0);
            g.Children.Add(s);

        }
        private void addtextboxgrid(Button s, Grid g, int m)
        {
            
           // s.TextAlignment = TextAlignment.Justify;
            s.FontWeight = FontWeights.Bold;
            var bc = new BrushConverter();
            s.Width = 700;
            s.Height = 46;
            s.Background = (Brush)bc.ConvertFrom("#008b8b ");
            s.Foreground = (Brush)bc.ConvertFrom("#e0ffff ");
            s.VerticalAlignment = VerticalAlignment.Top;
            s.HorizontalAlignment = HorizontalAlignment.Left;
            s.Margin = new Thickness(0, m, 0, 0);
            g.Children.Add(s);

        }
        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }
        private void add_tab(object sender, RoutedEventArgs e)
        {
            int count = tabs.Items.Count;

            TabItem tab = new TabItem();
            tab.Header = string.Format("Tab {0}", count);
            tab.Name = string.Format("tab{0}", count);

            TextBox txt = new TextBox();
            txt.Name = "txt";
            tab.Content = txt;

            tabs.Items.Insert(count, tab);
        }
        private void slct_info(object sender, RoutedEventArgs e)
        {
            
            TreeViewItem childItem = e.Source as TreeViewItem;
            /*
            if (cmp_info.IsSelected)
            {
                MyPopup.IsOpen = true;
                cmp_info.IsSelected = false;
            }*/
            if(childItem!=null)    
            childItem.IsSelected = false;
            tabs.SelectedIndex = 1;
            
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            formula_popup.IsOpen = true;
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            formula_popup.IsOpen = false;
        }
        private void DoSomething(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MessageBox.Show("Do Something!");
            
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string h = combo2.Text + ".";// + fill.Text;
               // source_voltage.Text = h;
            }
        }
        private void comboselection(object sender, SelectionChangedEventArgs e)
        {

            /*
            ComboBox c_item = new ComboBox();
            ComboBoxItem cb_item = new ComboBoxItem();
            TextBlock text_item = new TextBlock();
            Button button_item = new Button();
            string h = combo2.Text + ".";// + fill.Text;
            //source_voltage.Text = h;

            int s_index = combo2.SelectedIndex;
            XmlNode s_node = db.GetElementsByTagName("mtc:ProcessType")[s_index];
            //MessageBox.Show("rslt cnt:" + db.GetElementsByTagName("mtc:ProcessType")[1].Name);
            if(s_node.HasChildNodes)
            {
                // clear listviews
                //listview1.Items.Clear();
                //listview2.Items.Clear();
                listview3.Items.Clear();
                listview4.Items.Clear();
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
                            c_item.Name = "Result";
                        //c_item.Name = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        cb_item = new ComboBoxItem();
                        cb_item.Content = s_node.ChildNodes[i].FirstChild.Attributes["name"].Value;
                        c_item.Items.Add(cb_item);
                        cb_item.IsSelected = true;
                        //c_item.Height = 10;
                        //listview1.Items.Add(c_item);
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
                        //c_item.Height = 20;
                        cb_item.IsSelected = true;
                        //listview2.Items.Add(c_item);
                        //
                        text_item = new TextBlock();
                        text_item.Name = s_node.ChildNodes[i].Attributes["name"].Value;
                        text_item.Text = "Start at:\n"+"End before:";
                        //text_item.mo.AddHandler(RoutedEventArgs, MouseDoubleClickEvent);
                        listview3.Items.Add(text_item);
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

            tec_pro.Text = s_node.Attributes["name"].Value;
            //MessageBox.Show("rslt cnt:"+result_cnt.ToString());
            //ComboBoxItem childItem = e as ComboBoxItem;*/
            //*
            
            

        }
        private void addsep(Separator s,StackPanel p)
        {
            s.Height = 4;
            s.Opacity = 0;
            p.Children.Add(s);
            
        }
        private void addseph(Separator s, StackPanel p, int h)
        {
            s.Height = h;
            s.Opacity = 0;
            p.Children.Add(s);
        }
        private void addtext(TextBlock t, StackPanel p,int w)
        {
            //t.VerticalAlignment = VerticalAlignment.Stretch;
            t.FontWeight=FontWeights.Bold;
            t.Height = 30;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtextbox(TextBox t, StackPanel p, int w)
        {
            t.FontWeight = FontWeights.Bold;
            t.Height = 30;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtextf1(TextBlock t, StackPanel p, int w)
        {
            //t.FontFamily = FontFamily.FamilyName;
            t.Height = 30;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtexthw(TextBlock t, StackPanel p, int h, int w)
        {
            t.Margin = new Thickness(0, 16, 0, 0);
            t.TextAlignment = TextAlignment.Center;
            
            t.Height = h;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtexthwb(TextBlock t, StackPanel p, int h, int w)
        {
            t.Margin = new Thickness(0, 16, 0, 0);
            t.TextAlignment = TextAlignment.Center;
            t.FontWeight = FontWeights.Bold;
            t.Height = h;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtextboxhw(TextBox t, StackPanel p,int h, int w)
        {
            t.TextAlignment = TextAlignment.Center;
            t.Height = h;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addtextboxhwb(TextBox t, StackPanel p, int h, int w)
        {
            t.TextAlignment = TextAlignment.Center;
            t.FontWeight = FontWeights.Bold;
            t.Height = h;
            t.Width = w;
            p.Children.Add(t);

        }
        private void addbuttonhw(Button b, StackPanel p, int h, int w)
        {
            b.Margin = new Thickness(0, 12, 0, 4);
            b.Height = h;
            b.Width = w;
            p.Children.Add(b);

        }
        private void addtextf1d(TextBlock t, StackPanel p, int w)
        {
            //t.FontFamily = FontFamily.FamilyName;
            t.Height = 30;
            t.Width = w;
            p.Children.Add(t);

        }
        //add combobox to the stackpanel
        private void addcombo(ComboBox c, StackPanel p)
        {
            c.Height = 30;
            p.Children.Add(c);
        }
        //add checkbox to the stackpanel
        private void addcheck(CheckBox b, StackPanel p)
        {
            b.Height = 30;
            p.Children.Add(b);
        }
        //add expander node
        private void add_exp(object sender, RoutedEventArgs e)
        {
            Expander x = new Expander();
            x.Name = "case7";
        }
        private void setTechName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {

            }
        }
        public class CloseableTabItem : TabItem
        {
            

            static CloseableTabItem()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem),
                    new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
            }
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button closeButton = base.GetTemplateChild("PART_Close") as Button;
            if (closeButton != null)
                closeButton.Click += new System.Windows.RoutedEventHandler(closeButton_Click);
        }

        void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }
        private void change_colorcyan(object sender, RoutedEventArgs e)
        {
            mi2.IsChecked = false;
            MenuItem item = e.Source as MenuItem;
            item.IsChecked = true;
            var bc = new BrushConverter();

            menu1.Background = (Brush)bc.ConvertFrom("#008b8b");
            //borders
            treegrid.BorderBrush = (Brush)bc.ConvertFrom("#008b8b");
            treegrid.Background = (Brush)bc.ConvertFrom("#e0ffff");
            tvMain.Background = (Brush)bc.ConvertFrom("#e0ffff");
            sep1.Background = (Brush)bc.ConvertFrom("#008b8b");
            sep2.Background = (Brush)bc.ConvertFrom("#008b8b");
            sep3.Background = (Brush)bc.ConvertFrom("#008b8b");
            //textbox
            metrologyt.Foreground = (Brush)bc.ConvertFrom("#008b8b");
            powert.Foreground = (Brush)bc.ConvertFrom("#008b8b"); 
            textBlock.Foreground = (Brush)bc.ConvertFrom("#008b8b");
            //grids
            welcomegrid.Background= (Brush)bc.ConvertFrom("#e0ffff");
            editor.Background = (Brush)bc.ConvertFrom("#e0ffff");
            dock1.Background= (Brush)bc.ConvertFrom("#e0ffff");
            LayoutRoot.Background = (Brush)bc.ConvertFrom("#e0ffff");
            //menuitems
            file.Foreground = (Brush)bc.ConvertFrom("#e0ffff");
            edit.Foreground = (Brush)bc.ConvertFrom("#e0ffff");
            help.Foreground = (Brush)bc.ConvertFrom("#e0ffff");
            //separators
            split1.Background = (Brush)bc.ConvertFrom("#e0ffff");
        }
        private void change_colorblue(object sender, RoutedEventArgs e)
        {
            mi1.IsChecked = false;
            MenuItem item = e.Source as MenuItem;
            item.IsChecked = true;
            var bc = new BrushConverter();

            menu1.Background = (Brush)bc.ConvertFrom("#225a88");
            //borders
            treegrid.BorderBrush = (Brush)bc.ConvertFrom("#225a88");
            treegrid.Background = (Brush)bc.ConvertFrom("#ddefff");
            tvMain.Background = (Brush)bc.ConvertFrom("#ddefff");
            sep1.Background = (Brush)bc.ConvertFrom("#225a88");
            sep2.Background = (Brush)bc.ConvertFrom("#225a88");
            sep3.Background = (Brush)bc.ConvertFrom("#225a88");
            //textbox
            metrologyt.Foreground = (Brush)bc.ConvertFrom("#225a88");
            powert.Foreground = (Brush)bc.ConvertFrom("#225a88");
            textBlock.Foreground = (Brush)bc.ConvertFrom("#225a88");
            //grids
            welcomegrid.Background = (Brush)bc.ConvertFrom("#ddefff");
            editor.Background = (Brush)bc.ConvertFrom("#ddefff");
            dock1.Background = (Brush)bc.ConvertFrom("#ddefff");
            LayoutRoot.Background = (Brush)bc.ConvertFrom("#ddefff");
            //menuitems
            file.Foreground = (Brush)bc.ConvertFrom("#ddefff");
            edit.Foreground = (Brush)bc.ConvertFrom("#ddefff");
            help.Foreground = (Brush)bc.ConvertFrom("#ddefff");
            //separators
            split1.Background = (Brush)bc.ConvertFrom("#ddefff");
            
        }

    }
}
