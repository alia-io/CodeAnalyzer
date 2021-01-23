using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    static class SessionData
    {

        private static bool includeSubdirectories;
        private static bool includeRelationships;
        private static bool printToXml;
        private static string directoryPath;
        private static Queue<File> fileQueue;

        // returns true if successful
        // returns false if invalid arguments
        public static bool SetSessionData(string[] args)
        {
            
            if (args.Length < 1)
            {
                Console.WriteLine("Needs more command line arguments.");
                return false;
            }

            includeSubdirectories = false;
            includeRelationships = false;
            printToXml = false;
            directoryPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (!DetectArgument(args[i]))
                {
                    return false;
                }
            }

            if (directoryPath == null)
            {
                Console.WriteLine("No valid directory or file path detected.");
                return false;
            }

            return true;
        }

        // returns false if invalid argument
        // sets appropriate field and returns true otherwise
        private static bool DetectArgument(string arg)
        {
            if (arg.ToLower().Equals("/s") && !includeSubdirectories)
            {
                return (includeSubdirectories = true);
            }

            if (arg.ToLower().Equals("/r") && !includeRelationships)
            {
                return (includeRelationships = true);
            }

            if (arg.ToLower().Equals("/x") && !printToXml)
            {
                return (printToXml = true);
            }

            // TODO: check filepath & set directoryPath

            Console.WriteLine("Invalid argument.");
            return false;
        }

        // TODO
        // returns false if there are no .cs files in the filepath
        // returns true otherwise
        public static bool EnqueueFiles()
        {

            return false;
        }

    }
}
