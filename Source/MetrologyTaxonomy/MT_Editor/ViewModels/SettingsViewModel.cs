using Caliburn.Micro;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MT_DataAccessLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MT_Editor.Views.SettingsView;
using Parameter = MT_DataAccessLib.Parameter;

namespace MT_Editor.ViewModels
{
    internal class SettingsViewModel : Screen
    {
        private TaxonomyFactory factory;

        public SettingsViewModel()
        {
            factory = new TaxonomyFactory();
            Locked = Helper.Locked;
        }

        #region Commands

        public void LoadFromServer()
        {
            factory.Reload(true);
        }

        public void ExportXML()
        {
            SaveFileDialog saveFileDialog = new()
            {
                DefaultExt = "xml",
                FileName = TaxonomyFactory.Catalog + ".xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "XML Files (*.xml)|*.xml"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                File.WriteAllText(saveFileDialog.FileName, factory.Save(factory.GetAllTaxons()));
                File.Copy(TaxonomyFactory.LocalPath + "\\" + TaxonomyFactory.Catalog + ".xsd", saveFileDialog.FileName.Replace(".xml", ".xsd"));
            }
        }

        public void ExportXMLwXSLT()
        {
            CommonOpenFileDialog openFolderDialog = new()
            {
                Title = "Select Folder",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                IsFolderPicker = true,
            };

            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _ = factory.SaveWithXSLT(openFolderDialog.FileName + "\\");
            }
        }

        public void ExportXMLasHtml()
        {
            var taxonomy = factory.GetAllTaxons();
            string text = string.Empty;
            // Create Header
            string mtc = "https://cls-schemas.s3.us-west-1.amazonaws.com/MetrologyTaxonomyCatalog";
            string uom = "https://cls-schemas.s3.us-west-1.amazonaws.com/UOM_Database";
            text += string.Format("<html xmnls:mtc=\"{0}\" xmlns:uom=\"{1}\">", mtc, uom);
            text += "<head>";
            text += "<meta http-equiv=\"ContentType\" content=\"text/html; charset=UTF-8\">";
            text += "<link rel=\"stylesheet\" href=\"metrologytaxonomy.css\">";
            text += "</head></body>";
            text += "<h2>Metrology Taxonomy</h2>";
            text += "<h4>Click on a Taxon to view the details.  Hit Crtl+F to search for a Taxon Name</h4>";
            text += "<ul class=\"taxon-list\">";
            foreach (Taxon taxon in taxonomy)
            {
                text += "<li><a class=\"taxon\" href=\"javascript()\">";
                text += taxon.Name + "</a><div class=\"hide-details\">";
                text += CreateTaxonHtml(taxon);
                text += "</div></li>";

            }
            text += "</ul></body>";
            text += "<script type=\"text/javascript\" src=\"metrologytaxonomy.js\"></script>";
            text += "</html>";
            SaveFileDialog saveFileDialog = new()
            {
                DefaultExt = "html",
                FileName = TaxonomyFactory.Catalog + ".html",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Html Files (*.html)|*.html"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                var htmlText = Uri.UnescapeDataString(text).Replace("\n","").Replace("\t","");
                File.WriteAllText(saveFileDialog.FileName, htmlText, encoding: System.Text.Encoding.UTF8);
                var savePath = saveFileDialog.FileNames.Where(f => f.Contains(TaxonomyFactory.Catalog)).ToList()[0].Replace(TaxonomyFactory.Catalog.ToString() + ".html", "");
                if (!File.Exists(savePath + "metrologytaxonomy.js"))
                {
                    File.Copy(TaxonomyFactory.LocalPath + "metrologytaxonomy.js", savePath + "metrologytaxonomy.js");
                }
                if (!File.Exists(savePath + "metrologytaxonomy.css"))
                {
                    File.Copy(TaxonomyFactory.LocalPath + "metrologytaxonomy.css", savePath + "metrologytaxonomy.css");
                }
            }


        }

