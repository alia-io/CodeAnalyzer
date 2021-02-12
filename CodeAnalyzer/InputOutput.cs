using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalyzer
{
    class InputReader
    {
        public string[] FormattedInput { get; private set; }
            // Format (if options are present): "/S", "/R", "/X", "[path]"

        public string ErrorMessage { get; private set; }

        public InputReader()
        {
            this.FormattedInput = new string[4] { "", "", "", "" };
            this.ErrorMessage = "";
        }


        /* Returns true if successful; returns false if invalid argument */
        public bool FormatInput(string[] args)
        {
            if (args.Length < 1)
            {
                this.ErrorMessage = "Needs more command line arguments.";
                return false;
            }

            this.ErrorMessage = "\nArguments must include a valid directory path." +
                "\nPaths with spaces must be surrounded by quotation marks." +
                "\nAdditional arguments are optional. Valid arguments include:" +
                "\n\t/S - inlude subdirectories" +
                "\n\t/R - analyze relationship data" +
                "\n\t/X - print data to XML document";

            for (int i = 0; i < args.Length; i++)
            {
                if (!this.SetInputField(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /* Returns true if successful; returns false if invalid argument */
        private bool SetInputField(string arg)
        {
            /* Test Option input */

            if (arg.ToLower().Equals("/s"))
            {
                if (this.FormattedInput[0].Equals(""))
                {
                    this.FormattedInput[0] = "/S";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/r"))
            {
                if (this.FormattedInput[1].Equals(""))
                {
                    this.FormattedInput[1] = "/R";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/x"))
            {
                if (this.FormattedInput[2].Equals(""))
                {
                    this.FormattedInput[2] = "/X";
                    return true;
                }
                return false;
            }

            /* Test Directory Path input */
            return this.TestDirectoryPath(arg);
        }

        /* Returns true if successful; returns false if invalid argument */
        private bool TestDirectoryPath(string arg)
        {
            try
            {
                string directoryPath = Path.GetFullPath(arg);
                if (Directory.Exists(directoryPath))
                {
                    if (this.FormattedInput[3].Equals(""))
                    {
                        this.FormattedInput[3] = directoryPath;
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }

    class OutputWriter
    {
        private int tabs = 0;

        /* Route the files to the appropriate output writer */
        public void WriteOutput(List<ProgramFile> processedFileList, bool printToXML, bool printRelationships)
        {
            foreach (ProgramFile file in processedFileList)
            {
                if (printToXML && printRelationships)
                    this.WriteRelationshipsXML(file);
                else if (printToXML)
                    this.WriteFunctionsXML(file);
                else
                {
                    Console.Write("\n");
                    if (printRelationships)
                        this.PrintRelationships(file);
                    else
                        this.PrintFunctions(file);
                    Console.Write("\n");
                }
            }
        }

        private void PrintFunctions(ProgramType programType)
        {
            PrintTabs();

            /* ---------- Write the new element's name and data ---------- */

            if (programType.GetType() == typeof(ProgramFile))
            {
                Console.Write("------------------------------------------------------------------------------------------\n");
                Console.Write("File: " + programType.Name);
                Console.Write("\n------------------------------------------------------------------------------------------\n");
            }

            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                Console.Write("Namespace: " + programType.Name);
                tabs++;
            }

            else if (programType.GetType() == typeof(ProgramClass))
            {
                Console.Write("Class: " + programType.Name);
                tabs++;
            }

            else if (programType.GetType() == typeof(ProgramInterface))
            {
                Console.Write("Interface: " + programType.Name);
                tabs++;
            }

            else if (programType.GetType() == typeof(ProgramFunction))
            {
                Console.Write("Function: " + programType.Name);
                tabs += 2;
                /* ---------- Print function data ---------- */
                PrintTabs();
                Console.Write(">---> Size: " + ((ProgramFunction)programType).Size);
                PrintTabs();
                Console.Write(">---> Complexity: " + ((ProgramFunction)programType).Complexity);
                tabs--; // reset tabs
            }

            /* ---------- Repeat with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintFunctions(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--; // reset tabs
        }

        private void PrintRelationships(ProgramType programType)
        {
            PrintTabs();

            /* ---------- Write the new element's name and data ---------- */

            if (programType.GetType() == typeof(ProgramFile))
            {
                Console.Write("------------------------------------------------------------------------------------------\n");
                Console.Write("File: " + programType.Name);
                Console.Write("\n------------------------------------------------------------------------------------------\n");
            }

            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                Console.Write("Namespace: " + programType.Name);
                tabs++;
            }

            else if (programType.GetType() == typeof(ProgramClass))
            {
                Console.Write("Class: " + programType.Name);
                tabs += 2;

                /* ---------- Print class/interface data ---------- */

                if (((ProgramClass)programType).SuperClasses.Count > 0) // Inheritance, parents
                {
                    PrintTabs();
                    if (((ProgramClass)programType).SuperClasses.Count == 1)
                        Console.Write(">---> Superclass: " + ((ProgramClass)programType).SuperClasses[0].Name);
                    else
                    {
                        Console.Write(">---> Superclasses: ");
                        foreach (ProgramClassType superclass in ((ProgramClass)programType).SuperClasses)
                        {
                            if (superclass.Equals(((ProgramClass)programType).SuperClasses[0]))
                            {
                                Console.Write(superclass.Name);
                                continue;
                            }
                            Console.Write(", " + superclass.Name);
                        }
                    }
                }

                if (((ProgramClass)programType).SubClasses.Count > 0) // Inheritance, children
                {
                    PrintTabs();
                    if (((ProgramClass)programType).SubClasses.Count == 1)
                        Console.Write(">---> Subclass: " + ((ProgramClass)programType).SubClasses[0].Name);
                    else
                    {
                        Console.Write(">---> Subclasses: ");
                        foreach (ProgramClassType subclass in ((ProgramClass)programType).SubClasses)
                        {
                            if (subclass.Equals(((ProgramClass)programType).SubClasses[0]))
                            {
                                Console.Write(subclass.Name);
                                continue;
                            }
                            Console.Write(", " + subclass.Name);
                        }
                    }
                }

                if (((ProgramClass)programType).OwnedByClasses.Count > 0) // Aggregation, parents
                {
                    PrintTabs();
                    Console.Write(">---> Owned By: ");
                    foreach (ProgramClassType ownerclass in ((ProgramClass)programType).OwnedByClasses)
                    {
                        if (ownerclass.Equals(((ProgramClass)programType).OwnedByClasses[0]))
                        {
                            Console.Write(ownerclass.Name);
                            continue;
                        }
                        Console.Write(", " + ownerclass.Name);
                    }
                }

                if (((ProgramClass)programType).OwnedClasses.Count > 0) // Aggregation, children
                {
                    PrintTabs();
                    Console.Write(">---> Owner Of: ");
                    foreach (ProgramClassType ownedclass in ((ProgramClass)programType).OwnedClasses)
                    {
                        if (ownedclass.Equals(((ProgramClass)programType).OwnedClasses[0]))
                        {
                            Console.Write(ownedclass.Name);
                            continue;
                        }
                        Console.Write(", " + ownedclass.Name);
                    }
                }

                if (((ProgramClass)programType).UsedByClasses.Count > 0) // Using, parents
                {
                    PrintTabs();
                    Console.Write(">---> Used By: ");
                    foreach (ProgramClassType userclass in ((ProgramClass)programType).UsedByClasses)
                    {
                        if (userclass.Equals(((ProgramClass)programType).UsedByClasses[0]))
                        {
                            Console.Write(userclass.Name);
                            continue;
                        }
                        Console.Write(", " + userclass.Name);
                    }
                }

                if (((ProgramClass)programType).UsedClasses.Count > 0) // Using, children
                {
                    PrintTabs();
                    Console.Write(">---> Using: ");
                    foreach (ProgramClassType usedclass in ((ProgramClass)programType).UsedClasses)
                    {
                        if (usedclass.Equals(((ProgramClass)programType).UsedClasses[0]))
                        {
                            Console.Write(usedclass.Name);
                            continue;
                        }
                        Console.Write(", " + usedclass.Name);
                    }
                }
                tabs--; // reset tabs
            }

            else if (programType.GetType() == typeof(ProgramInterface))
            {
                Console.Write("Interface: " + programType.Name);
                tabs += 2;

                /* ---------- Print class/interface data ---------- */

                if (((ProgramInterface)programType).SuperClasses.Count > 0) // Inheritance, parents
                {
                    PrintTabs();
                    if (((ProgramInterface)programType).SuperClasses.Count == 1)
                        Console.Write(">---> Superclass: " + ((ProgramInterface)programType).SuperClasses[0].Name);
                    else
                    {
                        Console.Write(">---> Superclasses: ");
                        foreach (ProgramClassType superclass in ((ProgramInterface)programType).SuperClasses)
                        {
                            if (superclass.Equals(((ProgramInterface)programType).SuperClasses[0]))
                            {
                                Console.Write(superclass.Name);
                                continue;
                            }
                            Console.Write(", " + superclass.Name);
                        }
                    }
                }

                if (((ProgramInterface)programType).SubClasses.Count > 0) // Inheritance, children
                {
                    PrintTabs();
                    if (((ProgramInterface)programType).SubClasses.Count == 1)
                        Console.Write(">---> Subclass: " + ((ProgramInterface)programType).SubClasses[0].Name);
                    else
                    {
                        Console.Write(">---> Subclasses: ");
                        foreach (ProgramClassType subclass in ((ProgramInterface)programType).SubClasses)
                        {
                            if (subclass.Equals(((ProgramInterface)programType).SubClasses[0]))
                            {
                                Console.Write(subclass.Name);
                                continue;
                            }
                            Console.Write(", " + subclass.Name);
                        }
                    }
                }

                if (((ProgramInterface)programType).UsedByClasses.Count > 0) // Using, parents
                {
                    PrintTabs();
                    Console.Write(">---> Used By: ");
                    foreach (ProgramClassType userclass in ((ProgramInterface)programType).UsedByClasses)
                    {
                        if (userclass.Equals(((ProgramInterface)programType).UsedByClasses[0]))
                        {
                            Console.Write(userclass.Name);
                            continue;
                        }
                        Console.Write(", " + userclass.Name);
                    }
                }
                tabs--; // reset tabs
            }

            else if (programType.GetType() == typeof(ProgramFunction))
            {
                Console.Write("Function: " + programType.Name);
                tabs++;
            }

            /* ---------- Repeat with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintRelationships(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--; // reset tabs
        }

        private void WriteFunctionsXML(ProgramType programType)
        {

        }

        private void WriteRelationshipsXML(ProgramType programType)
        {

        }

        private void PrintAllOutputXML(ProgramType programType)
        {
            PrintTabs();

            /* ---------- Open the new element ---------- */
            if (programType.GetType() == typeof(ProgramFile))
            {
                Console.Write("<file name = \"" + programType.Name + "\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                Console.Write("<namespace name = \"" + programType.Name + "\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramClass))
            {
                Console.Write("<class name = \"" + programType.Name + "\">");
                tabs++;

                if (((ProgramClass)programType).SuperClasses.Count > 0) // Inheritance, parents
                    foreach (ProgramClassType superclass in ((ProgramClass)programType).SuperClasses)
                    {
                        PrintTabs();
                        Console.Write("<inheritance_parent>" + superclass.Name + "</inheritance_parent>");
                    }

                if (((ProgramClass)programType).SubClasses.Count > 0) // Inheritance, children
                    foreach (ProgramClassType subclass in ((ProgramClass)programType).SubClasses)
                    {
                        PrintTabs();
                        Console.Write("<inheritance_child>" + subclass.Name + "</inheritance_child>");
                    }

                if (((ProgramClass)programType).OwnedByClasses.Count > 0) // Aggregation, parents
                    foreach (ProgramClassType ownerclass in ((ProgramClass)programType).OwnedByClasses)
                    {
                        PrintTabs();
                        Console.Write("<aggregation_parent>" + ownerclass.Name + "</aggregation_parent>");
                    }

                if (((ProgramClass)programType).OwnedClasses.Count > 0) // Aggregation, children
                    foreach (ProgramClassType ownedclass in ((ProgramClass)programType).OwnedClasses)
                    {
                        PrintTabs();
                        Console.Write("<aggregation_child>" + ownedclass.Name + "</aggregation_child>");
                    }

                if (((ProgramClass)programType).UsedByClasses.Count > 0) // Using, parents
                    foreach (ProgramClassType userclass in ((ProgramClass)programType).UsedByClasses)
                    {
                        PrintTabs();
                        Console.Write("<using_parent>" + userclass.Name + "</using_parent>");
                    }

                if (((ProgramClass)programType).UsedClasses.Count > 0) // Using, children
                    foreach (ProgramClassType usedclass in ((ProgramClass)programType).UsedClasses)
                    {
                        PrintTabs();
                        Console.Write("<using_child>" + usedclass.Name + "</using_child>");
                    }
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                Console.Write("<interface name = \"" + programType.Name + "\">");
                tabs++;

                if (((ProgramInterface)programType).SuperClasses.Count > 0) // Inheritance, parents
                    foreach (ProgramClassType superclass in ((ProgramInterface)programType).SuperClasses)
                    {
                        PrintTabs();
                        Console.Write("<inheritance_parent>" + superclass.Name + "</inheritance_parent>");
                    }

                if (((ProgramInterface)programType).SubClasses.Count > 0) // Inheritance, children
                    foreach (ProgramClassType subclass in ((ProgramInterface)programType).SubClasses)
                    {
                        PrintTabs();
                        Console.Write("<inheritance_child>" + subclass.Name + "</inheritance_child>");
                    }

                if (((ProgramInterface)programType).UsedByClasses.Count > 0) // Using, parents
                    foreach (ProgramClassType userclass in ((ProgramInterface)programType).UsedByClasses)
                    {
                        PrintTabs();
                        Console.Write("<using_parent>" + userclass.Name + "</using_parent>");
                    }
            }
            else if (programType.GetType() == typeof(ProgramFunction))
            {
                Console.Write("<function name = \"" + programType.Name + "\">");
                tabs++;
                PrintTabs();
                Console.Write("<size>" + ((ProgramFunction)programType).Size + "</size>");

                PrintTabs();
                Console.Write("<complexity>" + ((ProgramFunction)programType).Complexity + "</complexity>");
            }

            /* ---------- Repeat with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintAllOutputXML(child);

            /* ---------- Close the element ---------- */
            tabs--;
            PrintTabs();
            if (programType.GetType() == typeof(ProgramFile))
                Console.Write("</file>");
            else if (programType.GetType() == typeof(ProgramNamespace))
                Console.Write("</namespace>");
            else if (programType.GetType() == typeof(ProgramClass))
                Console.Write("</class>");
            else if (programType.GetType() == typeof(ProgramInterface))
                Console.Write("</interface>");
            else if (programType.GetType() == typeof(ProgramFunction))
                Console.Write("</function>");
        }

        private void PrintTabs()
        {
            Console.Write("\n");
            for (int i = 0; i < tabs; i++)
                Console.Write("    ");
        }
    }
}
