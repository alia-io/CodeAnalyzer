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

            if (arg.ToLower().Equals("*.cs") || arg.ToLower().Equals("*.txt")) // Check for filetype
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
    class OutputWriter
    {
        private List<string> Lines { get; }
        private StringBuilder line = new StringBuilder("");
        private int tabs = 0;

        public OutputWriter() => Lines = new List<string>();

        /* Route the files to the appropriate output writer */
        public void WriteOutput(List<ProgramFile> processedFileList, string directoryPath, bool printToXML, bool printRelationships)
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
                if (this.WriteFile(this.NewFilePath(directoryPath, directoryPath.Split('\\')[directoryPath.Split('\\').Length - 1], printRelationships)))
                    Console.WriteLine("\nCode analysis XML file written!\n");
            }
        }

        /* Prints function data to standard output */
        private void PrintFunctions(ProgramType programType)
        {
            PrintTabs();

            /* Write the new element's name and data */

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
                this.PrintFunctionAnalysisData((ProgramFunction)programType);
            }

            /* Repeat recursively with child data */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintFunctions(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--;
        }

        /* Prints relationship data to standard output */
        private void PrintRelationships(ProgramType programType)
        {
            PrintTabs();

            /* Write the new element's name and data */

            if (programType.GetType() == typeof(ProgramFile))
                this.PrintProgramFile((ProgramFile)programType);

            else if (programType.GetType() == typeof(ProgramNamespace))
                this.PrintProgramNamespace((ProgramNamespace)programType);

            else if (programType.GetType() == typeof(ProgramClass))
            {
                this.PrintProgramClass((ProgramClass)programType);
                this.PrintClassAnalysisData((ProgramClass)programType);
            }

            else if (programType.GetType() == typeof(ProgramInterface))
            {
                this.PrintProgramInterface((ProgramInterface)programType);
                this.PrintInterfaceAnalysisData((ProgramInterface)programType);
            }

            else if (programType.GetType() == typeof(ProgramFunction))
                this.PrintProgramFunction((ProgramFunction)programType);

            /* Repeat recursively with child data */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    PrintRelationships(child);

            if (programType.GetType() != typeof(ProgramFile)) tabs--;
        }

        /* Writes function data to XML file */
        private void WriteFunctionsXML(ProgramType programType)
        {
            GetTabs(ref line);

            /* ---------- Open the new element ---------- */

            if (programType.GetType() == typeof(ProgramFile))
                this.WriteProgramFile((ProgramFile)programType);

            else if (programType.GetType() == typeof(ProgramNamespace))
                this.WriteProgramNamespace((ProgramNamespace)programType);

            else if (programType.GetType() == typeof(ProgramClass))
                this.WriteProgramClass((ProgramClass)programType);

            else if (programType.GetType() == typeof(ProgramInterface))
                this.WriteProgramInterface((ProgramInterface)programType);

            else if (programType.GetType() == typeof(ProgramFunction))
            {
                this.WriteProgramFunction((ProgramFunction)programType);
                this.WriteFunctionAnalysisData((ProgramFunction)programType);
            }

            /* ---------- Repeat recursively with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    WriteFunctionsXML(child);

            /* ---------- Close the element ---------- */
            tabs--;
            if (programType.ChildList.Count > 0 || programType.GetType() == typeof(ProgramFunction))
                GetTabs(ref line);

            this.WriteClosingTag(programType);
        }

        /* Writes relationship data to XML file */
        private void WriteRelationshipsXML(ProgramType programType)
        {
            GetTabs(ref line);

            /* ---------- Open the new element ---------- */

            if (programType.GetType() == typeof(ProgramFile))
                this.WriteProgramFile((ProgramFile)programType);

            else if (programType.GetType() == typeof(ProgramNamespace))
                this.WriteProgramNamespace((ProgramNamespace)programType);

            else if (programType.GetType() == typeof(ProgramClass))
            {
                this.WriteProgramClass((ProgramClass)programType);
                this.WriteClassAnalysisData((ProgramClass)programType);
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                this.WriteProgramInterface((ProgramInterface)programType);
                this.WriteInterfaceAnalysisData((ProgramInterface)programType);
            }
            else if (programType.GetType() == typeof(ProgramFunction))
                this.WriteProgramFunction((ProgramFunction)programType);

            /* ---------- Repeat recursively with child data ---------- */
            if (programType.ChildList.Count > 0)
                foreach (ProgramType child in programType.ChildList)
                    WriteRelationshipsXML(child);

            /* ---------- Close the element ---------- */
            tabs--;
            if (programType.ChildList.Count > 0
                || (programType.GetType() == typeof(ProgramInterface) && (((ProgramInterface)programType).SubClasses.Count > 0 || ((ProgramInterface)programType).SuperClasses.Count > 0))
                || (programType.GetType() == typeof(ProgramClass) && (((ProgramClass)programType).SubClasses.Count > 0 || ((ProgramClass)programType).SuperClasses.Count > 0
                || ((ProgramClass)programType).OwnedByClasses.Count > 0 || ((ProgramClass)programType).OwnedClasses.Count > 0
                || ((ProgramClass)programType).UsedByClasses.Count > 0 || ((ProgramClass)programType).UsedClasses.Count > 0)))
                    GetTabs(ref line);

            this.WriteClosingTag(programType);
        }

        private void PrintProgramFile(ProgramFile programFile)
        {
            Console.Write("----------------------------------------------------------------------\n");
            Console.Write("File: " + programFile.Name);
            Console.Write("\n----------------------------------------------------------------------\n");
        }

        private void PrintProgramNamespace(ProgramNamespace programNamespace)
        {
            Console.Write("Namespace: " + programNamespace.Name);
            tabs++;
        }

        private void PrintProgramClass(ProgramClass programClass)
        {
            Console.Write("Class: " + programClass.Name);
            tabs++;
        }

        private void PrintProgramInterface(ProgramInterface programInterface)
        {
            Console.Write("Interface: " + programInterface.Name);
            tabs++;
        }

        private void PrintProgramFunction(ProgramFunction programFunction)
        {
            Console.Write("Function: " + programFunction.Name);
            tabs++;
        }

        private void WriteProgramFile(ProgramFile programFile)
        {
            line.Append("<file name = \"");
            line.Append(programFile.Name);
            line.Append("\">");
            tabs++;
        }

        private void WriteProgramNamespace(ProgramNamespace programNamespace)
        {
            line.Append("<namespace name = \"");
            line.Append(programNamespace.Name);
            line.Append("\">");
            tabs++;
        }

        private void WriteProgramClass(ProgramClass programClass)
        {
            line.Append("<class name = \"");
            line.Append(programClass.Name);
            line.Append("\">");
            tabs++;
        }

        private void WriteProgramInterface(ProgramInterface programInterface)
        {
            line.Append("<interface name = \"");
            line.Append(programInterface.Name);
            line.Append("\">");
            tabs++;
        }

        private void WriteProgramFunction(ProgramFunction programFunction)
        {
            line.Append("<function name = \"");
            line.Append(programFunction.Name);
            line.Append("\">");
            tabs++;
        }

        private void PrintFunctionAnalysisData(ProgramFunction programFunction)
        {
            tabs++;
            PrintTabs();
            Console.Write(">---> Size: " + programFunction.Size);
            PrintTabs();
            Console.Write(">---> Complexity: " + programFunction.Complexity);
            tabs--;
        }

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

        private void PrintInterfaceAnalysisData(ProgramInterface programInterface)
        {
            this.PrintInheritanceData(programInterface);
            tabs--;
        }

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

        private void WriteFunctionAnalysisData(ProgramFunction programFunction)
        {
            GetTabs(ref line);
            line.Append("<size>");
            line.Append(programFunction.Size);
            line.Append("</size>");

            GetTabs(ref line);
            line.Append("<complexity>");
            line.Append(programFunction.Complexity);
            line.Append("</complexity>");
        }

        private void WriteClassAnalysisData(ProgramClass programClass)
        {
            this.WriteInheritanceData(programClass);

            if (programClass.OwnedByClasses.Count > 0) // Aggregation, parents
                foreach (ProgramClassType ownerclass in programClass.OwnedByClasses)
                {
                    GetTabs(ref line);
                    line.Append("<aggregation_parent>");
                    line.Append(ownerclass.Name);
                    line.Append("</aggregation_parent>");
                }

            if (programClass.OwnedClasses.Count > 0) // Aggregation, children
                foreach (ProgramClassType ownedclass in programClass.OwnedClasses)
                {
                    GetTabs(ref line);
                    line.Append("<aggregation_child>");
                    line.Append(ownedclass.Name);
                    line.Append("</aggregation_child>");
                }

            if (programClass.UsedByClasses.Count > 0) // Using, parents
                foreach (ProgramClassType userclass in programClass.UsedByClasses)
                {
                    GetTabs(ref line);
                    line.Append("<using_parent>");
                    line.Append(userclass.Name);
                    line.Append("</using_parent>");
                }

            if (programClass.UsedClasses.Count > 0) // Using, children
                foreach (ProgramClassType usedclass in programClass.UsedClasses)
                {
                    GetTabs(ref line);
                    line.Append("<using_child>");
                    line.Append(usedclass.Name);
                    line.Append("</using_child>");
                }
        }

        private void WriteInterfaceAnalysisData(ProgramInterface programInterface)
        {
            this.WriteInheritanceData(programInterface);
        }

        private void WriteInheritanceData(ProgramClassType programClassType)
        {
            if (programClassType.SuperClasses.Count > 0) // Inheritance, parents
                foreach (ProgramClassType superclass in programClassType.SuperClasses)
                {
                    GetTabs(ref line);
                    line.Append("<inheritance_parent>");
                    line.Append(superclass.Name);
                    line.Append("</inheritance_parent>");
                }

            if (programClassType.SubClasses.Count > 0) // Inheritance, children
                foreach (ProgramClassType subclass in programClassType.SubClasses)
                {
                    GetTabs(ref line);
                    line.Append("<inheritance_child>");
                    line.Append(subclass.Name);
                    line.Append("</inheritance_child>");
                }
        }

        private void WriteClosingTag(ProgramType programType)
        {
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

        /* Sets the filename and filepath to the specified path for the new XML file */
        private string NewFilePath(string directoryPath, string directoryName, bool printRelationships)
        {
            if (printRelationships)
                return directoryPath + "\\" + directoryName + "_relationships.xml";
            else
                return directoryPath + "\\" + directoryName + "_functions.xml";
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