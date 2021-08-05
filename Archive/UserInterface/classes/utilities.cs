using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using soa_1_03.viewModels;
using System.Reflection;
using System.Xml;
using soa_1_03.models;

namespace soa_1_03.classes
{
    public class utilities
    {
        #region Private Fields
        //private string _userPath;           //For now, saving to %user%/Documents. Eventually, will add user options.
        #endregion

        #region Constructor
        public utilities()
        {
            soaIsModified = false;
            userPath = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.Current.MainWindow.Title.ToString());
            dictDlgResults = new Dictionary<string, string>() { { "result", null }, { "path", null } };
            programPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            taxonomiesPath = string.Format("{0}{1}", programPath, @"\data files\taxonomies.xml");
        }

        #endregion

        #region Singleton
        private static readonly utilities instance = null;
        static utilities() { instance = new utilities(); }
        public static utilities GetInstance() { return instance; }
        #endregion

        #region Public Properties
        public bool soaIsModified { get; set; }
        public string userPath { get; set; }
        public string dlgPath { get; set; }
        public string programPath { get; set; }
        public string taxonomiesPath { get; set; }
        public bool editSelected { get; set; }
        public string activeFilePath { get; set; }
        public bool canceled { get; set; }
        public string cleanJson { get; set; }
        public Dictionary<string, string> dictDlgResults;
        public enum fileType { json, xml, txt,csv };
        public enum executeFlag { execute, halt };
        #endregion

        #region Methods

        #region OpenFileOps
        public static vmTaxonomy OpenSoa(vmTaxonomy viewModel)
        {
            utilities util = utilities.GetInstance();
            mSoa soaToOpen = new mSoa();
            util.dictDlgResults = OpenFileDialogBox(util.userPath, util.dictDlgResults);

            if (util.dictDlgResults["result"] != "False")
            {
                string s = File.ReadAllText(util.dictDlgResults["path"]);
                util.activeFilePath = util.dictDlgResults["path"];
                vmTaxonomy tempVm = DeserializeXml(s, viewModel);
                viewModel.vmSoa.Clear();
                viewModel.vmSoa.Add(tempVm.vmSoa[2]);
                viewModel.vmSoa.Add(tempVm.vmSoa[3]);
                viewModel.vmClient = tempVm.vmClient;
            }
            return viewModel;
        }

        private static vmTaxonomy DeserializeXml(string s, vmTaxonomy viewModel)
        {
            ObservableCollection<mSoa> tempMSoa = new ObservableCollection<mSoa>();
            vmTaxonomy tempVm = new vmTaxonomy();
            tempVm.vmSoa.Clear();

            XmlRootAttribute root = new XmlRootAttribute();
            root.ElementName = "vmTaxonomy";
            root.IsNullable = true;

            var serializer = new XmlSerializer(typeof(vmTaxonomy), root);
            using (var reader = new StringReader(s))
            {
                tempVm = (vmTaxonomy)serializer.Deserialize(reader);
            }
            return tempVm;
        }

        public static vmCmc DeserializeXmlCmc (string s)
        {
            vmCmc tempVm = new vmCmc();

            XmlRootAttribute root = new XmlRootAttribute();
            root.ElementName = "cmc_taxonomies";
            root.IsNullable = true;

            var serializer = new XmlSerializer(typeof(vmCmc), root);
            using (var reader = new StringReader(s))
            {
                tempVm = (vmCmc)serializer.Deserialize(reader);
            }
            return tempVm;
        }

        public static vmCmc GetTaxonomyMasterFromXml()
        {
            utilities util = utilities.GetInstance();
            vmCmc vm = new vmCmc();
            XmlDocument doc = new XmlDocument();
            doc.Load(util.taxonomiesPath);
            List<XmlNodeList> nl = new List<XmlNodeList>();
            nl.Add(doc.SelectNodes("/cmc_taxonomies/Measure/Measure"));
            nl.Add(doc.SelectNodes("/cmc_taxonomies/Source/Source"));
            foreach (XmlNodeList nlChild in nl)
            {
                foreach (XmlNode node in nlChild)
                {
                    mTaxonomy t = new mTaxonomy();
                    t.action = node["Action"].InnerText;
                    t.taxonomy = node["Taxonomy"].InnerText;
                    t.quantity = node["Quantity"].InnerText;
                    XmlNodeList nlReq = node.SelectNodes("Required_Parameter");
                    foreach (XmlNode reqNode in nlReq)
                    {
                        mRequiredParams param = new mRequiredParams();
                        param.parameter = reqNode.InnerText;
                        t.requiredParams.Add(param);
                    }

                    XmlNodeList nlOpt = node.SelectNodes("Optional_Parameter");
                    foreach (XmlNode optNode in nlOpt)
                    {
                        mOptionalParams param = new mOptionalParams();
                        param.parameter = optNode.InnerText;
                        t.optionalParams.Add(param);
                    }
                    vm.masterTaxonomy.Add(t);
                }
            }
            return vm;
        }

