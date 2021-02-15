/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  CentralData.cs - Defines and manages application session and program state data  ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace CodeAnalyzer
{
    /* Stores all data needed for code analysis and output writing */
    public class CodeAnalysisData
    {
        public List<ProgramFile> ProcessedFiles { get; } // List of file objects with all subtypes
        public ProgramClassTypeCollection ProgramClassTypes { get; } // Collection of all classes and interfaces in all files

        public CodeAnalysisData()
        {
            this.ProcessedFiles = new List<ProgramFile>();
            this.ProgramClassTypes = new ProgramClassTypeCollection();
        }
    }

    /* Stores the directory path, filetype, unprocessed file queue, and optional arguments */
    public class InputSessionData
    {
        public string DirectoryPath { get; private set; }
        public string FileType { get; private set; }
        public Queue<ProgramFile> FileQueue { get; private set; }
        public bool IncludeSubdirectories { get; private set; }
        public bool SetRelationshipData { get; private set; }
        public bool PrintToXml { get; private set; }

        public InputSessionData()
        {
            this.FileQueue = new Queue<ProgramFile>();
            this.IncludeSubdirectories = false;
            this.SetRelationshipData = false;
            this.PrintToXml = false;
        }

        /* Sets the file type to analyze and the optional settings */
        public void SetInputSessionData(string[] input)
        {
            this.DirectoryPath = input[3];

            if (input[0].Equals("/S"))
                this.IncludeSubdirectories = true;
            
            if (input[1].Equals("/R"))
                this.SetRelationshipData = true;
            
            if (input[2].Equals("/X"))
                this.PrintToXml = true;

            if (input[4].Equals("*.cs") || input[4].Equals("*.txt"))
                this.FileType = input[4];
                
        }

        /* Reads all files, creates and enqueues the ProgramFile objects with their raw text data */
        public void EnqueueFiles()
        {
            string[] filePaths;

            if (this.FileType.Equals("*.cs") || this.FileType.Equals("*.txt"))
            {
                if (this.IncludeSubdirectories)
                    filePaths = Directory.GetFiles(this.DirectoryPath, this.FileType, SearchOption.AllDirectories);
                else
                    filePaths = Directory.GetFiles(this.DirectoryPath, this.FileType, SearchOption.TopDirectoryOnly);

                foreach (string filePath in filePaths) // Read and enqueue all files
                {
                    string[] filePathArray = filePath.Split('\\');
                    string fileName = filePathArray[filePathArray.Length - 1];
                    this.FileQueue.Enqueue(new ProgramFile(filePath, fileName, File.ReadAllText(filePath)));
                }
            }
        }
    }
}
