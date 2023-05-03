using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace MT_DataAccessLib
{
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
                {
                    newValue.Add(lines[i]);
                }
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
                {
                    sb.AppendLine(reader.ReadOuterXml());
                }

                return sb.ToString();
            }
            return string.Empty;
        }

        public static void SaveLocalXSD(string url)
        {
            string localFileName = TaxonomyFactory.LocalPath + TaxonomyFactory.Catalog + ".xsd";
            if (!File.Exists(localFileName))
            {
                XmlTextReader reader = new XmlTextReader(url);
                XmlSchema schema = XmlSchema.Read(reader, ValidationCallback);
                schema.Write(Console.Out);
                FileStream file = new FileStream(localFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                XmlTextWriter xwriter = new XmlTextWriter(file, new UTF8Encoding());
                xwriter.Formatting = Formatting.Indented;
                schema.Write(xwriter);
            }
        }

        static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.Write("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Console.Write("ERROR: ");

            Console.WriteLine(args.Message);
        }
    }
}