        public static Dictionary<string, string> OpenFileDialogBox(string path, Dictionary<string, string> dlgResults)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = path;
            dlg.Filter = "XML file (*.xml)|*.xml";
            bool? result = dlg.ShowDialog();
            dlgResults["result"] = result.ToString();
            dlgResults["path"] = dlg.FileName;
            return dlgResults;
        }
        #endregion

        #region SaveFileOps
        public static bool CheckForEdits(object dirty, bool flag)
        {
            bool UpdateOriginalViewModel = false;
            bool IsModified = false;
            utilities util = utilities.GetInstance();
            IsModified = CompareObjects(dirty);

            if (IsModified == true)
            {
                string msg = string.Format("{0}{1}{2}", "This page contains unsaved changes.", Environment.NewLine, "Click 'Yes' to save changes, 'No' to discard unsaved changes, or 'Cancel' to return to previous document.");
                MessageBoxResult userResult = ShowMessageDialog(msg, "Unsaved Changes");
                if (userResult == MessageBoxResult.Yes)
                {
                    Dictionary<string, string> dlgResults = ShowSaveDialog(util.userPath);
                    if (dlgResults["result"] == "True")
                    {
                        SaveFile(dirty, dlgResults["path"], fileType.xml);
                        UpdateOriginalViewModel = true;
                    }
                }
                if (userResult == MessageBoxResult.Cancel)
                {
                    return UpdateOriginalViewModel;
                }
                else { IsModified = false; }
            }
            return UpdateOriginalViewModel;
        }

        public static void SaveFileAs(object dirty)
        {
            utilities util = utilities.GetInstance();
            Dictionary<string, string> dlgResults = ShowSaveDialog(util.userPath);
            if (dlgResults["result"] == "True")
            {
                SaveFile(dirty, dlgResults["path"], fileType.xml);
                CreateCleanJson(dirty);
            }
            util.activeFilePath = dlgResults["path"];
        }

        public static bool CompareObjects(object dirty)
        {
            utilities util = utilities.GetInstance();
            string jsonDirty = JsonConvert.SerializeObject(dirty, Newtonsoft.Json.Formatting.Indented);

            bool isModified = !jsonDirty.Equals(util.cleanJson);
            return isModified;
        }

        public static void CreateCleanJson(object vm)
        {
            utilities util = utilities.GetInstance();
            util.cleanJson = JsonConvert.SerializeObject(vm, Newtonsoft.Json.Formatting.Indented);
        }

        public static bool SaveFile(object current, string path, utilities.fileType fileType)
        {
            bool result = false;
            if (fileType == fileType.xml)
            {
                string s = ConvertToXml(current);
                File.WriteAllText(path, s);
            }
            utilities util = utilities.GetInstance();
            util.activeFilePath = path;
            CreateCleanJson(current);
            return result;
        }

        private static string ConvertToXml(object obj)
        {
            string s = null;
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(stringwriter, obj);
            s = stringwriter.ToString();
            return s;
        }

        public static Dictionary<string, string> ShowSaveDialog(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() { { "result", null }, { "path", null } };
            CreateDocsFolder(path);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = path;
            dlg.Filter = "XML file (*.xml)|*.xml";
            bool? result = dlg.ShowDialog();
            dict["result"] = result.ToString();
            dict["path"] = dlg.FileName;
            return dict;
        }

        public static MessageBoxResult ShowMessageDialog(string message, string title)
        {
            utilities util = utilities.GetInstance();
            util.canceled = false;
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxResult result = MessageBox.Show(message, title, button);
            if (result == MessageBoxResult.Cancel) { util.canceled = true; }
            return result;
        }

        public static void CreateDocsFolder(string path)
        {
            System.IO.Directory.CreateDirectory(path);
        }
        #endregion
        #endregion
    }
}
