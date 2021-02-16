using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;
using System;
using System.IO;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class InputReaderTests
    {
        [TestMethod]
        public void TestInputReader_FormatInput()
        {
            string[] input = { "/X", ".", "*.cs", "/s" };
            string[] expectedFormattedInput = { "/S", "", "/X", Path.GetFullPath("."), "*.cs" };
            string[] actualFormattedInput;

            InputReader inputReader = new InputReader();
            inputReader.FormatInput(input);
            actualFormattedInput = inputReader.FormattedInput;

            CollectionAssert.AreEquivalent(expectedFormattedInput, actualFormattedInput);
        }
    }

    [TestClass]
    public class OutputWriterTests
    {

    }
}
