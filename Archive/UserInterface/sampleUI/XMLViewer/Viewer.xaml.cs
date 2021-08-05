using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace XMLViewer
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        private XmlDocument _xmldocument;
        public Viewer()
        {
            InitializeComponent();
        }

        public XmlDocument xmlDocument
        {
            get { return _xmldocument; }
            set
            {
                _xmldocument = value;
                BindXMLDocument();
            }
        }

        private void BindXMLDocument()
        {
            if (_xmldocument == null)
            {
                xmlTree.ItemsSource = null;
                return;
            }

            XmlDataProvider provider = new XmlDataProvider();
            provider.Document = _xmldocument;
            Binding binding = new Binding();
            binding.Source = provider;
            binding.XPath = "child::node()";
            xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = _xmldocument.CreateElement("DENEME");
            _xmldocument.GetElementsByTagName("soa:AB_ID")[0].InnerText="0102030405";
            //XmlNodeList nodelist = _xmldocument.SelectNodes("/soa:SOADataMaster/soa:AB_ID");
            //nodelist[0].InnerText = "joojop";
            //_xmldocument.SelectSingleNode("/soa:SOADataMaster/soa:AB_ID").InnerText = "NewValue";
            //_xmldocument.AppendChild(rootNode);
            _xmldocument.Save("test-doc.xml");
        }
    }
}
