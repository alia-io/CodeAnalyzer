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
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CodeAnalyzer
{
    /* Parses input from command line into expected format */
    public class InputReader
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
                "\n\t/S - inlude files in subdirectories" +
                "\n\t/R - analyze class relationship data" +
                "\n\t/X - print data to XML document";
        }

        /* Accepts array of command line arguments; returns true if expected input, false if invalid argument */
        public bool FormatInput(string[] args)
        {
            if (args.Length < 2) // Need at least path and filetype arguments
                return false;

            for (int i = 0; i < args.Length; i++) // Check each argument
                if (!this.SetInputField(args[i]))
                    return false;

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

            if (arg.ToLower().Equals("*.cs") || arg.ToLower().Equals("*.java") || arg.ToLower().Equals("*.txt")) // Check for filetype
            {
                if (this.FormattedInput[4].Equals(""))
                {
                    this.FormattedInput[4] = arg.ToLower();
                    return true;
                }
                return false;
            }
            
            return this.TestDirectoryPath(arg); // Make sure directory path input is valid
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
    public class OutputWriter
    {
        private List<string> Lines { get; }
        private StringBuilder line = new StringBuilder("");
        private int tabs = 0;
        XDocument xml;

        public OutputWriter() => Lines = new List<string>();

        /* Route the files to the appropriate output writer */
        public void WriteOutput(List<ProgramFile> processedFileList, string directoryPath, string fileType, bool printToXML, bool printRelationships)
        {
            if (printToXML) xml = new XDocument(new XElement("project"));

            foreach (ProgramFile file in processedFileList)
            {
                if (printToXML)
                {
                    if (printRelationships)
                        this.WriteRelationshipsXML(xml.Root, file);
                    else
                        this.WriteFunctionsXML(xml.Root, file);
                }
                // Write to the console
                Console.Write("\n");
                if (printRelationships)
                    this.PrintRelationships(file);
                else
                    this.PrintFunctions(file);
                Console.Write("\n");
            }

            if (printToXML) // Write to the file
                this.WriteFile(directoryPath, directoryPath.Split('\\')[directoryPath.Split('\\').Length - 1], fileType, printRelationships);
        }

        /* Prints function data to standard output */
        private void PrintFunctions(ProgramType programType)
        {
            PrintTabs();

            // Find the type of the element, print its name (and data, when relevant)
            if (programType.GetType() == typeof(ProgramFile))
                this.PrintProgramFile((ProgramFile)programType);

            else if (programType.GetType() == typeof(ProgramNamespace))
                this.PrintProgramNamespace((ProgramNamespace)programType);

            else if (programType.GetType() == typeof(ProgramClass))
                this.PrintProgramClass((ProgramClass)programType);

            else if (programType.GetType() == typeof(ProgramInterface))
                this.PrintProgramInterface((ProgramInterface)programType);

            else if (programType.GetType() == typeof(ProgramFunction))
            {
                this.PrintProgramFunction((ProgramFunction)programType);
                // Also print analysis data for functions
                this.PrintFunctionAnalysisData((ProgramFunction)programType);
            }

            // Repeat recursively for each child
            foreach (ProgramType child in programType.ChildList)
                PrintFunctions(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--;
        }

        /* Prints relationship data to standard output */
        private void PrintRelationships(ProgramType programType)
        {
            PrintTabs();

            // Find the type of the element, print its name (and data, when relevant)
            if (programType.GetType() == typeof(ProgramFile))
                this.PrintProgramFile((ProgramFile)programType);

            else if (programType.GetType() == typeof(ProgramNamespace))
                this.PrintProgramNamespace((ProgramNamespace)programType);

            else if (programType.GetType() == typeof(ProgramClass))
            {
                this.PrintProgramClass((ProgramClass)programType);
                // Also print analysis data for classes
                this.PrintClassAnalysisData((ProgramClass)programType);
            }

            else if (programType.GetType() == typeof(ProgramInterface))
            {
                this.PrintProgramInterface((ProgramInterface)programType);
                // Also print analysis data for interfaces
                this.PrintInterfaceAnalysisData((ProgramInterface)programType);
            }

            else if (programType.GetType() == typeof(ProgramFunction))
                this.PrintProgramFunction((ProgramFunction)programType);

            // Repeat recursively for each child
            foreach (ProgramType child in programType.ChildList)
                PrintRelationships(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--;
        }

        /* Adds lines to write function data to XML file */
        private void WriteFunctionsXML(XElement parent, ProgramType programType)
        {
            XElement element = null;

            // Find the type and create the new element
            if (programType.GetType() == typeof(ProgramFile))
                element = new XElement("file", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramNamespace))
                element = new XElement("namespace", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramClass))
                element = new XElement("class", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramInterface))
                element = new XElement("interface", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramFunction)) // Also add analysis data for functions
                element = new XElement("function", new XAttribute("name", programType.Name),
                                                    new XElement("size", ((ProgramFunction)programType).Size),
                                                    new XElement("complexity", ((ProgramFunction)programType).Complexity));

            if (element != null)
            {
                parent.Add(element); // Add the element to its parent
                foreach (ProgramType child in programType.ChildList) // Repeat recursively for each child
                    WriteFunctionsXML(element, child);
            }
        }

        /* Adds lines to write relationship data to XML file */
        private void WriteRelationshipsXML(XElement parent, ProgramType programType)
        {
            XElement element = null;

            // Find the type and open the new element
            if (programType.GetType() == typeof(ProgramFile))
                element = new XElement("file", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramNamespace))
                element = new XElement("namespace", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramClass))
            {
                element = new XElement("class", new XAttribute("name", programType.Name));
                this.WriteClassAnalysisData((ProgramClass)programType, element); // Also add analysis data for classes
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                element = new XElement("interface", new XAttribute("name", programType.Name));
                this.WriteInterfaceAnalysisData((ProgramInterface)programType, element); // Also add analysis data for interfaces
            }
            else if (programType.GetType() == typeof(ProgramFunction))
                element = new XElement("function", new XAttribute("name", programType.Name));

            if (element != null)
            {
                parent.Add(element); // Add the element to its parent
                foreach (ProgramType child in programType.ChildList) // Repeat recursively for each child
                    WriteRelationshipsXML(element, child);
            }
        }

        /* Prints file name to standard output */
        private void PrintProgramFile(ProgramFile programFile)
        {
            Console.Write("----------------------------------------------------------------------\n");
            Console.Write("File: " + programFile.Name);
            Console.Write("\n----------------------------------------------------------------------\n");
        }

        /* Prints namespace name to standard output */
        private void PrintProgramNamespace(ProgramNamespace programNamespace)
        {
            Console.Write("Namespace: " + programNamespace.Name);
            tabs++;
        }

        /* Prints file name to standard output */
        private void PrintProgramClass(ProgramClass programClass)
        {
            Console.Write("Class: " + programClass.Name);
            tabs++;
        }

        /* Prints interface name to standard output */
        private void PrintProgramInterface(ProgramInterface programInterface)
        {
            Console.Write("Interface: " + programInterface.Name);
            tabs++;
        }

        /* Prints function name to standard output */
        private void PrintProgramFunction(ProgramFunction programFunction)
        {
            Console.Write("Function: " + programFunction.Name);
            tabs++;
        }

        /* Prints the function's size and complexity to standard output */
        private void PrintFunctionAnalysisData(ProgramFunction programFunction)
        {
            tabs++;
            PrintTabs();
            Console.Write(">---> Size: " + programFunction.Size);

            PrintTabs();
            Console.Write(">---> Complexity: " + programFunction.Complexity);
            tabs--;
        }

        /* Prints the class's relationship information to standard output */
        private void PrintClassAnalysisData(ProgramClass programClass)
        {
            this.PrintInheritanceData(programClass);

            if (programClass.OwnedByClasses.Count > 0) // Aggregation, parents
            {
                PrintTabs();
                Console.Write(">---> Owned By: ");
                foreach (ProgramClassType ownerclass in programClass.OwnedByClasses)
                {
                    if (!ownerclass.Equals(programClass.OwnedByClasses[0])) Console.Write(", ");
                    Console.Write(ownerclass.Name);
                }
            }

            if (programClass.OwnedClasses.Count > 0) // Aggregation, children
            {
                PrintTabs();
                Console.Write(">---> Owner Of: ");
                foreach (ProgramClassType ownedclass in programClass.OwnedClasses)
                {
                    if (!ownedclass.Equals(programClass.OwnedClasses[0])) Console.Write(", ");
                    Console.Write(ownedclass.Name);
                }
            }

            if (programClass.UsedByClasses.Count > 0) // Using, parents
            {
                PrintTabs();
                Console.Write(">---> Used By: ");
                foreach (ProgramClassType userclass in programClass.UsedByClasses)
                {
                    if (!userclass.Equals(programClass.UsedByClasses[0])) Console.Write(", ");
                    Console.Write(userclass.Name);
                }
            }

            if (programClass.UsedClasses.Count > 0) // Using, children
            {
                PrintTabs();
                Console.Write(">---> Using: ");
                foreach (ProgramClassType usedclass in programClass.UsedClasses)
                {
                    if (!usedclass.Equals(programClass.UsedClasses[0])) Console.Write(", ");
                    Console.Write(usedclass.Name);
                }
            }

            tabs--;
        }

        /* Prints the interface's relationship information to standard output */
        private void PrintInterfaceAnalysisData(ProgramInterface programInterface)
        {
            this.PrintInheritanceData(programInterface);
            tabs--;
        }

        /* Prints the inheritance information for a class or interface to standard output */
        private void PrintInheritanceData(ProgramClassType programClassType)
        {
            tabs++;

            if (programClassType.SuperClasses.Count > 0) // Inheritance, parents
            {
                PrintTabs();
                Console.Write(">---> Inherits: ");
                foreach (ProgramClassType superclass in programClassType.SuperClasses)
                {
                    if (!superclass.Equals(programClassType.SuperClasses[0])) Console.Write(", ");
                    Console.Write(superclass.Name);
                }
            }

            if (programClassType.SubClasses.Count > 0) // Inheritance, children
            {
                PrintTabs();
                Console.Write(">---> Inherited By: ");
                foreach (ProgramClassType subclass in programClassType.SubClasses)
                {
                    if (!subclass.Equals(programClassType.SubClasses[0])) Console.Write(", ");
                    Console.Write(subclass.Name);
                }
            }
        }

        /* Adds XML tags for the class's relationship data */
        private void WriteClassAnalysisData(ProgramClass programClass, XElement element)
        {
            this.WriteInheritanceData(programClass, element);

            if (programClass.OwnedByClasses.Count > 0) // Aggregation, parents
                foreach (ProgramClassType ownerclass in programClass.OwnedByClasses)
                    element.Add(new XElement("aggregation_parent", ownerclass.Name));

            if (programClass.OwnedClasses.Count > 0) // Aggregation, children
                foreach (ProgramClassType ownedclass in programClass.OwnedClasses)
                    element.Add(new XElement("aggregation_child", ownedclass.Name));

            if (programClass.UsedByClasses.Count > 0) // Using, parents
                foreach (ProgramClassType userclass in programClass.UsedByClasses)
                    element.Add(new XElement("using_parent", userclass.Name));

            if (programClass.UsedClasses.Count > 0) // Using, children
                foreach (ProgramClassType usedclass in programClass.UsedClasses)
                    element.Add(new XElement("using_child", usedclass.Name));
        }

        /* Adds XML tags for the interface's relationship data */
        private void WriteInterfaceAnalysisData(ProgramInterface programInterface, XElement element)
        {
            this.WriteInheritanceData(programInterface, element);
        }

        /* Adds XML tags of the inheritance information for a class or interface */
        private void WriteInheritanceData(ProgramClassType programClassType, XElement element)
        {
            if (programClassType.SuperClasses.Count > 0) // Inheritance, parents
                foreach (ProgramClassType superclass in programClassType.SuperClasses)
                    element.Add(new XElement("inheritance_parent", superclass.Name));

            if (programClassType.SubClasses.Count > 0) // Inheritance, children
                foreach (ProgramClassType subclass in programClassType.SubClasses)
                    element.Add(new XElement("inheritance_child", subclass.Name));
        }

        /* Sets the filename and filepath to the specified path for the new XML file */
        private string NewFilePath(string directoryPath, string directoryName, string fileType, bool printRelationships)
        {
            string fileName;

            if (printRelationships) fileName = directoryPath + "\\" + directoryName + "_relationships";
            else fileName = directoryPath + "\\" + directoryName + "_functions";

            if (fileType.Equals("*.cs")) fileName += ".cs";
            else if (fileType.Equals("*.java")) fileName += ".java";
            else if (fileType.Equals("*.txt")) fileName += ".txt";

            return fileName + ".xml";
        }

        /* Writes and saves the new XML file */
        private void WriteFile(string directoryPath, string directoryName, string fileType, bool printRelationships)
        {
            string filePath = this.NewFilePath(directoryPath, directoryName, fileType, printRelationships);

            try
            {
                xml.Save(filePath);
                if (printRelationships)
                    Console.WriteLine("\nRelationship analysis XML file written!\n");
                else
                    Console.WriteLine("\nFunction analysis XML file written!\n");
            }
            catch
            {
                Console.WriteLine("\nError: Unable to write to a new XML file.\n");
            }
        }

        /* Prints appropriate number of tabs for current line to standard output */
        private void PrintTabs()
        {
            Console.Write("\n");

            for (int i = 0; i < tabs; i++)
                Console.Write("    ");
        }
    }
}
