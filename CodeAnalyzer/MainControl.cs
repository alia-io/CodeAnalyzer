/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  MainControl.cs - Launches application, controls the flow of execution            ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

/* 
 * This application can be used to analyze C# code by reading files from a given directory.
 * By default, files of the given type (*.cs or *.txt) within the top directory are read,
 * and the sizes and complexities of all functions and methods are printed to the console.
 * Additional optional arguments can be given to alter application behavior.
 * 
 * Compile and run using Windows command prompt.
 *      - Compile using compile.bat
 *      - Run application using preset commands using run.bat
 *      
 *      - Run analyze your own code:
 *          .\bin\Release\CodeAnalyzer.exe [directory path] [file type] [optional args]
 *      
 *          - Arguments can be given in any order
 *          - Directory path must not contain any whitespace, unless it is enclosed in "quotation marks"
 *          - File type must be must be specified using one of the following options: *.cs or *.txt
 *          - Additional optional arguments supported (not case-sensitive):
 *              /S - search all subdirectories for files of specified type, in addition to specified directory
 *              /R - collect analysis data on class/interface relationships, instead of function size and complexity
 *              /X - in addition to printing data to terminal, save XML-formatted data in an XML file
 */

using System;
using System.Linq;

namespace CodeAnalyzer
{
    /* Entry point into the application */
    class Launcher
    {
        static void Main(string[] args)
        {
            // Create a new executor and start executing
            ProgramExecutor programExecutor = new ProgramExecutor();
            programExecutor.ExecuteProgram(args);
        }
    }

    /* Controls the flow of execution through the application */
    public class ProgramExecutor
    {
        // Central data objects
        private readonly InputSessionData inputSessionData;
        private readonly CodeAnalysisData codeAnalysisData;

        public ProgramExecutor()
        {
            this.inputSessionData = new InputSessionData();
            this.codeAnalysisData = new CodeAnalysisData();
        }

        /* Creates the main executive objects to perform all major tasks and activities, limiting access to central data */
        public void ExecuteProgram(string[] args)
        {
            InputReader inputReader = new InputReader();
            OutputWriter outputWriter = new OutputWriter();
            int numberOfFiles;

            /* 1: Parse the input arguments */
            if (!inputReader.FormatInput(args)) // If input is invalid, terminate the program
            {
                Console.WriteLine("\nProgram could not be executed with the given input." + inputReader.ErrorMessage);
                return;
            }

            /* 2: Set the session data from input: directory path, file type, and options /S, /R, /X */
            this.inputSessionData.SetInputSessionData(inputReader.FormattedInput);

            /* 3: Create file objects and read all files, enqueue them on the FileQueue */
            this.inputSessionData.EnqueueFiles();
            numberOfFiles = this.inputSessionData.FileQueue.Count();

            /* 4: Pre-process the text from each file on the FileQueue into lists of logical "words" */
            for (int i = 0; i < numberOfFiles; i++)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new FileProcessor(programFile).ProcessFile();
                this.inputSessionData.FileQueue.Enqueue(programFile);   // Enqueue the file again for secondary processing
            }

            /* 5: Use pre-processed text to establish the hierarchy of types and collect function data for each file on FileQueue */
            while (this.inputSessionData.FileQueue.Count > 0)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new CodeProcessor(programFile, this.codeAnalysisData.ProgramClassTypes).ProcessFileCode();
                this.codeAnalysisData.ProcessedFiles.Add(programFile);   // Add the file to the list of processed files
            }

            /* 6: Collect relationship data for each class and interface */
            foreach (ProgramClassType programClassType in this.codeAnalysisData.ProgramClassTypes)
            {
                new RelationshipProcessor(programClassType, this.codeAnalysisData.ProgramClassTypes).ProcessRelationships();
            }

            /* 7: Print the requested code analysis data to standard output and/or XML file */
            outputWriter.WriteOutput(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.DirectoryPath, this.inputSessionData.FileType, this.inputSessionData.PrintToXml, this.inputSessionData.SetRelationshipData);
        }
    }
}
