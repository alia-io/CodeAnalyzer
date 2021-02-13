using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /* Tests for file CodeAnalyzer.cs */
    public static class CodeAnalyzerTests
    {
        /* Tests for class FileProcessor */
        public static class FileProcessorTests
        {
            /* Test for function SetFileTextData - Prints text data from file to read */
            public static void PrintFileTextData(ProgramFile programFile)
            {
                Console.WriteLine("\nFile Name: " + programFile.Name);
                Console.Write("\n\n| ");
                foreach (string entry in programFile.FileTextData)
                    Console.Write(entry + " | ");
                Console.Write("\n\n");
            }
        }

        /* Tests for class CodeProcessor */
        public static class CodeProcessorTests
        {
            /* Test for functions ProcessProgramClassTypeData, ProcessFunctionData, NewClass, NewInterface, 
             * RemoveFunctionSignatureFromTextData - Prints text data from class, interface, or function to read */
            public static void PrintSubtypeTextData(ProgramClassType programClassType)
            {
                if (programClassType.GetType() == typeof(ProgramClass))
                    Console.WriteLine("\nClass Name: " + programClassType.Name);
                else if (programClassType.GetType() == typeof(ProgramInterface))
                    Console.WriteLine("\nInterface Name: " + programClassType.Name);
                else
                    Console.WriteLine("\nFunction Name: " + programClassType.Name);
                Console.Write("\n\n| ");
                foreach (string text in programClassType.TextData)
                    Console.Write(text + " | ");
                Console.Write("\n\n");
            }
        }
    }
}
