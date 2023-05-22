using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MT_DataAccessLib
{
    public class TaxonomyFactory
    {
        #region Properties and Taxon List Methods

        // We might need to see or set where our data is coming from
        public bool LoadedFromServer
        {
            get => TaxonomyLoader.LoadedFromServer;
            set => TaxonomyLoader.LoadedFromServer = value;
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

        // Get all non deprecated taxons
        public IEnumerable<Taxon> GetAllNonDeprecated()
        {
            if (taxonomy == null)
            {
                Load();
            }
            return taxonomy.Where(t => t.Deprecated.Equals(false));
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

        private static List<Taxon> taxonomy;

        public TaxonomyFactory(bool refresh = false, bool fromServer = false, bool readOnly = false)
        {
            Load(refresh, fromServer, readOnly);
        }

        private async void Load(bool refresh = false, bool fromServer = false, bool readOnly = false)
        {
            taxonomy = await TaxonomyLoader.LoadDataAsync(refresh, fromServer, readOnly);
        }

        public void Reload(bool fromServer = false)
        {
            Load(true, fromServer);
        }

        #endregion Properties and Taxon List Methods

        #region File IO

        public readonly static string LocalPath = Directory.GetCurrentDirectory() + "\\Resources\\";
        public const string Catalog = "MetrologyTaxonomyCatalog";
        private const string CatalogXml = Catalog + ".xml";
        private const string XmlFromServer = Namespaces.BASE_UIR + Catalog + ".xml";
        private const string XsdFromServer = Namespaces.BASE_UIR + Catalog + ".xsd";
        private const string CSS = "metrologytaxonomy.css";
        private const string JS = "metrologytaxonomy.js";
        private const string XSL = "metrologytaxonomy.xsl";
        private const string XmlWithXsl = "Local MetrologyTaxonomy.xml";

        // Add the files needed to use XSLT
        public async Task<bool> SaveWithXSLT(string selectedPath)
        {
            try
            {
                // see if the files we need exist, if so delete them
                if (File.Exists(selectedPath + XmlWithXsl))
                {
                    File.Delete(selectedPath + XmlWithXsl);
                }
                if (File.Exists(selectedPath + XSL))
                {
                    File.Delete(selectedPath + XSL);
                }
                if (File.Exists(selectedPath + CSS))
                {
                    File.Delete(selectedPath + CSS);
                }
                if (File.Exists(selectedPath + JS))
                {
                    File.Delete(selectedPath + JS);
                }

                // add current taxons xml file
                File.Create(selectedPath + XmlWithXsl).Close();
                await File.WriteAllTextAsync(selectedPath + XmlWithXsl, Save(GetAllTaxons(), false, true));

                // add xsl transformation
                File.Create(selectedPath + XSL).Close();
                string text = File.ReadAllText(LocalPath + XSL);
                await File.WriteAllTextAsync(selectedPath + XSL, text);

                // add css transformation
                File.Create(selectedPath + CSS).Close();
                text = File.ReadAllText(LocalPath + CSS);
                await File.WriteAllTextAsync(selectedPath + CSS, text);

                // add js transformation
                File.Create(selectedPath + JS).Close();
                text = File.ReadAllText(LocalPath + JS);
                await File.WriteAllTextAsync(selectedPath + JS, text);

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
                        {
                            xw.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"metrologytaxonomy.xsl\"");
                        }
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
            if (File.Exists(LocalPath + CatalogXml))
            {
                await File.WriteAllTextAsync(LocalPath + CatalogXml, xml);
            }
            else
            {
                return;
            }
            Reload();
        }

        private async void SaveLocal(string xml)
        {
            if (!File.Exists(LocalPath + CatalogXml))
            {
                File.Create(LocalPath + CatalogXml);
            }
            await File.WriteAllTextAsync(LocalPath + CatalogXml, xml);
            Reload();
        }

        public string ExportHTML(Taxon taxon)
        {
            // text to send back
            string text;

            //Name and Slug
            string name = taxon.Name;
            string slug = name;
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
            foreach (Parameter parameter in taxon.Parameters)
            {
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
                if (taxon.Name.ToLower().Contains("measure"))
                {
                    type = "Measured";
                }

                text += "<strong>" + type + " Value &amp; Uncertainty</strong>\n<ul>\n{results}\n</ul>";
                li = "\t<li>{name}{quantity}</li>\n";
                nextli = "";
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

        #endregion File IO

        #region Async Loader

        internal static class TaxonomyLoader
        {
            // Load from the xml file
            private static Task<List<Taxon>> loaderTask;

            private static bool loadedFromServer = true;

            public static bool LoadedFromServer
            {
                get { return loadedFromServer; }
                set { loadedFromServer = value; }
            }

            public static Task<List<Taxon>> LoadDataAsync(bool refresh = false, bool fromServer = false, bool readOnly = false)
            {
                if (refresh || loaderTask == null)
                {
                    loaderTask = LoadDataCoreAsync(fromServer, readOnly);
                }
                return loaderTask;
            }

            private static async Task<List<Taxon>> LoadDataCoreAsync(bool fromServer = false, bool readOnly = false)
            {
                List<Taxon> taxonomy = new List<Taxon>();
                try
                {
                    FileInfo file = null;
                    string xml = "";
                    if (!File.Exists(LocalPath + CatalogXml) || fromServer)
                    {
                        // create local xml file
                        xml = Tools.BuildXml(XmlFromServer);
                        if (!readOnly) // Make a file
                        {
                            File.Create(LocalPath + CatalogXml).Close();
                            File.WriteAllText(LocalPath + CatalogXml, xml);
                            file = new FileInfo(LocalPath + CatalogXml);
                            Tools.SaveLocalXSD(XsdFromServer);
                        }
                        LoadedFromServer = true;
                    }
                    else
                    {
                        file = new FileInfo(LocalPath + CatalogXml);
                        if (readOnly)
                        {
                            // Open the stream and read it back.
                            using (StreamReader sr = file.OpenText())
                            {
                                string s = "";
                                var sb = new StringBuilder();
                                while ((s = sr.ReadLine()) != null)
                                {
                                    sb.AppendLine(s);
                                }
                                xml = sb.ToString();
                            }
                        }
                        LoadedFromServer = false;
                    }

                    // deserrialize our file or string
                    StreamReader stream = null;
                    if (readOnly)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(xml);
                        stream = new StreamReader(new MemoryStream(bytes));
                    }
                    else
                    {
                        stream = new StreamReader(file.FullName);
                    }
                    XmlSerializer serializer = new(typeof(Taxonomy));
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

        #endregion Async Loader

        #region Tools
    }

    #endregion Tools
}