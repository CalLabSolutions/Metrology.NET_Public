// Copyright 2017 Cal Lab Solutions Inc.
//
//This file is part of SOA_DataAccessLibrary.DLL, a component of Metrology.NET
//
//SOA_DataAccessLibrary.DLL is dual licensed software.
//
//For licensing for use in any product with lessor restrictions than the GPL, 
//  or for use in any proprietary licensed product, 
//  or for use in any or product publicly sold, 
//  or for use in any or product developed under contract for a another party,
//  contact Cal Lab Solutions Inc., to obtain an appropriate license
//
//      Cal Lab Solutions, Inc.
//      P.O. Box 111113
//      Aurora, CO 80042
//      Office: 303.317.6670
//      Fax: 303.317.5295
//      Email: sales@callabsolutions.com
//
//For commercial internal use only or for any strictly non-commercial use, 
//  SOA_DataAccessLibrary.DLL is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  SOA_DataAccessLibrary.DLL is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with SOA_DataAccessLibrary.DL.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SOA_DataAccessLibrary
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
                    doc = XDocument.Parse(source, LoadOptions.PreserveWhitespace);
            }
            catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            if (!opResult.Success) doc = null;
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
            if (!opResult.Success) doc = null;
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
            get {
                return (uomDatabaseFilePath != null) ? uomDatabaseFilePath : "http://testsite2.callabsolutions.com/UnitsOfMeasure/UOM_Database.xml";
            }

            set {
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
                            presentation = "";
                    }
                    return presentation;
                }
            }

            public Alias(XElement aliasElement) {
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
                            presentation = "";
                    }
                    return presentation;
                }
            }

            /// <summary>
            /// returns true is symbol is found in any child Alias
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            private Boolean hasAlias(String symbol) {
                if (aliasCache.Count() == 0) loadAliases();
                return aliasCache.Keys.Contains(symbol);
            }            
            
            public decimal ConvertToBase(decimal value)
            {
                if (tobase == null) getConverters();
                return Math.Round(tobase(value), 28);
            }

            public decimal ConvertFromBase(decimal value)
            {
                if (frombase == null) getConverters();
                return Math.Round(frombase(value), 28);
            }
            /// <summary>
            /// returns presentation for this or any child with symbol
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public string getPresentation(String symbol)
            {
                if (this.symbol == symbol) return Presentation;
                if (hasAlias(symbol)) return aliasCache[symbol].Presentation;
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
                    return true;
                else return hasAlias(symbol);              
            }


            /// <summary>
            /// return this symbol and all Alias symbols
            /// </summary>
            public List<String> symbols
            {
                get
                {
                    if (aliasCache.Count() == 0) loadAliases();
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
                if (aliasCache.Count() == 0) loadAliases();
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
                            presentation = "";
                    } 
                    return presentation;
                }
            }

            public String name
            {
                get
                {
                    if (_name == null) _name = UomElement.Attribute("base_name").Value;
                    return _name;
                }
            }

            public String symbol
            {
                get
                {
                    if (_symbol == null) _symbol = UomElement.Attribute("symbol").Value;
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
                    return true;
                else return hasAlias(symbol);
            }

            /// <summary>
            /// returns presentation for this or any child with symbol
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public string getPresentation(String symbol)
            {
                if (this.symbol == symbol) return Presentation;
                if (hasAlias(symbol)) return aliasCache[symbol].Presentation;
                throw new Exception("invalid symbol");
            }

            /// <summary>
            /// return this symbol and all Alias symbols
            /// </summary>
            public List<String> symbols
            {
                get
                {
                    if (aliasCache.Count() == 0) loadAliases();
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
                        if (atrName != null) _name = atrName.Value;
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
                    if (_UoM == null) _UoM = new UofM(UomElement);
                    return _UoM;
                }
            }

            public Alternative getAlternative(string alternativeName)
            {
                if (altCache.Count() == 0) loadAlternatives();
                if (!altCache.Keys.Contains(alternativeName)) {
                   throw new Exception("invalid alternative");
                }
                return altCache[alternativeName];
            }

            public Boolean hasAlternative(string alternativeName)
            {
                if (altCache.Count() == 0) loadAlternatives();
                return altCache.Keys.Contains(alternativeName);
            }

            public IEnumerable<Alternative> Alternatives
            {
                get
                {
                    if (altCache.Count() == 0) loadAlternatives();
                    return altCache.Values;
                }
            }

            public List<String> AlternativeNames
            {
                get
                {
                    if (altCache.Count() == 0) loadAlternatives();
                    return altCache.Keys.ToList();                  
                }
            }

            private void loadAlternatives() {
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
            get { 
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
            {"soa", @"http://schema.metrology.net/ScopeOfAccreditation"},
            {"xsi", @"http://www.w3.org/2001/XMLSchema-instance"}, 
            {"xi", @"http://www.w3.org/2001/XInclude"},
            {"uom", @"http://schema.metrology.net/UnitsOfMeasure"},
            {"unc", @"http://schema.metrology.net/Uncertainty"},
            {"mml", @"http://www.w3.org/1998/Math/MathML"},
            {"xhtml", @"http://www.w3.org/1999/xhtml"},
            {"mtc", @"http://schema.metrology.net/MetrologyTaxonomyCatalog"}
        };

        public string this[string key]
        {
            get { return map[key]; }
        }

        public XAttribute[] NameSpaceDeclarations
        {
            get
            {
                List<XAttribute> list = new List<XAttribute>();
                foreach (string key in map.Keys)
                {
                    XNamespace ns = XNamespace.Get(map[key]);
                    list.Add(new XAttribute(key, ns)); 
                }
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
            get { return "http://testsite2.callabsolutions.com/UnitsOfMeasure/UOM_Database.xml"; }
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
            set { Element.Value = value;  }
        }

        public XmlNameSpaceElement setValue(string Value)
        {
            this.Value = Value;
            return this;
        }

        public XmlNameSpaceElement(string name, XNamespace ns) {
            this.ns = ns;
            this.element = new XElement(ns + name);
        }

        public XmlNameSpaceElement(XElement Element) {
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

        public XmlNameSpaceHelper getNameSpaceHelper(string elementName) {
            var element = getElement(elementName);
            if (element != null) {
                var ns = element.GetDefaultNamespace();
                return new XmlNameSpaceHelper(element, ns.NamespaceName);
            } else return null;
        }

        public String Value {
            get {return getValue();}
            set {setValue(value);}
        }

        public string getValue()
        {
            return (datasource is XElement) ? (datasource as XElement).Value : "";
        }

        public XmlNameSpaceHelper setValue(string value)
        {
            if (datasource is XElement) {
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
            if (helper != null) {
                return helper.setValue(value);
            } else return null;
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
            if (datasource is XElement) { 
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
            :base(datasource, Configuration.NameSpaces["mtc"]) {}
    }

    public class UncSpaceHelper : XmlNameSpaceHelper
    {
        private UncSpaceHelper() { }

        public UncSpaceHelper(XElement datasource)
            :base(datasource, Configuration.NameSpaces["unc"]) {}
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
                if (alt == null) throw new Exception("invalid UOM alternative");
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
                if (alt == null) throw new Exception("invalid UOM alternative");
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
                if ((valueString != "") && (quantity == null)) throw new Exception("invalid quantity");
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
                if (quantity == null) throw new Exception("invalid quantity");
                return quantity.AlternativeNames;
            }
        }

        public List<string> validAliases
        {
            get
            {
                if ((valueString != "") && (quantity == null)) throw new Exception("invalid quantity");
                if (_uom_alternative == "")
                {
                    return quantity.UoM.symbols;
                }
                else
                {
                    if (!quantity.hasAlternative(_uom_alternative)) throw new Exception("invalid uom_alternative");
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
                if ((valueString != "") && (quantity == null)) throw new Exception("invalid quantity");
                if (quantity != null)
                {
                    if (_uom_alternative == "")
                    {
                        var UoM = quantity.UoM;
                        if (UoM.hasSymbol(value))
                            _symbol = value;
                        else if (value == "")
                            _symbol = UoM.symbol;
                        else
                            throw new Exception("invalid symbol");
                    }
                    else
                    {
                        if (alternative.hasSymbol(value))
                            _symbol = value;
                        else if (value == "")
                            _symbol = alternative.symbol;
                        else
                            throw new Exception("invalid symbol");
                    }
                }
            }
        }

        public string format
        {
            get { return _format; }
            set {             
                try
                {  
                    // validate value is good format string by trying it
                    decimal v = 1.0m;
                    v.ToString(value); 
                    _format = value;
                }
                catch (Exception e)
                {
                    throw e;
                }
                 
            }
        }

        public string ValueString
        {
            get { return this.valueString; }
            set 
            {
                decimal v;
                if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) throw new Exception("improper value");
                this.valueString = value; 
            }
        }

        public decimal Value
        {
            get {
                decimal v;
                if (!decimal.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) throw new Exception("improper value");
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
                if (quantity == null) throw new Exception("invalid quantity");
                decimal v;
                if (!decimal.TryParse(ValueString, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) throw new Exception("improper value");
                return toBase(v);
            }
        }

        public string HTML_Presentation
        {
            get
            {
                if (quantity == null) throw new Exception("invalid quantity");
                decimal value = BaseValue;
                string units;
                if (uom_alternative != "")
                {
                    value = alternative.ConvertFromBase(value);
                    units = alternative.getPresentation(symbol);
                } else {
                    UomDataSource.UofM UoM =  quantity.UoM;
                    units = UoM.getPresentation(symbol);
                }
                return String.Format("<span><span>{0}</span>{1}</span>", value.ToString(format), units);
            }
        }

        protected void loadValue (XElement datasource)
        {
            if (datasource != null) valueString = datasource.Value;
        }

        public virtual void writeTo(XElement myElement) {
            var me = new XmlNameSpaceElement(myElement);
            if (alternative != null)
            {
                me.setAttribute("uom_alternative", uom_alternative);
                if (alternative.symbol != symbol) me.setAttribute("uom_alias_symbol", symbol);
            }
            else  // no alternative
            {
                if (quantity.UoM.symbol != symbol) me.setAttribute("uom_alias_symbol", symbol);
            }
            if (format != "") me.setAttribute("format", format);
            me.Value = ValueString;
        }

    }

    public class Uom_Quantity 
    {
        private string _name = "";

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public void writeTo(XElement Parent) {
            new UncSpaceHelper(Parent).addChild("Quantity").setAttribute("name", name);
        }

        private Uom_Quantity() {}

        public Uom_Quantity(String Name) {
            this._name = Name;
        }

        public Uom_Quantity(XElement datasource)
        {
            _name = new UomSpaceHelper(datasource).getAttribute("name");
        }
    }

    public class Mtc_Documentation 
    {
        private string document = "";

        public string Document
        {
            get { return document; }
            set { document = value; }
        }

        public void writeTo(XElement Parent)
        {
            new MtcSpaceHelper(Parent).addChild("Documentation").Value = document;
        }

        public Mtc_Documentation() { }

        public Mtc_Documentation(XElement datasource)
        {
            System.Xml.XmlReader reader = datasource.CreateReader();
            document = reader.ReadInnerXml();
        }
    }

    public class Mtc_ProcessResult
    {
        private string name = "";
        private UomDataSource.Quantity quantity = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public UomDataSource.Quantity Quantity
        {
            get { return quantity; }
            //set { quantity = value; }
        }

        private void setQuantity(UomSpaceHelper uomSpaceHelper)
        {
            var qtyElement = uomSpaceHelper.getElement("Quantity");
            if (qtyElement != null)
            {
                uomSpaceHelper = new UomSpaceHelper(qtyElement);
                string qty = uomSpaceHelper.getAttribute("name");
                quantity = UomDataSource.getQuantity(qty);
            }
        }

        public void writeTo(XElement ProcessType)
        {
            MtcSpaceHelper ProcessTypeHelper = new MtcSpaceHelper(ProcessType);
            var Result = ProcessTypeHelper.addChild("Result");
            if (name != "") Result.setAttribute("name", Name);
            quantity.writeTo(Result.Element);
        }

        private Mtc_ProcessResult() { }

        public Mtc_ProcessResult(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            UomSpaceHelper uomSpaceHelper = new UomSpaceHelper(datasource);
            setQuantity(uomSpaceHelper);
        }
    }

    public class Mtc_ProcessResults : IEnumerable<Mtc_ProcessResult>
    {
        private List<Mtc_ProcessResult> results = new List<Mtc_ProcessResult>();

        public Mtc_ProcessResult this[int index]
        {
            get { return results[index]; }
        }

        public Mtc_ProcessResult this[string name]
        {
            get { 
                var set = results.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return results.Count();
        }

        private void loadResults(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("Result");
            foreach (XElement el in els)
            {
                Mtc_ProcessResult result = new Mtc_ProcessResult(el);
                results.Add(result);
            }
        }

        public void writeTo(XElement ProcessType)
        {
            foreach (Mtc_ProcessResult result in results)
            {
                result.writeTo(ProcessType);
            }
        }

        public Mtc_ProcessResults() { } // Needed when Mtc_ProcessType is null.  Returns an empty Mtc_ProcessResults

        public Mtc_ProcessResults(XElement datasource)
        {
            loadResults(new MtcSpaceHelper(datasource));
        }

        public IEnumerator<Mtc_ProcessResult> GetEnumerator()
        {
            return results.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_Enumeration : IEnumerable<string>
    {
        HashSet<string> items = new HashSet<string>();

        public void remove(string value)
        {
            if (items.Contains(value)) items.Remove(value);
        }

        public void add(string value) {
            items.Add(value);
        }
             
        public int Count()
        {
            return items.Count();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void loadItems(XElement datasource)
        {
            var Items = new MtcSpaceHelper(datasource).getValues("Item");
            foreach (string value in Items)
            {
                add(value);
            }
        }

        public void writeTo(XElement Parameter)
        {
            var Enumeration = new MtcSpaceHelper(Parameter).addChild("Enumeration");
            foreach (string item in items)
            {
                Enumeration.addChild("Item").Value = item;
            }
        }

        public Mtc_Enumeration(params string[] values) {
            foreach (string value in values)
            {
                add(value);
            }    
        }

        public Mtc_Enumeration(XElement datasource)
        {
            loadItems(datasource);
        }
    }

    public class Mtc_Parameter 
    {
        public class Mtc_ParameterComparer : IEqualityComparer<Mtc_Parameter> // required to support creating lists with unique entries
        {
            public bool Equals(Mtc_Parameter x, Mtc_Parameter y)
            {
                return x.name.Equals(y.name);
            }

            public int GetHashCode(Mtc_Parameter parameter)
            {
                return parameter.name.GetHashCode();
            }
        }

        private string _name = "";
        private bool _optional = false;
        private UomDataSource.Quantity quantity = null;
        private Mtc_Enumeration enumeration = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool optional
        {
            get { return _optional; }
            set { _optional = value; }
        }

        public UomDataSource.Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private bool setQuantity(XElement datasource)
        {
            quantity = null;
            var qtyElement = new UomSpaceHelper(datasource).getElement("Quantity");
            if (qtyElement != null)
            {
                string qty = new UomSpaceHelper(qtyElement).getAttribute("name");
                quantity = UomDataSource.getQuantity(qty);
            }
            return quantity != null;
        }

        private bool setEnumeration(XElement datasource)
        {
            enumeration = null;
            var Enumeration = new MtcSpaceHelper(datasource).getElement("Enumeration");
            if (Enumeration != null)
            {
                enumeration = new Mtc_Enumeration(Enumeration);
            }
            else
            {
                enumeration = new Mtc_Enumeration();
            }
            return enumeration != null;
        }

        public void writeTo(XElement Parent)
        {
            var Parameter = new MtcSpaceHelper(Parent).addChild("Parameter").setAttribute("name", name).setAttribute("optional", optional.ToString());
            if (quantity != null)
            {
                quantity.writeTo(Parameter.Element);
            }
            else if (enumeration != null) 
            {
                enumeration.writeTo(Parameter.Element);
            }
        }

        private Mtc_Parameter() { }

        public Mtc_Parameter(string name, string quantity, Boolean optional) {
            _name = name;
            this.optional = optional;
            this.quantity = UomDataSource.getQuantity(quantity);
        }

        public Mtc_Parameter(string name, Mtc_Enumeration enumeration, Boolean optional)
        {
            _name = name;
            this.optional = optional;
            this.enumeration = enumeration;
        }

        public Mtc_Parameter(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            _name = mtcSpaceHelper.getAttribute("name");
            _optional = mtcSpaceHelper.getAttribute("optional").ToLower() == "true";
            if (!setQuantity(datasource))
            {
                setEnumeration(datasource);
            };
        }
    }

    public class Mtc_Parameters : IEnumerable<Mtc_Parameter>
    {
        private List<Mtc_Parameter> parameters = new List<Mtc_Parameter>();

        public Mtc_Parameter this[int index]
        {
            get { return parameters[index]; }
        }

        public Mtc_Parameter this[string name]
        {
            get {
                var set = parameters.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public Mtc_Parameter add(Mtc_Parameter value)
        {
            parameters.Add(value);
            return value;
        }

        public Mtc_Parameter add(string name, Mtc_Enumeration enumeration, Boolean optional)
        {
            Mtc_Parameter new_parameter = new Mtc_Parameter(name, enumeration, optional);
            parameters.Add(new_parameter);
            return new_parameter;
        }

        public Mtc_Parameter add(string name, string quantity, Boolean optional)
        {
            Mtc_Parameter new_parameter = new Mtc_Parameter(name, quantity, optional);
            parameters.Add(new_parameter);
            return new_parameter;
        }

        public int Count()
        {
            return parameters.Count();
        }

        private void loadParameters(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("Parameter");
            foreach(XElement el in els)
            {
                Mtc_Parameter parameter = new Mtc_Parameter(el);
                parameters.Add(parameter);
            }
        }
        public void writeTo(XElement Parent)
        {
            foreach (Mtc_Parameter parameter in parameters) {
                parameter.writeTo(Parent);
            }
        }

        public Mtc_Parameters() { } // Needed when Mtc_ProcessType is null.  Returns an empty Mtc_ProcessParameters

        public Mtc_Parameters(XElement datasource)
        {
            loadParameters(new MtcSpaceHelper(datasource));
        }
 
        public static Mtc_Parameters createDistinctUnion(Mtc_Parameters a, Mtc_Parameters b)
        {
            var union =  a.Union(b, new Mtc_Parameter.Mtc_ParameterComparer());
            Mtc_Parameters result = new Mtc_Parameters();
            result.parameters = union.ToList();
            return result;
        }

        public IEnumerator<Mtc_Parameter> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IParameterSource
    {
        Mtc_Parameters Parameters { get; }
    }

    public class Mtc_ProcessType : IParameterSource
    {
        private Unc_ProcessType parent = null;
        private string name = "";
        private Mtc_Documentation documentation = null;
        private Mtc_ProcessResults processResults = null;
        private Mtc_Parameters parameters = null;
        private Mtc_Documentation documenation = null;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Mtc_ProcessResults ProcessResults
        {
            get { return processResults; }
        }

        public Mtc_Parameters Parameters
        {
            get { return parameters; }
        }

        public Mtc_Documentation Documentation
        {
            get { return documentation; }
            set { documentation = value; }
        }

        public IList<string> ResultTypes{
            get { return processResults.Select(x => x.Quantity.name).ToList(); }
        }

        public UomDataSource.Quantity getQuantity()
        {
            UomDataSource.Quantity result = null;
            if (processResults.Count() == 1)
            {
                var qty = processResults[0].Quantity;
                result = (qty != null) ? UomDataSource.getQuantity(qty.name) : null;
            }
            return result; 
        }

        public UomDataSource.Quantity getQuantity(string parameterName)
        {
            UomDataSource.Quantity result = null;
            var procResult = processResults[parameterName]; 
            if (procResult != null) {
                var qty = procResult.Quantity;
                result = (qty != null) ? UomDataSource.getQuantity(qty.name) : null;
            } else {
                var param = Parameters[parameterName];
                if (param != null)
                {
                    var qty = param.Quantity;
                    if (qty != null)
                    {
                        result = UomDataSource.getQuantity(qty.name);
                    }
                }
            }
            return result;
        }

        public void writeTo(XElement UncProcessType)
        {
            MtcSpaceHelper UncProcessTypeHelper = new MtcSpaceHelper(UncProcessType);
            var ProcessType = UncProcessTypeHelper.addChild("ProcessType");
            processResults.writeTo(ProcessType.Element);
            //parameters.writeTo(ProcessType.Element);
            //documenation.writeTo(ProcessType.Element);
        }

        public Mtc_ProcessType(Unc_ProcessType parent) {
            this.parent = parent;
            name = parent.name;
            processResults = new Mtc_ProcessResults();
            parameters = new Mtc_Parameters();
            documenation = new Mtc_Documentation();
        }

        public Mtc_ProcessType(Unc_ProcessType parent, XElement datasource)
        {
            try
            {
                MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
                this.parent = parent; 
                name = mtcSpaceHelper.getAttribute("name");
                processResults = new Mtc_ProcessResults(datasource);
                parameters = new Mtc_Parameters(datasource);
                var el = mtcSpaceHelper.getElement("Documentation");
                if (el != null) documenation = new Mtc_Documentation(el);
            }
            catch (Exception e)
            {
                throw e;
            }
        }    
    }


    public class Mtc_Symbol
    {
        private string _parameter = "";
        private string _type = "";
        private UomDataSource.Quantity quantity = null;

        public string parameter
        {
            get { return _parameter; }
            set { _parameter = value; }
        }

        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

        public UomDataSource.Quantity Quantity
        {
            get { return quantity; }
        }

        private void setQuantity(IParameterSource parameterSource)
        {
            var param = parameterSource.Parameters[_parameter];
            if (param != null) quantity = param.Quantity;
        }

        public void writeTo(XElement Function) 
        {
            new MtcSpaceHelper(Function).addChild("Symbol").setAttribute("parameter", parameter).setAttribute("type", type);
        }

        private Mtc_Symbol() { }

        public Mtc_Symbol(XElement datasource, IParameterSource parameterSource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            _parameter = mtcSpaceHelper.getAttribute("parameter");
            _type = mtcSpaceHelper.getAttribute("type");
            setQuantity(parameterSource);
        }

    }

    public class Mtc_Symbols : IEnumerable<Mtc_Symbol>
    {
        private List<Mtc_Symbol> symbols = new List<Mtc_Symbol>();

        public Mtc_Symbol this[int index]
        {
            get { return symbols[index]; }
        }

        public Mtc_Symbol this[string parameterName]
        {
            get { 
                var set = symbols.Where(x => x.parameter == parameterName);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return symbols.Count();
        }

        private void loadSymbols(MtcSpaceHelper mtcSpaceHelper, IParameterSource parameterSource)
        {
            var els = mtcSpaceHelper.getElements("Symbol");
            foreach(XElement el in els)
            {
                Mtc_Symbol symbol = new Mtc_Symbol(el, parameterSource);
                symbols.Add(symbol);
            }
        }

        public void writeTo(XElement Function)
        {
            foreach (Mtc_Symbol symbol in symbols)
            {
                symbol.writeTo(Function);
            }
        }

        private Mtc_Symbols() { }

        public Mtc_Symbols(XElement datasource, IParameterSource parameterSource)
        {
            loadSymbols(new MtcSpaceHelper(datasource), parameterSource);
        }

        public IEnumerator<Mtc_Symbol> GetEnumerator()
        {
            return symbols.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

 

    public class Mtc_Function 
    {

        private string _function_name = "";
        private string expression = "";
        private Mtc_Symbols symbolDefinitions = null;
        private EvaluationEngine evaluator = new EvaluationEngine();
        private UomDataSource.Quantity quantity = null;

        public string function_name
        {
            get { return _function_name; }
            set { _function_name = value; }
        }

        public string Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        public UomDataSource.Quantity Quantity 
        {
            get { return quantity; }
        }

        public Mtc_Symbols SymbolDefinitions
        {
            get { return symbolDefinitions; }
        }

        protected void loadExpression(MtcSpaceHelper mtcSpaceHelper)
        {
            var expressionElement = mtcSpaceHelper.getElement("Expression");
            if (expressionElement != null) expression = fixWhiteSpace(expressionElement.Value);
            evaluator.Parse(expression);
        }

        public IList<string> ExpressionSymbols
        {
            get { return evaluator.GetVariables().Keys.ToList(); }
        }

        public IList<string> Variables
        {
            get { return SymbolDefinitions.Where(x => x.type == "Variable").Select(y => y.parameter).ToList(); }
        }

        public IList<string> Constants
        {
            get { return SymbolDefinitions.Where(x => x.type == "Constant").Select(y => y.parameter).ToList(); }
        }

        public void setSymbol(string name, double value) {
            evaluator.SetVariable(name, value);
        }

        public void setSymbol(string name, int value)
        {
            evaluator.SetVariable(name, value);
        }

        public decimal? evaluate()
        {
            return Math.Round((decimal)evaluator.Execute(), 28);
        }

        private string fixWhiteSpace(string raw)
        {
            string r1, r2, r3;
            r3 = raw.Trim();
            do {
               r1 = Regex.Replace(r3, "\t", " ");
               r2 = Regex.Replace(r1, "\n", " ");
               r3 = Regex.Replace(r2, "  ", " ");
            } while (r1 != r3);
            return r3;
        }

        protected void setQuantity(MtcSpaceHelper mtcSpaceHelper)
        {
            var resultElement = mtcSpaceHelper.getElement("Result");
            if (resultElement != null)
            {
                var qtyElement = new UomSpaceHelper(resultElement).getElement("Quantity");
                if (qtyElement != null)
                {
                    string qty = new UomSpaceHelper(qtyElement).getAttribute("name");
                    quantity = UomDataSource.getQuantity(qty);
                }
            }
        }

        public void writeTo(XElement Parent)
        {
            var Function = new MtcSpaceHelper(Parent).addChild("Function").setAttribute("function_name", function_name);
            Function.addChild("Expression").Value = expression;
            quantity.writeTo(Function.addChild("Result").Element);
            symbolDefinitions.writeTo(Function.Element);
        } 

        private Mtc_Function() { }

        public Mtc_Function(XElement functionElement, IParameterSource parameterSource)
        {
            var Function = new MtcSpaceHelper(functionElement);
            _function_name = Function.getAttribute("function_name");
            loadExpression(Function);
            setQuantity(Function);
            symbolDefinitions = new Mtc_Symbols(Function.getElement(), parameterSource);
        }    
    }

    public class Mtc_CMCUncertainty : Mtc_Function {

        public Mtc_CMCUncertainty(XElement datasource, Mtc_Technique technique)
            : base(datasource, technique){}  
    }

    public class Mtc_Functions : IEnumerable<Mtc_Function>
    {
        private List<Mtc_Function> functions = new List<Mtc_Function>();

        public Mtc_Function this[int index]
        {
            get { return functions[index]; }
        }

        public Mtc_Function this[string name]
        {
            get
            {
                var set = functions.Where(x => x.function_name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return functions.Count();
        }

        private void loadFunctions(MtcSpaceHelper mtcSpaceHelper, IParameterSource parameterSource)
        {
            var els = mtcSpaceHelper.getElements("Function");
            foreach (XElement el in els)
            {
                Mtc_Function uncertainty = new Mtc_Function(el, parameterSource);
                functions.Add(uncertainty);
            }
        }

        public void writeTo(XElement Technique)
        {
            foreach (Mtc_Function function in functions)
            {
                function.writeTo(Technique);
            }
        }

        public Mtc_Functions() { }

        public Mtc_Functions(XElement datasource, IParameterSource parameterSource)
        {
            loadFunctions(new MtcSpaceHelper(datasource), parameterSource);
        }

        public IEnumerator<Mtc_Function> GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_CMCUncertainties : IEnumerable<Mtc_CMCUncertainty>
    {
        private List<Mtc_CMCUncertainty> uncertainties = new List<Mtc_CMCUncertainty>();

        public Mtc_CMCUncertainty this[int index]
        {
            get { return uncertainties[index]; }
        }

        public Mtc_CMCUncertainty this[string name]
        {
            get { 
                var set = uncertainties.Where(x => x.function_name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return uncertainties.Count();
        }

        private void loadUncertainties(MtcSpaceHelper mtcSpaceHelper, Mtc_Technique technique)
        {
            var els = mtcSpaceHelper.getElements("CMCUncertainty");
            foreach (XElement el in els)
            {
                Mtc_CMCUncertainty uncertainty = new Mtc_CMCUncertainty(el, technique);
                uncertainties.Add(uncertainty);
            }
        }

        public Mtc_CMCUncertainties() {}

        public Mtc_CMCUncertainties(XElement datasource, Mtc_Technique technique)
        {
            loadUncertainties(new MtcSpaceHelper(datasource), technique);
        }

        public void writeTo(XElement Technique)
        {
            foreach (Mtc_CMCUncertainty uncertainty in uncertainties)
            {
                uncertainty.writeTo(Technique);
            }
        }

        public IEnumerator<Mtc_CMCUncertainty> GetEnumerator()
        {
            return uncertainties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_Role
    {
        private string _name = "";
        private List<string> deviceTypes = new List<string>();

        public string name
        {
            get { return _name; }
            set { name = value; }
        }

        public ICollection<string> DeviceTypes
        {
            get { return deviceTypes; }
        }

        private void loadDeviceTypes(MtcSpaceHelper mtcSpaceHelper)
        {
            var el1 = mtcSpaceHelper.getElement("DeviceTypes");
            if (el1 != null)
            {
                deviceTypes = new UncSpaceHelper(el1).getValues("DeviceType");
            }
            else
            {
                var el2 = mtcSpaceHelper.getElement("DeviceType");
                if (el2 != null) deviceTypes.Add(el2.Value);
            }
        }

        public void writeTo(XElement RequiredEquipment)
        {
            var Role = new MtcSpaceHelper(RequiredEquipment).addChild("Role").setAttribute("name", name);
            if (deviceTypes.Count() == 1)
            {
                Role.addChild("DeviceType").Value = deviceTypes[0];
            }
            else if (deviceTypes.Count > 1)
            {
                var DeviceTypes = Role.addChild("DeviceTypes");
                foreach (string value in deviceTypes)
                {
                    DeviceTypes.addChild("DeviceType").Value = value;
                }
            }
        }

        private Mtc_Role() { }

        public Mtc_Role(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            _name = mtcSpaceHelper.getAttribute("name");
            loadDeviceTypes(mtcSpaceHelper);
        }
    }

    public class Mtc_Roles : IEnumerable<Mtc_Role>
    {
        private List<Mtc_Role> roles = new List<Mtc_Role>();

        public Mtc_Role this[int index]
        {
            get { return roles[index]; }
        }

        public Mtc_Role this[string name]
        {
            get { 
                var set = roles.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return roles.Count();
        }

        private void loadRoles(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("Role");
            foreach(XElement el in els) {
                Mtc_Role role = new Mtc_Role(el);
                roles.Add(role);
            }
        }

        public void writeTo(XElement RequiredEquipment)
        {
            foreach (Mtc_Role role in roles)
            {
                role.writeTo(RequiredEquipment);
            }
        }

        public Mtc_Roles() { }

        public Mtc_Roles(XElement datasource)
        {
            loadRoles(new MtcSpaceHelper(datasource));
        }

        public IEnumerator<Mtc_Role> GetEnumerator()
        {
            return roles.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_RequiredEquipment 
    {

        private Mtc_Roles roles = null;

        public Mtc_Roles Roles
        {
            get { return roles; }
        }

        public void writeTo(XElement Technique) {
            var RequiredEquipment = new MtcSpaceHelper(Technique).addChild("RequiredEquipment");
            roles.writeTo(RequiredEquipment.Element);
        }
         
        public Mtc_RequiredEquipment() {
            this.roles = new Mtc_Roles();
        }

        public Mtc_RequiredEquipment(XElement datasource)
        {
            roles = new Mtc_Roles(datasource);
        }
    }


    public class Mtc_Range_Boundary : AbstractValue
    {
        private string _test = "";

        public enum RangeType {Result, Parameter};

        public string test
        {
            get { return _test; }
            set { _test = value; }
        }

        protected Mtc_Range_Boundary() { }

        public Mtc_Range_Boundary(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
        {
            try
            {
                switch (rType)
                {
                    case RangeType.Result:
                        int rngCnt = processType.ProcessResults.Count();
                        if (rngCnt == 1)
                            quantity = processType.getQuantity();
                        else
                            quantity = processType.getQuantity(rangeName);
                        break;
                    case RangeType.Parameter:
                        quantity = processType.getQuantity(rangeName);
                        break;
                    default:
                        break;
                }                
                MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
                test = mtcSpaceHelper.getAttribute("test");
                uom_alternative = mtcSpaceHelper.getAttribute("uom_alternative");
                symbol = mtcSpaceHelper.getAttribute("uom_alias_symbol");
                format = mtcSpaceHelper.getAttribute("format");
                if (datasource != null) valueString = datasource.Value;

            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Mtc_Range_End : Mtc_Range_Boundary
    {
        override public void writeTo(XElement Parent)
        {
            var End = new MtcSpaceHelper(Parent).addChild("End");
            base.writeTo(End.Element);
            End.setAttribute("test", this.test);
        }
        private Mtc_Range_End() { }

        public Mtc_Range_End(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
            : base(datasource, processType, rangeName, rType) { }
    }

    public class Mtc_Range_Start : Mtc_Range_Boundary 
    {

        override public void writeTo(XElement Parent)
        {         
            var Start = new MtcSpaceHelper(Parent).addChild("Start");
            Start.setAttribute("test", this.test);
            base.writeTo(Start.Element);
        }

        private Mtc_Range_Start() { }

        public Mtc_Range_Start(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
            : base(datasource, processType, rangeName, rType) { }
    }

    public class Mtc_Range 
    {
        private string _name = "";   
        private Mtc_Range_Start start = null;
        private Mtc_Range_End end = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Mtc_Range_Start Start
        {
            get { return start; }
            set { start = value; }
        }

        public Mtc_Range_End End
        {
            get { return end; }
            set { end = value; }
        }

        public void writeTo(XElement Technique)
        {
            var ResultRange = new MtcSpaceHelper(Technique).addChild("ResultRange");
            if (name != "") ResultRange.setAttribute("name", name);
            start.writeTo(ResultRange.Element);
            end.writeTo(ResultRange.Element);
        }
        
        private Mtc_Range() { }

        public Mtc_Range(XElement datasource, Mtc_ProcessType processType, Mtc_Range_Boundary.RangeType rType)
        {
            try
            {
                MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
                name = mtcSpaceHelper.getAttribute("name");
                var el1 = mtcSpaceHelper.getElement("Start");
                var el2 = mtcSpaceHelper.getElement("End");
                if (el1 != null) start = new Mtc_Range_Start(el1, processType, name, rType);
                if (el2 != null) end = new Mtc_Range_End(el2, processType, name, rType);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }


    public class Mtc_ResultRanges : IEnumerable<Mtc_Range>
    {
        private List<Mtc_Range> ranges = new List<Mtc_Range>();
        private Mtc_ProcessType processType = null;

        public Mtc_Range this[int index]
        {
            get { return ranges[index]; }
        }

        public Mtc_Range this[string name]
        {
            get {
                var set = ranges.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return ranges.Count();
        }

        private void loadRanges(MtcSpaceHelper mtcSpaceHelper, Mtc_ProcessType processType)
        {
            var els = mtcSpaceHelper.getElements("ResultRange");
            foreach (XElement el in els)
            {
                Mtc_Range range = new Mtc_Range(el, processType, Mtc_Range_Boundary.RangeType.Result);
                ranges.Add(range);
            }
        }

        public void writeTo(XElement Technique)
        {
            foreach (Mtc_Range range in ranges)
            {
                range.writeTo(Technique);
            }
        }

        public Mtc_ResultRanges() { 
        
        }

        public Mtc_ResultRanges(XElement datasource, Mtc_ProcessType processType)
        {
            this.processType = processType;
            loadRanges(new MtcSpaceHelper(datasource),  processType);
        }

        public IEnumerator<Mtc_Range> GetEnumerator()
        {
            return ranges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_ParameterRanges : IEnumerable<Mtc_Range>
    {
        private List<Mtc_Range> ranges = new List<Mtc_Range>();
        private Mtc_ProcessType processType = null;

        public Mtc_Range this[int index]
        {
            get { return ranges[index]; }
        }

        public Mtc_Range this[string name]
        {
            get {
                var set = ranges.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return ranges.Count();
        }

        private void loadRanges(MtcSpaceHelper mtcSpaceHelper, Mtc_ProcessType processType)
        {
            var els = mtcSpaceHelper.getElements("ParameterRange");
            foreach (XElement el in els)
            {
                Mtc_Range range = new Mtc_Range(el, processType, Mtc_Range_Boundary.RangeType.Parameter);
                ranges.Add(range);
            }
        }

        public void writeTo(XElement Technique)
        {
            foreach (Mtc_Range range in ranges) {
                range.writeTo(Technique);
            }
        }

        public Mtc_ParameterRanges() { }

        public Mtc_ParameterRanges(XElement datasource, Mtc_ProcessType processType)
        {
            this.processType = processType;
            loadRanges(new MtcSpaceHelper(datasource), processType);
        }

        public IEnumerator<Mtc_Range> GetEnumerator()
        {
            return ranges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class Mtc_Technique : IParameterSource
    {
        private Unc_Technique parent = null;
        private string name = "";
        private string process = "";
        private Mtc_ResultRanges resultRanges = null;
        private Mtc_Parameters parameters = null;
        private Mtc_ParameterRanges parameterRanges = null;
        private Mtc_RequiredEquipment requiredEquipment = null;
        private Mtc_Functions functions = null;
        private Mtc_CMCUncertainties cmcUncertainties = null;
        private Mtc_Documentation documentation = null;
        
        private Unc_CMCs cMCs;
        private Mtc_ProcessType processType = null;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ProcessTypeName
        {
            get { return process; }
            set { process = value; }
        }

        public Mtc_ProcessType ProcessType
        {
            get { return processType; }
        }

        public Mtc_Parameters Parameters
        {
            get { return parameters; }
        }


        public Mtc_ResultRanges ResultRanges
        {
            get { return resultRanges; }
        }

        public Mtc_ParameterRanges ParameterRanges
        {
            get { return parameterRanges; }
        }

        public Mtc_RequiredEquipment RequiredEquipment
        {
            get { return requiredEquipment; }
            //set { requiredEquipment = value; }
        }

        public Mtc_CMCUncertainties CMCUncertainties
        {
            get { return cmcUncertainties; }
            set { cmcUncertainties = value; }
        }

        public Mtc_Documentation Documentation
        {
            get { return documentation; }
            //set { documentation = value; }
        }

        public void writeTo(XElement UncTechnique)
        {
            var Technique = new MtcSpaceHelper(UncTechnique).addChild("Technique").setAttribute("name", name).setAttribute("process", process);
            resultRanges.writeTo(Technique.Element);
            parameters.writeTo(Technique.Element);
            parameterRanges.writeTo(Technique.Element);
            requiredEquipment.writeTo(Technique.Element);
            if (functions != null) functions.writeTo(Technique.Element);
            cmcUncertainties.writeTo(Technique.Element);
            documentation.writeTo(Technique.Element);
        }

        private void setProcessType(Unc_CMCs cMCs, string processName)
        {
            var mtcProcessTypes = cMCs.ProcessTypes.Select( x => x.ProcessType).Where(y => y.Name == processName);
            processType = (mtcProcessTypes.Count() > 0) ? mtcProcessTypes.First() : null;
        }

        private void loadParameters(Mtc_Parameters procParameters, Mtc_Parameters thisParameters)
        {
            parameters = Mtc_Parameters.createDistinctUnion(procParameters, thisParameters);
        }

        public UomDataSource.Quantity getQuantity(string parameterName)
        {

            UomDataSource.Quantity result = null;
            var param = Parameters[parameterName];
            if (param != null)
            {
                var qty = param.Quantity;
                if (qty != null)
                {
                    result = UomDataSource.getQuantity(qty.name);
                }
            }
            return result;
        }

        public Mtc_Technique(Unc_Technique parent, Unc_CMCs cMCs)
        {
            this.parent = parent;
            this.cMCs = cMCs;
            this.name = "";
            this.process = "";
            this.processType = null;
            this.parameters = new Mtc_Parameters();
            this.resultRanges = new Mtc_ResultRanges();
            this.parameterRanges = new Mtc_ParameterRanges();
            this.requiredEquipment = new Mtc_RequiredEquipment();
            this.cmcUncertainties = new Mtc_CMCUncertainties();
            this.documentation = new Mtc_Documentation();
        }

        public Mtc_Technique(XElement datasource, Unc_Technique parent, Unc_CMCs cMCs)
        {
            try
            {
                this.parent = parent;
                this.cMCs = cMCs;
                MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
                name = mtcSpaceHelper.getAttribute("name");
                process = mtcSpaceHelper.getAttribute("process");
                setProcessType(cMCs, process);
                loadParameters(this.processType.Parameters, new Mtc_Parameters(datasource));
                resultRanges = new Mtc_ResultRanges(datasource, processType);
                parameterRanges = new Mtc_ParameterRanges(datasource, processType);
                var el1 = mtcSpaceHelper.getElement("RequiredEquipment");
                var el2 = mtcSpaceHelper.getElement("Documentation");
                if (el1 != null) requiredEquipment = new Mtc_RequiredEquipment(el1);
                if (el2 != null) cmcUncertainties = new Mtc_CMCUncertainties(datasource, this);
                documentation = new Mtc_Documentation(el2);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Unc_ConstantValue : AbstractValue 
    {

        private string _const_parameter_name = "";
        private Unc_Template template = null;

        public string const_parameter_name
        {
            get { return _const_parameter_name; }
            set { _const_parameter_name = value; }
        }

        public override void writeTo(XElement Range)
        {
            var ConstantValue = new UncSpaceHelper(Range).addChild("ConstantValue").setAttribute("const_parameter_name", const_parameter_name);
            base.writeTo(ConstantValue.Element);
        }

        public Unc_ConstantValue(XElement datasource, Unc_Template template, string functionName)
        {
            try
            {
                this.template = template;
                UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
                const_parameter_name = uncSpaceHelper.getAttribute("const_parameter_name");
                quantity = template.getQuantity(_const_parameter_name);
                uom_alternative = uncSpaceHelper.getAttribute("uom_alternative");
                symbol = uncSpaceHelper.getAttribute("uom_alias_symbol");
                format = uncSpaceHelper.getAttribute("format");
                loadValue(datasource);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }    
        }
    }

    public class Unc_ConstantValues : IEnumerable<Unc_ConstantValue>
    {
        private List<Unc_ConstantValue> constants = new List<Unc_ConstantValue>();
        private Unc_Template template = null;

        public Unc_ConstantValue this[int index]
        {
            get { return constants[index]; }
        }

        public Unc_ConstantValue this[string name]
        {
            get { 
                var set = constants.Where(x => x.const_parameter_name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return constants.Count();
        }

        private void loadConstants(UncSpaceHelper uncSpaceHelper, Unc_Template template, string functionName)
        {
            var els = uncSpaceHelper.getElements("ConstantValue");
            foreach (XElement el in els)
            {
                Unc_ConstantValue constant = new Unc_ConstantValue(el, template, functionName);
                constants.Add(constant);
            }
        }

        public void writeTo(XElement Range)
        {
            foreach (Unc_ConstantValue constant in constants)
            {
                constant.writeTo(Range);
            }
        }

        private Unc_ConstantValues() { }

        public Unc_ConstantValues(XElement datasource, Unc_Template template, string functionName)
        {
            this.template = template;
            loadConstants(new UncSpaceHelper(datasource), template, functionName);
        }

        public IEnumerator<Unc_ConstantValue> GetEnumerator()
        {
            return constants.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_Range_Boundary : AbstractValue
    {
        private string _test = "";
        private Unc_Template template = null;

        public string test
        {
            get { return _test; }
            set { _test = value; }
        }

        public override void writeTo(XElement myElement) {
            var me = new XmlNameSpaceElement(myElement);    
            me.setAttribute("test", test); 
            base.writeTo(myElement);       
        }

        protected Unc_Range_Boundary() { }

        public Unc_Range_Boundary(XElement datasource, Unc_Template template, string variableName)
        {
            try
            {
                this.template = template;
                UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
                test = uncSpaceHelper.getAttribute("test");
                quantity = template.getQuantity(variableName);
                uom_alternative = uncSpaceHelper.getAttribute("uom_alternative");
                symbol = uncSpaceHelper.getAttribute("uom_alias_symbol");
                format = uncSpaceHelper.getAttribute("format");
                loadValue(datasource);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Unc_Range_End : Unc_Range_Boundary 
    {
         public override void writeTo(XElement Parent) {
            var End = new UncSpaceHelper(Parent).addChild("End");
            base.writeTo(End.Element);
        }

        private Unc_Range_End() { }

        public Unc_Range_End(XElement datasource, Unc_Template template, string variableName)
            : base(datasource, template, variableName) { }
    }

    public class Unc_Range_Start : Unc_Range_Boundary 
    {

        public override void writeTo(XElement Parent) {
            var Start = new UncSpaceHelper(Parent).addChild("Start");
            base.writeTo(Start.Element);
        }

        private Unc_Range_Start() { }

        public Unc_Range_Start(XElement datasource, Unc_Template template, string variableName)
            : base(datasource, template, variableName) { }
    }

    public class Unc_Range 
    {
        private Unc_Range_Start start = null;
        private Unc_Range_End end = null;
        private Unc_ConstantValues constants = null;
        private Unc_Ranges ranges = null;
        private Unc_Template template = null;
        private string variable_name = "";
        private string variable_type = "";

        public string Variable_name
        {
            get { return variable_name; }
            //set { variable_name = value; }
        }

        public string Variable_type
        {
            get { return variable_type; }
            //set { variable_type = value; }
        }

        public Unc_Range_Start Start
        {
            get { return start; }
            //set { start = value; }
        }

        public Unc_Range_End End
        {
            get { return end; }
            //set { end = value; }
        }

        public Unc_ConstantValues ConstantValues
        {
            get { return constants; }
            set { constants = value; }
        }

        public Unc_Ranges Ranges
        {
            get { return ranges; }
            set { ranges = value; }
        }

        public void writeTo(XElement Ranges)
        {
            var Range = new UncSpaceHelper(Ranges).addChild("Range");
            start.writeTo(Range.Element);
            end.writeTo(Range.Element);
            if (ranges.Count() > 0)
            {
                ranges.writeTo(Range.Element);
            }
            else { 
                constants.writeTo(Range.Element);
            }
        }

        protected Unc_Range() { }

        public Unc_Range(XElement datasource, Unc_Template template, Unc_Ranges parent, string functionName)
        {
            try
            {
                this.template = template;
                variable_name = parent.variable_name;
                variable_type = parent.variable_type;
                UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
                var el1 = uncSpaceHelper.getElement("Start");
                var el2 = uncSpaceHelper.getElement("End");
                if (el1 != null) start = new Unc_Range_Start(el1, template, Variable_name);
                if (el2 != null) end = new Unc_Range_End(el2, template, Variable_name);
                constants = new Unc_ConstantValues(datasource, template, functionName);
                var rgsElement = uncSpaceHelper.getElement("Ranges");
                ranges = (rgsElement != null) ? new Unc_Ranges(rgsElement, template, functionName) : new Unc_Ranges();
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }            
        }
    }

    public class Unc_Ranges : IEnumerable<Unc_Range>
    {
        private List<Unc_Range> ranges = new List<Unc_Range>();
        private Unc_Template template = null;
        private string _variable_name = "";
        private string _variable_type = "";
       
        public string variable_name
        {
            get { return _variable_name; }
            set { _variable_name = value; }
        }

        public string variable_type
        {
            get { return _variable_type; }
            set { _variable_type = value; }
        }

        public Unc_Range this[int index]
        {
            get { return ranges[index]; }
        }

        public Unc_Ranges this[string name]
        {
            get { return new Unc_Ranges(ranges.Where(x => x.Variable_name == name).ToList()); }
        }

        public int Count()
        {
            return ranges.Count();
        }

        private void loadRanges(UncSpaceHelper uncSpaceHelper, Unc_Template template, string functionName)
        {
            var els = uncSpaceHelper.getElements("Range");
            foreach (XElement el in els)
            {
                Unc_Range range = new Unc_Range(el, template, this, functionName);
                ranges.Add(range);
            }
        }

        public Unc_Ranges Add(Unc_Range Range)
        {
            ranges.Add(Range);
            return this;
        }

        public Unc_Ranges Remove(Unc_Range Range)
        {
            if (ranges.Contains(Range)) ranges.Remove(Range);
            return this;
        }

        public void writeTo(XElement Parent)
        {
            var Ranges = new UncSpaceHelper(Parent).addChild("Ranges").setAttribute("variable_name", variable_name).setAttribute("variable_type", variable_type);
            foreach (Unc_Range range in ranges)
            {
                range.writeTo(Ranges.Element);
            }
        }

        public Unc_Ranges() {} // Needed if parent has no Ranges element.  Return empty Unc_Ranges 

        public Unc_Ranges(Unc_Template Template, string FunctionName, String VariableName, String VariableType, IEnumerable<Unc_Range> Ranges)
        {
            this.template = Template;
            this._variable_name = VariableName;
            this._variable_type = VariableType;
            ranges.AddRange(Ranges);
        }

        public Unc_Ranges(XElement datasource, Unc_Template template, string functionName) 
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            _variable_name = uncSpaceHelper.getAttribute("variable_name");
            _variable_type = uncSpaceHelper.getAttribute("variable_type");
            loadRanges(uncSpaceHelper, template, functionName);
        }

        private  Unc_Ranges(List<Unc_Range>  ranges)
        {
            this.ranges = ranges;
        }

        public IEnumerator<Unc_Range> GetEnumerator()
        {
            return ranges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_RangeOverride
    {     
        private string _name = "";
        private Unc_Range_Start start = null;
        private Unc_Range_End end = null;
        private Unc_Template template = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Unc_Range_Start Start
        {
            get { return start; }
            set { start = value; }
        }

        public Unc_Range_End End
        {
            get { return end; }
            set { end = value; }
        }

        public void writeTo(XElement Technique) {
            var RangeOverride = new UncSpaceHelper(Technique).addChild("RangeOverride").setAttribute("name", name);
            start.writeTo(RangeOverride.Element);
            end.writeTo(RangeOverride.Element);
        }

        protected Unc_RangeOverride() { }

        public Unc_RangeOverride(XElement datasource, Unc_Template template)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            _name = uncSpaceHelper.getAttribute("name");
            var el1 = uncSpaceHelper.getElement("Start");
            var el2 = uncSpaceHelper.getElement("End");
            if (el1 != null) start = new Unc_Range_Start(el1, template, _name);
            if (el2 != null) end = new Unc_Range_End(el2, template, _name);
        }
    }

    public class Unc_ParameterRangeOverrides : IEnumerable<Unc_RangeOverride>
    {
        private List<Unc_RangeOverride> rangeOverrides = new List<Unc_RangeOverride>();
        private Unc_Template template = null;

        public Unc_RangeOverride this[int index]
        {
            get { return rangeOverrides[index]; }
        }

        public Unc_RangeOverride this[string name]
        {
            get { 
                var set = rangeOverrides.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return rangeOverrides.Count();
        }

        private void loadRangeOverrides(UncSpaceHelper uncSpaceHelper, Unc_Template template)
        {
            var els = uncSpaceHelper.getElements("ParameterRangeOverride");
            foreach (XElement el in els){
                Unc_RangeOverride rangeOverride = new Unc_RangeOverride(el, template);
                rangeOverrides.Add(rangeOverride);
            }
        }

        public void writeTo(XElement Technique)
        {
            foreach (Unc_RangeOverride rangeOverride in rangeOverrides) {
                rangeOverride.writeTo(Technique);
            }
        }
        
        public Unc_ParameterRangeOverrides(Unc_Template template) {
            this.template = template;
        }

        public Unc_ParameterRangeOverrides(XElement datasource, Unc_Template template)
        {
            this.template = template;
            loadRangeOverrides(new UncSpaceHelper(datasource), template);
        }

        public IEnumerator<Unc_RangeOverride> GetEnumerator()
        {
            return rangeOverrides.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_ResultOverrides : IEnumerable<Unc_RangeOverride>
    {
        private List<Unc_RangeOverride> rangeOverrides = new List<Unc_RangeOverride>();
        private Unc_Template template = null;

        public Unc_RangeOverride this[int index]
        {
            get { return rangeOverrides[index]; }
        }

        public Unc_RangeOverride this[string name]
        {
            get { 
                var set = rangeOverrides.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }


        public int Count()
        {
            return rangeOverrides.Count();
        }

        private void loadRangeOverrides(UncSpaceHelper uncSpaceHelper, Unc_Template template)
        {
            var els = uncSpaceHelper.getElements("ResultRangeOverride");
            foreach (XElement el in els){
                Unc_RangeOverride rangeOverride = new Unc_RangeOverride(el, template);
                rangeOverrides.Add(rangeOverride);
            }
        }

        public void writeTo(XElement Technique) {
            foreach (Unc_RangeOverride rangeOverride in rangeOverrides) {
                rangeOverride.writeTo(Technique);
            }
        }

        public Unc_ResultOverrides(Unc_Template template)
        {
            this.template = template;
        }

        public Unc_ResultOverrides(XElement datasource, Unc_Template template)
        {
            this.template = template;
            loadRangeOverrides(new UncSpaceHelper(datasource), template);
        }


        public IEnumerator<Unc_RangeOverride> GetEnumerator()
        {
            return rangeOverrides.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_TemplateTechnique
    {
        private string _name = "";
        private Unc_ResultOverrides resultRangeOverrides = null;
        private Unc_ParameterRangeOverrides parameterRangeOverrides = null;
        private Unc_Template template = null;

        public string name
        {
            get { return _name; }
            //set { name = value; }
        }

        public Unc_ResultOverrides ResultRangeOverrides
        {
            get { return resultRangeOverrides; }
        }
 
        public Unc_ParameterRangeOverrides ParameterRangeOverrides
        {
            get { return parameterRangeOverrides; }
        }

        public void writeTo(XElement Template) {
            var Technique = new UncSpaceHelper(Template).addChild("Technique").setAttribute("name", name);
            if (resultRangeOverrides != null) {
               resultRangeOverrides.writeTo(Technique.Element);
            }
            if (parameterRangeOverrides != null) {
               parameterRangeOverrides.writeTo(Technique.Element);
            }
        }

        public Unc_TemplateTechnique(Unc_Template template)
        {
            this.template = template;
            _name = "";
            resultRangeOverrides = new Unc_ResultOverrides(template);
            parameterRangeOverrides = new Unc_ParameterRangeOverrides(template);
        }

        public Unc_TemplateTechnique(XElement datasource, Unc_Template template)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            _name = uncSpaceHelper.getAttribute("name");
            resultRangeOverrides = new Unc_ResultOverrides(datasource, template);
            parameterRangeOverrides = new Unc_ParameterRangeOverrides(datasource, template);
        }
    }

    public class Unc_InfluenceQuantity
    {
        private string _name = "";
        private Uom_Quantity quantity = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Uom_Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public void writeTo(XElement Parent) {
            var InfluenceQuantity = new UncSpaceHelper(Parent).addChild("InfluenceQuantity").setAttribute("name", name);
            quantity.writeTo(InfluenceQuantity.Element);
        }

        private Unc_InfluenceQuantity() { }

        public Unc_InfluenceQuantity(String Name, Uom_Quantity Quantity)
        {
            this._name = Name;
            this.quantity = Quantity;
        }

        public Unc_InfluenceQuantity(XElement datasource)
        {
            _name = new UncSpaceHelper(datasource).getAttribute("name");
            var el = new UomSpaceHelper(datasource).getElement("Quantity");
            if (el != null) quantity = new Uom_Quantity(el);
        }
    }


    public class Unc_InfluenceQuantities : IEnumerable<Unc_InfluenceQuantity>
    {
        private List<Unc_InfluenceQuantity> quantities = new List<Unc_InfluenceQuantity>();

        public Unc_InfluenceQuantity this[int index]
        {
            get { return quantities[index]; }
        }

        public Unc_InfluenceQuantity this[string name]
        {
            get { 
                var set = quantities.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First(): null; 
            }
        }

        public int Count()
        {
            return quantities.Count();
        }

        private void loadQuantities(UncSpaceHelper uncSpaceHelper)
        {
            var el1 = uncSpaceHelper.getElement("InfluenceQuantities");
            if (el1 != null)
            {
                var els = new UncSpaceHelper(el1).getElements("InfluenceQuantity");
                foreach (var qty in els)
                {
                    Unc_InfluenceQuantity quantity = new Unc_InfluenceQuantity(qty);
                    quantities.Add(quantity);
                }
            }
            else
            {
                var el2 = uncSpaceHelper.getElement("InfluenceQuantity");
                if (el2 != null)
                {
                    Unc_InfluenceQuantity quantity = new Unc_InfluenceQuantity(el2);
                    quantities.Add(quantity);
                }
            }
        }

        public Unc_InfluenceQuantities Add(Unc_InfluenceQuantity Quantity) {
            quantities.Add(Quantity);
            return this;
        }

        public Unc_InfluenceQuantities Remove(Unc_InfluenceQuantity Quantity)
        {
            if (quantities.Contains(Quantity)) quantities.Remove(Quantity);
            return this;
        }

        public IEnumerator<Unc_InfluenceQuantity> GetEnumerator()
        {
            return quantities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void writeTo(XElement Template) {
            foreach (Unc_InfluenceQuantity quantity in quantities) {
                quantity.writeTo(Template);
            }
        }

        public Unc_InfluenceQuantities() { }

        public Unc_InfluenceQuantities(IEnumerable<Unc_InfluenceQuantity> Quantities)
        {
            quantities.AddRange(Quantities);
        }

        public Unc_InfluenceQuantities(XElement datasource)
        {
            loadQuantities(new UncSpaceHelper(datasource));
        }
    }

    public class Unc_Assertion
    {
        private string _type;
        private string name = "";
        private string value = "";
        static string[] valid_types = {"", "generic", "equipment" };

        public string type
        {
            get { return (_type =="") ? "generic" : _type; }
            set { _type = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Value
        {
            get { return this.value; }
            set {
                if (Unc_Assertion.valid_types.Contains(value))
                {
                   this.value = value; 
                } else
                {
                    throw new Exception("Invalid Assertion type");
                }
            }
        }

        private void loadName(XElement Assertion)
        {
            if (Assertion != null) name = Assertion.Value;
        }

        private void loadValue(XElement Assertion)
        {
            if (Assertion != null) value = Assertion.Value;
        }

        public void writeTo(XElement Case)
        {
            var Assertion = new UncSpaceHelper(Case).addChild("Assertion");
            if (_type != "") Assertion.setAttribute("type", type);
            Assertion.addChild("Name").Value = Name;
            Assertion.addChild("Value").Value = Value;
        }

        private Unc_Assertion() {}

        public Unc_Assertion(String Name, String Value) 
        {
            this.Name = Name;
            this.Value = Value;
        }

        public Unc_Assertion(XElement Assertion)
        {
            UncSpaceHelper AssertionHelper = new UncSpaceHelper(Assertion);
            _type = AssertionHelper.getAttribute("type");
            var el1 = AssertionHelper.getElement("Name");
            var el2 = AssertionHelper.getElement("Value");
            if (el1 != null) loadName(el1);
            if (el2 != null) loadValue(el2);
        }
    }

    public class Unc_Assertions : IEnumerable<Unc_Assertion>
    {
        private List<Unc_Assertion> assertions = new List<Unc_Assertion>();

        public Unc_Assertion this[int index]
        {
            get { return assertions[index];}
        }

        public Unc_Assertion this[string name]
        {
            get { 
                var set = assertions.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First(): null; 
            }
        }

        public int Count()
        {
            return assertions.Count();
        }

        private void loadAssertions(UncSpaceHelper uncSpaceHelper)
        {
            var els = uncSpaceHelper.getElements("Assertion");
            foreach (XElement el in els)
            {
                Unc_Assertion assertion = new Unc_Assertion(el);
                assertions.Add(assertion);
            }
        }

        public Unc_Assertions Add(Unc_Assertion Assertion)
        {
            assertions.Add(Assertion);
            return this;
        }

        public Unc_Assertions Remove(Unc_Assertion Assertion)
        {
            if (assertions.Contains(Assertion)) assertions.Remove(Assertion);
            return this;
        }

        public IEnumerator<Unc_Assertion> GetEnumerator()
        {
            return assertions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void writeTo(XElement Case)
        {
            foreach (Unc_Assertion assertion in assertions)
            {
                assertion.writeTo(Case);
            }
        }

        private Unc_Assertions() {}

        public Unc_Assertions(IEnumerable<Unc_Assertion> Assertions) {
            assertions.AddRange(Assertions);
        }

        public Unc_Assertions(XElement datasource)
        {
            loadAssertions(new UncSpaceHelper(datasource));
        }

    }

    public class Unc_Case
    {
        private Unc_Assertions assertions = null;
        private Unc_Ranges ranges = null;
        private Unc_Template template = null;

        public Unc_Assertions Assertions
        {
            get { return assertions; }
        }

        public Unc_Ranges Ranges
        {
            get { return ranges; }
        }

        public void writeTo(XElement Switch)
        {
            var Case = new UncSpaceHelper(Switch).addChild("Case");
            assertions.writeTo(Case.Element);
            ranges.writeTo(Case.Element);
        }

        private Unc_Case() {}

        public Unc_Case(Unc_Assertions Assertions, Unc_Template Template, String FunctionName, String VariableName, String VariableType, IEnumerable<Unc_Range> Ranges )
        {
            this.template = Template;
            this.assertions = Assertions;
            this.ranges = new Unc_Ranges(template, FunctionName, VariableName, VariableType, Ranges);
        }

        public Unc_Case(XElement datasource, Unc_Template template, string functionName)
        {
            try
            {
                this.template = template;
                this.assertions = new Unc_Assertions(datasource);
                var rgsElement = new UncSpaceHelper(datasource).getElement("Ranges");
                if (rgsElement != null)
                    ranges = new Unc_Ranges(rgsElement, template, functionName);
                else
                    ranges = new Unc_Ranges(datasource, template, functionName);

            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Unc_Cases : IEnumerable<Unc_Case>
    {
        private List<Unc_Case> cases = new List<Unc_Case>();
        private Unc_Template template = null;

        public Unc_Case this[int index]
        {
            get { return cases[index]; }
        }

        public int Count()
        {
            return cases.Count();
        }

        private void loadCases(UncSpaceHelper uncSpaceHelper, Unc_Template template, string functionName)
        {
            var el1 = uncSpaceHelper.getElement("Switch");
            if (el1 != null)
            {
                var els = new UncSpaceHelper(el1).getElements("Case");
                foreach (XElement el2 in els)
                {
                    Unc_Case _case = new Unc_Case(el2, template, functionName);
                    cases.Add(_case);
                }
            }
        }

        // ranges are loaded as a Unc_Case that happens to have no Assertion 
        // for that reason, ranges are always accessed through a case
        private void loadRanges(XElement datasource, Unc_Template template, string functionName)
        {
            var el = new UncSpaceHelper(datasource).getElement("Ranges");
            if (el != null)
            {
                Unc_Case _case = new Unc_Case(el, template, functionName);
                cases.Add(_case);
            }
        }

        public IEnumerator<Unc_Case> GetEnumerator()
        {
            return cases.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void writeTo(XElement CMCFunction)
        {
            var Function = new XmlNameSpaceElement(CMCFunction);
            if ((cases.Count == 1) && (cases[0].Assertions.Count() == 0)) {
                cases[0].Ranges.writeTo(CMCFunction);
            } else if (cases.Count > 1) {
                var Switch = new UncSpaceHelper(CMCFunction).addChild("Switch");
                foreach (Unc_Case _case in cases)
                {
                    _case.writeTo(Switch.Element);
                }
            }
        }

        private Unc_Cases() {}

        public Unc_Cases Add(Unc_Case Case)
        {
            cases.Add(Case);
            return this;
        }

        public Unc_Cases Remove(Unc_Case Case)
        {
            if (cases.Contains(Case)) cases.Remove(Case);
            return this;
        }



        public Unc_Cases(Unc_Template Template, IEnumerable<Unc_Case> Cases)
        {
            this.template = Template;
            this.cases.AddRange(Cases);
        }

        public Unc_Cases(XElement datasource, Unc_Template template, string functionName) 
        {
            try
            {
                this.template = template;
                loadCases(new UncSpaceHelper(datasource), template, functionName);
                if (cases.Count() == 0) 
                    loadRanges(datasource, template, functionName);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }


    }


    public class Unc_CMCFunction
    {
        private string _name = "";
        private Unc_Cases cases = null;
        private Unc_Template template = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Unc_Cases Cases
        {
            get { return cases; }
        }

        /// <summary>
        /// A consolidated list of all Assertion Names appearing under this CMCFunction 
        /// </summary>
        public IList<string> AssertionNames
        {
            get
            {
                return Cases.SelectMany(y => y.Assertions.Select(z => z.Name)).Distinct().ToList();
            }
        }

        /// <summary>
        /// A consolidated list of all Range variable_name values appearing under this CMCFunction 
        /// </summary>
        public IList<string> RangeVariables
        {
            get
            {
                return getRangeVariables();
            }
        }

        private IList<string> getRangeVariables()
        {
            HashSet<string> variables = new HashSet<string>();
            Stack<Unc_Ranges> rStack = new Stack<Unc_Ranges>();
            foreach (Unc_Case c in cases)
            {
                var rngs = c.Ranges;
                rStack.Push(rngs);
            }
            while (rStack.Count > 0)
            {
                var rngs = rStack.Pop();
                foreach (var r in rngs)
                {
                    variables.Add(r.Variable_name);
                    if (r.Ranges != null) rStack.Push(r.Ranges);
                }
            }
            return variables.ToList();
        }


        /// <summary>
        /// A consolidated list of all Assertion Values for a Given Assertion Name appearing under this CMCFunction 
        /// </summary>
        public List<string> getAssertionValuesByAssertionName(string assertionName)
        {
            return Cases.SelectMany(x => x.Assertions.Where(y => y.Name == assertionName).Select(z => z.Value)).Distinct().ToList();
        }

        public void writeTo(XElement Template) {
            var CMCFunction = new UncSpaceHelper(Template).addChild("CMCFunction").setAttribute("name", name);
            Cases.writeTo(CMCFunction.Element);
        }

        private Unc_CMCFunction() { }

        public Unc_CMCFunction(Unc_Template Template, Unc_Cases Cases, String Name)
        {
            this.template = Template;
            this.cases = Cases;
            this._name = Name;
        }

        public Unc_CMCFunction(XElement datasource, Unc_Template template)
        {
            try
            {
                this.template = template;
                UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
                _name = uncSpaceHelper.getAttribute("name");
                cases = new Unc_Cases(datasource, template, _name);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }


    public class Unc_CMCFunctions : IEnumerable<Unc_CMCFunction>
    {
        private List<Unc_CMCFunction> functions = new List<Unc_CMCFunction>();
        private Unc_Template template = null;

        public Unc_CMCFunction this[int index]
        {
            get { return functions[index]; }
        }

        public Unc_CMCFunction this[string name]
        {
            get { 
                var set = functions.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }
     

        public int Count()
        {
            return functions.Count();
        }

        private void loadFunctions(UncSpaceHelper uncSpaceHelper, Unc_Template template)
        {
            var els = uncSpaceHelper.getElements("CMCFunction");
            foreach (XElement el in els)
            {
                Unc_CMCFunction function = new Unc_CMCFunction(el, template);
                functions.Add(function);
            }
        }

        public Unc_CMCFunctions Add(Unc_CMCFunction Function)
        {
            functions.Add(Function);
            return this;
        }

        public Unc_CMCFunctions Remove(Unc_CMCFunction Function)
        {
            if (functions.Contains(Function)) functions.Remove(Function);
            return this;
        }

        public IEnumerator<Unc_CMCFunction> GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void writeTo(XElement Template) {
            foreach (Unc_CMCFunction function in functions ) {
                function.writeTo(Template);
            }
        }

        public Unc_CMCFunctions(Unc_Template template)
        {
            this.template = template;
        }

        public Unc_CMCFunctions(Unc_Template template, IEnumerable<Unc_CMCFunction> functions)
        {
            this.template = template;
            this.functions.AddRange(functions);
        }

        public Unc_CMCFunctions(XElement datasource, Unc_Template template)
        {
            this.template = template;
            loadFunctions(new UncSpaceHelper(datasource), template);
        }


    }

    public class Unc_Template
    {
        private Unc_InfluenceQuantities influenceQuantities = null;
        private Unc_TemplateTechnique templateTechnique = null;
        private Unc_CMCFunctions cmcFunctions = null;
        private Unc_CMCs cMCs = null;
        private Mtc_Technique mtcTechnique = null;
        private Mtc_ProcessType mtcProcessType = null;

        public Unc_InfluenceQuantities InfluenceQuantities
        {
            get { return influenceQuantities; }
            //set { influenceQuantities = value; }
        }

        public Unc_TemplateTechnique TemplateTechnique
        {
            get { return templateTechnique; }
            //set { templateTechnique = value; }
        }

        public Unc_CMCFunctions CMCUncertaintyFunctions
        {
            get { return cmcFunctions; }
            //set { cmcFunctions = value; }
        }

        public Mtc_Technique MtcTechnique
        {
            get { return mtcTechnique; }
            //set { mtcTechnique = value; }
        }

        public Mtc_ProcessType MtcProcessType
        {
            get { return mtcProcessType; }
            //set { mtcProcessType = value; }
        }

        public Mtc_ProcessResults ProcessResults
        {
            get { return (mtcProcessType != null) ? mtcProcessType.ProcessResults : new Mtc_ProcessResults(); }
        }

        public IList<string> ResultTypes
        {
            get { return (mtcProcessType != null) ? mtcProcessType.ResultTypes : null; }
        }

        public Mtc_CMCUncertainty getCMCUncertaintyByFunctionName(string functionName)
        {
            return (mtcTechnique != null) ? mtcTechnique.CMCUncertainties[functionName] : null;
        }

        public string getCMCUncertaintyFunctionExpression(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.Expression : "";
        }

        public IList<string> getCMCUncertaintyFunctionSymbols(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.ExpressionSymbols : new List<string>();
        }

        public IList<string> getCMCFunctionVariables(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.Variables : new List<string>();
        }

        public IList<string> getCMCFunctionConstants(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.Constants : new List<string>();
        }

        public decimal? evaluateCMCFunction(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.evaluate() : null;
        }

        public void setCMCFunctionSymbol(string functionName, string symbolName, double value)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            if (unc != null) unc.setSymbol(symbolName, value);
        }

        public IList<string> getCMCFunctionRangeVariables(string functionName)
        {
            var fnc = CMCUncertaintyFunctions[functionName];
            return (fnc != null) ? fnc.RangeVariables : new List<string>();
        }

        public IList<string> getCMCFunctionAssertionNames(string functionName)
        {
            var fnc = CMCUncertaintyFunctions[functionName];
            return (fnc != null) ? fnc.AssertionNames : new List<string>();
        }

        public IList<string> getCMCFunctionAssertionValues(string functionName, string assertionName)
        {
            var fnc = CMCUncertaintyFunctions[functionName];
            return fnc.getAssertionValuesByAssertionName(assertionName);
        }

        public UomDataSource.Quantity getQuantity(string parameterName)
        {
            return (mtcTechnique != null) ? mtcTechnique.getQuantity(parameterName) : null;
        }

        public void writeTo(XElement CMC) {
            var Template = new UncSpaceHelper(CMC).addChild("Template");
            if (influenceQuantities.Count() == 1) {
                influenceQuantities[0].writeTo(Template.Element);
            } else if (influenceQuantities.Count() > 1) {
                influenceQuantities.writeTo(Template.Element);
            }
            templateTechnique.writeTo(Template.Element);
            cmcFunctions.writeTo(Template.Element);
        }

        public Unc_Template(Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            influenceQuantities = new Unc_InfluenceQuantities();
            templateTechnique = new Unc_TemplateTechnique(this);
            mtcTechnique = null;
            mtcProcessType = null;
            cmcFunctions = new Unc_CMCFunctions(this);
        }

        public Unc_Template(XElement datasource, Unc_CMCs cMCs)
        {
            try
            {
                this.cMCs = cMCs;
                influenceQuantities = new Unc_InfluenceQuantities(datasource);
                UncSpaceHelper unsSpaceHelper = new UncSpaceHelper(datasource);
                var el = unsSpaceHelper.getElement("Technique");
                if (el != null) templateTechnique = new Unc_TemplateTechnique(el, this);
                if (templateTechnique != null)
                {
                    Unc_Technique uncTech = cMCs.Technique[templateTechnique.name];
                    mtcTechnique = (uncTech != null) ? uncTech.Technique : null;
                    mtcProcessType = (mtcTechnique != null) ? mtcTechnique.ProcessType : null;
                }
                cmcFunctions = new Unc_CMCFunctions(datasource, this);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }


    public class Unc_Templates : IEnumerable<Unc_Template>
    {
        private List<Unc_Template> templates = new List<Unc_Template>();
        private Unc_CMCs cMCs = null;

        public Unc_Template this[int index]
        {
            get { return templates[index]; }
        }

        public int Count()
        {
            return templates.Count();
        }

        private void loadTemplates(UncSpaceHelper uncSpaceHelper, Unc_CMCs cMCs)
        {
            var els = uncSpaceHelper.getElements("Template");
            foreach (XElement el in els)
            {
                Unc_Template template = new Unc_Template(el, cMCs);
                templates.Add(template);
            }
        }

        public void writeTo(XElement CMC) {
            foreach (Unc_Template template in templates) {
                template.writeTo(CMC);
            }
        }

        public Unc_Templates(Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            templates.Add(new Unc_Template(cMCs));
        }

        public Unc_Templates(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            loadTemplates(new UncSpaceHelper(datasource), cMCs);
        }

        public IEnumerator<Unc_Template> GetEnumerator()
        {
            return templates.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_CmcCategory
    {
        private string _name = "";
        private Unc_CmcCategory subcategory = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Unc_CmcCategory SubCategory
        {
            get { return subcategory; }
            set { subcategory = value; }
        }

        public void writeTo(XElement Parent)
        {
            var Category = new UncSpaceHelper(Parent).addChild("Category").setAttribute("name", name);
            if (subcategory != null)
            {
                subcategory.writeTo(Category.Element);
            }
        }

        public Unc_CmcCategory() {
            _name = "";
            subcategory = null;
        }

        public Unc_CmcCategory(XElement datasource)
        {
            var Category = new UncSpaceHelper(datasource);
            _name = Category.getAttribute("name");
            var SubCategory = Category.getNameSpaceHelper("Category");
            if (SubCategory != null)
            {
                subcategory = new Unc_CmcCategory(SubCategory.getElement());
            }
        }
    }

    public class Unc_DUT
    {
        private List<string> deviceTypes = new List<string>();

        public ICollection<string> DeviceTypes
        {
            get { return deviceTypes; }
        }

        private void loadDeviceTypes(MtcSpaceHelper mtcSpaceHelper)
        {
            var el1 = mtcSpaceHelper.getElement("DeviceTypes");
            if (el1 != null)
            {
                deviceTypes = new UncSpaceHelper(el1).getValues("DeviceType");
            }
            else
            {
                var el2 = mtcSpaceHelper.getElement("DeviceType");
                if (el2 != null) deviceTypes.Add(el2.Value);
            }
        }

        public void writeTo(XElement CMC)
        {
            var DUT = new UncSpaceHelper(CMC).addChild("DUT");
            if (deviceTypes.Count == 1)
            {
                DUT.addChild("DeviceType").Value = deviceTypes[0];
            }
            else if (deviceTypes.Count > 1)
            {
                var DeviceTypes = DUT.addChild("DeviceTypes");
                foreach (string value in deviceTypes)
                {
                    DeviceTypes.addChild("DeviceType").Value = value;
                }
            }
        }

        public Unc_DUT() { }

        public Unc_DUT(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            loadDeviceTypes(mtcSpaceHelper);
        }
    }

    public class Unc_CMC 
    {
        private Unc_CmcCategory category = null;
        private Unc_DUT dut = null; 
        private Unc_Templates templates = null;
        private Unc_CMCs cMCs = null;

        public Unc_CmcCategory Category
        {
            get { return category; }
            //set { categgory = value; }
        }

        public Unc_DUT DUT
        {
            get { return dut; }
        }


        public Unc_Templates Templates
        {
            get { return templates; }
            //set { template = value; }
        }

        public void writeTo(XElement CMCs)
        {
            var CMC = new UncSpaceHelper(CMCs).addChild("CMC");
            category.writeTo(CMC.Element);
            dut.writeTo(CMC.Element);
            templates.writeTo(CMC.Element);
        }

        public Unc_CMC(Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            this.category = new Unc_CmcCategory();
            this.dut = new Unc_DUT();
            this.templates = new Unc_Templates(cMCs);
        }

        public Unc_CMC(XElement datasource, Unc_CMCs cMCs)
        {
            try
            {
                this.cMCs = cMCs;
                UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
                var el = uncSpaceHelper.getElement("Category");
                if (el != null) category = new Unc_CmcCategory(el);
                el = uncSpaceHelper.getElement("DUT");
                if (el != null) dut = new Unc_DUT(el);
                templates = new Unc_Templates(datasource, cMCs);
            } 
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }

    }

    public class Unc_CMCList : IEnumerable<Unc_CMC>
    {
        private List<Unc_CMC> cmcList = new List<Unc_CMC>();
        private Unc_CMCs cMCs = null;

        public Unc_CMC this[int index]
        {
            get { return cmcList[index]; }
        }

        public int Count()
        {
            return cmcList.Count();
        }

        private void loadCMCs(UncSpaceHelper uncSpaceHelper, Unc_CMCs cMCs)
        {
            var els = uncSpaceHelper.getElements("CMC");
            foreach (XElement el in els)
            {
                Unc_CMC cmc = new Unc_CMC(el, cMCs);
                cmcList.Add(cmc);
            }
        }

        public void writeTo(XElement CMCs)
        {
            foreach (Unc_CMC CMC in cmcList)
            {
                CMC.writeTo(CMCs);
            }
        }

        public Unc_CMCList(Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            cmcList.Add(new Unc_CMC(cMCs));
        }

        public Unc_CMCList(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            loadCMCs(new UncSpaceHelper(datasource), cMCs);
        }
    
        public IEnumerator<Unc_CMC> GetEnumerator()
        {
 	        return cmcList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
 	        return GetEnumerator();
        }
    }

    public class Unc_Technique
    {
        private string _name = "";
        private string _process = "";
        private Uri uri = null;
        private Mtc_Technique technique = null;
        private Unc_CMCs cMCs = null;

        public string Uri
        {
            get { return uri.AbsoluteUri; }
            set { uri = new Uri(value); }
        }

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string process
        {
            get { return _process; }
            set { _process = value; }
        }

        public Mtc_Technique Technique
        {
            get { return technique; }
            set { technique = value; }
        }


        public void writeTo(XElement CMCs)
        {
            var Technique = new UncSpaceHelper(CMCs).addChild("Technique").setAttribute("name", name).setAttribute("process", process);
            if (uri != null)
            {
                Technique.addChild("ExternalDefinition").setAttribute("uri", Uri);
            }
            else if (technique != null)
            {
                technique.writeTo(Technique.Element);
            }
        }

        public Unc_Technique(Unc_CMCs cMCs) {
            this.cMCs = cMCs;
            name = "";
            process = "";
            uri = null;
            technique = new Mtc_Technique(this, cMCs);
        }

        public Unc_Technique(XElement Technique, Unc_CMCs cMCs)
        {
            try
            {
                this.cMCs = cMCs;
                UncSpaceHelper TechniqueHelper = new UncSpaceHelper(Technique);
                name = TechniqueHelper.getAttribute("name");
                process = TechniqueHelper.getAttribute("process");
                var ExternalDefintionHelper = TechniqueHelper.getNameSpaceHelper("ExternalDefintion");
                if (ExternalDefintionHelper != null)
                {
                    string link = ExternalDefintionHelper.getAttribute("uri");
                    if (link != "") uri = new Uri(link);
                    XMLDataSource externaldefinition = new XMLDataSource();
                    OpResult result = externaldefinition.load(uri);
                    if (!result.Success) throw new Exception(result.Error);
                    var ExternalTechniqueHelper = new MtcSpaceHelper(externaldefinition.Doc.Root);
                    technique = new Mtc_Technique(ExternalTechniqueHelper.getElement(), this, cMCs);
                }
                else
                {
                    var localTechnique = new MtcSpaceHelper(Technique).getElement("Technique");
                    if (localTechnique != null)
                    {
                        technique = new Mtc_Technique(localTechnique, this, cMCs);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Unc_Techniques : IEnumerable<Unc_Technique>
    {
        private List<Unc_Technique> techniques = new List<Unc_Technique>();
        private Unc_CMCs cMCs = null;

        public Unc_Technique this[int index]
        {
            get { return techniques[index]; }
        }

        public Unc_Technique this[string name]
        {
            get { 
                var set = techniques.Where(x => x.name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count() 
        {
            return techniques.Count();
        }

        private void loadTechniques(UncSpaceHelper uncSpaceHelper, Unc_CMCs cMCs)
        {
            var els = uncSpaceHelper.getElements("Technique");
            foreach (XElement el in els)
            {
                Unc_Technique technique = new Unc_Technique(el, cMCs);
                techniques.Add(technique);
            }
        }


        public void writeTo(XElement CMCs) {
            foreach (Unc_Technique technique in techniques) {
               technique.writeTo(CMCs);
            }
        }

        public Unc_Techniques(Unc_CMCs cMCs) {
            this.cMCs = cMCs;
            techniques.Add(new Unc_Technique(cMCs));
        }

        public Unc_Techniques(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            loadTechniques(new UncSpaceHelper(datasource), cMCs);
        }

        public IEnumerator<Unc_Technique> GetEnumerator()
        {
            return techniques.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_ProcessType
    {
        private string _name = "";
        private Uri uri = null;
        private Mtc_ProcessType processType = null;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Uri
        {
            get { return uri.AbsoluteUri; }
            set { uri = new Uri(value); }
        }

        public Mtc_ProcessType ProcessType
        {
            get { return processType; }
            set { processType = value; }
        }

        public void writeTo(XElement CMCs)
        {
            SoaSpaceHelper CMCsHelper = new SoaSpaceHelper(CMCs);
            var ProcessType = CMCsHelper.addChild("ProcessType").setAttribute("name", name);
            if (uri != null)
            {
                ProcessType.addChild("ExternalDefinition").setAttribute("uri", Uri);
            }
            else if (processType != null)
            {
                processType.writeTo(ProcessType.Element);
            }
        }

        public Unc_ProcessType() {
            ProcessType = new Mtc_ProcessType(this); 
        }

        public Unc_ProcessType(XElement datasource)
        {
            UncSpaceHelper ProcessTypeHelper = new UncSpaceHelper(datasource);
            _name = ProcessTypeHelper.getAttribute("name");
            var ExternalDefintionHelper = ProcessTypeHelper.getNameSpaceHelper("ExternalDefintion");
            if (ExternalDefintionHelper != null)
            { 
                string link = ExternalDefintionHelper.getAttribute("uri");
                if (link != "") uri = new Uri(link);
                XMLDataSource externaldefinition = new XMLDataSource();
                OpResult result = externaldefinition.load(uri);
                if (!result.Success) throw new Exception(result.Error);
                var ExternalHelper = new MtcSpaceHelper(externaldefinition.Doc.Root); 
                processType = new Mtc_ProcessType(this, ExternalHelper.getElement());
            }
            else
            {
                var local = new MtcSpaceHelper(datasource).getElement("ProcessType");
                if (local != null)
                {
                    processType = new Mtc_ProcessType(this, local);
                }
            }
        }
    }



    public class Unc_ProcessTypes : IEnumerable<Unc_ProcessType>
    {
        List<Unc_ProcessType> processTypes = new List<Unc_ProcessType>();

        public Unc_ProcessType this[int index]
        {
            get { return processTypes[index]; }
        }

        public Unc_ProcessType this[string name]
        {
            get { 
                var matches = processTypes.Where(x => (string) x.name == name);
                return (matches.Count() > 0) ? matches.First() : null;
            }
        }
    
        public int Count()
        {
            return processTypes.Count(); 
        }

        private void loadProcessTypes(UncSpaceHelper uncSpaceHelper)
        {
            var els = uncSpaceHelper.getElements("ProcessType");
            foreach (XElement el in els)
            {
                Unc_ProcessType processType = new Unc_ProcessType(el);
                processTypes.Add(processType);
            }
        }

        public void writeTo(XElement CMCs) {
            foreach (Unc_ProcessType ProcessType in processTypes)
            {
                ProcessType.writeTo(CMCs);
            }
        }

        public Unc_ProcessTypes() {
           processTypes.Add(new Unc_ProcessType());
        }

        public Unc_ProcessTypes(XElement datasource)
        {
            loadProcessTypes(new UncSpaceHelper(datasource));
        }

        public IEnumerator<Unc_ProcessType> GetEnumerator()
        {
            return processTypes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_CMCs
    {
        private Unc_ProcessTypes processTypes = null;
        private Unc_Techniques techniques = null;
        private Unc_CMCList cmcs = null;
        private Soa_Activity activity = null;

        public Unc_ProcessTypes ProcessTypes
        {
            get { return processTypes; }
        }

        public Unc_Techniques Technique
        {
            get { return techniques; }
        }

        public Unc_CMCList CMC
        {
            get { return cmcs; }
        }

        public void writeTo(XElement Activity)
        {
            SoaSpaceHelper AcitivityHelper = new SoaSpaceHelper(Activity);
            var CMCs = AcitivityHelper.addChild("CMCs");
            processTypes.writeTo(CMCs.Element);
            techniques.writeTo(CMCs.Element);
            cmcs.writeTo(CMCs.Element);
        }

        public Unc_CMCs(Soa_Activity activity) {
            processTypes = new Unc_ProcessTypes();
            techniques = new Unc_Techniques(this);
            cmcs = new Unc_CMCList(this);       
        }

        public Unc_CMCs(XElement datasource, Soa_Activity activity)
        {
            this.activity = activity;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            try
            {
              processTypes = new Unc_ProcessTypes(datasource);
              techniques = new Unc_Techniques(datasource, this);
              cmcs = new Unc_CMCList(datasource, this);
            }
            catch (Exception e)
            {
                throw new Exception(this.ToString() + " constructor " + e.Message);
            }
        }
    }

    public class Soa_Activity 
    {
        private Unc_CMCs cMCs = null;

        public Unc_ProcessTypes ProcessTypes
        {
            get { return cMCs.ProcessTypes; }
            //set { processTypes = value; }
        }

        public Unc_Techniques Techniques
        {
            get { return cMCs.Technique; }
        }

        public Unc_CMCList CMCs
        {
            get { return cMCs.CMC; }
        }

        public  ReadOnlyCollection<Unc_Template> Templates
        {
            get { return cMCs.CMC.Select(x => x.Templates).SelectMany(y => y).ToList().AsReadOnly(); }
        }

        public void writeTo(XElement Actitivities) {
            SoaSpaceHelper ActitivitiesHelper = new SoaSpaceHelper(Actitivities);
            var Activity = ActitivitiesHelper.addChild("Activity");
            cMCs.writeTo(Activity.Element);
        }

        public Soa_Activity() {
            cMCs = new Unc_CMCs(this);
        }

        public Soa_Activity(XElement datasource)
        {
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            var el = uncSpaceHelper.getElement("CMCs");
            if (el != null) cMCs = new Unc_CMCs(el, this);
        }
    }


    public class Soa_Activities : IEnumerable<Soa_Activity>
    {
        private List<Soa_Activity> activities = new List<Soa_Activity>();

        public Soa_Activity this[int index]
        {
            get { return activities[index]; }
            //set { activities = value; }
        }

        public int Count()
        {
            return activities.Count();
        }

        private void loadActivities(SoaSpaceHelper soaSpaceHelper)
        {
            var els = soaSpaceHelper.getElements("Activity");
            foreach (XElement el in els)
            {
                Soa_Activity activity = new Soa_Activity(el);
                activities.Add(activity);
            }
        }

        public void writeTo(XElement CapabilityScope)
        {
            SoaSpaceHelper CapabilityScopeHelper = new SoaSpaceHelper(CapabilityScope);
            var Activities = CapabilityScopeHelper.addChild("Activities");
            foreach (Soa_Activity activity in activities) {
                activity.writeTo(Activities.Element);
            }
        }

        public Soa_Activities() {
            activities.Add(new Soa_Activity());
        }

        public Soa_Activities(XElement datasource)
        {
            loadActivities(new SoaSpaceHelper(datasource));
        }

        public IEnumerator<Soa_Activity> GetEnumerator()
        {
            return activities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Soa_ContactInfo_PhoneNumbers : IEnumerable<string>
    {
        private HashSet<string> phoneNumbers = new HashSet<string>();

        public IEnumerable<string> PhoneNumbers
        {
            get { return phoneNumbers; }
        }

        public void addPhoneNumber(string number)
        {
            phoneNumbers.Add(number);
        }

        public void removePhoneNumber(string number)
        {
            phoneNumbers.Remove(number);
        }

        public int Count()
        {
            return phoneNumbers.Count();
        }

        public void writeTo(XElement ContactInfo)
        {
           SoaSpaceHelper ContactInfoHelper = new SoaSpaceHelper(ContactInfo);
           foreach (string number in phoneNumbers) {
               ContactInfoHelper.addChild("PhoneNumber").Value = number;
           }
        }

        public Soa_ContactInfo_PhoneNumbers() { }

        public Soa_ContactInfo_PhoneNumbers(XElement ContactInfo)
        {
            var PhoneNumbers = new SoaSpaceHelper(ContactInfo).getElements("PhoneNumber");
            foreach (XElement PhoneNumber in PhoneNumbers)
            {
                phoneNumbers.Add(PhoneNumber.Value);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return phoneNumbers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Soa_ContactInfo_EmailAccounts : IEnumerable<string>
    {
        private HashSet<string> emailAccounts = new HashSet<string>();

        public void addEmail(string email)
        {
            emailAccounts.Add(email);
        }

        public void removeEmail(string email)
        {
            emailAccounts.Remove(email);
        }

        public int Count()
        {
            return emailAccounts.Count();
        }

        public void writeTo(XElement ContactInfo)
        {
           SoaSpaceHelper ContactInfoHelper = new SoaSpaceHelper(ContactInfo);
           foreach (string email in emailAccounts)
           {
               ContactInfoHelper.addChild("email").Value = email;
           }
        }

        public Soa_ContactInfo_EmailAccounts() { }

        public Soa_ContactInfo_EmailAccounts(XElement ContactInfo)
        {
            var EmailAccounts = new SoaSpaceHelper(ContactInfo).getElements("email");
            foreach (XElement email in EmailAccounts)
            {
                emailAccounts.Add(email.Value);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return emailAccounts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Soa_ContactInfo_Urls : IEnumerable<string>
    {
        private HashSet<string> urls = new HashSet<string>();

        public void addUrl(string url)
        {
            urls.Add(url);
        }

        public void removeUrl(string url)
        {
            urls.Remove(url);
        }

        public int Count()
        {
            return urls.Count();
        }

        public void writeTo(XElement ContactInfo)
        {
           SoaSpaceHelper ContactInfoHelper = new SoaSpaceHelper(ContactInfo);
           foreach (string url in urls) {
               ContactInfoHelper.addChild("URL").Value = url;
           }
        }

        public Soa_ContactInfo_Urls() { }

        public Soa_ContactInfo_Urls(XElement ContactInfo)
        {
            var URLs = new SoaSpaceHelper(ContactInfo).getElements("URL");
            foreach (XElement URL in URLs)
            {
                urls.Add(URL.Value);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return urls.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class Soa_ContactInfo
    {
        private Soa_ContactInfo_PhoneNumbers phoneNumbers = null;
        private Soa_ContactInfo_EmailAccounts emailAccounts = null;
        private Soa_ContactInfo_Urls urls = null;

        public Soa_ContactInfo_PhoneNumbers PhoneNumbers
        {
            get { return phoneNumbers; }
        }

        public Soa_ContactInfo_EmailAccounts EmailAccounts
        {
            get { return emailAccounts; }
        }

        public Soa_ContactInfo_Urls Urls
        {
            get { return urls; }
        }

        public void writeTo(XElement Location)
        {
            var ContactInfo = new SoaSpaceHelper(Location).addChild("ContactInfo");
            phoneNumbers.writeTo(ContactInfo.Element);
            emailAccounts.writeTo(ContactInfo.Element);
            urls.writeTo(ContactInfo.Element);
        }

        public Soa_ContactInfo() {
            phoneNumbers = new Soa_ContactInfo_PhoneNumbers();
            emailAccounts = new Soa_ContactInfo_EmailAccounts();
            urls = new Soa_ContactInfo_Urls();       
        }

        public Soa_ContactInfo(XElement ContactInfo)
        {
            phoneNumbers = new Soa_ContactInfo_PhoneNumbers(ContactInfo);
            emailAccounts = new Soa_ContactInfo_EmailAccounts(ContactInfo);
            urls = new Soa_ContactInfo_Urls(ContactInfo);
        }

    }


    public class Soa_CapabilityScope_Location_OrganizationAddress
    {
        private string street = "";
        private string city = "";
        private string state = "";
        private string zip = "";

        public string Street
        {
            get { return street; }
            set { street = value; }
        }

        public string City
        {
            get { return city; }
            set { city = value; }
        }

        public string State
        {
            get { return state; }
            set { state = value; }
        }

        public string Zip
        {
            get { return zip; }
            set { zip = value; }
        }

        public void writeTo(XElement Location)
        {
            var OrganizationAddress = new SoaSpaceHelper(Location).addChild("OrganizationAddress");
            OrganizationAddress.addChild("Street").Value = Street;
            OrganizationAddress.addChild("City").Value = City;
            OrganizationAddress.addChild("State").Value = State;
            OrganizationAddress.addChild("Zip").Value = Zip;
        }

        public Soa_CapabilityScope_Location_OrganizationAddress() {
            street = "";
            city = "";
            state = "";
            zip = "";
        }

        public Soa_CapabilityScope_Location_OrganizationAddress(XElement datasource) 
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            street = soaSpaceHelper.getValue("Street");
            city = soaSpaceHelper.getValue("City");
            state = soaSpaceHelper.getValue("State");
            zip = soaSpaceHelper.getValue("Zip");
        }
    }

    public class Soa_CapabilityScope_Location
    {
        private Soa_CapabilityScope_Location_OrganizationAddress organizationAddress = null;
        private string contactName = "";
        private string _id = "";
        private Soa_ContactInfo contactInfo = null;

        public Soa_CapabilityScope_Location_OrganizationAddress Address
        {
            get { return organizationAddress; }
            set { organizationAddress = value; }
        }

        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string ContactName
        {
            get { return contactName; }
            set { contactName = value; }
        }


        public Soa_ContactInfo ContactInfo
        {
            get { return contactInfo; }
            set { contactInfo = value; }
        }

        public void writeTo(XElement Locations)
        {
            var Location = new SoaSpaceHelper(Locations).addChild("Location").setAttribute("id", id);
            organizationAddress.writeTo(Location.Element);
            Location.addChild("ContactName").Value = ContactName;
            contactInfo.writeTo(Location.Element);
        }


        public Soa_CapabilityScope_Location() {
            organizationAddress = new Soa_CapabilityScope_Location_OrganizationAddress();
            contactName = "";
            contactInfo = new Soa_ContactInfo();  
        }

        public Soa_CapabilityScope_Location(XElement datasource) 
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            var el1 = soaSpaceHelper.getElement("OrganizationAddress");
            var el2 = soaSpaceHelper.getElement("ContactInfo");
            if (el1 != null) organizationAddress = new Soa_CapabilityScope_Location_OrganizationAddress(el1);
            contactName = soaSpaceHelper.getValue("ContactName");
            if (el2 != null) contactInfo = new Soa_ContactInfo(el2);
        }
    }

    public class Soa_CapabilityScope_Locations : IEnumerable<Soa_CapabilityScope_Location>
    {
        private List<Soa_CapabilityScope_Location> locations = new List<Soa_CapabilityScope_Location>();

        public Soa_CapabilityScope_Location this[int index]
        {
            get { return locations[index]; }
           // set { locations = value; }
        }

        public int Count()
        {
            return locations.Count();
        }

        private void loadLocations(SoaSpaceHelper soaSpaceHelper)
        {
            var els = soaSpaceHelper.getElements("Location");
            foreach (XElement el in els)
            {
                Soa_CapabilityScope_Location location = new Soa_CapabilityScope_Location(el);
                locations.Add(location);
            }
        }

        public void writeTo(XElement CapabilityScope)
        {
            var Locations = new SoaSpaceHelper(CapabilityScope).addChild("Locations");
            foreach (Soa_CapabilityScope_Location location in locations)
            {
                location.writeTo(Locations.Element);
            }
        }

        public Soa_CapabilityScope_Locations() {
            locations.Add(new Soa_CapabilityScope_Location());
        }

        public Soa_CapabilityScope_Locations(XElement datasource) 
        {
            loadLocations(new SoaSpaceHelper(datasource));
        }

        public IEnumerator<Soa_CapabilityScope_Location> GetEnumerator()
        {
            return locations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Soa_CapabilityScope 
    {
        private string measuringEntity = "";
        private Soa_CapabilityScope_Locations locations = null;
        private Soa_Activities activities = null;
        private string scopeNotes = "";
        private string version = "";
        private string localLanguage = "";

        public string MeasuringEntity
        {
            get { return measuringEntity; }
            set { measuringEntity = value; }
        }

        public Soa_CapabilityScope_Locations Locations
        {
            get { return locations; }
            //set { locations = value; }
        }

        public Soa_Activities Activities
        {
            get { return activities; }
            //set { activities = value; }
        }

        public string ScopeNotes
        {
            get { return scopeNotes; }
            //set { scopeNotes = value; }
        }

        public string Version
        {
            get { return version; }
            //set { version = value; }
        }

        public string LocalLanguage
        {
            get { return localLanguage; }
            //set { localLanguage = value; }
        }
        public void writeTo(XElement root)
        {
            SoaSpaceHelper rootHelper = new SoaSpaceHelper(root);
            var CapabilityScope = rootHelper.addChild("CapabilityScope");
            CapabilityScope.addChild("MeasuringEntity");
            locations.writeTo(CapabilityScope.Element);
            activities.writeTo(CapabilityScope.Element);
            XiSpaceHelper uomHelper = new XiSpaceHelper(CapabilityScope.Element);
            uomHelper.addChild("include").Element.Add(new XAttribute("href", Configuration.UomDatabaseURL));
            CapabilityScope.addChild("ScopeNotes");
            CapabilityScope.addChild("Version");
            CapabilityScope.addChild("LocaleLanguage");
        }

        public Soa_CapabilityScope() {
            locations = new Soa_CapabilityScope_Locations();
            activities = new Soa_Activities();
            scopeNotes = "";
            version = "";
            localLanguage = "";       
        }

        public Soa_CapabilityScope(XElement datasource)
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            measuringEntity = soaSpaceHelper.getValue("MeasuringEntity");
            var el1 = soaSpaceHelper.getElement("Locations");
            var el2 = soaSpaceHelper.getElement("Activities");
            if (el1 != null) locations = new Soa_CapabilityScope_Locations(el1);
            if (el2 != null) activities = new Soa_Activities(el2);
            scopeNotes = soaSpaceHelper.getValue("ScopeNotes");
            version = soaSpaceHelper.getValue("Version");
            localLanguage = soaSpaceHelper.getValue("LocalLanguage");
        }
    }

    public class Soa_ScopeUrl
    {
        private string scopeType = "";
        private string url = "";

        public string ScopeType
        {
            get { return scopeType; }
            set { scopeType = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public Soa_ScopeUrl() {
            ScopeType = "";
            Url = "";
        }

        public Soa_ScopeUrl(XElement datasource) 
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            ScopeType = soaSpaceHelper.getAttribute("ScopeType");
            Url = soaSpaceHelper.getAttribute("URL");
        }
    }

    public class Soa_ScopeUrls : IEnumerable<Soa_ScopeUrl>
    {
        private List<Soa_ScopeUrl> scopeUrls = new List<Soa_ScopeUrl>();

        public Soa_ScopeUrl this[int index]
        {
            get { return scopeUrls[index]; }
        }

        public int Count()
        {
            return scopeUrls.Count();
        }

        private void loadScopeUrls(SoaSpaceHelper soaSpaceHelper)
        {
            var els = soaSpaceHelper.getElements("ScopeURL");
            foreach (XElement el in els)
            {
                Soa_ScopeUrl scopeUrl = new Soa_ScopeUrl(el);
                scopeUrls.Add(scopeUrl); 
            }
        }

        public void writeTo(XElement root) {
            SoaSpaceHelper rootHelper = new SoaSpaceHelper(root);
            rootHelper.addChild("ScopeURLs").addChild("ScopeURL").addAttributes("ScopeType","CheckSum","URL");
        } 

        public Soa_ScopeUrls() {
            scopeUrls.Add(new Soa_ScopeUrl());
        }

        public Soa_ScopeUrls(XElement datasource) 
        {
            loadScopeUrls(new SoaSpaceHelper(datasource));
        }

        public IEnumerator<Soa_ScopeUrl> GetEnumerator()
        {
            return scopeUrls.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Soa {
        private XDocument datasource;
        private XNamespace nsSoa = Configuration.NameSpaces["soa"];

        private string ab_ID = "";
        private string ab_Logo_Signature = "";
        private string scope_ID_Number = "";
        private Soa_ScopeUrls scopeUrls = null;
        private string criteria = "";
        private string effectiveDate = "";
        private string expirationDate = "";
        private string statement = "";
        private Soa_CapabilityScope capabilityScope = null;
        private string humanReadableDocument = "";
        private string visualAidsScript = "";

        public string Ab_ID
        {
            get { return ab_ID; }
            set { ab_ID = value; }
        }

        public string Ab_Logo_Signature
        {
            get { return ab_Logo_Signature; }
            set { ab_Logo_Signature = value; }
        }

        public string Scope_ID_Number
        {
            get { return scope_ID_Number; }
            set { scope_ID_Number = value; }
        }

        public Soa_ScopeUrls ScopeUrls
        {
            get { return scopeUrls; }
            set { scopeUrls = value; }
        }

        public string Criteria
        {
            get { return criteria; }
            set { criteria = value; }
        }

        public string EffectiveDate
        {
            get { return effectiveDate; }
            set { effectiveDate = value; }
        }


        public string ExpirationDate
        {
            get { return expirationDate; }
            set { expirationDate = value; }
        }

        public string Statement
        {
            get { return statement; }
            set { statement = value; }
        }

        public Soa_CapabilityScope CapabilityScope
        {
            get { return capabilityScope; }
            set { capabilityScope = value; }
        }

        public string HumanReadableDocument
        {
            get { return humanReadableDocument; }
            //set { humanReadableDocument = value; }
        }

        public string VisualAidsScript
        {
            get { return visualAidsScript; }
            //set { visualAidsScript = value; }
        }

        public ReadOnlyCollection<string> ResultTypes
        {
            get {
                var set1 = CapabilityScope.Activities.SelectMany(x => x.Templates);
                return set1.SelectMany(x => x.ResultTypes).Distinct().ToList().AsReadOnly();
            }
        }

        private XElement getElement(string elementName)
        {
            var els = datasource.Descendants(nsSoa + elementName);
            return (els.Count() > 0) ? els.First() : null;
        }

        private string getValue(string elementName)
        {
            XElement element = getElement(elementName);
            return (element != null) ? element.Value : "";
        }

        public void writeTo(XDocument doc) {
            if (doc.Root != null) doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "UTF-8", "no");
            SoaSpaceHelper docHelper = new SoaSpaceHelper(doc);
            var root = docHelper.addChild("SOADataMaster");
            root.Element.Add(Configuration.NameSpaces.NameSpaceDeclarations);
            root.addChild("AB_ID");
            root.addChild("AB_Logo-Signature");
            root.addChild("Scope_ID_Number");
            if (scopeUrls != null) scopeUrls.writeTo(root.Element);
            root.addChild("Criteria");
            root.addChild("EffectiveDate");
            root.addChild("ExpirationDate");
            root.addChild("Statement");
            if (capabilityScope != null) capabilityScope.writeTo(root.Element);
            root.addChild("HumanReadableDocument");
            root.addChild("VisualAidsScript");
        }

        public Soa() {
            ab_ID = "";
            ab_Logo_Signature = "";
            scope_ID_Number = "";
            scopeUrls = new Soa_ScopeUrls();
            criteria = "";
            effectiveDate = "";
            expirationDate = "";
            statement = "";
            capabilityScope = new Soa_CapabilityScope();
            humanReadableDocument = "";
            visualAidsScript = "";              
        }

        public Soa(XDocument datasource)
        {
            this.datasource = datasource;
            ab_ID = getValue("AB_ID");
            ab_Logo_Signature = getValue("AB_Logo-Signature");
            scope_ID_Number = getValue("Scope_ID_Number");
            var el1 = getElement("ScopeURLs");
            if (el1 != null) scopeUrls = new Soa_ScopeUrls(el1);
            criteria = getValue("Criteria");
            effectiveDate = getValue("EffectiveDate");
            expirationDate = getValue("ExpirationDate");
            statement = getValue("Statement");
            var el2 = getElement("CapabilityScope");
            if (el2 != null) capabilityScope = new Soa_CapabilityScope(el2);
            humanReadableDocument = getValue("HumanReadableDocument");
            visualAidsScript = getValue("VisualAidsScript");
        }
    }

    public class OpResult {
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


    public class SOA_DataAccess : XMLDataSource
    {
        
        Soa soa = null;

        public Soa SOADataMaster
        {
            get {
                if (soa == null)
                {
                   OpResult opResult = build();
                   if (!opResult.Success) throw new Exception(opResult.Error);
                   Doc = null; // no longer need doc
                }
                return soa; 
            }
        }

        private OpResult build()
        {
            OpResult opResult = new OpResult();
            try { 
                if (Doc == null) throw new Exception("XML not loaded");
                soa = new Soa(Doc);
            } catch (Exception e)
            {
                opResult.Success = false;
                opResult.Error = e.Message;
            }
            return opResult;
        }

    }
}
