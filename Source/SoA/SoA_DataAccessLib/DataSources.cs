using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SOA_DataAccessLib
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
                if ((source.Length < URI_MAX_LENGTH) && (Uri.IsWellFormedUriString(Uri.EscapeUriString(source), UriKind.Absolute)))
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
                        stream.Close();
                    }
                }
                else if (File.Exists(source))
                {
                    using (FileStream stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        opResult = load(stream);
                        stream.Close();
                    }
                }
                else
                    doc = XDocument.Parse(source, LoadOptions.PreserveWhitespace);
            }
            catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            finally
            {
                GC.Collect();
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

    public static class UomDataSource
    {
        private static string uomDatabaseFilePath = null;

        public static string DatabasePath
        {
            get
            {
                return (uomDatabaseFilePath != null) ? uomDatabaseFilePath : "https://raw.githubusercontent.com/NCSLI-MII/measurand-taxonomy/main/UOM_Database.xml";
                // "http://testsite2.callabsolutions.com/UnitsOfMeasure/UOM_database.xml";
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
        [Serializable]
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
                    if (_symbol == null) _symbol = altElement.Attribute("symbol").Value;
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
                    if (op.Success) doc = datasource.Doc;
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
            //if (qtyCache.Count == 0)
            //{
            var qtysList = new Dictionary<string, Quantity>();
            if (Database != null)
            {
                
                var qtys = Database.Descendants(ns + "Quantity");
                if (qtys != null)
                {
                    foreach (XElement el in qtys)
                    {
                        var name = (string)el.Attribute("name");
                        var els = qtys.Where(x => (string)x.Attribute("name") == name);
                        qtysList[name] = (els.Count() > 0) ? new Quantity(els.First()) : null;
                    }
                }
                qtysList = qtysList.OrderBy(entry => entry.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            //}
            return qtysList;
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

    public abstract class AbstractValue
    {
        protected UomDataSource.Quantity quantity = null;
        protected UomDataSource.Alternative alternative = null;
        private string _uom_alternative = "";
        private string _symbol = "";
        private string _format = "";
        protected string valueString = "";

        private decimal toBase(decimal value)
        {
            if (quantity == null) throw new Exception("invalid quantity");
            if (uom_alternative != "")
            {
                var alt = quantity.getAlternative(uom_alternative);
                if (alt == null)
                {
                    throw new Exception("invalid UOM alternative");
                }
                value = alt.ConvertToBase(value);
            }
            return value;
        }

        private decimal fromBase(decimal value)
        {
            if (quantity == null) throw new Exception("invalid quantity");
            if (uom_alternative != "")
            {
                var alt = quantity.getAlternative(uom_alternative);
                if (alt == null)
                {
                    throw new Exception("invalid UOM alternative");
                }
                value = alt.ConvertFromBase(value);
            }
            return value;
        }

        public string Quantity
        {
            get { return (quantity != null) ? quantity.name : ""; }
            set
            {
                if ((value != Quantity) || (value == ""))
                {
                    // if quantity is changed or reset, reset everything tied to quantity
                    quantity = null;
                    _uom_alternative = "";
                    _symbol = "";
                }
                if (value != "")
                {
                    quantity = UomDataSource.getQuantity(value);
                    if (quantity != null)
                    {
                        _symbol = quantity.UoM.symbol;
                    }
                    else
                    {
                        throw new Exception("invalid quantity");
                    }
                }
            }
        }

        public string uom_alternative
        {
            get { return _uom_alternative; }
            set
            {
                decimal old_base_value = 0.0m;
                if ((valueString != "") && (quantity == null))
                {
                    throw new Exception("invalid quantity");
                }
                if (_uom_alternative != value)
                {   // new alternative
                    if (valueString != "")
                    {
                        old_base_value = BaseValue;
                    }
                    if (value.Trim() == "")
                    {
                        _uom_alternative = "";
                    }
                    else if (quantity.hasAlternative(value))
                    {
                        _uom_alternative = value;
                    }
                    else
                    {
                        throw new Exception("invalid alternative");
                    }
                    if (_uom_alternative != "")
                    {
                        // set symbol to default symbol of new alternative
                        alternative = quantity.getAlternative(_uom_alternative);
                        _symbol = alternative.symbol;
                    }
                    else
                    {
                        // set symbol to default symbol of base UOM;
                        _symbol = quantity.UoM.symbol;
                        alternative = null;
                    }
                    if (valueString != "")
                    {
                        ValueString = fromBase(old_base_value).ToString();
                    }
                }
            }
        }

        public List<string> validAlternatives
        {
            get
            {
                if (quantity == null)
                {
                    throw new Exception("invalid quantity");
                }
                return quantity.AlternativeNames;
            }
        }

        public List<string> validAliases
        {
            get
            {
                if ((valueString != "") && (quantity == null))
                {
                    throw new Exception("invalid quantity");
                }
                if (_uom_alternative == "")
                {
                    return quantity.UoM.symbols;
                }
                else
                {
                    if (!quantity.hasAlternative(_uom_alternative))
                    {
                        throw new Exception("invalid uom_alternative");
                    }
                    var alternative = quantity.getAlternative(_uom_alternative);
                    return alternative.symbols;
                }
            }
        }

        public string symbol
        {
            get { return _symbol; }
            set
            {
                if ((valueString != "") && (quantity == null))
                {
                    throw new Exception("invalid quantity");
                }
                if (quantity != null)
                {
                    if (_uom_alternative == "")
                    {
                        var UoM = quantity.UoM;
                        if (UoM.hasSymbol(value))
                        {
                            _symbol = value;
                        }
                        else if (value == "")
                        {
                            _symbol = UoM.symbol;
                        }
                        else
                        {
                            throw new Exception("invalid symbol");
                        }
                    }
                    else
                    {
                        if (alternative.hasSymbol(value))
                        {
                            _symbol = value;
                        }
                        else if (value == "")
                        {
                            _symbol = alternative.symbol;
                        }
                        else
                        {
                            throw new Exception("invalid symbol");
                        }
                    }
                }
            }
        }

        public string format
        {
            get { return _format; }
            set
            {
                try
                {
                    // validate value is good format string by trying it
                    decimal v = 1.0m;
                    v.ToString(value);
                    _format = value;
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }

        public string ValueString
        {
            get { return this.valueString; }
            set
            {
                decimal v;
                if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    throw new Exception("improper value");
                }
                this.valueString = value;
            }
        }

        public decimal Value
        {
            get
            {
                decimal v;
                if (!decimal.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    throw new Exception("improper value");
                }
                return v;
            }
            set
            {
                this.valueString = value.ToString();
            }
        }

        public decimal BaseValue
        {
            get
            {
                if (quantity == null)
                {
                    throw new Exception("invalid quantity");
                }
                decimal v;
                if (!decimal.TryParse(ValueString, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    throw new Exception("improper value");
                }
                return toBase(v);
            }
        }

        public string HTML_Presentation
        {
            get
            {
                if (quantity == null)
                {
                    throw new Exception("invalid quantity");
                }
                decimal value = BaseValue;
                string units;
                if (uom_alternative != "")
                {
                    value = alternative.ConvertFromBase(value);
                    units = alternative.getPresentation(symbol);
                }
                else
                {
                    UomDataSource.UofM UoM = quantity.UoM;
                    units = UoM.getPresentation(symbol);
                }
                return String.Format("<span><span>{0}</span>{1}</span>", value.ToString(format), units);
            }
        }

        protected void loadValue(XElement datasource)
        {
            if (datasource != null)
            {
                valueString = datasource.Value;
            }
        }

        public virtual void writeTo(XElement myElement)
        {
            var me = new XmlNameSpaceElement(myElement);
            if (alternative != null)
            {
                me.setAttribute("uom_alternative", uom_alternative);
                if (alternative.symbol != symbol)
                {
                    me.setAttribute("uom_alias_symbol", symbol);
                }
            }
            else  // no alternative
            {
                if (quantity.UoM.symbol != symbol)
                {
                    me.setAttribute("uom_alias_symbol", symbol);
                }
            }
            if (format != "")
            {
                me.setAttribute("format", format);
            }
            me.Value = ValueString;
        }

    }
}
