using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{

    /*
     * Compile and run in command line:
     * Tools > Command Line > Developer Command Prompt
     * csc /target:exe /out:CodeAnalyzer.exe *.cs
     * CodeAnalyzer.exe [args]
     */

    class Launcher
    {
        static void Main(string[] args)
        {
            ProgramExecutor programExecutor = new ProgramExecutor();
            programExecutor.ExecuteProgram(args);
        }
    }

    public class ProgramExecutor
    {
        private InputSessionData inputSessionData;
        private CodeAnalysisData codeAnalysisData;

        public ProgramExecutor()
        {
            this.inputSessionData = new InputSessionData();
            this.codeAnalysisData = new CodeAnalysisData();
        }

        public void ExecuteProgram(string[] args)
        {
            /* ---------------------- Reading and Setting the Input ------------------------- */

            InputReader inputReader = new InputReader();

            // parse the input
            if (!inputReader.FormatInput(args)) // if input is invalid
            {
                Console.WriteLine("\nProgram could not be executed. " + inputReader.ErrorMessage);
                return;
            }

            // set the session data - /S, /R, /X options and directory path
            this.inputSessionData.SetData(inputReader.FormattedInput);

            /* Debugging */
            Console.WriteLine("\nInput Session Data:" +
                "\nIncluding Subdirectories: " + this.inputSessionData.includeSubdirectories +
                "\nIncluding Relationships: " + this.inputSessionData.setRelationshipData +
                "\nPrint to XML: " + this.inputSessionData.printToXml +
                "\nDirectory Path: " + this.inputSessionData.directoryPath);
            /* End of debugging */

            // new thread to enqueue files?

        }

    }
}
