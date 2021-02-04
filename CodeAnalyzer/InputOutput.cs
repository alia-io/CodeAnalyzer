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
            string directoryPath = "";

            try
            {
                directoryPath = Path.GetFullPath(arg);
            }
            catch (Exception)
            {
                return false;
            }

            if (Directory.Exists(directoryPath))
            {
                if (this.FormattedInput[3].Equals(""))
                {
                    this.FormattedInput[3] = directoryPath;
                    return true;
                }
                return false;
            }

            return false;
        }
    }

    class OutputWriter
    {
        private int tabs = 0;

        public void PrintToStandardOutput(List<ProgramFile> processedFileList, bool printRelationshipData)
        {
            foreach (ProgramFile programFile in processedFileList)
            {
                this.NavigateProgramTypes_StdOut(programFile, printRelationshipData);
            }
        }

        public void PrintToFile(List<ProgramFile> processedFileList, bool printRelationshipData)
        {

        }

        private void NavigateProgramTypes_StdOut(ProgramType programType, bool printRelationshipData)
        {
            Console.Write("\n\n");
            for (int i = 0; i < tabs; i++)
                Console.Write("\t");

            /* ---------- Print out the programType information ---------- */
            if (programType.GetType() == typeof(ProgramFile))
            {
                Console.WriteLine("File: " + programType.Name);
                /*for (int i = 0; i < tabs; i++)
                    Console.Write("\t");
                Console.Write("\t- FilePath: " + ((ProgramFile)programType).FilePath);*/
            }
            else if (programType.GetType() == typeof(ProgramNamespace))
            {
                Console.WriteLine("Namespace: " + programType.Name);
            }
            else if (programType.GetType() == typeof(ProgramClass))
            {
                Console.WriteLine("Class: " + programType.Name);
                /*for (int i = 0; i < tabs; i++)
                    Console.Write("\t");
                Console.Write("\t- Modifiers: " + ((ProgramClass)programType).Modifiers);*/
            }
            else if (programType.GetType() == typeof(ProgramFunction))
            {
                Console.WriteLine("Function: " + programType.Name);
                /*for (int i = 0; i < tabs; i++)
                    Console.Write("\t");
                Console.Write("\t- Signature: " + ((ProgramFunction)programType).Modifiers 
                    + " " + ((ProgramFunction)programType).ReturnType 
                    + " " + programType.Name
                    + ((ProgramFunction)programType).Parameters
                    + " " + ((ProgramFunction)programType).BaseParameters);
                Console.Write("\n");
                for (int i = 0; i < tabs; i++)
                    Console.Write("\t");
                Console.Write("\t- Size: " + ((ProgramFunction)programType).Size);
                Console.Write("\n");
                for (int i = 0; i < tabs; i++)
                    Console.Write("\t");
                Console.Write("\t- Complexity: " + ((ProgramFunction)programType).Complexity);*/
            }

            /* ---------- Repeat with child data ---------- */
            if (programType.ChildList.Count > 0)
            {
                tabs++;
                foreach (ProgramType child in programType.ChildList)
                {
                    this.NavigateProgramTypes_StdOut(child, printRelationshipData);
                }
                tabs--;
            }
        }

    }
}
