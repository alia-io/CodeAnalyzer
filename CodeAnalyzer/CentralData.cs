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
        public List<ProgramFile> ProcessedFiles { get; }
        public ProgramObjectTypeCollection ProgramObjectTypes { get; }

        public CodeAnalysisData()
        {
            this.ProcessedFiles = new List<ProgramFile>();
            this.ProgramObjectTypes = new ProgramObjectTypeCollection();
        }

        public void AddFile(ProgramFile file) { this.ProcessedFiles.Add(file); }
        public void AddObject(ProgramClass programClass) { this.ProgramObjectTypes.Add(programClass); }
        public void AddObject(ProgramInterface programInterface) { this.ProgramObjectTypes.Add(programInterface); }
        public void AddObject(ProgramStruct programStruct) { this.ProgramObjectTypes.Add(programStruct); }
        public void AddObject(ProgramEnum programEnum) { this.ProgramObjectTypes.Add(programEnum); }
    }

    public class InputSessionData
    {
        public string DirectoryPath { get; set; }
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
        }

        public void EnqueueFiles()
        {
            string[] filePaths;

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
                string[] filePathArray = filePath.Split('/');
                string fileName = filePathArray[filePathArray.Length - 1];
                this.FileQueue.Enqueue(new ProgramFile(filePath, fileName, File.ReadAllText(filePath)));
            }
        }
    }
}
