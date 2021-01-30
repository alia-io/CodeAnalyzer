using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalyzer
{
    class CodeAnalysisData
    {
        public List<File> ProcessedFiles { get; }
        public ProgramTypeCollection Classes { get; }

        public CodeAnalysisData()
        {
            this.ProcessedFiles = new List<File>();
            this.Classes = new ProgramTypeCollection();
        }
    }

    public class InputSessionData
    {
        public string DirectoryPath { get; set; }
        public Queue<string> FileQueue { get; set; } // file paths
        public bool IncludeSubdirectories { get; set; }
        public bool SetRelationshipData { get; set; }
        public bool PrintToXml { get; set; }

        public InputSessionData()
        {
            this.FileQueue = new Queue<string>();
            this.IncludeSubdirectories = false;
            this.SetRelationshipData = false;
            this.PrintToXml = false;
        }

        public void SetInputSessionData(string[] input)
        {
            string[] filePaths;

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

            if (this.IncludeSubdirectories)
            {
                filePaths = Directory.GetFiles(this.DirectoryPath, "*.cs", SearchOption.AllDirectories);
            }
            else
            {
                filePaths = Directory.GetFiles(this.DirectoryPath, "*.cs", SearchOption.TopDirectoryOnly);
            }

            foreach (string filePath in filePaths)
            {
                FileQueue.Enqueue(filePath);
            }
        }
    }
}
