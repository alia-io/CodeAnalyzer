/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  InputOutput.cs - Parses command line input, formats and prints analysis output   ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CodeAnalyzer
{
    /* Parses input from command line into expected format */
    class InputReader
    {
        // Format (if options are present): "/S", "/R", "/X", "[path]", "*.[filetype]"
        public string[] FormattedInput { get; private set; }

        public string ErrorMessage { get; private set; }

        public InputReader()
        {
            this.FormattedInput = new string[5] { "", "", "", "", "" };
            this.ErrorMessage = "\nArguments must include a valid directory path." +
                "\nPaths with spaces must be surrounded by quotation marks." +
                "\nThe file type to analyze must be specified: *.cs or *.txt." +
                "\nAdditional arguments are optional. Valid arguments include:" +
                "\n\t/S - inlude subdirectories" +
                "\n\t/R - analyze relationship data" +
                "\n\t/X - print data to XML document";
        }

        /* Accepts array of command line arguments; returns true if expected input, false if invalid argument */
        public bool FormatInput(string[] args)
        {
            if (args.Length < 2) // Need at least path and filetype arguments
                return false;

            for (int i = 0; i < args.Length; i++) // Check each argument
            {
                if (!this.SetInputField(args[i]))
                {
                    return false;
                }
            }

            // Path and filetype are not optional arguments
            if (this.FormattedInput[3].Equals("") || this.FormattedInput[4].Equals(""))
                return false;

            return true;
        }

        /* Tests a single argument for validity - saves it to FormattedInput if valid */
        private bool SetInputField(string arg)
        {
            if (arg.ToLower().Equals("/s")) // Check for subdirectory option
            {
                if (this.FormattedInput[0].Equals(""))
                {
                    this.FormattedInput[0] = "/S";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/r")) // Check for relationship option
            {
                if (this.FormattedInput[1].Equals(""))
                {
                    this.FormattedInput[1] = "/R";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/x")) // Check for xml option
            {
                if (this.FormattedInput[2].Equals(""))
                {
                    this.FormattedInput[2] = "/X";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("*.cs") || arg.ToLower().Equals("*.txt")) // Check for filetype
            {
                if (this.FormattedInput[4].Equals(""))
                {
                    this.FormattedInput[4] = arg.ToLower();
                    return true;
                }
                return false;
            }

            /* Test Directory Path input */
            return this.TestDirectoryPath(arg);
        }

        /* Tests whether an argument is a valid directory path - saves it to FormattedInput if valid */
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

    /* Prints the requested code analysis data */
    class OutputWriter
    {
        private List<string> Lines { get; }
        private StringBuilder line = new StringBuilder("");
        private int tabs = 0;

        public OutputWriter() => Lines = new List<string>();

        /* Route the files to the appropriate output writer */
        public void WriteOutput(List<ProgramFile> processedFileList, string directoryPath, /*string directoryName,*/ bool printToXML, bool printRelationships)
        {
            foreach (ProgramFile file in processedFileList)
            {
                if (printToXML)
                {
                    if (printRelationships)
                        this.WriteRelationshipsXML(file);
                    else
                        this.WriteFunctionsXML(file);
                }
                else
                {
                    // Write to the console
                    Console.Write("\n");
                    if (printRelationships)
                        this.PrintRelationships(file);
                    else
                        this.PrintFunctions(file);
                    Console.Write("\n");
                }
            }

            if (printToXML)
            {
                // Write to the file
                string directoryName = directoryPath.Split('\\')[directoryPath.Split('\\').Length - 1];
                if (this.WriteFile(this.NewFilePath(directoryPath, directoryName, printRelationships)))
                    Console.WriteLine("\nCode analysis XML file written!\n");
            }
        }

        /* Prints function data to standard output */
        private void PrintFunctions(ProgramType programType)
        {
            PrintTabs();

            /* Write the new element's name and data */

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
                /* Print function data */
                PrintTabs();
                Console.Write(">---> Size: " + ((ProgramFunction)programType).Size);
                PrintTabs();
                Console.Write(">---> Complexity: " + ((ProgramFunction)programType).Complexity);
                tabs--; // Reset tabs
            }

            /* Repeat recursively with child data */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintFunctions(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--; // Reset tabs
        }

        /* Prints relationship data to standard output */
        private void PrintRelationships(ProgramType programType)
        {
            PrintTabs();

            /* Write the new element's name and data */

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

                /* Print class/interface relationship data */

                if (((ProgramClass)programType).SuperClasses.Count > 0) // Inheritance, parents
                {
                    PrintTabs();
                    Console.Write(">---> Inherits: ");
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

                if (((ProgramClass)programType).SubClasses.Count > 0) // Inheritance, children
                {
                    PrintTabs();
                    Console.Write(">---> Inherited By: ");
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
                tabs--; // Reset tabs
            }

            else if (programType.GetType() == typeof(ProgramInterface))
            {
                Console.Write("Interface: " + programType.Name);
                tabs += 2;

                /* Print class/interface relationship data */

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

            /* Repeat recursively with child data */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintRelationships(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--; // Reset tabs
        }

        /* Sets the filename and filepath to the specified path for the new XML file */
        private string NewFilePath(string directoryPath, string directoryName, bool printRelationships)
        {
            if (printRelationships)
                return directoryPath + "\\" + directoryName + "_relationships.xml";
            else
                return directoryPath + "\\" + directoryName + "_functions.xml";
        }

        /* Writes function data to XML file */
        private void WriteFunctionsXML(ProgramType programType)
        {
            GetTabs(ref line);

            /* ---------- Open the new element ---------- */
            if (programType.GetType() == typeof(ProgramFile))
            {
                line.Append("<file name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                line.Append("<namespace name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramClass))
            {
                line.Append("<class name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                line.Append("<interface name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramFunction))
            {
                line.Append("<function name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;

                GetTabs(ref line);
                line.Append("<size>");
                line.Append(((ProgramFunction)programType).Size);
                line.Append("</size>");

                GetTabs(ref line);
                line.Append("<complexity>");
                line.Append(((ProgramFunction)programType).Complexity);
                line.Append("</complexity>");
            }

            /* ---------- Repeat recursively with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    WriteFunctionsXML(child);

            /* ---------- Close the element ---------- */
            tabs--;
            if (programType.ChildList.Count > 0)
                GetTabs(ref line);
            if (programType.GetType() == typeof(ProgramFile))
                line.Append("</file>");
            else if (programType.GetType() == typeof(ProgramNamespace))
                line.Append("</namespace>");
            else if (programType.GetType() == typeof(ProgramClass))
                line.Append("</class>");
            else if (programType.GetType() == typeof(ProgramInterface))
                line.Append("</interface>");
            else if (programType.GetType() == typeof(ProgramFunction))
                line.Append("</function>");
        }

        /* Writes relationship data to XML file */
        private void WriteRelationshipsXML(ProgramType programType)
        {
            GetTabs(ref line);

            /* ---------- Open the new element ---------- */
            if (programType.GetType() == typeof(ProgramFile))
            {
                line.Append("<file name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                line.Append("<namespace name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }
            else if (programType.GetType() == typeof(ProgramClass))
            {
                line.Append("<class name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;

                if (((ProgramClass)programType).SuperClasses.Count > 0) // Inheritance, parents
                    foreach (ProgramClassType superclass in ((ProgramClass)programType).SuperClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<inheritance_parent>");
                        line.Append(superclass.Name);
                        line.Append("</inheritance_parent>");
                    }

                if (((ProgramClass)programType).SubClasses.Count > 0) // Inheritance, children
                    foreach (ProgramClassType subclass in ((ProgramClass)programType).SubClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<inheritance_child>");
                        line.Append(subclass.Name);
                        line.Append("</inheritance_child>");
                    }

                if (((ProgramClass)programType).OwnedByClasses.Count > 0) // Aggregation, parents
                    foreach (ProgramClassType ownerclass in ((ProgramClass)programType).OwnedByClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<aggregation_parent>");
                        line.Append(ownerclass.Name);
                        line.Append("</aggregation_parent>");
                    }

                if (((ProgramClass)programType).OwnedClasses.Count > 0) // Aggregation, children
                    foreach (ProgramClassType ownedclass in ((ProgramClass)programType).OwnedClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<aggregation_child>");
                        line.Append(ownedclass.Name);
                        line.Append("</aggregation_child>");
                    }

                if (((ProgramClass)programType).UsedByClasses.Count > 0) // Using, parents
                    foreach (ProgramClassType userclass in ((ProgramClass)programType).UsedByClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<using_parent>");
                        line.Append(userclass.Name);
                        line.Append("</using_parent>");
                    }

                if (((ProgramClass)programType).UsedClasses.Count > 0) // Using, children
                    foreach (ProgramClassType usedclass in ((ProgramClass)programType).UsedClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<using_child>");
                        line.Append(usedclass.Name);
                        line.Append("</using_child>");
                    }
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                line.Append("<interface name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;

                if (((ProgramInterface)programType).SuperClasses.Count > 0) // Inheritance, parents
                    foreach (ProgramClassType superclass in ((ProgramInterface)programType).SuperClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<inheritance_parent>");
                        line.Append(superclass.Name);
                        line.Append("</inheritance_parent>");
                    }

                if (((ProgramInterface)programType).SubClasses.Count > 0) // Inheritance, children
                    foreach (ProgramClassType subclass in ((ProgramInterface)programType).SubClasses)
                    {
                        GetTabs(ref line);
                        line.Append("<inheritance_child>");
                        line.Append(subclass.Name);
                        line.Append("</inheritance_child>");
                    }
            }
            else if (programType.GetType() == typeof(ProgramFunction))
            {
                line.Append("<function name = \"");
                line.Append(programType.Name);
                line.Append("\">");
                tabs++;
            }

            /* ---------- Repeat recursively with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    WriteRelationshipsXML(child);

            /* ---------- Close the element ---------- */
            tabs--;
            if (programType.ChildList.Count > 0)
                GetTabs(ref line);
            if (programType.GetType() == typeof(ProgramFile))
                line.Append("</file>");
            else if (programType.GetType() == typeof(ProgramNamespace))
                line.Append("</namespace>");
            else if (programType.GetType() == typeof(ProgramClass))
                line.Append("</class>");
            else if (programType.GetType() == typeof(ProgramInterface))
                line.Append("</interface>");
            else if (programType.GetType() == typeof(ProgramFunction))
                line.Append("</function>");
        }

        private bool WriteFile(string filePath)
        {
            try
            {
                File.WriteAllLines(filePath, Lines.ToArray());
            }
            catch
            {
                Console.WriteLine("\nError: Unable to write to a new XML file.\n");
                return false;
            }
            return true;
        }

        private void PrintTabs()
        {
            Console.Write("\n");

            for (int i = 0; i < tabs; i++)
                Console.Write("    ");
        }

        private void GetTabs(ref StringBuilder line)
        {
            if (line.Length > 0)
            {
                Lines.Add(line.ToString());
                line.Clear();
            }

            for (int i = 0; i < tabs; i++)
                line.Append("    ");
        }
    }
}
