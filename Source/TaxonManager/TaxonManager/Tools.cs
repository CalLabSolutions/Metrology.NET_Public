using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CalLabSolutions.TaxonManager
{
    public static class Tools
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
                {
                    newValue.Add(lines[i]);
                }
            }
            return newValue.Count > 0 ? string.Join(" ", newValue) : value;
        }

        public static string BuildXml(string url)
        {
            XmlTextReader reader = new XmlTextReader(url);
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            if (reader != null)
            {
                while (reader.Read())
                {
                    sb.AppendLine(reader.ReadOuterXml());
                }

                return sb.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Create an html page for taxonomy list
        /// </summary>
        /// <param name="taxonomy">Taxonomy object</param>
        /// <returns>Html string</returns>
        public static string CreateTaxonomyHtml(Taxonomy taxonomy)
        {
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
            foreach(Taxon taxon in taxonomy.Taxons)
            {
                text += "<li><a class=\"taxon\" href=\"javascript()\">";
                text += taxon.Name + "</a><div class=\"hide-details\">";
                text += CreateTaxonHtml(taxon, false);
                text += "</div></li>";

            }
            text += "</ul></body>";
            text += "<script type=\"text/javascript\" src=\"metrologytaxonomy.js\"></script>";
            text += "</html>";
            return text;
        }

        /// <summary>
        /// Create a taxon html page
        /// </summary>
        /// <param name="taxon">Taxon object</param>
        /// <param name="addSlug">True to add the SLUG to the HTML</param>
        /// <returns></returns>
        public static string CreateTaxonHtml(Taxon taxon, bool addSlug = true)
        {
            // text to send back
            string taxonText = string.Empty;

            if (addSlug)
            {//Name and Slug
                string name = taxon.Name;
                string slug = name;
                slug = slug.Replace(".", "-").ToLower();
                string header = "<!--\nName: {name}\n\nSlug: {slug}\n\nHTML: -->\n";
                header = header.Replace("{name}", name);
                header = header.Replace("{slug}", slug);
                taxonText = header;
                string mtc = "https://cls-schemas.s3.us-west-1.amazonaws.com/MetrologyTaxonomyCatalog";
                string uom = "https://cls-schemas.s3.us-west-1.amazonaws.com/UOM_Database";
                taxonText += string.Format("<html xmnls:mtc=\"{0}\" xmlns:uom=\"{1}\">", mtc, uom);
                taxonText += "<head>";
                taxonText += "<meta http-equiv=\"ContentType\" content=\"text/html; charset=UTF-8\">";
                taxonText += "<link rel=\"stylesheet\" href=\"metrologytaxonomy.css\">";
                taxonText += "</head></body>";
                taxonText += "<h2>Metrology Taxon - " + taxon.Name + "</h2>";
            }
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
                li = "\t<li>\n{table1}</li>\n";
                nextli = string.Empty;
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
                                table1 += "<tr><td>" + catList[z].Name +"</td><td>" + catList[z].Value + "</td></tr>\n";
                            }
                        }
                    }

                    table1 += "</tbody>\n</table>\n";
                    nextli = nextli.Replace("{table1}", table1);
                    taxonText = taxonText.Replace("{references}", nextli);
                }
            }
            return taxonText;
        }

    }
}
