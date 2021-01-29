using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class CodeAnalysisData
    {
        List<File> processedFiles;
        List<Class> classes;

        public CodeAnalysisData()
        {
            this.processedFiles = new List<File>();
            this.classes = new List<Class>();
        }
    }

    public class InputSessionData
    {
        private Queue<string> fileQueue; // full file paths
        public bool includeSubdirectories { get; set; }
        public bool setRelationshipData { get; set; }
        public bool printToXml { get; set; }
        public string directoryPath { get; set; }

        public InputSessionData()
        {
            this.fileQueue = new Queue<string>();
            this.includeSubdirectories = false;
            this.setRelationshipData = false;
            this.printToXml = false;
        }

        public void SetData(string[] input)
        {
            if (input[0].Equals("/S")) this.includeSubdirectories = true;
            if (input[1].Equals("/R")) this.setRelationshipData = true;
            if (input[2].Equals("/X")) this.printToXml = true;
            this.directoryPath = input[3];
        }
    }
}