        /// <summary>
        /// Create a taxon html page
        /// </summary>
        /// <param name="taxon">Taxon object</param>
        /// <returns></returns>
        public static string CreateTaxonHtml(Taxon taxon)
        {
            // text to send back
            string taxonText = string.Empty;
            try
            {
                // html
                string definition = "<p>{definition}{deprecated}</p>\n\n<!--more-->\n\n";
                definition = definition.Replace("{definition}", taxon.Definition);
                if (taxon.Deprecated)
                {
                    definition = definition.Replace("{deprecated}", "\nDeprecated - " + taxon.Replacement);
                }
                else
                {
                    definition = definition.Replace("{deprecated}", "");
                }
                taxonText += definition;

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
                    taxonText += "<strong>Required Parameters</strong>\n<ul>\n{required_params}\n</ul>\n";
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
                    taxonText = taxonText.Replace("{required_params}", nextli);
                }
                if (optional.Count > 0)
                {
                    li = "\t<li>{name}{definition}{quantity}</li>\n";
                    nextli = "";
                    taxonText += "<strong>Optional Parameters</strong>\n<ul>\n{optional_params}\n</ul>\n";
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
                    taxonText = taxonText.Replace("{optional_params}", nextli);
                }

                // results
                if (taxon.Results != null)
                {
                    string type = "Output";
                    if (taxon.Name.ToLower().Contains("measure"))
                    {
                        type = "Measured";
                    }

                    taxonText += "<strong>" + type + " Value &amp; Uncertainty</strong>\n<ul>\n{results}\n</ul>";
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
                    taxonText = taxonText.Replace("{results}", nextli);
                }
                if (taxon.ExternalReferences != null)
                {
                    List<Reference> references = taxon.ExternalReferences.References;
                    taxonText += "<strong>References</strong>\n<ul>\n{references}\n</ul>";
                    li = "\t<li>\n<div class=\"reftable\">Reference</div>{table1}</li>\n";
                    nextli = "";
                    foreach (Reference refer in references)
                    {
                        nextli += li;
                        string table1 = "<table>\n<tbody>\n<tr><th>URL Name</th><th>URL</th></tr>\n";
                        string refUrl = refer.ReferenceUrl.UrlValue;
                        string refName = refer.ReferenceUrl.UrlName;
                        table1 += "<tr><td>" + refName + "</td><td>" + refUrl + "</td></tr>\n";
                        List<CategoryTag> catList = refer.CategoryTagList;
                        if (catList != null)
                        {
                            int catCount = catList.Count;
                            if (catCount > 0)
                            {
                                table1 += "<tr><th>Category Name</th><th>Category</th></tr>\n";
                                for (int z = 0; z < catCount; z++)
                                {
                                    table1 += "<tr><td>" + catList[z].Name + "</td><td>" + catList[z].Value + "</td></tr>\n";
                                }
                            }
                        }
                        table1 += "</tbody>\n</table>\n";
                        nextli = nextli.Replace("{table1}", table1);
                    }
                    taxonText = taxonText.Replace("{references}", nextli);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return taxonText;
        }

        public void LoadLocalXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                DefaultExt = "xml",
                Filter = "XML Files (*.xml)|*.xml"
            };
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                string xml = File.ReadAllText(openFileDialog.FileName);
                factory.ReplaceLocal(xml);
            }
        }

        public void Unlock()
        {
            if (Password == "MII_Taxonomy_1998")
            {
                Helper.Locked = false;
                Locked = false;
                Helper.Shell.Refresh();
                Settings.PasswordBox.Password = "";
            }
            else
            {
                Helper.Locked = true;
                Locked = true;
            }
        }

        public void LockIt()
        {
            Helper.Locked = true;
            Locked = true;
            Helper.Shell.Refresh();
        }

        #endregion Commands

        #region Properties

        public string Password { private get; set; }

        private bool saveLocal = true;

        public bool SaveLocal
        {
            get { return saveLocal; }
            set
            {
                saveLocal = value;
                Helper.SaveLocal = saveLocal;
                NotifyOfPropertyChange(() => SaveLocal);
            }
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set
            {
                locked = value;
                NotifyOfPropertyChange(() => Locked);
            }
        }

        #endregion Properties
    }
}