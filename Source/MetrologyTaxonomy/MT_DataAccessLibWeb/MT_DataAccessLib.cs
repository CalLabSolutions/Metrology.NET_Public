using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MT_DataAccessLib
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
    [XmlRoot(Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd", IsNullable = false)]
    public partial class Taxonomy
    {
        private Taxon[] taxons;

        [XmlElement("Taxon")]
        public Taxon[] Taxons
        {
            get { return taxons; }
            set { taxons = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
    public partial class Taxon
    {
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

        [XmlElement("ExternalReference")]
        public ExternalReference ExternalReference
        {
            get { return externalReference; }
            set { externalReference = value; }
        }

        private Result[] results;

        [XmlElement("Result")]
        public Result[] Results
        {
            get { return results; }
            set { results = value; }
        }

        private Parameter[] parameters;

        [XmlElement("Parameter")]
        public Parameter[] Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        [XmlElement("Discipline")]
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

        //private string documentation = "";

        //[XmlElement("Documentation")]
        //public string Documentation
        //{
        //    get { return documentation;  }
        //    set { documentation = value;  }
        //}
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
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

        [XmlElement("Quantity", Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/UOM_Database.xsd")]
        public Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
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

        [XmlElement("Quantity", Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/UOM_Database.xsd")]
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
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/UOM_Database.xsd")]
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
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
    public class Discipline
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string[] subDisciplines = { };

        [XmlElement("SubDiscipline")]
        public string[] SubDisciplines
        {
            get { return subDisciplines; }
            set { subDisciplines = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
    public class ExternalReference
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private CategoryTag[] categoryTags = { };

        [XmlElement("CategoryTag")]
        public CategoryTag[] CategoryTags
        {
            get { return categoryTags; }
            set { categoryTags = value; }
        }

        private string url;

        [XmlElement("url")]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd")]
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
        private const string DataStore = @"DataStore/";
        private const string XmlIn = "TestMTC.xml";

        // Get all the taxons
        public IEnumerable<Taxon> GetAllTaxons()
        {
            if (taxonomy == null)
            {
                Load();
            }
            return taxonomy;
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



        public string SaveXml(IEnumerable<Taxon> taxonomy)
        {
            try
            {
                Taxonomy toXml = new Taxonomy
                {
                    Taxons = taxonomy.ToArray()
                };
                XmlSerializer serializer = new XmlSerializer(typeof(Taxonomy));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("mtc", "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/TestMTC.xsd");
                ns.Add("uom", "file:///C:/_Programming/Metrology.NET_Public/Source/MetrologyTaxonomy/MT_UI/DataStore/UOM_Database.xsd");
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
                        serializer.Serialize(xw, toXml, ns);
                    }

                    string xml = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return xml;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "";
            }
        }


        private static IList<Taxon> taxonomy;
        public TaxonomyFactory()
        {
            Load();
        }

        private void Load(bool refresh = false)
        {
            taxonomy = TaxonomyLoader.LoadData(refresh);
        }

        public void Reload()
        {
            Load(true);
        }

        internal static class TaxonomyLoader
        {


            // Load from the xml file
            private static List<Taxon> loaderTask;

            public static List<Taxon> LoadData(bool refresh = false)
            {
                if (refresh || loaderTask == null)
                {
                    loaderTask = LoadDataCore();
                }
                return loaderTask;
            }

            private static List<Taxon> LoadDataCore()
            {
                List<Taxon> taxonomy = new List<Taxon>();
                try
                {
                    var stream = new StreamReader(File.OpenRead(DataStore + XmlIn));
                    XmlSerializer serializer = new XmlSerializer(typeof(Taxonomy));
                    Taxonomy fromXml = (Taxonomy)serializer.Deserialize(stream);
                    foreach (Taxon taxon in fromXml.Taxons)
                    {
                        taxonomy.Add(taxon);
                    }
                    if (taxonomy.Count == 0)
                    {
                        taxonomy.Add(new Taxon() { Name = "No Taxonmies Available" });
                    }
                    taxonomy.OrderBy(taxon => taxon.Name);
                    return taxonomy;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    taxonomy.Clear();
                    taxonomy.Add(new Taxon() { Name = "Error Loading XML file" });
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
        }

    }
}