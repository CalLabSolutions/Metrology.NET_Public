using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CalLabSolutions.TaxonManager
{
    public enum MenuSelection
    {
        compile = 1,
        decompile = 2,
        export = 3,
        import = 4,
        help = 5,
    }

    public class TaxonManager
    {
        private static readonly string _localFullFileName = "MTC_Local";

        private static Help help;

        private static string taxonomyPath = string.Empty;

        public static string Path
        {
            get { return taxonomyPath; }
            set { taxonomyPath = value; }
        }

        private static List<Taxon> taxonomy;

        public static List<Taxon> TaxonomyList
        {
            get { return taxonomy; }
            set { taxonomy = value; }
        }

        private static Logger logger;

        public TaxonManager(string[] args)
        {
            Main(args);
        }

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            SetPathToCurrentDir();
            logger = new Logger(Path);
            TaxonomyList = new List<Taxon>();
            if (args.Length == 0)
            {
                // Download the server file on start up if not present
                if (!File.Exists(Path + _localFullFileName + ".xml"))
                {
                    LoadBaseTaxonomyFile(true);
                }
                StartMenuSelection();
            }
        }

        #region Menus construction

        /// <summary>
        /// Create the main menu
        /// </summary>
        private static void CreateMenu()
        {
            Console.WriteLine("Select menu items below by index number:");
            Console.WriteLine("1: Compile files into a single Taxonomy XML file.");
            Console.WriteLine("2: Decompile a Taxonomy file into multiple single Taxon files.");
            Console.WriteLine("3: Export Taxon files as XML or HTML.");
            Console.WriteLine("4: Import full Taxomomy from server");
            Console.WriteLine("5: Help");
            Console.WriteLine("6: Exit\n");
        }

        /// <summary>
        /// Create Compile menu and validate user responses
        /// </summary>
        /// <returns>ConsoleKeyInfo object</returns>
        private static ConsoleKeyInfo CreateAndValidateCompileMenu()
        {
            ConsoleKeyInfo cki = CreateCompileMenu();
            Console.WriteLine("\n");
            int key = 0;
            if (int.TryParse(cki.KeyChar.ToString(), out key))
            {
                if (key < 1 || key > 3)
                {
                    logger.Write("Invalid user selection from Compile menu, " + key.ToString() + " was selected instead of 1, 2, or 3.");
                    Console.WriteLine("That was an invalid selection.\nPlease hit Enter key to try again or hit Esc key to exit the compile action.\nPress keys Ctrl and C to quit the application.\n");
                    ConsoleKeyInfo retry = Console.ReadKey();
                    if (retry.Key == ConsoleKey.Escape)
                    {
                        return retry;
                    }
                    else
                    {
                        cki = CreateAndValidateCompileMenu();
                    }
                }
            }

            return cki;
        }

        /// <summary>
        /// Create the Compile menu
        /// </summary>
        /// <returns>User entered key tapped</returns>
        private static ConsoleKeyInfo CreateCompileMenu()
        {
            Console.WriteLine("Please select one of the following options:");
            Console.WriteLine("1: Replace MTC_Local.xml file with files in Taxon folder.");
            Console.WriteLine("2: Combine single Taxon files into a group Taxon file.");
            Console.WriteLine("3: Combine group Taxonomy files into a larger Taxonomy file.\n");
            WaitForKey();
            return Console.ReadKey(true);
        }

        /// <summary>
        /// Create the Decompile menu and validate user responses
        /// </summary>
        /// <returns>ConsoleKeyInfo</returns>
        private static ConsoleKeyInfo CreateAndValidateDecompileMenu()
        {
            ConsoleKeyInfo cki = CreateDecompileMenu();
            Console.WriteLine("\n");
            int key = 0;
            if (int.TryParse(cki.KeyChar.ToString(), out key))
            {
                if (key < 1 || key > 2)
                {
                    logger.Write("Invalid user selection from Decompile menu, " + key.ToString() + " was selected instead of 1 or 2");
                    Console.WriteLine("That was an invalid selection.\nPlease hit Enter key to try again or hit Esc key to reload the main menu.\nPress keys Ctrl and C to quit the application.\n");
                    ConsoleKeyInfo retry = Console.ReadKey();
                    if (retry.Key == ConsoleKey.Escape)
                    {
                        return retry;
                    }
                    else
                    {
                        cki = CreateAndValidateDecompileMenu();
                    }
                }
            }

            return cki;
        }

        /// <summary>
        /// Creates the Decompile menu
        /// </summary>
        /// <returns>User entered key tapped</returns>
        private static ConsoleKeyInfo CreateDecompileMenu()
        {
            Console.WriteLine("Please select one of the following options:");
            Console.WriteLine("1: Decompile MTC_Local.xml file into individual Taxon files in Taxon folder.");
            Console.WriteLine("2: Decompile combined Taxonomy files into Individual Taxon files.\n");
            WaitForKey();
            return Console.ReadKey(true);
        }

        /// <summary>
        /// Creates and validates the export menu
        /// </summary>
        /// <returns>ConsoleKeyInfo object</returns>
        private static ConsoleKeyInfo CreateAndValidateExportMenu()
        {
            ConsoleKeyInfo cki = CreateExportMenu();
            Console.WriteLine("\n");
            int key = 0;
            if (int.TryParse(cki.KeyChar.ToString(), out key))
            {
                if (key < 1 || key > 2)
                {
                    logger.Write("Invalid user selections from the Export menu, " + key.ToString() + " was selected instead of 1 or 2.");
                    Console.WriteLine("That was an invalid selection.\nPlease hit Enter key to try again or use Esc key to reload the main menu.\nPress keys Ctrl and C to quit the application");
                    ConsoleKeyInfo retry = Console.ReadKey();
                    if (retry.Key == ConsoleKey.Escape)
                    {
                        return retry;
                    }
                    else
                    {
                        cki = CreateAndValidateExportMenu();
                    }
                }
            }

            return cki;
        }

        /// <summary>
        /// Creates the export menu
        /// </summary>
        /// <returns>ConsoleKeyInfo object</returns>
        private static ConsoleKeyInfo CreateExportMenu()
        {
            Console.WriteLine("Please select one of the following options:");
            Console.WriteLine("1: Export complete taxonomy as HTML.");
            Console.WriteLine("2: Export single taxon as HTML.\n");
            WaitForKey();
            return Console.ReadKey(true);
        }

        /// <summary>
        /// Adds spinner to show while waiting on user input
        /// </summary>
        private static void WaitForKey()
        {
            Console.Write("   ");
            int waitTimer = 0;
            do
            {
                Thread.Sleep(300);
                if (waitTimer == 5)
                {
                    // If you increase the number of counts before replacing the wait text
                    // Add additional '\b \b' writes to match the timer count
                    Console.Write("\b \b");
                    Console.Write("\b \b");
                    Console.Write("\b \b");
                    Console.Write("\b \b");
                    waitTimer = 0;
                }
                else
                {
                    Console.Write('\u002A');
                }
                waitTimer += 1;

            } while (!Console.KeyAvailable);
        }

        #endregion Menus construction

        #region File and Taxon Interactions

        /// <summary>
        /// Load the base Taxonomy XML file overwriting the local version
        /// </summary>
        /// <param name="getFromServer">True to retrieve from the server</param>
        private static void LoadBaseTaxonomyFile(bool getFromServer = false)
        {
            if (getFromServer)
            {
                Console.WriteLine("Downloading the latest Taxonomy from the server and saving locally.\nSaving it to the default path of\n" + Path +
                    "\nFile name is " + _localFullFileName + ".xml." +
                    "\nHit Enter key to continue.\n");
            }
            else
            {
                Console.WriteLine("Loading full Taxonomy file.");
                if (File.Exists(Path + _localFullFileName + ".xml"))
                {
                    Console.WriteLine("This will overwrite the file in this path, \n" + Path + _localFullFileName +
                        ".\nHit Enter key to continue or Esc key to exit importing master Taxonomy file.\n");
                }
            }

            ConsoleKeyInfo cki = Console.ReadKey();
            if (cki.Key == ConsoleKey.Escape)
            {
                StartMenuSelection();
                return;
            }

            if (getFromServer)
            {
                LoadFullXml(true, true);
            }
            else
            {
                LoadFullXml();
            }
        }

        /// <summary>
        /// Load the full taxonomy XML file
        /// </summary>
        /// <param name="refresh">True to refresh the load task</param>
        /// <param name="fromServer">True to retrieve the full taxonomy from the server</param>
        private static async void LoadFullXml(bool refresh = false, bool fromServer = false)
        {
            TaxonomyList = await TaxonomyLoader.LoadDataAsync(refresh, fromServer, logger);
            if ((TaxonomyList != null || TaxonomyList.Count != 0) && !fromServer)
            {
                Taxonomy toXml = new Taxonomy
                {
                    Taxons = TaxonomyList
                };

                var xml = SerializeTaxons(true, toXml);
                if (string.IsNullOrWhiteSpace(xml))
                {
                    Console.WriteLine("XML was not created. Reloading the menu.\n");
                    StartMenuSelection();
                }
                SaveLocal(xml, _localFullFileName, Path);
            }
        }

        /// <summary>
        /// Serialize taxons into an string
        /// </summary>
        /// <param name="fullList">True to serialize the full taxonomy file</param>
        /// <param name="toXml">Taxonomy object to serialize</param>
        /// <param name="xslt">True to serialize with XSL</param>
        /// <returns></returns>
        private static string SerializeTaxons(bool fullList, object toXml)
        {
            try
            {
                var serializeType = fullList ? typeof(Taxonomy) : typeof(Taxon);
                XmlSerializer serializer = new XmlSerializer(serializeType);
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
                        serializer.Serialize(xw, toXml, ns);
                    }

                    string xml = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return xml;
                }
            }
            catch (Exception e)
            {
                logger.Write(e.GetType() + ": " + e.Message);
                Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets the main path value to the current directory
        /// </summary>
        private static void SetPathToCurrentDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            if (!currentDir.EndsWith('\\'))
            {
                currentDir += "\\";
            }
            DirectoryInfo di = Directory.CreateDirectory(currentDir);
            Path = di.FullName;
        }

        /// <summary>
        /// Take a list of files and convert them to a list of Taxons
        /// </summary>
        /// <param name="fileNames">List of file names including path</param>
        /// <param name="addMultipleTaxons">True to convert combined taxon files, false to combine single taxon files.</param>
        /// <returns>List of Taxons</returns>
        public static List<Taxon> FileToTaxonomyConverter(List<string> fileNames, bool addMultipleTaxons = false)
        {
            List<Taxon> taxonList = new List<Taxon>();
            foreach (string taxonFile in fileNames)
            {
                string xml = string.Empty;
                try
                {
                    var file = new FileInfo(taxonFile);
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

                    // deserialize string
                    StreamReader stream = null;
                    byte[] bytes = Encoding.UTF8.GetBytes(xml);
                    stream = new StreamReader(file.FullName);
                    if (addMultipleTaxons)
                    {
                        XmlSerializer serializer = new(typeof(Taxonomy));
                        Taxonomy fromXml = (Taxonomy)serializer.Deserialize(stream);
                        int count = 0;
                        taxonList.InsertRange(count, fromXml.Taxons);
                    }
                    else
                    {
                        XmlSerializer serializer = new(typeof(Taxon));
                        Taxon fromXml = (Taxon)serializer.Deserialize(stream);
                        taxonList.Add(fromXml);
                    }
                    stream.Close();
                }
                catch (Exception e)
                {
                    logger.Write(e.GetType() + ": " + e.Message);
                    Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                }
            }
            return taxonList;
        }

        /// <summary>
        /// Gets a list of files to compile into a new taxonomy XML
        /// </summary>
        /// <param name="path">Path to retrieve files from</param>
        /// <returns>List of file names as strings</returns>
        private static List<string> GetFileList(string path)
        {
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            List<string> files = new List<string>();
            Console.WriteLine("Enter the XML files to include in the compilation.\nUse a comma to separate the names.\n");
            string fileList = Console.ReadLine();
            string[] fileNames = fileList.Split(",");
            foreach (string fileName in fileNames)
            {
                string cleanFileName = fileName;
                if (!cleanFileName.EndsWith(".xml"))
                {
                    cleanFileName += ".xml";
                }
                files.Add(path + cleanFileName);
            }
            return files;
        }

        /// <summary>
        /// Save a local copy of a file
        /// </summary>
        /// <param name="content">The content to write to the file</param>
        /// <param name="fileName">XML file name</param>
        /// <param name="path">Path to save the file into</param>
        /// <param name="htmlFile">True if this is an html file false if XML</param>
        private static async void SaveLocal(string content, string fileName, string path, bool htmlFile = false)
        {
            if (!path.EndsWith('\\'))
            {
                path += "\\";
            }
            string xmlOut = fileName + (htmlFile ? ".html" : ".xml");
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }
                await File.WriteAllTextAsync(path + xmlOut, content);
            }
            catch (IOException e)
            {
                logger.Write(e.GetType() + ": " + e.Message);
                Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
            }
        }

        #endregion File and Taxon Interactions

        #region Menu select and help

        /// <summary>
        /// Perform an action based on the user key selected
        /// </summary>
        private static void SelectMenuItem()
        {
            ConsoleKeyInfo cki = Console.ReadKey(true);
            Console.WriteLine("\n");
            int key = 0;
            if (int.TryParse(cki.KeyChar.ToString(), out key))
            {
                Console.WriteLine(((MenuSelection)key) + "\n");
                if (key > 0 && key < 5)
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            Compile();
                            break;

                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            Decompile();
                            break;

                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            Export();
                            break;

                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            Import();
                            break;

                        default:
                            GetHelpAndRestart();
                            break;
                    }
                }
                else if (key == 5)
                {
                    GetHelpAndRestart();
                }
                else if (key == 6)
                {
                    Environment.Exit(0);
                }

            }
            else if (cki.KeyChar.ToString() == "?" || cki.Key == ConsoleKey.H)
            {
                GetHelpAndRestart();
            }
            else
            {
                Console.WriteLine("You entered " + cki.KeyChar.ToString() + ".\n");
                Console.WriteLine("This is an invalid selection.\n");
                Console.WriteLine("Reloading the main menu.\n");
                StartMenuSelection();
            }
        }

        /// <summary>
        /// Show the user help and allow exit or restart
        /// </summary>
        private static void GetHelpAndRestart()
        {
            help = new Help("?");
            help.ProvideHelp();
            StartMenuSelection();
        }

        /// <summary>
        /// Select the action based on the user input
        /// </summary>
        private static void StartMenuSelection()
        {
            CreateMenu();
            WaitForKey();
            SelectMenuItem();
        }

        #endregion Menu select and help

        #region Actions

        /// <summary>
        /// Compile a set of taxon files into a new taxonomy file
        /// </summary>
        private static void Compile()
        {
            string taxonomyPath = Path;
            string taxonPath = taxonomyPath + "Taxons\\";
            List<string> fileNames = new List<string>();
            bool overwriteMaster = false;

            ConsoleKeyInfo cki = CreateAndValidateCompileMenu();
            if (cki.Key != ConsoleKey.Escape)
            {
                try
                {
                    if (cki.Key == ConsoleKey.D1 || cki.Key == ConsoleKey.NumPad1)
                    {
                        if (Directory.Exists(taxonPath) && Directory.GetFiles(taxonPath).Length > 0)
                        {
                            overwriteMaster = true;
                            IEnumerable<string> files = Directory.EnumerateFiles(taxonPath, "*.xml");
                            fileNames = files.ToList();
                        }
                    }
                    else if (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.D3 || cki.Key == ConsoleKey.NumPad2 || cki.Key == ConsoleKey.NumPad3)
                    {
                        string folderType = (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.NumPad2) ? "single taxon files are located" : "combined files are located";
                        Console.WriteLine("Enter the path to the folder where the {0}.", folderType + ".\n");
                        taxonPath = Console.ReadLine();
                        Console.WriteLine("To add specific files rather than all files in the folder hit Enter otherwise hit any other key.\n");
                        DirectoryInfo di = Directory.CreateDirectory(taxonPath);
                        if (!di.FullName.EndsWith("\\"))
                        {
                            taxonPath = di.FullName + "\\";
                        }
                        if (Console.ReadKey().Key == ConsoleKey.Enter)
                        {
                            fileNames = GetFileList(taxonPath);
                        }
                        else
                        {
                            IEnumerable<string> fileList = Directory.EnumerateFiles(taxonPath, "*.xml");
                            fileNames = fileList.ToList();
                        }
                    }
                    TaxonomyList = FileToTaxonomyConverter(fileNames, (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.NumPad2 ? false : true));

                    if (TaxonomyList != null && taxonomy.Count > 0)
                    {
                        Taxonomy toXml = new Taxonomy
                        {
                            Taxons = TaxonomyList
                        };
                        string xml = SerializeTaxons(true, toXml);
                        if (overwriteMaster)
                        {
                            SaveLocal(xml, _localFullFileName, taxonomyPath);
                        }
                        else
                        {
                            Console.WriteLine("Enter the path to the folder where you want to save the compiled file.\n");
                            taxonomyPath = Console.ReadLine();
                            if (!Directory.Exists(taxonomyPath))
                            {
                                Directory.CreateDirectory(taxonomyPath);
                            }
                            Console.WriteLine("Enter the file name you want to save.\n");
                            string fileName = Console.ReadLine();
                            SaveLocal(xml, fileName, taxonomyPath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Due to an error reloading main menu.\n");
                    }
                }
                catch (Exception e)
                {
                    logger.Write(e.GetType() + ": " + e.Message);
                    Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                    Console.WriteLine("Due to an error reloading main menu.\n");
                }
            }
            StartMenuSelection();
        }

        /**
         * Take single local file and break down into individual Taxon
         */
        private static void Decompile()
        {
            string taxonomyPath = Path;
            string taxonPath = taxonomyPath + "Taxons\\";
            List<string> fileNames = new List<string>();
            ConsoleKeyInfo cki = CreateAndValidateDecompileMenu();
            if (cki.Key != ConsoleKey.Escape)
            {
                if (cki.Key == ConsoleKey.D1 || cki.Key == ConsoleKey.NumPad1)
                {
                    string fileName = _localFullFileName + ".xml";
                    if (File.Exists(taxonomyPath + fileName))
                    {
                        fileNames.Add(taxonomyPath + fileName);
                    }
                }
                else if (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.NumPad2)
                {
                    Console.WriteLine("Enter the path to the folder containing the file you want to decompile into individual taxon files.\n");
                    taxonomyPath = Console.ReadLine();
                    DirectoryInfo di = Directory.CreateDirectory(taxonomyPath);
                    if (!di.FullName.EndsWith("\\"))
                    {
                        taxonomyPath = di.FullName + "\\";
                    }

                    Console.WriteLine("Enter the name of the specific file to decompile from that folder.\n");
                    string fileName = taxonomyPath + Console.ReadLine();
                    if (!fileName.EndsWith(".xml"))
                    {
                        fileName += ".xml";
                    }
                    fileNames.Add(fileName);
                }
                try
                {

                    TaxonomyList = FileToTaxonomyConverter(fileNames, true);
                    if (TaxonomyList != null && TaxonomyList.Count > 0)
                    {
                        if (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.NumPad2)
                        {
                            Console.WriteLine("Enter the path to where you want to save the taxon Files.\nFile names will be based on the individual Taxon.\n");
                            taxonPath = Console.ReadLine();
                            if (!taxonPath.EndsWith("\\"))
                            {
                                taxonPath += "\\";
                            }
                        }

                        Taxonomy toXml = new Taxonomy
                        {
                            Taxons = TaxonomyList
                        };
                        for (int i = 0; i < toXml.Taxons.Count; i++)
                        {
                            string fileName = toXml.Taxons[i].Name.Replace('.', '_');
                            string xml = SerializeTaxons(false, toXml.Taxons[i]);
                            SaveLocal(xml, fileName, taxonPath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Due to an error reloading main menu.\n");
                    }
                }
                catch (IOException exc)
                {
                    logger.Write(exc.GetType() + ": " + exc.Message);
                    Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                    Console.WriteLine("Due to an error reloading main menu.\n");
                }
            }
            StartMenuSelection();
        }

        /**
         * Export created Taxonomy file
         */
        private static void Export()
        {
            ConsoleKeyInfo cki = CreateAndValidateExportMenu();
            if (cki.Key != ConsoleKey.Escape)
            {
                // XML Taxonomy export
                if (cki.Key == ConsoleKey.D1 || cki.Key == ConsoleKey.NumPad1)
                {
                    Console.WriteLine("Enter the full path including file name for the taxonomy to export.\n");
                    List<string> filePath = new List<string>();
                    filePath.Add(Console.ReadLine());
                    List<Taxon> taxonomy = FileToTaxonomyConverter(filePath, true);
                    if (taxonomy != null && taxonomy.Count > 0)
                    {
                        Taxonomy xmlTaxonomy = new Taxonomy
                        {
                            Taxons = taxonomy
                        };
                        string fileName = "Local_MetrologyTaxonomy";
                        Console.WriteLine("Please enter the path to folder to save your HTML into.\nFile name will be '" + fileName + "'.\n");
                        string htmlPath = Console.ReadLine();
                        if (!Directory.Exists(htmlPath))
                        {
                            Directory.CreateDirectory(htmlPath);
                        }
                        var html = Tools.CreateTaxonomyHtml(xmlTaxonomy);
                        SaveLocal(html, fileName, htmlPath, true);
                    }
                    else
                    {
                        logger.Write("Taxons could not be found at path " + filePath);
                        Console.WriteLine("Due to an error reloading main menu.\n");
                    }
                }
                else if (cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.NumPad2) // HTML Taxon export
                {
                    Console.WriteLine("Enter the full path including file name of the taxon to export.\n");
                    List<string> filePath = new List<string>();
                    filePath.Add(Console.ReadLine());
                    List<Taxon> taxons = FileToTaxonomyConverter(filePath);
                    if (taxons != null && taxons.Count > 0)
                    {
                        Taxon taxon = taxons[0];
                        var html = Tools.CreateTaxonHtml(taxon);
                        string fileName = "HTML - " + taxon.Name;
                        fileName = fileName.Replace(".", "_");
                        Console.WriteLine("Please enter the path to folder to save your HTML into.\nFile name will be '" + fileName + "'.\n");
                        string htmlPath = Console.ReadLine();
                        if (!Directory.Exists(htmlPath))
                        {
                            Directory.CreateDirectory(htmlPath);
                        }
                        SaveLocal(html, fileName, htmlPath, true);
                    }
                    else
                    {
                        logger.Write("Taxons could not be found at path " + filePath);
                        Console.WriteLine("Due to an error reloading main menu.\n");
                    }
                }
            }
            StartMenuSelection();
        }

        /**
         * Import the server Taxonomy file
         */
        private static void Import()
        {
            Console.WriteLine("This will overwrite the master taxonomy file located at\n" + Path +
                ".\nTo import from the server hit enter key.\n" +
                "To return to the main menu hit Esc key.\nTo exit the application press Ctrl key and C key at the same time.\n" +
                "To import from the local machine hit any other key.\n");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                LoadBaseTaxonomyFile(true);
            }
            else if (Console.ReadKey().Key == ConsoleKey.Escape)
            {
                StartMenuSelection();
            }
            else
            {
                Console.WriteLine("Please enter the full path to the file you want to import.\n");
                string path = Console.ReadLine();
                if (path.EndsWith("\\"))
                {
                    Console.WriteLine("Please enter the file name to import.\n");
                    path += Console.ReadLine();
                    if (!path.EndsWith(".xml"))
                    {
                        path += ".xml";
                    }
                }

                string xml = Tools.BuildXml(path);
                try
                {
                    File.WriteAllText(Path + _localFullFileName, xml);
                }
                catch (IOException e)
                {
                    logger.Write(e.GetType() + ": " + e.Message);
                    Console.WriteLine("An exception has been caught, Look in the log file at " + Logger.logPath + ".\n");
                }
            }

            StartMenuSelection();
        }

        #endregion Actions
    }
}
