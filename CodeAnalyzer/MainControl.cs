﻿using System;
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

            /* -------------------- Reading and Analyzing the Files -------------------- */

            while (this.inputSessionData.FileQueue.Count > 0)
            {
                FileProcessor fileProcessor = new FileProcessor();
                fileProcessor.ProcessFile(this.codeAnalysisData, this.inputSessionData.FileQueue.Dequeue(), this.inputSessionData.SetRelationshipData);
            }

            /* test 
            Console.Write("\n|");
            foreach (string text in codeAnalysisData.ProcessedFiles[0].FileTextData)
            {
                Console.Write(" " + text + " |");
            }
            Console.Write("\n\n\n");
            /* end of test */

            /* -------------------- Reading and Analyzing the Relationship Data -------------------- */
            if (this.inputSessionData.SetRelationshipData)
            {
                // TODO ......
            }

            /* -------------------- Printing the Output Data -------------------- */
            OutputWriter outputWriter = new OutputWriter();
            if (this.inputSessionData.PrintToXml)
            {
                outputWriter.PrintToFile(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.SetRelationshipData);
            }
            else
            {
                outputWriter.PrintToStandardOutput(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.SetRelationshipData);
            }
        }

    }
}
