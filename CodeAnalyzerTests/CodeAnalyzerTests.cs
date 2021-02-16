using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class CodeAnalyzerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            /* Test code from ClassRelationships demo code, by Dr. Fawcett and Dr. Kaya */
            string inputFileText = "\t/////////////////////////////////////////////////////////////////////"
                + "\n\t// class A will be aggregated by class B"
                + "\n\t//"
                + "\n\tpublic class A"
                + "\n\t{"
                + "\n\t\tpublic A() { Console.Write(\"\\n A Constructed\"); }"
                + "\n\t\t~A() { Console.Write(\"\\n A destroyed\"); }"
                + "\n\t\tpublic void say()"
                + "\n\t\t{"
                + "\n\t\t\tConsole.Write(\"\\n A here\");"
                + "\n\t\t\tConsole.Write(\"\\n A {0}\", showType.eval(this));"
                + "\n\t\t}"
                + "\n\t}";
        }
    }
}