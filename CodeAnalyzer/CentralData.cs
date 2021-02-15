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
    class CodeAnalysisData
    {
        public List<ProgramFile> ProcessedFiles { get; } // List of file objects with all subtypes
        public ProgramClassTypeCollection ProgramClassTypes { get; } // Collection of all classes and interfaces in all files

        public CodeAnalysisData()
        {
            this.ProcessedFiles = new List<ProgramFile>();
            this.ProgramClassTypes = new ProgramClassTypeCollection();
        }
    }

    /* Stores the directory,  */
    public class InputSessionData
    {
        public string DirectoryPath { get; set; }
        private string fileType;
        public Queue<ProgramFile> FileQueue { get; set; }
        public bool IncludeSubdirectories { get; set; }
        public bool SetRelationshipData { get; set; }
        public bool PrintToXml { get; set; }

        public InputSessionData()
        {
            this.FileQueue = new Queue<ProgramFile>();
            this.IncludeSubdirectories = false;
            this.SetRelationshipData = false;
            this.PrintToXml = false;
        }

        public void SetInputSessionData(string[] input)
        {
            this.DirectoryPath = input[3];

            if (input[0].Equals("/S"))
            {
                this.IncludeSubdirectories = true;
            }
            
            if (input[1].Equals("/R"))
            {
                this.SetRelationshipData = true;
            }
            
            if (input[2].Equals("/X"))
            {
                this.PrintToXml = true;
            }

            if (input[4].Equals("*.cs") || input[4].Equals("*.txt"))
                this.fileType = input[4];
                
        }

        public void EnqueueFiles()
        {
            string[] filePaths;

            if (this.fileType.Equals("*.cs") || this.fileType.Equals("*.txt"))
            {
                if (this.IncludeSubdirectories)
                    filePaths = Directory.GetFiles(this.DirectoryPath, this.fileType, SearchOption.AllDirectories);
                else
                    filePaths = Directory.GetFiles(this.DirectoryPath, this.fileType, SearchOption.TopDirectoryOnly);

                foreach (string filePath in filePaths)
                {
                    string[] filePathArray = filePath.Split('\\');
                    string fileName = filePathArray[filePathArray.Length - 1];
                    this.FileQueue.Enqueue(new ProgramFile(filePath, fileName, File.ReadAllText(filePath)));
                }
            }
        }
    }
}
