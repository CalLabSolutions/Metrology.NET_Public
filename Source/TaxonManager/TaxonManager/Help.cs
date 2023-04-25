using System;

namespace CalLabSolutions.TaxonManager
{
    class Help
    {
        private static ConsoleKeyInfo initialCommand;
        public Help(string command)
        {
            if (command.Equals("?") || command.ToLower().Equals("h"))
            {
                initialCommand = HelpMenu();
            }
        }

        public void ProvideHelp()
        {
            if (string.IsNullOrWhiteSpace(initialCommand.KeyChar.ToString()))
            {
                initialCommand = HelpMenu();
                Console.WriteLine("\n");
            }
            if (initialCommand.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("Reloading main menu.\n");
                return;
            }
            var command = initialCommand.Key;
            switch (command)
            {
                case ConsoleKey.D1:
                    CompileHelp();
                    break;

                case ConsoleKey.D2:
                    DecompileHelp();
                    break;

                case ConsoleKey.D3:
                    ExportHelp();
                    break;

                case ConsoleKey.D4:
                    ImportHelp();
                    break;

                default:
                    ConsoleKeyInfo keyInfo = EntryErrorHelp();
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("\n");
                        HelpMenu();
                    }
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    break;
            }
            initialCommand = new ConsoleKeyInfo();
        }

        public ConsoleKeyInfo EntryErrorHelp()
        {
            Console.WriteLine("That was an invalid selection.\nValid options are 1 to 4.\n" +
                "Hit Enter key to return to the help menu,\nEsc key to exit help and return to the main menu,\n or keys Ctrl and C to quit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine("\n");
            return keyInfo;
        }

        private ConsoleKeyInfo HelpMenu()
        {
            Console.WriteLine("Please Enter the number of the command you wish to get help with.\nPress Esc to exit help");
            Console.WriteLine("1. " + ((MenuSelection)1));
            Console.WriteLine("2. " + ((MenuSelection)2));
            Console.WriteLine("3. " + ((MenuSelection)3));
            Console.WriteLine("4. " + ((MenuSelection)4) + "\n");
            return Console.ReadKey(true);
        }

        private void CompileHelp()
        {
            Console.WriteLine("COMPILE: selection 1.\nThis action will take a set of XML files and combine them into a single XML file.\n" +
                "There are 3 options to choose what you want to compile.\n");
            Console.WriteLine("The compile options selection criteria is as follows:\n" +
                "1: will overwrite the existing MTC_Local.xml Taxonomy file in the application path " +
                "with files you have added or modified in the Taxon folder.\n" +
                "2: will create a Taxonomy file based on individual Taxon files you created or modified.\n" +
                "3: will create a Taxonomy file based on a group of files you may have created from single Taxon files.\n\n" +
                "You will specify the path to these files to be compiled and then you will specify where to save the compiled file.\n" +
                "These last 2 options will have no effect on the base Taxonomy file.\n");
        }

        private void DecompileHelp()
        {
            Console.WriteLine("DECOMPILE: selection 2\nThis action will take a Taxonomy file containing multiple taxons and break it down into individual Taxon files.\n" +
                "These will then be saved into a folder of your choice. Then you can edit those individual files and recombine them using the Compile command.\n");
            Console.WriteLine("The decompile options selction criteria is as follows:\n" +
                "1: will decompile the MTC_Local.xml Taxonomy file into individual Taxon files and place them in the Taxon folder.\n" +
                "2: will decompile a group Taxonomy you created using the compile action into individual Taxon files\n" +
                "You will specify where those single files get saved. It should be a folder outside of the existing Taxon folder.\n");
        }

        private void ExportHelp()
        {
            Console.WriteLine("EXPORT: selection 4\nThis action will export files as one of 2 types.\n" + 
                "1: As a XML file with a *.xsl, *.css and *.js file\n" +
                "2: As html from a single Taxon file.\nYou can then load these files into a browser.\n");
        }

        private void ImportHelp()
        {
            Console.WriteLine("IMPORT: selection 5\nThis action will overwrite the MTC_Local.xml file in the application path.\n" + 
                "You can import the Taxonomy file from the MII Server or from a local path you specify.\n");
        }
    }
}
