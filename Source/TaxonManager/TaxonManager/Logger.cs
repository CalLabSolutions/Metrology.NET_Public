using System;
using System.IO;
using System.Linq;


namespace CalLabSolutions.TaxonManager
{
    public class Logger
    {
        public static string logPath = string.Empty;

        private static string logFile = "TaxonManager";

        private static string today = string.Empty;

        public Logger(string path)
        {
            logPath = path;
            if (logPath.Length > 0)
            {
                logPath = logPath.Replace("\\", "/");
                if (logPath.EndsWith("/"))
                {
                    logPath = logPath.TrimEnd('/');
                }
                logPath = logPath + "/" + logFile;
            }
            else
            {
                logPath = "./" + logFile;
            }

            DateTime dateTime = DateTime.Now;
            
            if (!today.Contains(dateTime.ToShortDateString()))
            {
                today = dateTime.ToShortDateString();
                today = today.Replace("/", "_");
                if (logPath.Contains(".txt"))
                {
                    var splitPath = logPath.Split(".txt");
                    splitPath[0] = splitPath[0] + "_" + today;
                    logPath = splitPath[0] + splitPath[1];
                }
                else
                {
                    logPath += "_" + today + ".txt";
                }
            }
        }

        

        public void Write(string message)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            string filePath = logPath;
            try
            {
                if (File.Exists(filePath))
                {
                    ostrm = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                }
                else
                {
                    ostrm = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                }
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open " + filePath + " for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            Console.Write(DateTime.Now.ToString() + ": " + message + "\r");
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }
    }
}
