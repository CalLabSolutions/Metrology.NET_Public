using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Windows.Storage;

namespace MT_DataAccessLib
{
    static class Namespaces
    {
        public const string BASE_UIR = "https://cls-schemas.s3.us-west-1.amazonaws.com/";
        public const string MTC = BASE_UIR + "MetrologyTaxonomyCatalog";
        public const string UOM = BASE_UIR + "UOM_Database";
        public const string MathML_NS = "http://www.w3.org/1998/Math/MathML";
        public const string MathML_SL = "http://www.w3.org/Math/XMLSchema/mathml3/mathml3.xsd";
    }
    
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    [XmlRoot(Namespace = Namespaces.MTC, IsNullable = false)]
    public class Taxonomy
    {
        private List<Taxon> taxons;

        [XmlElement("Taxon")]
        public List<Taxon> Taxons
        {
            get { return taxons; }
            set { taxons = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Taxon : ICloneable
    {
        public Taxon() { }
        protected Taxon(Taxon taxon)
        {
            Name = taxon.Name;
            Deprecated = taxon.Deprecated;
            Replacement = taxon.Replacement;
            ExternalReference = taxon.ExternalReference;
            Parameters = taxon.Parameters;
            Results = taxon.Results;
            Discipline = taxon.Discipline;
            Definition = taxon.Definition;
        }
        
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool deprecated;

        [XmlAttribute("deprecated")]
        public bool Deprecated
        {
            get { return deprecated; }
            set { deprecated = value; }
        }

        private string replacement = "";

        [XmlAttribute("replacement")]
        public string Replacement
        {
            get { return replacement; }
            set { replacement = value; }
        }

        private ExternalReference externalReference;

        [XmlElement("ExternalReference", IsNullable = false)]
        public ExternalReference ExternalReference
        {
            get { return externalReference; }
            set { externalReference = value; }
        }

        private List<Result> results;

        [XmlElement("Result")]
        public List<Result> Results
        {
            get { return results; }
            set { results = value; }
        }

        private List<Parameter> parameters;

        [XmlElement("Parameter")]
        public List<Parameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        [XmlElement("Discipline", IsNullable = false)]
        public Discipline Discipline
        {
            get { return discipline; }
            set { discipline = value; }
        }

        private string definition = "";

        [XmlElement("Definition")]
        public string Definition
        {
            get { return definition; }
            set { definition = TaxonomyFactory.Tools.Format(value); }
        }

        private Discipline discipline;

        public object Clone()
        {
            return new Taxon(this);
        }

        //private string documentation = "";

        //[XmlElement("Documentation")]
        //public string Documentation
        //{
        //    get { return documentation;  }
        //    set { documentation = value;  }
        //}
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Result
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Quantity quantity;

        [XmlElement("Quantity", Namespace = Namespaces.UOM)]
        public Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Parameter
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool optional = false;

        [XmlAttribute("optional")]
        public bool Optional
        {
            get { return optional; }
            set { optional = value; }
        }

        private Quantity quantity;

        [XmlElement("Quantity", Namespace = Namespaces.UOM)]
        public Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private string definition = "";

        public string Definition
        {
            get { return definition; }
            set { definition = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.UOM)]
    public class Quantity
    {
        private string name;

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Discipline
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<string> subDisciplines;

        [XmlElement("SubDiscipline", IsNullable = false)]
        public List<string> SubDisciplines
        {
            get { return subDisciplines; }
            set { subDisciplines = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class ExternalReference
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<CategoryTag> categoryTags;

        [XmlElement("CategoryTag", IsNullable = false)]
        public List<CategoryTag> CategoryTags
        {
            get { return categoryTags; }
            set { categoryTags = value; }
        }

        private string url = "";

        [XmlElement("url")]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class CategoryTag
    {
        private string name = "";

        [XmlElement("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string value = "";

        [XmlElement("value")]
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }



    public class TaxonomyFactory
    {
        private const string DataStore = @"ms-appx:///DataStore/";       
        private const string XmlFromServer = Namespaces.BASE_UIR + "MetrologyTaxonomyCatalog.xml";
        private const string CSS = "metrologytaxonomy.css";
        private const string JS = "metrologytaxonomy.js";
        private const string XSL = "metrologytaxonomy.xsl";
        private const string XmlOut = @"MTC_Local.xml";

        // We might need to see where our data is coming from
       public bool LoadedFromServer
        {
            get { return TaxonomyLoader.LoadedFromServer; }
            set { TaxonomyLoader.LoadedFromServer = value;  }
        }

        // Get all the taxons
        public IEnumerable<Taxon> GetAllTaxons()
        {
            if (taxonomy == null)
            {
                Load();
            }
            return taxonomy.OrderBy(taxon => taxon.Name).ToList();
        }

        // add Taxon to our list and reorder
        public IEnumerable<Taxon> Add(Taxon taxon)
        {
            if (taxonomy == null)
            {
                Load();
            }
            taxonomy.Add(taxon);
            taxonomy = taxonomy.OrderBy(t => t.Name).ToList();
            return taxonomy;
        }

        // add Taxon to our list and reorder
        public IEnumerable<Taxon> Edit(Taxon taxon, string oldTaxonName)
        {
            if (taxonomy == null)
            {
                Load();
            }
            taxonomy.RemoveAll(t => t.Name.Equals(oldTaxonName));
            taxonomy.Add(taxon);
            taxonomy = taxonomy.OrderBy(t => t.Name).ToList();
            return taxonomy;
        }

        // delete taxon
        public IEnumerable<Taxon> Delete(Taxon taxon)
        {
            if (taxonomy == null)
            {
                Load();
            }         
            taxonomy.RemoveAll(t => t.Name.Equals(taxon.Name));
            return taxonomy;
        }

        // get taxon count
        public int Count()
        {
            if (taxonomy != null)
            {
                return taxonomy.Count;
            }
            return 0;
        }


        // Search by Name
        public IEnumerable<Taxon> GetByName(string search, string filter)
        {
            // Check if we hare filtering just Source or Measure
            filter = filter == null ? "all" : filter.ToLower();
            if (search == null && (filter == "measure" || filter == "source" || filter == "deprecated"))
            {
                switch (filter)
                {
                    case "measure":
                        return taxonomy.Where(t => t.Name.ToLower().Contains(".measure."));
                    case "source":
                        return taxonomy.Where(t => t.Name.ToLower().Contains(".source."));
                    case "deprecated":
                        return taxonomy.Where(t => t.Deprecated == true);
                }
            }

            if (search == null) return taxonomy; // if  null here just return all

            search = search.ToLower();
            switch (filter)
            {
                case "all":
                    return taxonomy.Where(t => t.Name.ToLower().Contains(search));
                case "measure":
                    return taxonomy
                        .Where(t => t.Name.ToLower().Contains(".measure."))
                        .Where(t => t.Name.ToLower().Contains(search));
                case "source":
                    return taxonomy
                        .Where(t => t.Name.ToLower().Contains(".source."))
                        .Where(t => t.Name.ToLower().Contains(search));
                case "results":
                    return taxonomy
                        .Where(t => t.Results
                        .Where(w => w.Name.ToLower().Contains(search)).Count() > 0);
                case "parameters":
                    return taxonomy
                        .Where(t => t.Parameters
                        .Where(w => w.Name.ToLower().Contains(search)).Count() > 0);
                case "deprecated":
                    return taxonomy
                        .Where(t => t.Deprecated == true)
                        .Where(t => t.Name.ToLower().Contains(search));
            }
            return Enumerable.Empty<Taxon>();

        }

        // Add the files needed to use XSLT
        public async Task<bool> SaveWithXSLT(StorageFolder folder)
        {
            try
            {
                // see if the files we need exist, if so delete them
                var newFile = await folder.TryGetItemAsync("Local MetrologyTaxonomy.xml");
                if (newFile != null)
                {
                    await newFile.DeleteAsync();
                }

                newFile = await folder.TryGetItemAsync(XSL);
                if (newFile != null)
                {
                    await newFile.DeleteAsync();
                }

                newFile = await folder.TryGetItemAsync(CSS);
                if (newFile != null)
                {
                    await newFile.DeleteAsync();
                }

                newFile = await folder.TryGetItemAsync(JS);
                if (newFile != null)
                {
                    await newFile.DeleteAsync();
                }

                // add current taxons xml file
                var file = await folder.CreateFileAsync("Local MetrologyTaxonomy.xml");
                await FileIO.WriteTextAsync(file, this.Save(GetAllTaxons(), false, true));
                
                // add xsl transformation
                file = await folder.CreateFileAsync(XSL);
                var nextFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(DataStore + XSL));
                string text = await FileIO.ReadTextAsync(nextFile);
                await FileIO.WriteTextAsync(file, text);

                // add css transformation
                file = await folder.CreateFileAsync(CSS);
                nextFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(DataStore + CSS));
                text = await FileIO.ReadTextAsync(nextFile);
                await FileIO.WriteTextAsync(file, text);

                // add js transformation
                file = await folder.CreateFileAsync(JS);
                nextFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(DataStore + JS));
                text = await FileIO.ReadTextAsync(nextFile);
                await FileIO.WriteTextAsync(file, text);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public string Save(IEnumerable<Taxon> taxonomy, bool local = false, bool xslt = false)
        {
            try
            {
                Taxonomy toXml = new Taxonomy
                {
                    Taxons = taxonomy.ToList()
                };
                XmlSerializer serializer = new XmlSerializer(typeof(Taxonomy));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("mtc", Namespaces.MTC);
                ns.Add("uom", Namespaces.UOM);
                using (var ms = new MemoryStream())
                {
                    var xmlWriterSettings = new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        OmitXmlDeclaration = false,
                        Indent = true
                    };
                    using (var xw = XmlWriter.Create(ms, xmlWriterSettings))
                    {
                        if (xslt)
                            xw.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"metrologytaxonomy.xsl\"");
                        serializer.Serialize(xw, toXml, ns);
                    }

                    string xml = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    if (local)
                    {
                        SaveLocal(xml);
                    }
                    return xml;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "";
            }
        }

        public async void ReplaceLocal(string xml)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            var isFile = await localFolder.TryGetItemAsync(XmlOut);
            if (isFile != null)
            {
                file = await localFolder.GetFileAsync(XmlOut);
            }
            else
            {
                return;
            }
            await FileIO.WriteTextAsync(file, xml);
            Reload();
        }

        private async void SaveLocal(string xml)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            var isFile = await localFolder.TryGetItemAsync(XmlOut);
            if (isFile == null)
            {
                file = await localFolder.CreateFileAsync(XmlOut);
            }
            else
            {
                file = await localFolder.GetFileAsync(XmlOut);
            }
            // if (await Validate())
            // {
                await FileIO.WriteTextAsync(file, xml);
                Reload();
            // }
            
        }

        // TODO: Get the validaton working
        private async Task<bool> Validate()
        {
            // get our local folder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            
            // Set up our schema and settings
            var settings = new XmlReaderSettings();
            XmlSchemaSet schemaSet = new XmlSchemaSet();

            schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);
            schemaSet.Add(Namespaces.MTC, Namespaces.MTC + ".xsd");            
            schemaSet.Add(Namespaces.UOM, Namespaces.UOM + ".xsd");
            schemaSet.Add(Namespaces.MathML_NS, Namespaces.MathML_SL);
            
            // compile schemas
            schemaSet.Compile();

            // Add our imports as needed
            XmlSchema mtcSchema = null;
            XmlSchema uomSchema = null;
            XmlSchema mathSchema = null;
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                switch (schema.TargetNamespace)
                {
                    case Namespaces.MTC:
                        mtcSchema = schema;
                        break;
                    case Namespaces.UOM:
                        uomSchema = schema;
                        break;
                    case Namespaces.MathML_NS:
                        mathSchema = schema;
                        break;
                }
            }

            XmlSchemaImport uomImport = new XmlSchemaImport();
            uomImport.Namespace = Namespaces.UOM;
            uomImport.SchemaLocation = Namespaces.UOM + ".xsd";
            uomImport.Schema = uomSchema;
            mtcSchema.Includes.Add(uomImport);

            XmlSchemaImport mathImport = new XmlSchemaImport();
            mathImport.Namespace = Namespaces.MathML_NS;
            mathImport.SchemaLocation = Namespaces.MathML_SL;
            mathImport.Schema = mathSchema;
            uomSchema.Includes.Add(mathImport);

            // reporocess and compile
            schemaSet.Reprocess(mtcSchema);
            schemaSet.Reprocess(uomSchema);
            schemaSet.Reprocess(mathSchema);

            schemaSet.Compile();

            settings.Schemas = schemaSet;

            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            var xmlFile = await localFolder.GetFileAsync(XmlOut);
            var xmlPath = new Uri(xmlFile.Path);
            try
            {
                XmlReader reader = XmlReader.Create(xmlPath.ToString(), settings);
                while (reader.Read())
                reader.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private static void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Debug.Write("WARNING: ");               
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Debug.Write("ERROR: ");                
            }
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Exception.LineNumber);
            // I can not get this import to work so ignore it
            if (e.Message.Contains("MathML:math"))
            {
                return;
            } else
            {
                throw new Exception(e.Message);
            }
        }


        private static List<Taxon> taxonomy;
        public TaxonomyFactory()
        {
            Load();
        }

        private async void Load(bool refresh = false, bool fromServer = false)
        {
            taxonomy = await TaxonomyLoader.LoadDataAsync(refresh, fromServer);
        }

        public void Reload(bool fromServer = false)
        {
            Load(true, fromServer);
        }


        public string ExportHTML(Taxon taxon)
        {
            // text to send back
            string text;
            
            //Name and Slug
            string name = taxon.Name;
            string slug = name.Replace("TestProcess.", "");
            slug = slug.Replace(".", "-").ToLower();
            string header = "<!--\nName: {name}\n\nSlug: {slug}\n\n";
            header = header.Replace("{name}", name);
            header = header.Replace("{slug}", slug);
            text = header;

            // html
            string definition = "HTML: -->\n<p>{definition}{deprecated}</p>\n\n<!--more-->\n\n";
            definition = definition.Replace("{definition}", taxon.Definition);
            if (taxon.Deprecated)
            {
                definition = definition.Replace("{deprecated}", "\nDeprecated - " + taxon.Replacement);
            }
            else
            {
                definition = definition.Replace("{deprecated}", "");
            }
            text += definition;

            // seperate required and optional parameters
            List<Parameter> required = new List<Parameter>();
            List<Parameter> optional = new List<Parameter>();
            foreach (Parameter parameter in taxon.Parameters){
                if (parameter.Optional)
                {
                    optional.Add(parameter);
                }
                else
                {
                    required.Add(parameter);
                }
            }

            // Parameters
            string li = "\t<li>{name}{definition}{quantity}</li>\n";
            string nextli = "";
            if (required.Count > 0)
            {                
                text += "<strong>Required Parameters</strong>\n<ul>\n{required_params}\n</ul>\n";
                foreach (Parameter parameter in required)
                {
                    nextli += li;
                    nextli = nextli.Replace("{name}", parameter.Name);
                    if (parameter.Definition != null && parameter.Definition != "")
                    {
                        nextli = nextli.Replace("{definition}", " - " + parameter.Definition);
                    }
                    else
                    {
                        nextli = nextli.Replace("{definition}", "");
                    }
                    if (parameter.Quantity != null)
                    {
                        nextli = nextli.Replace("{quantity}", "<br>Quantity - " + parameter.Quantity.Name);
                    }
                    else
                    {
                        nextli = nextli.Replace("{quantity}", "");
                    }
                }
                text = text.Replace("{required_params}", nextli);
            }
            if (optional.Count > 0)
            {
                li = "\t<li>{name}{definition}{quantity}</li>\n";
                nextli = "";
                text += "<strong>Optional Parameters</strong>\n<ul>\n{optional_params}\n</ul>\n";
                foreach (Parameter parameter in optional)
                {
                    nextli += li;
                    nextli = nextli.Replace("{name}", parameter.Name);
                    if (parameter.Definition != null && parameter.Definition != "")
                    {
                        nextli = nextli.Replace("{definition}", " - " + parameter.Definition);
                    }
                    else
                    {
                        nextli = nextli.Replace("{definition}", "");
                    }
                    if (parameter.Quantity != null)
                    {
                        nextli = nextli.Replace("{quantity}", "<br>Quantity - " + parameter.Quantity.Name);
                    }
                    else
                    {
                        nextli = nextli.Replace("{quantity}", "");
                    }
                }
                text = text.Replace("{optional_params}", nextli);
            }

            // results
            if (taxon.Results != null)
            {
                string type = "Output";
                if (taxon.Name.ToLower().Contains("measure")) type = "Measured";
                text += "<strong>" + type + " Value &amp; Uncertainty</strong>\n<ul>\n{results}\n</ul>";
                li = "\t<li>{name}{quantity}</li>\n";                
                foreach (Result result in taxon.Results)
                {
                    nextli += li;
                    nextli = nextli.Replace("{name}", result.Name);
                    if (result.Quantity != null)
                    {
                        nextli = nextli.Replace("{quantity}", "<br>Quantity - " + result.Quantity.Name);
                    }
                    else
                    {
                        nextli = nextli.Replace("{quantity}", "");
                    }
                }
                text = text.Replace("{results}", nextli);
            }
            return text;
        }

        internal static class TaxonomyLoader
        {
            // Load from the xml file
            private static Task<List<Taxon>> loaderTask;
            private static bool loadedFromServer = true;
            public static bool LoadedFromServer
            {
                get { return loadedFromServer;  }
                set { loadedFromServer = value;  }
            }

            public static Task<List<Taxon>> LoadDataAsync(bool refresh = false, bool fromServer = false)
            {
                if (refresh || loaderTask == null)
                {
                    loaderTask = LoadDataCoreAsync(fromServer);
                }
                return loaderTask;
            }

            private static async Task<List<Taxon>> LoadDataCoreAsync(bool fromServer = false)
            {
                List<Taxon> taxonomy = new List<Taxon>();
                try
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFile file = null;
                    var isFile = await localFolder.TryGetItemAsync(XmlOut);
                    if (isFile == null || fromServer)
                    {
                        // create local xml file
                        string xml = Tools.BuildXml(XmlFromServer);
                        file = await localFolder.CreateFileAsync(XmlOut);
                        await FileIO.WriteTextAsync(file, xml);
                        LoadedFromServer = true;
                    }
                    else
                    {
                        file = await localFolder.GetFileAsync(XmlOut);
                        LoadedFromServer = false;
                    }
                    
                    var stream = new StreamReader(file.Path);
                    XmlSerializer serializer = new XmlSerializer(typeof(Taxonomy));
                    Taxonomy fromXml = (Taxonomy)serializer.Deserialize(stream);
                    foreach (Taxon taxon in fromXml.Taxons)
                    {
                        taxonomy.Add(taxon);
                    }
                    stream.Close();
                    return taxonomy;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    taxonomy.Clear();                    
                    return taxonomy;
                }
            }
        }

        internal static class Tools
        {
            public static string Format(string value)
            {
                value = value.Replace("\t", "");
                value = value.Replace("\r", "");
                string[] lines = value.Split('\n');
                List<string> newValue = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();
                    if (lines[i].Length > 0)
                        newValue.Add(lines[i]);
                }
                return newValue.Count > 0 ? string.Join(" ", newValue) : value;
            }

            public static string BuildXml(string url)
            {
                XmlTextReader reader = new XmlTextReader(url);
                StringBuilder sb = new StringBuilder();
                if (reader != null)
                {
                    while (reader.Read())
                        sb.AppendLine(reader.ReadOuterXml());

                    return sb.ToString();
                }
                return string.Empty;
            }

        }

        protected class XmlXsdResolver : XmlUrlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
        }

    }
}