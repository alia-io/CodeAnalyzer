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
    }

    public class InputSessionData
    {

        private bool includeSubdirectories;
        private bool includeRelationships;
        private bool printToXml;
        private string directoryPath;
        private Queue<File> fileQueue;

        // returns true if successful
        // returns false if invalid arguments
        public bool SetSessionData(string[] args)
        {
            
            if (args.Length < 1)
            {
                Console.WriteLine("Needs more command line arguments.");
                return false;
            }

            this.includeSubdirectories = false;
            this.includeRelationships = false;
            this.printToXml = false;
            this.directoryPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (!DetectArgument(args[i]))
                {
                    return false;
                }
            }

            if (this.directoryPath == null)
            {
                Console.WriteLine("No valid directory or file path detected.");
                return false;
            }

            return true;
        }

        // returns false if invalid argument
        // sets appropriate field and returns true otherwise
        private bool DetectArgument(string arg)
        {
            if (arg.ToLower().Equals("/s") && !this.includeSubdirectories)
            {
                return (this.includeSubdirectories = true);
            }

            if (arg.ToLower().Equals("/r") && !this.includeRelationships)
            {
                return (this.includeRelationships = true);
            }

            if (arg.ToLower().Equals("/x") && !this.printToXml)
            {
                return (this.printToXml = true);
            }

            // TODO: check filepath & set directoryPath

            Console.WriteLine("Invalid argument.");
            return false;
        }

        // TODO
        // returns false if there are no .cs files in the filepath
        // returns true otherwise
        public bool EnqueueFiles()
        {

            return false;
        }

    }
}
