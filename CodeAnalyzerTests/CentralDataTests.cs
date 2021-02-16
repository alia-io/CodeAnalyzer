using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;
using System.IO;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class InputSessionDataTests
    {
        [TestMethod]
        public void TestInputSessionData_SetInputSessionData_1()
        {
            string[] input = { "/S", "/R", "/X", "D:\\CSE-681_CodeAnalyzer\\CodeAnalyzer", "*.cs" };
            string expectedDirectoryPath = "D:\\CSE-681_CodeAnalyzer\\CodeAnalyzer";
            string expectedFileType = "*.cs";
            bool expectedIncludeSubdirectories = true;
            bool expectedSetRelationshipData = true;
            bool expectedPrintToXml = true;
            string actualDirectoryPath;
            string actualFileType;
            bool actualIncludeSubdirectories;
            bool actualSetRelationshipData;
            bool actualPrintToXml;

            InputSessionData inputSessionData = new InputSessionData();
            inputSessionData.SetInputSessionData(input);

            actualDirectoryPath = inputSessionData.DirectoryPath;
            actualFileType = inputSessionData.FileType;
            actualIncludeSubdirectories = inputSessionData.IncludeSubdirectories;
            actualSetRelationshipData = inputSessionData.SetRelationshipData;
            actualPrintToXml = inputSessionData.PrintToXml;

            Assert.AreEqual(expectedDirectoryPath, actualDirectoryPath);
            Assert.AreEqual(expectedFileType, actualFileType);
            Assert.AreEqual(expectedIncludeSubdirectories, actualIncludeSubdirectories);
            Assert.AreEqual(expectedSetRelationshipData, actualSetRelationshipData);
            Assert.AreEqual(expectedPrintToXml, actualPrintToXml);
        }

        [TestMethod]
        public void TestInputSessionData_SetInputSessionData_2()
        {
            string[] input = { "", "/R", "/X", "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\Assets", "*.txt" };
            string expectedDirectoryPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\Assets";
            string expectedFileType = "*.txt";
            bool expectedIncludeSubdirectories = false;
            bool expectedSetRelationshipData = true;
            bool expectedPrintToXml = true;
            string actualDirectoryPath;
            string actualFileType;
            bool actualIncludeSubdirectories;
            bool actualSetRelationshipData;
            bool actualPrintToXml;

            InputSessionData inputSessionData = new InputSessionData();
            inputSessionData.SetInputSessionData(input);

            actualDirectoryPath = inputSessionData.DirectoryPath;
            actualFileType = inputSessionData.FileType;
            actualIncludeSubdirectories = inputSessionData.IncludeSubdirectories;
            actualSetRelationshipData = inputSessionData.SetRelationshipData;
            actualPrintToXml = inputSessionData.PrintToXml;

            Assert.AreEqual(expectedDirectoryPath, actualDirectoryPath);
            Assert.AreEqual(expectedFileType, actualFileType);
            Assert.AreEqual(expectedIncludeSubdirectories, actualIncludeSubdirectories);
            Assert.AreEqual(expectedSetRelationshipData, actualSetRelationshipData);
            Assert.AreEqual(expectedPrintToXml, actualPrintToXml);
        }

        [TestMethod]
        public void TestInputSessionData_SetInputSessionData_3()
        {
            string[] input = { "", "", "", Path.GetFullPath("."), "*.cs" };
            string expectedDirectoryPath = Path.GetFullPath(".");
            string expectedFileType = "*.cs";
            bool expectedIncludeSubdirectories = false;
            bool expectedSetRelationshipData = false;
            bool expectedPrintToXml = false;
            string actualDirectoryPath;
            string actualFileType;
            bool actualIncludeSubdirectories;
            bool actualSetRelationshipData;
            bool actualPrintToXml;

            InputSessionData inputSessionData = new InputSessionData();
            inputSessionData.SetInputSessionData(input);

            actualDirectoryPath = inputSessionData.DirectoryPath;
            actualFileType = inputSessionData.FileType;
            actualIncludeSubdirectories = inputSessionData.IncludeSubdirectories;
            actualSetRelationshipData = inputSessionData.SetRelationshipData;
            actualPrintToXml = inputSessionData.PrintToXml;

            Assert.AreEqual(expectedDirectoryPath, actualDirectoryPath);
            Assert.AreEqual(expectedFileType, actualFileType);
            Assert.AreEqual(expectedIncludeSubdirectories, actualIncludeSubdirectories);
            Assert.AreEqual(expectedSetRelationshipData, actualSetRelationshipData);
            Assert.AreEqual(expectedPrintToXml, actualPrintToXml);
        }
    }
}
