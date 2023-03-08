using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace MT_DataAccessLib
{
    public class XMLDataSource
    {
        private XDocument doc = null;

        public XDocument Doc
        {
            get { return doc; }
            set { doc = value; }
        }

        /// <summary>
        ///     opens the xml document.
        /// </summary>
        /// <param name="source">
        ///     a valid Uri or file path to a .xml file, or an XML string
        /// </param>
        /// <returns>success or failure status</returns>
        public OpResult load(string source)
        {
            const int URI_MAX_LENGTH = 2083;
            OpResult opResult = new OpResult();
            try
            {
                if (source.Length < URI_MAX_LENGTH && Uri.IsWellFormedUriString(Uri.EscapeUriString(source), UriKind.Absolute))
                {
                    string uri_string = Uri.EscapeUriString(source);
                    WebRequest request = WebRequest.Create(uri_string);
                    request.Timeout = 1000 * 60; // 1 minute;
                    request.UseDefaultCredentials = true;
                    request.Proxy.Credentials = request.Credentials;
                    WebResponse response = (WebResponse)request.GetResponse();
                    using (Stream stream = response.GetResponseStream())
                    {
                        opResult = load(stream);
                    }
                }
                else if (File.Exists(source))
                {
                    using (FileStream stream = new FileStream(source, FileMode.Open))
                    {
                        opResult = load(stream);
                    }
                }
                else
                {
                    doc = XDocument.Parse(source, LoadOptions.PreserveWhitespace);
                }
            }
            catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            if (!opResult.Success)
            {
                doc = null;
            }
            return opResult;
        }

        /// <summary>
        ///     opens the xml document.
        /// </summary>
        /// <param name="source">
        ///     a valid Uri to a .xml file, or an XML string
        /// </param>
        /// <returns>success or failure status</returns>
        public OpResult load(Uri source)
        {
            OpResult opResult = new OpResult();
            try
            {
                WebRequest request = WebRequest.Create(source);
                request.Timeout = 1000 * 60; // 1 minute;
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = request.Credentials;
                WebResponse response = (WebResponse)request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    opResult = load(stream);
                }
            }
            catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            if (!opResult.Success)
            {
                doc = null;
            }
            return opResult;
        }

        public OpResult load(Stream stream)
        {
            OpResult opResult = new OpResult();
            try
            {
                string xml = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
                opResult = load(xml);
            }
            catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            return opResult;
        }
    }

    /// <summary>
    /// Singleton wrapper for the UoM_Database
    /// </summary>
    public static class UomDataSource
    {
        private static string uomDatabaseFilePath = null;

        public static string DatabasePath
        {
            get
            {
                return (uomDatabaseFilePath != null) ? uomDatabaseFilePath : "http://testsite2.callabsolutions.com/UnitsOfMeasure/UOM_database.xml";
            }

            set
            {
                uomDatabaseFilePath = value;
            }
        }

        /// <summary>
        /// object type held in UomDataSource.UofM.aliasCache and UomDataSource.Alternative.aliasCache 
        ///   that is used as a faster alternative to repeated LINQ queries returning the same results over and over again
        /// </summary>
        public class Alias
        {
            XElement aliasElement;
            private string presentation = null;

            public string Presentation
            {
                get
                {
                    if (presentation == null)
                    {
                        var PresentationElement = aliasElement.Element(ns + "Presentation");
                        if (PresentationElement != null)
                        {
                            var reader = PresentationElement.CreateReader();
                            reader.MoveToContent();
                            presentation = reader.ReadInnerXml();
                        }
                        else
                        {
                            presentation = "";
                        }
                    }
                    return presentation;
                }
            }

            public Alias(XElement aliasElement)
            {
                this.aliasElement = aliasElement;
            }

        }

        /// <summary>
        /// object type held in UomDataSource.Quantity.altCache that is used as a faster alternative to repeated LINQ queries returning the same results over and over again
        /// </summary>
        public class Alternative
        {
            private XElement altElement = null;
            private decimal offset, scaleFactor, linScaleFactor, logScaleFactor;
            string _symbol = null;
            string presentation = null;
            private Func<decimal, decimal> tobase = null;
            private Func<decimal, decimal> frombase = null;
            private Dictionary<String, Alias> aliasCache = new Dictionary<string, Alias>();

            private void loadAliases()
            {
                aliasCache.Clear();
                string symbol;
                var set = altElement.Element(ns + "Aliases").Elements(ns + "Alias");
                foreach (var alias in set)
                {
                    symbol = alias.Attribute("symbol").Value;
                    aliasCache[symbol] = new Alias(alias);
                }
            }

            private void getConverters()
            {
                var linconv = altElement.Element(ns + "LinearConverter");
                var logconv = altElement.Element(ns + "LogarithmicConverter");
                if (linconv != null)
                {
                    // base_value = (value - Offset) * ScaleFactor
                    offset = decimal.Parse((string)linconv.Attribute("Offset"), NumberStyles.Float, CultureInfo.InvariantCulture);
                    scaleFactor = decimal.Parse((string)linconv.Attribute("ScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                    tobase = (x => (x - offset) * scaleFactor);
                    frombase = (x => (x / scaleFactor) + offset);
                }
                else if (logconv != null)
                {
                    // base_value = LinScaleFactor * (10.0 ^ (value/LogScaleFactor)) 
                    linScaleFactor = decimal.Parse((string)logconv.Attribute("LinScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                    logScaleFactor = decimal.Parse((string)logconv.Attribute("LogScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                    tobase = (x => linScaleFactor * DecimalMath.Pow(10.0m, x / logScaleFactor));
                    frombase = (x => logScaleFactor * DecimalMath.Log10(x / linScaleFactor));
                }
            }



            public String symbol
            {
                get
                {
                    if (_symbol == null)
                    {
                        _symbol = altElement.Attribute("symbol").Value;
                    }
                    return _symbol;
                }
            }

            private string Presentation
            {
                get
                {
                    if (presentation == null)
                    {
                        var PresentationElement = altElement.Element(ns + "Presentation");
                        if (PresentationElement != null)
                        {
                            var reader = PresentationElement.CreateReader();
                            reader.MoveToContent();
                            presentation = reader.ReadInnerXml();
                        }
                        else
                        {
                            presentation = "";
                        }
                    }
                    return presentation;
                }
            }

            /// <summary>
            /// returns true is symbol is found in any child Alias
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            private Boolean hasAlias(String symbol)
            {
                if (aliasCache.Count() == 0)
                {
                    loadAliases();
                }
                return aliasCache.Keys.Contains(symbol);
            }

            public decimal ConvertToBase(decimal value)
            {
                if (tobase == null)
                {
                    getConverters();
                }
                return Math.Round(tobase(value), 28);
            }

            public decimal ConvertFromBase(decimal value)
            {
                if (frombase == null)
                {
                    getConverters();
                }
                return Math.Round(frombase(value), 28);
            }
            /// <summary>
            /// returns presentation for this or any child with symbol
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public string getPresentation(String symbol)
            {
                if (this.symbol == symbol)
                {
                    return Presentation;
                }
                if (hasAlias(symbol))
                {
                    return aliasCache[symbol].Presentation;
                }
                throw new Exception("invalid symbol");
            }

            /// <summary>
            /// returns true is symbol is found in this or any child
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public Boolean hasSymbol(String symbol)
            {
                if (this.symbol == symbol)
                {
                    return true;
                }
                else
                {
                    return hasAlias(symbol);
                }
            }


            /// <summary>
            /// return this symbol and all Alias symbols
            /// </summary>
            public List<String> symbols
            {
                get
                {
                    if (aliasCache.Count() == 0)
                    {
                        loadAliases();
                    }
                    List<String> list = new List<String>();
                    list.Add(symbol);
                    list.AddRange(aliasCache.Keys);
                    return list;
                }
            }

            private Alternative() { }

            public Alternative(XElement altElement)
            {
                this.altElement = altElement;
            }

        }


        public class UofM
        {
            XElement UomElement;
            private string _name = null;
            private string _symbol = null;
            private string presentation = null;
            private Dictionary<String, Alias> aliasCache = new Dictionary<string, Alias>();

            private void loadAliases()
            {
                aliasCache.Clear();
                string symbol;
                var aliases = UomElement.Element(ns + "Aliases");
                if (aliases != null)
                {
                    var set = aliases.Elements(ns + "Alias");
                    foreach (var alias in set)
                    {
                        symbol = alias.Attribute("symbol").Value;
                        aliasCache[symbol] = new Alias(alias);
                    }
                }
            }

            /// <summary>
            /// returns true is symbol is found in any child Alias
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            private Boolean hasAlias(String symbol)
            {
                if (aliasCache.Count() == 0)
                {
                    loadAliases();
                }
                return aliasCache.Keys.Contains(symbol);
            }

            private string Presentation
            {
                get
                {
                    if (presentation == null)
                    {
                        var PresentationElement = UomElement.Element(ns + "Presentation");
                        if (PresentationElement != null)
                        {
                            var reader = PresentationElement.CreateReader();
                            reader.MoveToContent();
                            presentation = reader.ReadInnerXml();
                        }
                        else
                        {
                            presentation = "";
                        }
                    }
                    return presentation;
                }
            }

            public String name
            {
                get
                {
                    if (_name == null)
                    {
                        _name = UomElement.Attribute("base_name").Value;
                    }
                    return _name;
                }
            }

            public String symbol
            {
                get
                {
                    if (_symbol == null)
                    {
                        _symbol = UomElement.Attribute("symbol").Value;
                    }
                    return _symbol;
                }
            }

            /// <summary>
            /// returns true is symbol is found in this or any child
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public Boolean hasSymbol(String symbol)
            {
                if (this.symbol == symbol)
                {
                    return true;
                }
                else
                {
                    return hasAlias(symbol);
                }
            }

            /// <summary>
            /// returns presentation for this or any child with symbol
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public string getPresentation(String symbol)
            {
                if (this.symbol == symbol)
                {
                    return Presentation;
                }
                if (hasAlias(symbol))
                {
                    return aliasCache[symbol].Presentation;
                }
                throw new Exception("invalid symbol");
            }

            /// <summary>
            /// return this symbol and all Alias symbols
            /// </summary>
            public List<String> symbols
            {
                get
                {
                    if (aliasCache.Count() == 0)
                    {
                        loadAliases();
                    }
                    List<String> list = new List<String>();
                    list.Add(symbol);
                    list.AddRange(aliasCache.Keys);
                    return list;
                }
            }

            public UofM(XElement UomElement)
            {
                this.UomElement = UomElement;
            }
        }

        /// <summary>
        /// object type held in UomDataSource.qtyCache that is used as a faster alternative to repeated LINQ queries returning the same results over and over again
        /// </summary>
        public class Quantity
        {
            private XElement qtyElement = null;
            private string _name = "";
            private XElement uomElement = null;
            private UofM _UoM = null;
            private Dictionary<string, Alternative> altCache = new Dictionary<string, Alternative>();

            public string name
            {
                get
                {
                    if (_name == "")
                    {
                        var atrName = QtyElement.Attribute("name");
                        if (atrName != null)
                        {
                            _name = atrName.Value;
                        }
                    }
                    return _name;
                }
            }

            public XElement QtyElement
            {
                get { return qtyElement; }
            }

            public XElement UomElement
            {
                get
                {
                    if (uomElement == null)
                    {
                        XNamespace ns = Configuration.NameSpaces["uom"];
                        uomElement = qtyElement.Ancestors(ns + "UOM").Single();
                    }
                    return uomElement;
                }
            }

            public UofM UoM
            {
                get
                {
                    if (_UoM == null)
                    {
                        _UoM = new UofM(UomElement);
                    }
                    return _UoM;
                }
            }

            public Alternative getAlternative(string alternativeName)
            {
                if (altCache.Count() == 0)
                {
                    loadAlternatives();
                }
                if (!altCache.Keys.Contains(alternativeName))
                {
                    throw new Exception("invalid alternative");
                }
                return altCache[alternativeName];
            }

            public Boolean hasAlternative(string alternativeName)
            {
                if (altCache.Count() == 0)
                {
                    loadAlternatives();
                }
                return altCache.Keys.Contains(alternativeName);
            }

            public IEnumerable<Alternative> Alternatives
            {
                get
                {
                    if (altCache.Count() == 0)
                    {
                        loadAlternatives();
                    }
                    return altCache.Values;
                }
            }

            public List<String> AlternativeNames
            {
                get
                {
                    if (altCache.Count() == 0)
                    {
                        loadAlternatives();
                    }
                    return altCache.Keys.ToList();
                }
            }

            private void loadAlternatives()
            {
                altCache.Clear();
                string alternativeName;
                var set = UomElement.Descendants(ns + "Alternative");
                foreach (var alternative in set)
                {
                    alternativeName = alternative.Attribute("name").Value;
                    if (!altCache.Keys.Contains(alternativeName))
                    {
                        altCache[alternativeName] = new Alternative(alternative);
                    }
                }
            }

            public void writeTo(XElement parent)
            {
                new UomSpaceHelper(parent).addChild("Quantity").setAttribute("name", name);
            }

            private Quantity() { }

            public Quantity(XElement qtyElement)
            {
                this.qtyElement = qtyElement;
            }
        }

        private static Dictionary<string, Quantity> qtyCache = new Dictionary<string, Quantity>();

        private static XNamespace ns = Configuration.NameSpaces["uom"];
        private static XDocument doc = null;

        private static XDocument Doc
        {
            get
            {
                if (doc == null)
                {
                    XMLDataSource datasource = new XMLDataSource();
                    OpResult op = datasource.load(UomDataSource.DatabasePath);
                    if (op.Success)
                    {
                        doc = datasource.Doc;
                    }
                }
                return doc;
            }
        }

        public static XElement Database
        {
            get { return Doc.Root; }
        }

        public static Dictionary<string, Quantity> getQuantities()
        {
            if (qtyCache.Count == 0)
            {
                if (Database != null)
                {
                    var qtys = Database.Descendants(ns + "Quantity");
                    if (qtys != null)
                    {
                        foreach (XElement el in qtys)
                        {
                            var name = (string)el.Attribute("name");
                            var els = qtys.Where(x => (string)x.Attribute("name") == name);
                            qtyCache[name] = (els.Count() > 0) ? new Quantity(els.First()) : null;
                        }
                    }
                    qtyCache = qtyCache.OrderBy(entry => entry.Key).ToDictionary(x => x.Key, x => x.Value);
                }
            }
            return qtyCache;
        }

        public static Quantity getQuantity(string QuantityName)
        {
            if (!qtyCache.Keys.Contains(QuantityName))
            {
                qtyCache[QuantityName] = null;
                if (Database != null)
                {
                    var qtys = Database.Descendants(ns + "Quantity");
                    if (qtys != null)
                    {
                        var els = qtys.Where(x => (string)x.Attribute("name") == QuantityName);
                        qtyCache[QuantityName] = (els.Count() > 0) ? new Quantity(els.First()) : null;
                    }
                }
            }
            return (qtyCache.Keys.Contains(QuantityName)) ? qtyCache[QuantityName] : null;
        }

        public static Boolean hasQuantity(string QuantityName)
        {
            return getQuantity(QuantityName) != null;
        }

    }

    public class NameSpaces
    {
        private Dictionary<string, string> map = new Dictionary<string, string>(){
            {"soa", @"https://cls-schemas.s3.us-west-1.amazonaws.com/SOA_Master_Datafile"},
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

    public class DecimalMath
    {
        // Adjust this to modify the precision
        const int ITERATIONS = 27;
        static decimal log10 = 0.0m;

        // power series
        public static decimal Exp(decimal power)
        {
            int iteration = ITERATIONS;
            decimal result = 1;
            while (iteration > 0)
            {
                var fatorial = Factorial(iteration);
                result += Pow(power, iteration) / fatorial;
                iteration--;
            }
            return result;
        }

        // natural logarithm series
        public static decimal LogN(decimal number)
        {
            decimal aux = (number - 1);
            decimal result = 0;
            int iteration = ITERATIONS;
            while (iteration > 0)
            {
                result += Pow(aux, iteration) / iteration;
                iteration--;
            }
            return result;
        }

        // logarithm base 10
        public static decimal Log10(decimal number)
        {
            if (log10 == 0.0m)
            {
                log10 = LogN(10.0m);
            }
            return LogN(number) / log10;
        }

        public static decimal Factorial(long number)
        {
            decimal f = 1.0m;
            for (long n = number; n > 1; n--)
            {
                f *= n;
            }
            return f;
        }

        public static decimal Pow(decimal value, long number)
        {
            decimal f = 1.0m;
            for (long n = number; n > 1; n--)
            {
                f *= value;
            }
            return f;
        }

        public static decimal Pow(decimal baseValue, decimal exponent)
        {
            return Exp(exponent * LogN(baseValue));
        }

    }

    public class OpResult
    {
        private bool success = true;
        private string error = "not set";

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        public string Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}
