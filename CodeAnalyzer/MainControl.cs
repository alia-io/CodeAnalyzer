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
            /* -------------------- Reading and Setting the Input -------------------- */

            InputReader inputReader = new InputReader();

            // parse the input
            if (!inputReader.FormatInput(args)) // if input is invalid
            {
                Console.WriteLine("\nProgram could not be executed with the given input. " + inputReader.ErrorMessage);
                return;
            }

            // set the session data - /S, /R, /X options and directory path
            // TODO: maybe add a new thread here to enqueue files?
            this.inputSessionData.SetInputSessionData(inputReader.FormattedInput);

            /* Debugging 
            Console.WriteLine("\n---------- Input Session Data ----------" +
                "\nIncluding Subdirectories: " + this.inputSessionData.includeSubdirectories +
                "\nIncluding Relationships: " + this.inputSessionData.setRelationshipData +
                "\nPrint to XML: " + this.inputSessionData.printToXml +
                "\nDirectory Path: " + this.inputSessionData.directoryPath +
                "\n\nFiles in Queue:");
            foreach (string filePath in this.inputSessionData.fileQueue)
            {
                Console.WriteLine("\n    " + filePath);
            }
             End of debugging */

            /* -------------------- Reading and Analyzing the Files -------------------- */

            //while (this.inputSessionData.FileQueue.Count > 0)
            //{
                FileProcessor fileProcessor = new FileProcessor();
                fileProcessor.ProcessFile(this.inputSessionData.FileQueue.Dequeue(), this.inputSessionData.SetRelationshipData);
                this.codeAnalysisData.ProcessedFiles.Add(fileProcessor.FileData);
                foreach (Class programClass in fileProcessor.ClassList)
                {
                    this.codeAnalysisData.Classes.Add(programClass);
                }
            //}

            /* Test */



            /* End of test */


        }

    }
}
