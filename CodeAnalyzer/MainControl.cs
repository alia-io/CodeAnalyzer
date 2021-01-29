using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{

    /*
     * Compile and run in command line:
     * csc /target.exe /out:CodeAnalyzer.exe *.cs
     * CodeAnalyzer.exe [args]
     */

    static class Launcher
    {
        static void Main(string[] args)
        {
            //if (InputSessionData.SetSessionData(args)) // arguments are valid and set
            //{
                Console.WriteLine("Analyzing your C# files...");
                /* TODO:
                 * Until fileQueue is empty, grab a file from it & process the file
                 */
            //}

        }
    }

    public class ProgramExecutor
    {
        private InputSessionData inputSessionData;
        private CodeAnalysisData codeAnalysisData;
    }
}
