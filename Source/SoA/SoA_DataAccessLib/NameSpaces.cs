using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOA_DataAccessLib
{
    public class NameSpaces
    {
        private Dictionary<string, string> map = new Dictionary<string, string>(){
            {"soa", @"https://cls-schemas.s3.us-west-1.amazonaws.com/SOA_Master_DataFile"},
            {"xsi", @"http://www.w3.org/2001/XMLSchema-instance"},
            {"xi", @"http://www.w3.org/2001/XInclude"},
            {"uom", @"http://schema.metrology.net/UOM_Database"},
            {"unc", @"https://cls-schemas.s3.us-west-1.amazonaws.com/Uncertainty"},
            {"mml", @"http://www.w3.org/1998/Math/MathML"},
            {"xhtml", @"http://www.w3.org/1999/xhtml"},
            {"mtc", @"https://cls-schemas.s3.us-west-1.amazonaws.com/MetrologyTaxonomyCatalog"}
        };

        public string this[string key]
        {
            get { return map[key]; }
        }

        public string key(string value)
        {
            return map.FirstOrDefault(x => x.Value == value).Key;
        }

        public XAttribute[] NameSpaceDeclarations
        {
            get
            {
                List<XAttribute> list = new List<XAttribute>();
                foreach (string key in map.Keys)
                {
                    XNamespace ns = XNamespace.Get(map[key]);
                    /*
                     * list.Add(new XAttribute(key, ns));
                     * We need to identify this as a namespace (XNamespace.Xmlns + key, ns) 
                     * or the tag prefixes will not get appended where needed.  Which in turn
                     * makes the xml file unable to be deserialized back into an object
                     */
                    list.Add(new XAttribute(XNamespace.Xmlns + key, ns));
                }
                // Add the xsi:schemalocation
                XNamespace xsi = XNamespace.Get(map["xsi"]);
                XNamespace schemaLocation = XNamespace.Get(map["soa"] + " " + map["soa"] + ".xsd");
                list.Add(new XAttribute(xsi + "schemaLocation", schemaLocation));
                return list.ToArray();
            }
        }
    }

    static class Configuration
    {
        private static NameSpaces nameSpaces = new NameSpaces();

        public static NameSpaces NameSpaces
        {
            get { return Configuration.nameSpaces; }
        }

        public static String UomDatabaseURL
        {
            get { return "http://testsite2.callabsolutions.com/UnitsOfMeasure/UOM_database.xml"; }
        }

        public static string NamespaceKey(string value)
        {
            return Configuration.nameSpaces.key(value);
        }
    }


    public class XmlNameSpaceElement
    {
        private XNamespace ns;
        private XElement element;

        public XElement Element
        {
            get { return element; }
        }

        public XmlNameSpaceElement addChild(string name)
        {
            XmlNameSpaceElement child = new XmlNameSpaceElement(name, this.ns);
            return addElement(child);
        }

        private XmlNameSpaceElement addElement(XmlNameSpaceElement child)
        {
            element.Add(child.element);
            return child;
        }

        public XmlNameSpaceElement addAttribute(string name)
        {
            if (element.Attribute(name) == null)
            {
                XAttribute attribute = new XAttribute(name, "");
                element.Add(attribute);
            }
            return this;
        }

        public XmlNameSpaceElement setAttribute(string name, string value)
        {
            if (element.Attribute(name) != null)
            {
                element.Attribute(name).Value = value;
            }
            else
            {
                XAttribute attribute = new XAttribute(name, value);
                element.Add(attribute);
            }
            return this;
        }

        public XmlNameSpaceElement addAttributes(params string[] names)
        {
            foreach (string name in names)
            {
                addAttribute(name);
            }
            return this;
        }

        public string getAttribute(string attributeName)
        {
            string value = "";
            var atr = element.Attribute(attributeName);
            value = (atr != null) ? atr.Value : "";
            return value;
        }

        public String Value
        {
            get { return Element.Value; }
            set { Element.Value = value; }
        }

        public XmlNameSpaceElement setValue(string Value)
        {
            this.Value = Value;
            return this;
        }

        public XmlNameSpaceElement(string name, XNamespace ns)
        {
            this.ns = ns;
            this.element = new XElement(ns + name);
        }

        public XmlNameSpaceElement(XElement Element)
        {
            this.ns = Element.GetDefaultNamespace();
            this.element = Element;
        }
    }

    public class XmlNameSpaceHelper
    {
        private XNamespace ns;

        private XContainer datasource = null;

        public XNamespace Ns
        {
            get { return ns; }
        }

        public XContainer Datasource
        {
            get { return datasource; }
        }

        public IEnumerable<XElement> getElements(string elementName)
        {
            return datasource.Elements(ns + elementName);
        }

        public XElement getElement(string elementName)
        {
            var els = getElements(elementName);
            return (els.Count() > 0) ? els.First() : null;
        }

        public XElement getElement()
        {
            return (datasource is XElement) ? (datasource as XElement) : null;
        }

        public XmlNameSpaceHelper getNameSpaceHelper(string elementName)
        {
            var element = getElement(elementName);
            if (element != null)
            {
                var ns = element.GetDefaultNamespace();
                return new XmlNameSpaceHelper(element, ns.NamespaceName);
            }
            else return null;
        }

        public String Value
        {
            get { return getValue(); }
            set { setValue(value); }
        }

        public string getValue()
        {
            return (datasource is XElement) ? (datasource as XElement).Value : "";
        }

        public XmlNameSpaceHelper setValue(string value)
        {
            if (datasource is XElement)
            {
                (datasource as XElement).Value = value;
            }
            return this;
        }

        public string getValue(string elementName)
        {
            XElement element = getElement(elementName);
            return (element != null) ? element.Value : "";
        }

        public XmlNameSpaceHelper setValue(string elementName, string value)
        {
            var helper = getNameSpaceHelper(elementName);
            if (helper != null)
            {
                return helper.setValue(value);
            }
            else return null;
        }

        public List<string> getValues(string elementName)
        {
            List<string> collection = new List<string>();
            var els = getElements(elementName);
            foreach (XElement el in els)
            {
                collection.Add(el.Value);
            }
            return collection;
        }

        public string getAttribute(string attributeName)
        {
            string value = "";
            if (datasource is XElement)
            {
                var atr = (datasource as XElement).Attribute(attributeName);
                value = (atr != null) ? atr.Value : "";
            }
            return value;
        }

        public XmlNameSpaceElement addChild(string name)
        {
            XmlNameSpaceElement child = new XmlNameSpaceElement(name, ns);
            if (datasource != null)
            {
                if (datasource is XElement) (datasource as XElement).Add(child.Element);
                if (datasource is XDocument) (datasource as XDocument).Add(child.Element);
            }
            return child;
        }

        protected XmlNameSpaceHelper() { }

        public XmlNameSpaceHelper(XContainer datasource, string namespaceValue)
        {
            this.datasource = datasource;
            ns = XNamespace.Get(namespaceValue);
        }
    }

    public class UomSpaceHelper : XmlNameSpaceHelper
    {
        private UomSpaceHelper() { }

        public UomSpaceHelper(XElement datasource)
            : base(datasource, Configuration.NameSpaces["uom"]) { }
    }

    public class MtcSpaceHelper : XmlNameSpaceHelper
    {
        private MtcSpaceHelper() { }

        public MtcSpaceHelper(XElement datasource)
            : base(datasource, Configuration.NameSpaces["mtc"]) { }
    }

    public class UncSpaceHelper : XmlNameSpaceHelper
    {
        private UncSpaceHelper() { }

        public UncSpaceHelper(XElement datasource)
            : base(datasource, Configuration.NameSpaces["unc"]) { }
    }

    public class XiSpaceHelper : XmlNameSpaceHelper
    {
        private XiSpaceHelper() { }

        public XiSpaceHelper(XElement datasource)
            : base(datasource, Configuration.NameSpaces["xi"]) { }
    }

    public class SoaSpaceHelper : XmlNameSpaceHelper
    {
        private SoaSpaceHelper() { }

        public SoaSpaceHelper(XContainer datasource)
            : base(datasource, Configuration.NameSpaces["soa"]) { }

    }
}
