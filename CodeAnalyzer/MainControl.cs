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
     * 
     * .bat files: create 2 text files
     * compile.bat : 
     * (search for devenv in VS's location)
     * devenv [path to sln] /build debug .compile
     * run.bat: 
     * .\Debug\Project2.exe [flags]
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
        private readonly InputSessionData inputSessionData;
        private readonly CodeAnalysisData codeAnalysisData;

        public ProgramExecutor()
        {
            this.inputSessionData = new InputSessionData();
            this.codeAnalysisData = new CodeAnalysisData();
        }

        public void ExecuteProgram(string[] args)
        {
            /* -------------------- Reading and Setting the Input -------------------- */

            InputReader inputReader = new InputReader();
            int numberOfFiles;

            // parse the input
            if (!inputReader.FormatInput(args)) // if input is invalid
            {
                Console.WriteLine("\nProgram could not be executed with the given input. " + inputReader.ErrorMessage);
                return;
            }

            // set the session data - /S, /R, /X options and directory path
            this.inputSessionData.SetInputSessionData(inputReader.FormattedInput);

            // create and read all the files, and enqueue them on the FileQueue
            this.inputSessionData.EnqueueFiles();
            numberOfFiles = this.inputSessionData.FileQueue.Count();

            /* -------------------- Reading and Analyzing the Files -------------------- */

            /* ---------- 1: Preprocess the file text into a list of logical text entities ---------- */
            for (int i = 0; i < numberOfFiles; i++)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new FileProcessor(programFile).ProcessFile();
                this.inputSessionData.FileQueue.Enqueue(programFile);   // enqueue the file again for secondary processing
            }

            /* ---------- 2: Establish the hierarchy of types and collect function data ---------- */
            while (this.inputSessionData.FileQueue.Count > 0)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new CodeProcessor(programFile, this.codeAnalysisData.ProgramClassTypes).ProcessFileCode();
                this.codeAnalysisData.ProcessedFiles.Add(programFile);   // add file to processed files list
            }

            /* ---------- 3: Collect class relationship data ---------- */
            foreach (ProgramClassType programClassType in this.codeAnalysisData.ProgramClassTypes)
            {
                new RelationshipProcessor(programClassType, this.codeAnalysisData.ProgramClassTypes).ProcessRelationships();
            }

            /* -------------------- Printing the Output Data -------------------- */
            OutputWriter outputWriter = new OutputWriter();
            outputWriter.WriteOutput(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.PrintToXml, this.inputSessionData.SetRelationshipData);

        }

    }
}
