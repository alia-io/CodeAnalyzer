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
        public void TestInputReader_FormatInput_1()
        {
            string[] input = { "/X", ".", "*.cs", "/s" };
            string[] expectedFormattedInput = { "/S", "", "/X", Path.GetFullPath("."), "*.cs" };
            string[] actualFormattedInput;

            InputReader inputReader = new InputReader();
            inputReader.FormatInput(input);
            actualFormattedInput = inputReader.FormattedInput;

            CollectionAssert.AreEquivalent(expectedFormattedInput, actualFormattedInput);
        }

        [TestMethod]
        public void TestInputReader_FormatInput_2()
        {
            string[] input = { "/s", "/r", "/X", "*.txt", "." };
            string[] expectedFormattedInput = { "/S", "/R", "/X", Path.GetFullPath("."), "*.txt" };
            string[] actualFormattedInput;

            InputReader inputReader = new InputReader();
            inputReader.FormatInput(input);
            actualFormattedInput = inputReader.FormattedInput;

            CollectionAssert.AreEquivalent(expectedFormattedInput, actualFormattedInput);
        }

        [TestMethod]
        public void TestInputReader_FormatInput_3()
        {
            string[] input = { "*.cs", "C:" };
            string[] expectedFormattedInput = { "", "", "", "C:\\", "*.cs" };
            string[] actualFormattedInput;

            InputReader inputReader = new InputReader();
            inputReader.FormatInput(input);
            actualFormattedInput = inputReader.FormattedInput;

            CollectionAssert.AreEquivalent(expectedFormattedInput, actualFormattedInput);
        }
    }
}
