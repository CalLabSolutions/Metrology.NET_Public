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
            OpResult opResult = new OpResult();
            try
            {
                if (Uri.IsWellFormedUriString(source, UriKind.Absolute))
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
                return (uomDatabaseFilePath != null) ? uomDatabaseFilePath : "http://schema.metrology.net/UOM_Database.xml";
            }

            set {
                uomDatabaseFilePath = value;
            }
        } 

        /// <summary>
        /// object type held in UomDataSource.Quantity.altCache that is used as a faster alternative to repeated LINQ queries returning the same results over and over again
        /// </summary>
        public class Alternative
        {
            private XElement altElement = null;
            private decimal offset, scaleFactor, linScaleFactor, logScaleFactor;
            private Func<decimal, decimal> converter = null;

            public decimal ConvertToBase(decimal value)
            {
                if (converter == null)
                {
                    var linconv = altElement.Element(ns + "LinearConverter");
                    var logconv = altElement.Element(ns + "LogarithmicConverter");
                    if (linconv != null)
                    {
                        // base_value = (value - Offset) * ScaleFactor
                        offset = decimal.Parse((string) linconv.Attribute("Offset"), NumberStyles.Float, CultureInfo.InvariantCulture);
                        scaleFactor = decimal.Parse((string)linconv.Attribute("ScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                        converter =  (x => (x - offset) * scaleFactor);
                    }
                    else if (logconv != null)
                    {
                        // base_value = LinScaleFactor * (10.0 ^ (value/LogScaleFactor)) 
                        linScaleFactor = decimal.Parse((string)logconv.Attribute("LinScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                        logScaleFactor = decimal.Parse((string)logconv.Attribute("LogScaleFactor"), NumberStyles.Float, CultureInfo.InvariantCulture);
                        converter = (x => linScaleFactor * DecimalMath.Pow(10.0m, x / logScaleFactor));
                    }
                }
                return Math.Round(converter(value), 28);
            }

            private Alternative() { }

            public Alternative(XElement altElement)
            {
                this.altElement = altElement;
            }
             
        }


        /// <summary>
        /// object type held in UomDataSource.qtyCache that is used as a faster alternative to repeated LINQ queries returning the same results over and over again
        /// </summary>
        public class Quantity
        {
            private XElement qtyElement = null;
            private string name = "";
            private XElement uomElement = null;
            private Dictionary<string, Alternative> altCache = new Dictionary<string, Alternative>();

            public string Name
            {
                get
                {
                    if (name == "")
                    {
                        var atrName = QtyElement.Attribute("name");
                        if (atrName != null) name = atrName.Value;
                    }
                    return name;
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
                        var set = qtyElement.Ancestors(ns + "UOM");
                        uomElement = (set.Count() > 0) ? set.First() : null;
                    }
                    return uomElement;
                }
            }

            public Alternative getAlternative(string alternativeName)
            {
                if (!altCache.Keys.Contains(alternativeName)) {
                    var set = UomElement.Descendants(ns + "Alternative").Where(x => (string)x.Attribute("name") == alternativeName);
                    if (set.Count() > 0) {
                        altCache[alternativeName] = new Alternative(set.First());
                    }
                    else
                    {
                        throw new Exception("invalid alternative");
                    }
                }
               return altCache[alternativeName];
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

        public static Quantity getQuantityByName(string QuantityName)
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
            return qtyCache[QuantityName];
        }

    }


    public class NameSpaces
    {
        private Dictionary<string, string> map = new Dictionary<string, string>(){
            {"soa", @"http://schema.metrology.net/ScopeOfAccreditation"},
            {"uom", @"http://schema.metrology.net/UnitsOfMeasure"},
            {"unc", @"http://schema.metrology.net/Uncertainty"},
            {"mtc", @"http://schema.metrology.net/MetrologyTaxononyCatatalog"},
            {"xi", @"http://www.w3.org/2001/XInclude"},
            {"xhtml", @"http://www.w3.org/1999/xhtml"}
        };

        public string this[string key]
        {
            get { return map[key]; }
        }
    }

    static class Configuration
    {
        private static NameSpaces nameSpaces = new NameSpaces();

        public static NameSpaces NameSpaces
        {
           get { return Configuration.nameSpaces; }    
        }
    }

    public class XmlNameSpaceHelper
    {
        private XNamespace ns;

        private XElement datasource = null;

        public XNamespace Ns
        {
            get { return ns; }
        }

        public XElement Datasource
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

        public string getValue(string elementName)
        {
            XElement element = getElement(elementName);
            return (element != null) ? element.Value : "";
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
            var atr = datasource.Attribute(attributeName);
            return (atr != null) ? atr.Value : "";
        }

        protected XmlNameSpaceHelper() { }

        public XmlNameSpaceHelper(XElement datasource, string namespaceValue)
        {
            this.datasource = datasource;
            ns = namespaceValue;
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

    public class SoaSpaceHelper : XmlNameSpaceHelper
    {
        private SoaSpaceHelper() { }

        public SoaSpaceHelper(XElement datasource)
            : base(datasource, Configuration.NameSpaces["soa"]) { }
    }


    public abstract class AbstractValue
    {
        protected UomDataSource.Quantity quantity = null;
        protected string uom_alternative = "";
        protected string uom_alias_symbol = "";
        protected string format = "";
        protected string value = "";

        public string Quantity
        {
            get { return (quantity != null) ? quantity.Name : ""; }
            set { quantity = UomDataSource.getQuantityByName(value); }
        }

        public string Uom_alternative
        {
            get { return uom_alternative; }
            //set { uom_alternative = value; }
        }

        public string Uom_alias_symbol
        {
            get { return uom_alias_symbol; }
            //set { uom_alias_symbol = value; }
        }

        public string Format
        {
            get { return format; }
            //set { format = value; }
        }

        public string Value
        {
            get { return this.value; }
            //set { this.value = value; }
        }

        public decimal BaseValue
        {
            get
            {
                decimal v;
                if (!decimal.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) throw new Exception("numeric syntax error");
                if (quantity == null) throw new Exception("invalid quantity");
                if ((Uom_alternative != "") && (Uom_alternative != Quantity))
                {
                    var alt = quantity.getAlternative(Uom_alternative);
                    if (alt == null) throw new Exception("invalid UOM alternative");
                    v = alt.ConvertToBase(v);
                }
                return v;
            }
        }

        protected void loadValue (XElement datasource)
        {
            if (datasource != null) value = datasource.Value;
        } 
 
    }

    public class Uom_Quantity 
    {
        private string name = "";

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        private Uom_Quantity() {}

        public Uom_Quantity(XElement datasource)
        {
            name = new UomSpaceHelper(datasource).getAttribute("name");
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

        private Mtc_Documentation() { }

        public Mtc_Documentation(XElement datasource)
        {
            ///TODO
        }
    }

    public class Mtc_ProcessResult
    {
        private string name = "";
        private Uom_Quantity quantity = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public Uom_Quantity Quantity
        {
            get { return quantity; }
            //set { quantity = value; }
        }

        private Mtc_ProcessResult() { }

        public Mtc_ProcessResult(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            var el = new UomSpaceHelper(datasource).getElement("Quantity");
            if (el != null) quantity = new Uom_Quantity(el);
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
            var els = mtcSpaceHelper.getElements("ProcessResult");
            foreach (XElement el in els)
            {
                Mtc_ProcessResult result = new Mtc_ProcessResult(el);
                results.Add(result);
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

    public class Mtc_ProcessParameter 
    {
        private string name = "";
        private bool optional = false;
        private Uom_Quantity quantity = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public bool Optional
        {
            get { return optional; }
            //set { optional = value; }
        }

        public Uom_Quantity Quantity
        {
            get { return quantity; }
            //set { quantity = value; }
        }

        private Mtc_ProcessParameter() { }

        public Mtc_ProcessParameter(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            optional = mtcSpaceHelper.getAttribute("optional").ToLower() == "true";
            var el = new UomSpaceHelper(datasource).getElement("Quantity");
            if (el != null) quantity = new Uom_Quantity(el);
        }
    }

    public class Mtc_ProcessParameters : IEnumerable<Mtc_ProcessParameter>
    {
        private List<Mtc_ProcessParameter> parameters = new List<Mtc_ProcessParameter>();

        public Mtc_ProcessParameter this[int index]
        {
            get { return parameters[index]; }
        }

        public Mtc_ProcessParameter this[string name]
        {
            get {
                var set = parameters.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return parameters.Count();
        }

        private void loadParameters(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("ProcessParameter");
            foreach(XElement el in els)
            {
                Mtc_ProcessParameter parameter = new Mtc_ProcessParameter(el);
                parameters.Add(parameter);
            }
        }

        public Mtc_ProcessParameters() { } // Needed when Mtc_ProcessType is null.  Returns an empty Mtc_ProcessParameters

        public Mtc_ProcessParameters(XElement datasource)
        {
            loadParameters(new MtcSpaceHelper(datasource));
        }

        public IEnumerator<Mtc_ProcessParameter> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_ProcessType 
    {
        private string name = "";
        private Mtc_Documentation documentation = null;
        private Mtc_ProcessResults processResults = null;
        private Mtc_ProcessParameters processParameters = null;
        private Mtc_Documentation documenation = null;


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Mtc_ProcessResults ProcessResult
        {
            get { return processResults; }
        }

        public Mtc_ProcessParameters ProcessParameter
        {
            get { return processParameters; }
        }

        public Mtc_Documentation Documentation
        {
            get { return documentation; }
            set { documentation = value; }
        }

        public IList<string> ResultTypes{
            get { return processResults.Select(x => x.Quantity.Name).ToList(); }
        }

        private Mtc_ProcessType() { }

        public Mtc_ProcessType(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            processResults = new Mtc_ProcessResults(datasource);
            processParameters = new Mtc_ProcessParameters(datasource);
            var el = mtcSpaceHelper.getElement("Documentation");
            if (el != null) documenation = new Mtc_Documentation(el);
        }    
    }


    public class Mtc_CMCParameter
    {
        private string name = "";
        private string type = "";
        private Uom_Quantity quantity = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public string Type
        {
            get { return type; }
            //set { type = value; }
        }

        public Uom_Quantity Quantity
        {
            get { return quantity; }
            //set { quantity = value; }
        }

        private Mtc_CMCParameter() { }

        public Mtc_CMCParameter(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            type = mtcSpaceHelper.getAttribute("type");
            var el = new UomSpaceHelper(datasource).getElement("Quantity");
            if (el != null) quantity = new Uom_Quantity(el);
        }

    }

    public class Mtc_CMCParameters : IEnumerable<Mtc_CMCParameter>
    {
        private List<Mtc_CMCParameter> parameters = new List<Mtc_CMCParameter>();

        public Mtc_CMCParameter this[int index]
        {
            get { return parameters[index]; }
        }

        public Mtc_CMCParameter this[string name]
        {
            get { 
                var set = parameters.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return parameters.Count();
        }

        private void loadParameters(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("CMCParameter");
            foreach(XElement el in els)
            {
                Mtc_CMCParameter parameter = new Mtc_CMCParameter(el);
                parameters.Add(parameter);
            }
        }

        private Mtc_CMCParameters() { }

        public Mtc_CMCParameters(XElement datasource)
        {
            loadParameters(new MtcSpaceHelper(datasource));
        }

        public IEnumerator<Mtc_CMCParameter> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Mtc_CMCUncertainty 
    {

        private string proccess_function_name = "";
        private string formula = "";
        private Mtc_CMCParameters parameters = null;
        private EvaluationEngine evaluator = new EvaluationEngine();
        private Uom_Quantity quantity = null;

        public string Proccess_function_name
        {
            get { return proccess_function_name; }
            //set { proccess_function_name = value; }
        }

        public string FunctionText
        {
            get { return formula; }
            //set { formula = value; }
        }

        public Uom_Quantity Quantity 
        {
            get { return quantity; }
        }

        public Mtc_CMCParameters CMCParameters
        {
            get { return parameters; }
        }

        private void loadFormula(XElement datasource)
        {
            if (datasource != null) formula = fixWhiteSpace(datasource.Value);
            evaluator.Parse(formula);
        }

        public IList<string> Symbols
        {
            get { return evaluator.GetVariables().Keys.ToList(); }
        }

        public IList<string> Variables
        {
            get { return CMCParameters.Where(x => x.Type == "Variable").Select(y => y.Name).ToList(); }
        }

        public IList<string> Constants
        {
            get { return CMCParameters.Where(x => x.Type == "Constant").Select(y => y.Name).ToList(); }
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
            r3 = raw;
            do {
               r1 = Regex.Replace(r3, "\t", " ");
               r2 = Regex.Replace(r1, "\n", " ");
               r3 = Regex.Replace(r2, "  ", " ");
            } while (r1 != r3);
            return r3;
        }

        private void setQuantity(MtcSpaceHelper mtcSpaceHelper)
        {
            var valueElement = mtcSpaceHelper.getElement("CMCValue");
            if (valueElement != null)
            {
                var qtyElement = new UomSpaceHelper(valueElement).getElement("Quantity");
                if (qtyElement != null)
                {
                    quantity = new Uom_Quantity(qtyElement);
                }
            }
        }

        private Mtc_CMCUncertainty() { }

        public Mtc_CMCUncertainty(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            proccess_function_name = mtcSpaceHelper.getAttribute("proccess_function_name");      
            loadFormula(mtcSpaceHelper.getElement("CMCFormula"));
            setQuantity(mtcSpaceHelper);
            parameters = new Mtc_CMCParameters(datasource);
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
                var set = uncertainties.Where(x => x.Proccess_function_name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return uncertainties.Count();
        }

        private void loadUncertainties(MtcSpaceHelper mtcSpaceHelper)
        {
            var els = mtcSpaceHelper.getElements("CMCUncertainty");
            foreach (XElement el in els)
            {
                Mtc_CMCUncertainty uncertainty = new Mtc_CMCUncertainty(el);
                uncertainties.Add(uncertainty);
            }
        }

        private Mtc_CMCUncertainties() { }

        public Mtc_CMCUncertainties(XElement datasource)
        {
            loadUncertainties(new MtcSpaceHelper(datasource));
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
        private string name = "";
        private string equipment_type = "";

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public string Equipment_type
        {
            get { return equipment_type; }
            //set { equipment_type = value; }
        }

        private Mtc_Role() { }

        public Mtc_Role(XElement datasource)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            equipment_type = mtcSpaceHelper.getAttribute("equipment_type");
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
                var set = roles.Where(x => x.Name == name);
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

        private Mtc_Roles() { }

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
         
        private Mtc_RequiredEquipment() { }

        public Mtc_RequiredEquipment(XElement datasource)
        {
            roles = new Mtc_Roles(datasource);
        }
    }


    public class Mtc_Range_Boundary : AbstractValue
    {
        private string test = "";

        public enum RangeType {Result, Parameter};

        public string Test
        {
            get { return test; }
            //set { test = value; }
        }

        private void setQuantity(string qtyName, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
        {
            if (processType != null)
            {
                Uom_Quantity uomQty = null;
                switch (rType)
                {
                    case Mtc_Range_Boundary.RangeType.Result:
                        var parentResultRange = (rangeName != "") ? processType.ProcessResult[rangeName] : processType.ProcessResult[0];
                        if (parentResultRange != null) uomQty = parentResultRange.Quantity;
                        break;

                    case Mtc_Range_Boundary.RangeType.Parameter:
                        var parentParameterRange = (rangeName != "") ? processType.ProcessParameter[rangeName] : null;
                        if (parentParameterRange != null) uomQty = parentParameterRange.Quantity;
                        break;
                }
                if (uomQty != null)
                {
                    if ((qtyName == "") || (qtyName == uomQty.Name))
                        quantity = UomDataSource.getQuantityByName(uomQty.Name);
                    else
                        throw new Exception("quantity mismatch");
                }     
            }
        }
     
        protected Mtc_Range_Boundary() { }

        public Mtc_Range_Boundary(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            test = mtcSpaceHelper.getAttribute("test");
            setQuantity(mtcSpaceHelper.getAttribute("quantity"), processType, rangeName, rType);
            uom_alternative = mtcSpaceHelper.getAttribute("uom_alternative");
            uom_alias_symbol = mtcSpaceHelper.getAttribute("uom_alias_symbol");
            format = mtcSpaceHelper.getAttribute("format");
            if (datasource != null) value = datasource.Value;
        }
    }

    public class Mtc_Range_End : Mtc_Range_Boundary
    {
        private Mtc_Range_End() { }

        public Mtc_Range_End(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
            : base(datasource, processType, rangeName, rType) { }
    }

    public class Mtc_Range_Start : Mtc_Range_Boundary 
    {
        private Mtc_Range_Start() { }

        public Mtc_Range_Start(XElement datasource, Mtc_ProcessType processType, string rangeName, Mtc_Range_Boundary.RangeType rType)
            : base(datasource, processType, rangeName, rType) { }
    }

    public class Mtc_Range 
    {
        private string name = "";   
        private Mtc_Range_Start start = null;
        private Mtc_Range_End end = null;

        public string Name
        {
            get { return name; }
            set { name = value; }
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
        
        private Mtc_Range() { }

        public Mtc_Range(XElement datasource, Mtc_ProcessType processType, Mtc_Range_Boundary.RangeType rType)
        {
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            var el1 = mtcSpaceHelper.getElement("Start");
            var el2 = mtcSpaceHelper.getElement("End");
            if (el1 != null) start = new Mtc_Range_Start(el1, processType, name, rType);
            if (el2 != null) end = new Mtc_Range_End(el2, processType, name, rType);
        }
    }


    public class Mtc_ProccessResultRanges : IEnumerable<Mtc_Range>
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
                var set = ranges.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First() : null;
            }
        }

        public int Count()
        {
            return ranges.Count();
        }

        private void loadRanges(MtcSpaceHelper mtcSpaceHelper, Mtc_ProcessType processType)
        {
            var els = mtcSpaceHelper.getElements("ProcessResultRange");
            foreach (XElement el in els)
            {
                Mtc_Range range = new Mtc_Range(el, processType, Mtc_Range_Boundary.RangeType.Result);
                ranges.Add(range);
            }
        }

        private Mtc_ProccessResultRanges() { }

        public Mtc_ProccessResultRanges(XElement datasource, Mtc_ProcessType processType)
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

    public class Mtc_ProccessParameterRanges : IEnumerable<Mtc_Range>
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
                var set = ranges.Where(x => x.Name == name);
                return (set.Count() > 0) ? set.First() : null; 
            }
        }

        public int Count()
        {
            return ranges.Count();
        }

        private void loadRanges(MtcSpaceHelper mtcSpaceHelper, Mtc_ProcessType processType)
        {
            var els = mtcSpaceHelper.getElements("ProccessParameterRanges");
            foreach (XElement el in els)
            {
                Mtc_Range range = new Mtc_Range(el, processType, Mtc_Range_Boundary.RangeType.Parameter);
                ranges.Add(range);
            }
        }

        private Mtc_ProccessParameterRanges() { }

        public Mtc_ProccessParameterRanges(XElement datasource, Mtc_ProcessType processType)
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


    public class Mtc_Technique 
    {

        private string name = "";
        private string process = "";
        private Mtc_ProccessResultRanges proccessResultRanges = null;
        private Mtc_ProccessParameterRanges proccessParameterRanges = null;
        private Mtc_RequiredEquipment requiredEquipment = null;
        private Mtc_CMCUncertainties cmcUncertainties = null;
        private Mtc_Documentation documentation = null;
        private Unc_CMCs cMCs;
        private Mtc_ProcessType processType = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public string ProcessTypeName
        {
            get { return process; }
            //set { process = value; }
        }

        public Mtc_ProcessType ProcessType
        {
            get { return processType; }
        }

        public Mtc_ProccessResultRanges ProccessResultRange
        {
            get { return proccessResultRanges; }
        }

        public Mtc_ProccessParameterRanges ProccessParameterRange
        {
            get { return proccessParameterRanges; }
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

        private void setProcessType(Unc_CMCs cMCs, string processName)
        {
            var mtcProcessTypes = cMCs.ProcessTypes.Select( x => x.ProcessType).Where(y => y.Name == processName);
            processType = (mtcProcessTypes.Count() > 0) ? mtcProcessTypes.First() : null;
        }
        
        private Mtc_Technique() { }

        public Mtc_Technique(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            MtcSpaceHelper mtcSpaceHelper = new MtcSpaceHelper(datasource);
            name = mtcSpaceHelper.getAttribute("name");
            process = mtcSpaceHelper.getAttribute("process");
            setProcessType(cMCs, process);
            proccessResultRanges = new Mtc_ProccessResultRanges(datasource, processType);
            proccessParameterRanges = new Mtc_ProccessParameterRanges(datasource, processType);
            var el1 = mtcSpaceHelper.getElement("RequiredEquipment");
            var el2 = mtcSpaceHelper.getElement("Documentation");
            if (el1 != null) requiredEquipment = new Mtc_RequiredEquipment(el1);
            if (el2 != null) cmcUncertainties = new Mtc_CMCUncertainties(datasource);
            documentation = new Mtc_Documentation(el2);
        }
    }

    public class Unc_ConstantValue : AbstractValue 
    {

        private string _const_parameter_name = "";
        private Unc_Template template = null;

        public string const_parameter_name
        {
            get { return _const_parameter_name; }
            //set { _const_parameter_name = value; }
        }

        private void setQuantity(string qtyName, Unc_Template template, string functionName, string variableName)
        {
            Mtc_CMCUncertainty cmcUncertainty = null;
            var technique = template.MtcTechnique;
            if (technique != null)
            {
                cmcUncertainty = technique.CMCUncertainties[functionName];
            }
            if (cmcUncertainty != null)
            {
                Uom_Quantity uomQty = null;
                var parentCMCValue = (variableName != "") ? cmcUncertainty.CMCParameters[variableName] : null;
                if (parentCMCValue != null) uomQty = parentCMCValue.Quantity;
                if (uomQty != null)
                {
                    if ((qtyName == "") || (qtyName == uomQty.Name))
                        quantity = UomDataSource.getQuantityByName(uomQty.Name);
                    else
                        throw new Exception("quantity mismatch");
                }
            }
        }

        public Unc_ConstantValue(XElement datasource, Unc_Template template, string functionName)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            _const_parameter_name = uncSpaceHelper.getAttribute("const_parameter_name");
            uom_alternative = uncSpaceHelper.getAttribute("uom_alternative");
            uom_alias_symbol = uncSpaceHelper.getAttribute("uom_alias_symbol");
            format = uncSpaceHelper.getAttribute("format");
            setQuantity(uncSpaceHelper.getAttribute("quantity"), template, functionName, _const_parameter_name);
            loadValue(datasource);
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
        private string test = "";
        private Unc_Template template = null;

        public string Test
        {
            get { return test; }
            //set { test = value; }
        }

        private void setQuantity(string qtyName, Mtc_ProcessType processType, string variableName)
        {
            if (processType != null)
            {
                Uom_Quantity uomQty = null;
                var parentParameterRange = (variableName != "") ? processType.ProcessParameter[variableName] : null;
                if (parentParameterRange != null) uomQty = parentParameterRange.Quantity;
                if (uomQty != null)
                {
                    if ((qtyName == "") || (qtyName == uomQty.Name))
                        quantity = UomDataSource.getQuantityByName(uomQty.Name);
                    else
                        throw new Exception("quantity mismatch");
                }
            }
        }

        protected Unc_Range_Boundary() { }

        public Unc_Range_Boundary(XElement datasource, Unc_Template template, string variableName)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            test = uncSpaceHelper.getAttribute("test");
            setQuantity(uncSpaceHelper.getAttribute("quantity"), template.MtcProcessType, variableName);
            uom_alternative = uncSpaceHelper.getAttribute("uom_alternative");
            uom_alias_symbol = uncSpaceHelper.getAttribute("uom_alias_symbol");
            format = uncSpaceHelper.getAttribute("format");
            loadValue(datasource);
        }
    }

    public class Unc_Range_End : Unc_Range_Boundary 
    {
        private Unc_Range_End() { }

        public Unc_Range_End(XElement datasource, Unc_Template template, string variableName)
            : base(datasource, template, variableName) { }
    }

    public class Unc_Range_Start : Unc_Range_Boundary 
    {

        private Unc_Range_Start() { }

        public Unc_Range_Start(XElement datasource, Unc_Template template, string variableName)
            : base(datasource, template, variableName) { }
    }

    public class Unc_Range 
    {
        private string variable_name = "";
        private string variable_type = "";
        private Unc_Range_Start start = null;
        private Unc_Range_End end = null;
        private Unc_ConstantValues constansts = null;
        private Unc_Ranges ranges = null;
        private Unc_Template template = null;

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
            get { return constansts; }
            set { constansts = value; }
        }

        public Unc_Ranges Ranges
        {
            get { return ranges; }
            set { ranges = value; }
        }

        protected Unc_Range() { }

        public Unc_Range(XElement datasource, Unc_Template template, string functionName)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            variable_name = uncSpaceHelper.getAttribute("variable_name");
            variable_type = uncSpaceHelper.getAttribute("variable_type");
            var el1 = uncSpaceHelper.getElement("Start");
            var el2 = uncSpaceHelper.getElement("End");
            if (el1 != null) start = new Unc_Range_Start(el1, template, Variable_name);
            if (el2 != null) end = new Unc_Range_End(el2, template, Variable_name);
            constansts = new Unc_ConstantValues(datasource, template, functionName);
            var rgsElement = uncSpaceHelper.getElement("Ranges");
            ranges = (rgsElement != null) ? new Unc_Ranges(rgsElement, template, functionName) : new Unc_Ranges();
        }
    }

    public class Unc_Ranges : IEnumerable<Unc_Range>
    {
        private List<Unc_Range> ranges = new List<Unc_Range>();
        private Unc_Template template = null;

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
                Unc_Range range = new Unc_Range(el, template, functionName);
                ranges.Add(range);
            }
        }

        public Unc_Ranges() {} // Needed if parent has no Ranges element.  Return empty Unc_Ranges 

        public Unc_Ranges(XElement datasource, Unc_Template template, string functionName) 
        {
            this.template = template;
            loadRanges(new UncSpaceHelper(datasource), template, functionName);
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
        private string name = "";
        private Unc_Range_Start start = null;
        private Unc_Range_End end = null;
        private Unc_Template template = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
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

        protected Unc_RangeOverride() { }

        public Unc_RangeOverride(XElement datasource, Unc_Template template)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            name = uncSpaceHelper.getAttribute("name");
            var el1 = uncSpaceHelper.getElement("Start");
            var el2 = uncSpaceHelper.getElement("End");
            if (el1 != null) start = new Unc_Range_Start(el1, template, name);
            if (el2 != null) end = new Unc_Range_End(el2, template, name);
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
                var set = rangeOverrides.Where(x => x.Name == name);
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

        private Unc_ParameterRangeOverrides() { }

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
                var set = rangeOverrides.Where(x => x.Name == name);
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

        private Unc_ResultOverrides() { }

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
        private string name = "";
        private Unc_ResultOverrides resultOverrides = null;
        private Unc_ParameterRangeOverrides parameterOverrides = null;
        private Unc_Template template = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public Unc_ResultOverrides ResultOverrides
        {
            get { return resultOverrides; }
        }
 
        public Unc_ParameterRangeOverrides ParameterOverrides
        {
            get { return parameterOverrides; }
        }

        private Unc_TemplateTechnique() { }

        public Unc_TemplateTechnique(XElement datasource, Unc_Template template)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            name = uncSpaceHelper.getAttribute("name");
            resultOverrides = new Unc_ResultOverrides(datasource, template);
            parameterOverrides = new Unc_ParameterRangeOverrides(datasource, template);
        }
    }

    public class Unc_InfluenceQuantity
    {
        private string name = "";
        private Uom_Quantity quantity = null;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Uom_Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private Unc_InfluenceQuantity() { }

        public Unc_InfluenceQuantity(XElement datasource)
        {
            name = new UncSpaceHelper(datasource).getAttribute("name");
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
                var set = quantities.Where(x => x.Name == name);
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

        private Unc_InfluenceQuantities() { }

        public Unc_InfluenceQuantities(XElement datasource)
        {
            loadQuantities(new UncSpaceHelper(datasource));
        }

        public IEnumerator<Unc_InfluenceQuantity> GetEnumerator()
        {
            return quantities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Unc_Assertion
    {
        private string name = "";
        private string value = "";

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public string Value
        {
            get { return this.value; }
            //set { this.value = value; }
        }

        private void loadName(XElement datasource)
        {
            if (datasource != null) name = datasource.Value;
        }

        private void loadValue(XElement datasource)
        {
            if (datasource != null) value = datasource.Value;
        }

        private Unc_Assertion() {}

        public Unc_Assertion(XElement datasource)
        {
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            var el1 = uncSpaceHelper.getElement("Name");
            var el2 = uncSpaceHelper.getElement("Value");
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

        private Unc_Assertions() {}

        public Unc_Assertions(XElement datasource)
        {
            loadAssertions(new UncSpaceHelper(datasource));
        }

        public IEnumerator<Unc_Assertion> GetEnumerator()
        {
            return assertions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        private Unc_Case() {}

        public Unc_Case(XElement datasource, Unc_Template template, string functionName)
        {
            this.template = template;
            assertions = new Unc_Assertions(datasource);
            var rgsElement = new UncSpaceHelper(datasource).getElement("Ranges");
            if (rgsElement != null) ranges = new Unc_Ranges(rgsElement, template, functionName);
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

        private Unc_Cases() {}

        public Unc_Cases(XElement datasource, Unc_Template template, string functionName) 
        {
            this.template = template;
            loadCases(new UncSpaceHelper(datasource), template, functionName);
            if (cases.Count() == 0) loadRanges(datasource, template, functionName);
        }

        public IEnumerator<Unc_Case> GetEnumerator()
        {
            return cases.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class Unc_CMCFunction
    {
        private string name = "";
        private Unc_Cases cases = null;
        private Unc_Template template = null;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Unc_Cases Cases
        {
            get { return cases; }
            set { cases = value; }
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

        private Unc_CMCFunction() { }

        public Unc_CMCFunction(XElement datasource, Unc_Template template)
        {
            this.template = template;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            name = uncSpaceHelper.getAttribute("name");
            cases = new Unc_Cases(datasource, template, name);
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
                var set = functions.Where(x => x.Name == name);
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

        private Unc_CMCFunctions() { }

        public Unc_CMCFunctions(XElement datasource, Unc_Template template)
        {
            this.template = template;
            loadFunctions(new UncSpaceHelper(datasource), template);
        }

        public IEnumerator<Unc_CMCFunction> GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public Unc_CMCFunctions CMCFunctions
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
            get { return (mtcProcessType != null) ? mtcProcessType.ProcessResult : new Mtc_ProcessResults(); }
        }

        public IList<string> ResultTypes
        {
            get { return (mtcProcessType != null) ? mtcProcessType.ResultTypes : null; }
        }

        public Mtc_CMCUncertainty getCMCUncertaintyByFunctionName(string functionName)
        {
            return (mtcTechnique != null) ? mtcTechnique.CMCUncertainties[functionName] : null;
        }

        public string getCMCFunctionText(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.FunctionText : "";
        }

        public IList<string> getCMCFunctionSymbols(string functionName)
        {
            var unc = getCMCUncertaintyByFunctionName(functionName);
            return (unc != null) ? unc.Symbols : new List<string>();
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
            var fnc = CMCFunctions[functionName];
            return (fnc != null) ? fnc.RangeVariables : new List<string>();
        }

        public IList<string> getCMCFunctionAssertionNames(string functionName)
        {
            var fnc = CMCFunctions[functionName];
            return (fnc != null) ? fnc.AssertionNames : new List<string>();
        }

        public IList<string> getCMCFunctionAssertionValues(string functionName, string assertionName)
        {
            var fnc = CMCFunctions[functionName];
            return fnc.getAssertionValuesByAssertionName(assertionName);
        }

        public Unc_Template(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            influenceQuantities = new Unc_InfluenceQuantities(datasource);
            UncSpaceHelper unsSpaceHelper = new UncSpaceHelper(datasource);
            var el = unsSpaceHelper.getElement("Technique");
            if (el != null) templateTechnique = new Unc_TemplateTechnique(el, this);
            if (templateTechnique != null) {
                Unc_Technique uncTech = cMCs.Technique[templateTechnique.Name];
                mtcTechnique = (uncTech != null) ? uncTech.Technique : null;
                mtcProcessType = (mtcTechnique != null) ? mtcTechnique.ProcessType : null;
            }
            cmcFunctions = new Unc_CMCFunctions(datasource, this);        
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

        private Unc_Templates() { }

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
        private string name = "";

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        private Unc_CmcCategory() { }

        public Unc_CmcCategory(XElement datasource)
        {
            name = new UncSpaceHelper(datasource).getAttribute("name");
        }
    }

    public class Unc_CMC 
    {
        private Unc_CmcCategory category = null;
        private List<string> dut_Types = new List<string>();
        private Unc_Templates templates = null;
        private Unc_CMCs cMCs = null;

        public Unc_CmcCategory Category
        {
            get { return category; }
            //set { categgory = value; }
        }
 
        public List<string> Dut_Types
        {
            get { return dut_Types; }
            //set { dut_Types = value; }
        }

        public Unc_Templates Templates
        {
            get { return templates; }
            //set { template = value; }
        }

        private void loadDutTypes(UncSpaceHelper uncSpaceHelper)
        {
            var el1 = uncSpaceHelper.getElement("DUT_Types");
            if (el1 != null)
            {
                dut_Types = new UncSpaceHelper(el1).getValues("DUT_Type");
            }
            else
            {
                var el2 = uncSpaceHelper.getElement("DUT_Type");
                if (el2 != null) dut_Types.Add(el2.Value);
            }
        }

        private Unc_CMC() {}

        public Unc_CMC(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            var el = uncSpaceHelper.getElement("Category");
            if (el != null) category = new Unc_CmcCategory(el);
            loadDutTypes(uncSpaceHelper);
            templates = new Unc_Templates(datasource, cMCs);
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

        private Unc_CMCList() { }

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
        private string name = "";
        private Mtc_Technique technique = null;
        private Unc_CMCs cMCs = null;

        public string Name
        {
            get { return name; }
            //set { technique_id = value; }
        }

        public Mtc_Technique Technique
        {
            get { return technique; }
            //set { technique = value; }
        }

        private Unc_Technique() { }

        public Unc_Technique(XElement datasource, Unc_CMCs cMCs)
        {
            this.cMCs = cMCs;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            name = uncSpaceHelper.getAttribute("name");
            var external = uncSpaceHelper.getElement("ExternalDefintion");
            if (external != null)
            {
                /// TODO instantiate technique using external definition
            }
            else
            {
                var local = new MtcSpaceHelper(datasource).getElement("Technique");
                if (local != null)
                {
                    technique = new Mtc_Technique(local, cMCs);
                }
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
                var set = techniques.Where(x => x.Name == name);
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

        private Unc_Techniques() { }

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
        private string name = "";
        private Mtc_ProcessType processType = null;

        public string Name
        {
            get { return name; }
            //set { name = value; }
        }

        public Mtc_ProcessType ProcessType
        {
            get { return processType; }
            //set { processType = value; }
        }

        private Unc_ProcessType() { }

        public Unc_ProcessType(XElement datasource)
        {
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            name = uncSpaceHelper.getAttribute("name");
            var external = uncSpaceHelper.getElement("ExternalDefintion");
            if (external != null)
            {
                /// TODO instantiate processType using external definition
            }
            else
            {
                var local = new MtcSpaceHelper(datasource).getElement("ProcessType");
                if (local != null)
                {
                    processType = new Mtc_ProcessType(local);
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
                var matches = processTypes.Where(x => (string) x.Name == name);
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

        private Unc_ProcessTypes() {}

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

        private Unc_CMCs() { }

        public Unc_CMCs(XElement datasource, Soa_Activity activity)
        {
            this.activity = activity;
            UncSpaceHelper uncSpaceHelper = new UncSpaceHelper(datasource);
            processTypes = new Unc_ProcessTypes(datasource);
            techniques = new Unc_Techniques(datasource, this);
            cmcs = new Unc_CMCList(datasource, this);
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

        private Soa_Activity() { }

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

        private Soa_Activities() { }

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

    public class Soa_CapabilityScope_Location_ContactInfo
    {
        private string phoneNumber = "";
        private string email = "";
        private string url = "";

        public string PhoneNumber
        {
            get { return phoneNumber; }
            //set { phoneNumber = value; }
        }

        public string Email
        {
            get { return email; }
            //set { email = value; }
        }

        public string Url
        {
            get { return url; }
            //set { url = value; }
        }

        private Soa_CapabilityScope_Location_ContactInfo() { }

        public Soa_CapabilityScope_Location_ContactInfo(XElement datasource)
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            phoneNumber = soaSpaceHelper.getValue("PhoneNumber");
            email = soaSpaceHelper.getValue("email");
            url = soaSpaceHelper.getValue("URL");
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
            //set { street = value; }
        }

        public string City
        {
            get { return city; }
            //set { city = value; }
        }

        public string State
        {
            get { return state; }
            //set { state = value; }
        }

        public string Zip
        {
            get { return zip; }
            //set { zip = value; }
        }

        private Soa_CapabilityScope_Location_OrganizationAddress() { }

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
        private Soa_CapabilityScope_Location_ContactInfo contactInfo = null;

        public Soa_CapabilityScope_Location_OrganizationAddress OrganizationAddress
        {
            get { return organizationAddress; }
            //set { organizationAddress = value; }
        }

        public string ContactName
        {
            get { return contactName; }
            //set { contactName = value; }
        }

        public Soa_CapabilityScope_Location_ContactInfo ContactInfo
        {
            get { return contactInfo; }
           // set { contactInfo = value; }
        }

        private Soa_CapabilityScope_Location() { }

        public Soa_CapabilityScope_Location(XElement datasource) 
        {
            SoaSpaceHelper soaSpaceHelper = new SoaSpaceHelper(datasource);
            var el1 = soaSpaceHelper.getElement("OrganizationAddress");
            var el2 = soaSpaceHelper.getElement("ContactInfo");
            if (el1 != null) organizationAddress = new Soa_CapabilityScope_Location_OrganizationAddress(el1);
            contactName = soaSpaceHelper.getValue("ContactName");
            if (el2 != null) contactInfo = new Soa_CapabilityScope_Location_ContactInfo(el2);
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

        private Soa_CapabilityScope_Locations() { }

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

        private Soa_CapabilityScope() { }

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

        private Soa_ScopeUrl() { }

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

        private Soa_ScopeUrls() { }

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
           // set { ab_ID = value; }
        }

        public string Ab_Logo_Signature
        {
            get { return ab_Logo_Signature; }
            //set { ab_Logo_Signature = value; }
        }

        public string Scope_ID_Number
        {
            get { return scope_ID_Number; }
            //set { scope_ID_Number = value; }
        }

        public Soa_ScopeUrls ScopeUrls
        {
            get { return scopeUrls; }
            //set { scopeUrls = value; }
        }

        public string Criteria
        {
            get { return criteria; }
            //set { criteria = value; }
        }

        public string EffectiveDate
        {
            get { return effectiveDate; }
           // set { effectiveDate = value; }
        }


        public string ExpirationDate
        {
            get { return expirationDate; }
            //set { expirationDate = value; }
        }

        public string Statement
        {
            get { return statement; }
            //set { statement = value; }
        }

        public Soa_CapabilityScope CapabilityScope
        {
            get { return capabilityScope; }
            //set { capabilityScope = value; }
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

        private Soa() { }

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
