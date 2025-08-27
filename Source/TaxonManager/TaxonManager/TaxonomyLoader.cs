using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CalLabSolutions.TaxonManager
{
    class TaxonomyLoader
    {
        private const string XmlFromServer = Namespaces.BASE_UIR + "MeasurandTaxonomyCatalog.xml";

        private const string XmlOut = "MTC_Local.xml";

        // Load from the xml file
        private static Task<List<Taxon>> loaderTask;

        private static bool loadedFromServer = true;

        public static bool LoadedFromServer
        {
            get { return loadedFromServer; }
            set { loadedFromServer = value; }
        }

        public static Task<List<Taxon>> LoadDataAsync(bool refresh = false, bool fromServer = false, Logger logger = null)
        {
            if (refresh || loaderTask == null)
            {
                loaderTask = LoadDataCoreAsync(fromServer, logger);
            }
            return loaderTask;
        }

        private static async Task<List<Taxon>> LoadDataCoreAsync(bool fromServer = false, Logger logger = null)
        {
            List<Taxon> taxonomy = new List<Taxon>();
            try
            {
                FileInfo file = null;

                string xml = "";
                if (!File.Exists(TaxonManager.Path + XmlOut) || fromServer)
                {
                    // create local xml file
                    xml = Tools.BuildXml(XmlFromServer);
                    File.Create(TaxonManager.Path + XmlOut).Close();
                    File.WriteAllText(TaxonManager.Path + XmlOut, xml);
                    file = new FileInfo(TaxonManager.Path + XmlOut);
                    LoadedFromServer = true;
                }
                else
                {
                    file = new FileInfo(TaxonManager.Path + XmlOut);
                    LoadedFromServer = false;
                }

                // deserialize our file or string
                StreamReader stream = new StreamReader(file.FullName);
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
                logger.Write(e.GetType() + ": " + e.Message);
                Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                taxonomy.Clear();
                return taxonomy;
            }
        }
    }
}
