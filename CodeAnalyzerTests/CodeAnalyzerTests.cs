using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;
using System.Collections.Generic;
using System.IO;
using System;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class FileProcessorTests
    {
        [TestMethod]
        public void TestFileProcessor_ProcessFile_1()
        {
            string inputFileText = "public virtual (int x, int y) Move(int direction, int seconds)"
                + "\r\n{"
                + "\r\n\tif (direction == 0) // move north"
                + "\r\n\t\tLocationY += seconds * Speed;"
                + "\r\n\telse if (direction == 1) // move east"
                + "\r\n\t\tLocationX += seconds * Speed;"
                + "\r\n\telse if (direction == 2) // move south"
                + "\r\n\t\tLocationY -= seconds * Speed;"
                + "\r\n\telse if (direction == 3) // move west"
                + "\r\n\t\tLocationX -= seconds * Speed;"
                + "\r\n\treturn (LocationX, LocationY);"
                + "\r\n}";

            string[] expectedFileTextDataArray = { "public", "virtual", "(", "int", "x", ",", "int", "y", ")", "Move", "(", "int", "direction", ",", "int", "seconds", ")",
                " ", "{", " ", "if", "(", "direction", "==", "0", ")", "//", "move", "north", " ", "LocationY", "+=", "seconds", "*", "Speed", ";", " ", "else", "if",
                "(", "direction", "==", "1", ")", "//", "move", "east", " ", "LocationX", "+=", "seconds", "*", "Speed", ";", " ", "else", "if", "(", "direction",
                "==", "2", ")", "//", "move", "south", " ", "LocationY", "-=", "seconds", "*", "Speed", ";", " ", "else", "if", "(", "direction", "==", "3", ")", "//",
                "move", "west", " ", "LocationX", "-=", "seconds", "*", "Speed", ";", " ", "return", "(", "LocationX", ",", "LocationY", ")", ";", " ", "}", " " };

            List<string> expectedFileTextData = new List<string>();
            List<string> actualFileTextData;
            ProgramFile testProgramFile = new ProgramFile("C:\\filepath1", "TestFileName1.cs", inputFileText);

            expectedFileTextData.AddRange(expectedFileTextDataArray);

            FileProcessor fileProcessor = new FileProcessor(testProgramFile);
            fileProcessor.ProcessFile();

            actualFileTextData = testProgramFile.FileTextData;

            CollectionAssert.AreEqual(expectedFileTextData, actualFileTextData);
        }

        [TestMethod]
        public void TestFileProcessor_ProcessFile_2()
        {
            string inputFileText = "for (int j = 0; j < 5; j++)"
                + "\r\n{"
                + "\r\n\tif (pipelinedInstructions[i, j] != 0)"
                + "\r\n\t{"
                + "\r\n\t\tLabel label = FindLabel(ConvertToLabelName(i + 1, pipelinedInstructions[i, j]));"
                + "\r\n\t\tif (label != null)"
                + "\r\n\t\t{"
                + "\r\n\t\t\tswitch (j)"
                + "\r\n\t\t\t{"
                + "\r\n\t\t\t\tcase 0: label.Content = \" IF\"; break;"
                + "\r\n\t\t\t\tcase 1: label.Content = \"ID\"; break;"
                + "\r\n\t\t\t\tcase 2: label.Content = \"EX\"; break;"
                + "\r\n\t\t\t\tcase 3: label.Content = \" M\"; break;"
                + "\r\n\t\t\t\tcase 4: label.Content = \"WB\"; break;"
                + "\r\n\t\t\t}"
                + "\r\n\t\t}"
                + "\r\n\t}"
                + "\r\n}";

        string[] expectedFileTextDataArray = { "for", "(", "int", "j", "=", "0", ";", "j", "<", "5", ";", "j", "++", ")", " ", "{", " ", "if", "(", "pipelinedInstructions",
                "[", "i", ",", "j", "]", "!=", "0", ")", " ", "{", " ", "Label", "label", "=", "FindLabel", "(", "ConvertToLabelName", "(", "i", "+", "1", ",", "pipelinedInstructions",
                "[", "i", ",", "j", "]", ")", ")", ";", " ", "if", "(", "label", "!=", "null", ")", " ", "{", " ", "switch", "(", "j", ")", " ", "{", " ", "case", "0", ":", "label",
                ".", "Content", "=", "\"", "IF", "\"", ";", "break", ";", " ", "case", "1", ":", "label", ".", "Content", "=", "\"", "ID", "\"", ";", "break", ";", " ", "case", "2",
                ":", "label", ".", "Content", "=", "\"", "EX", "\"", ";", "break", ";", " ", "case", "3", ":", "label", ".", "Content", "=", "\"", "M", "\"", ";", "break", ";", " ",
                "case", "4", ":", "label", ".", "Content", "=", "\"", "WB", "\"", ";", "break", ";", " ", "}", " ", "}", " ", "}", " ", "}", " " };

            List<string> expectedFileTextData = new List<string>();
            List<string> actualFileTextData;
            ProgramFile testProgramFile = new ProgramFile("C:\\filepath2", "TestFileName2.cs", inputFileText);

            expectedFileTextData.AddRange(expectedFileTextDataArray);

            FileProcessor fileProcessor = new FileProcessor(testProgramFile);
            fileProcessor.ProcessFile();

            actualFileTextData = testProgramFile.FileTextData;

            CollectionAssert.AreEqual(expectedFileTextData, actualFileTextData);
        }
    }

    [TestClass]
    public class CodeProcessorTests
    {
        [TestMethod]
        public void TestCodeProcessor_ProcessFileCode_1()
        {
            CodeProcessor codeProcessor;
            ProgramFile expectedProgramFile;
            ProgramClassTypeCollection expectedProgramClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassType tempProgramClassType;
            FileProcessor fileProcessorForActual;
            ProgramFile actualProgramFile;
            ProgramClassTypeCollection actualProgramClassTypeCollection = new ProgramClassTypeCollection();

            string filePath = Path.GetFullPath("..\\..\\..\\CodeAnalyzerTests\\TestInputFiles\\TestFile1.cs");
            string fileName = "TestFile1.cs";
            string fileText = "";

            if (File.Exists(filePath))
                fileText = File.ReadAllText(filePath);

            expectedProgramFile = new ProgramFile(filePath, fileName, fileText);
            tempProgramClassType = new ProgramInterface("IAnimalActions", "");
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramInterface("IHumanActions", "");
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Animal", "");
            tempProgramClassType.ChildList.Add(new ProgramFunction("Animal", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Move", "", "", new List<string>(), ""));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Pet", "");
            tempProgramClassType.ChildList.Add(new ProgramFunction("Pet", "", "", new List<string>(), ""));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Dog", "");
            tempProgramClassType.ChildList.Add(new ProgramFunction("Dog", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Talk", "", "", new List<string>(), ""));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Human", "");
            tempProgramClassType.ChildList.Add(new ProgramFunction("Human", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Talk", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Move", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GoToSchool", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GraduateSchool", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GoToWork", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyPet", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyDog", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyCar", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("SellCar", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("FillCarFuelTank", "", "", new List<string>(), ""));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Car", "");
            tempProgramClassType.ChildList.Add(new ProgramFunction("Car", "", "", new List<string>(), ""));
            tempProgramClassType.ChildList.Add(new ProgramFunction("FillTank", "", "", new List<string>(), ""));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);

            actualProgramFile = new ProgramFile(filePath, fileName, fileText);
            fileProcessorForActual = new FileProcessor(actualProgramFile);
            fileProcessorForActual.ProcessFile();
            codeProcessor = new CodeProcessor(actualProgramFile, actualProgramClassTypeCollection);
            codeProcessor.ProcessFileCode();

            CheckAllChildLists(expectedProgramFile, actualProgramFile);
            CollectionAssert.AreEqual(expectedProgramClassTypeCollection, actualProgramClassTypeCollection);
        }

        [TestMethod]
        public void TestCodeProcessor_ProcessFileCode_2()
        {

        }

        public static void CheckAllChildLists(ProgramType expected, ProgramType actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.GetType(), actual.GetType());
            Console.WriteLine("\n");
            Console.WriteLine("For ProgramType: " + expected.Name + " \\\\ " + actual.Name);
            Console.WriteLine("Expected childlist count: " + expected.ChildList.Count);
            Console.WriteLine("Actual childlist count: " + actual.ChildList.Count);
            Console.WriteLine("\n");
            Assert.AreEqual(expected.ChildList.Count, actual.ChildList.Count);

            for (int i = 0; i < expected.ChildList.Count; i++)
                CheckAllChildLists(expected.ChildList[i], actual.ChildList[i]);
        }
    }

    [TestClass]
    public class RelationshipProcessorTests
    {
        [TestMethod]
        public void TestRelationshipProcessor_ProcessRelationships_1()
        {

        }

        [TestMethod]
        public void TestRelationshipProcessor_ProcessRelationships_2()
        {

        }
    }
}
