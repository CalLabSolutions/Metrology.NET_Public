using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Xml;

namespace soa_1_03
{
    public sealed class UoM : Inpc
    {
        private XmlDocument uomDb = new XmlDocument();
        private XmlNamespaceManager nsMgr;
        private string rootNode = "/a:UOMDatabase/a:Dimensions/a:Dimension/a:UOMs/a:UOM";
        public List<basenames> _lBasenames = new List<basenames>();

        //Private constructor
        private UoM()
        {
            loadUom();
            nsMgr = new XmlNamespaceManager(uomDb.NameTable);
            nsMgr.AddNamespace("a", "http://schema.metrology.net/UnitsOfMeasure");
            popUom();
        }

        #region Singleton
        //Private static instance of the same class
        private static readonly UoM instance = null;

        static UoM()
        {
            instance = new UoM();
        }

        public static UoM GetInstance()
        {
            //return existing instance
            return instance;
        }
        #endregion

        #region public properties

        #endregion

        #region Methods
        private string loadUom()
        {
            string exePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            string dataPath = exePath.Replace(@"\bin\Debug", @"\data files\UOM_Database.xml");
            uomDb.Load(dataPath);
            return dataPath;
        }

        private void popUom()
        {
            XmlNodeList baseNodes = uomDb.SelectNodes(rootNode, nsMgr);

            foreach (XmlNode node in baseNodes)
            {
                basenames tempBasename = new basenames();
                tempBasename.name = (node.Attributes["base_name"].Value);
                tempBasename.symbol = (node.Attributes["symbol"].Value);

                string nodeString = "a:Quantities/a:Quantity";
                tempBasename.quantities = getNodeValues(node, nodeString, "name", nsMgr);

                nodeString = "a:Aliases/a:Alias";
                tempBasename.aliases = getNodeValues(node, nodeString, "symbol", nsMgr);

                tempBasename.alts = GetAltValues(node, nsMgr);

                _lBasenames.Add(tempBasename);
            }
        }

        private static List<alternatives> GetAltValues(XmlNode rootNode, XmlNamespaceManager nsMgr)
        {
            List<alternatives> altValues = new List<alternatives>();
            List<string> tempList = new List<string>();

            XmlNodeList nodes = rootNode.SelectNodes("a:Alternatives/a:Alternative", nsMgr);
            foreach (XmlNode node in nodes)
            {
                alternatives alt = new alternatives();
                alt.altName = node.Attributes["name"].Value;
                alt.altSymbol = node.Attributes["symbol"].Value;
                string nodeString = "a:Aliases/a:Alias";
                alt.altAliases = getNodeValues(node, nodeString, "symbol", nsMgr);
                altValues.Add(alt);
            }

            return altValues;
        }

        private static List<string> getNodeValues(XmlNode rootNode, string nodeString, string attribute, XmlNamespaceManager nsMgr)
        {
            List<string> nodeValues = new List<string>();

            XmlNodeList nodes = rootNode.SelectNodes(nodeString, nsMgr);
            foreach (XmlNode node in nodes)
            {
                nodeValues.Add(node.Attributes[attribute].Value);
            }

            return nodeValues;
        }
        #endregion
    }

    #region Sub Classes
    public class basenames
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public List<string> quantities { get; set; }
        public List<string> aliases { get; set; }
        public List<alternatives> alts { get; set; }

        public basenames()
        {
            this.quantities = new List<string>();
            this.aliases = new List<string>();
            this.alts = new List<alternatives>();
        }
    } 

    public class alternatives
    {
        public string altName { get; set; }
        public string altSymbol { get; set; }
        public List<string> altAliases { get; set; }

        public alternatives()
        {
            this.altAliases = new List<string>();
        }
    }
    #endregion
}
