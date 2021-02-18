using CodeAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

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
        public void TestCodeProcessor_ProcessFileCode_Structure()
        {
            CodeProcessor codeProcessor;
            ProgramFile expectedProgramFile;
            ProgramClassTypeCollection expectedProgramClassTypeCollection = new ProgramClassTypeCollection();
            FileProcessor fileProcessorForActual;
            ProgramFile actualProgramFile;
            ProgramClassTypeCollection actualProgramClassTypeCollection = new ProgramClassTypeCollection();

            ProgramClassType tempProgramClassType;
            List<string> empty = new List<string>();

            string filePath = Path.GetFullPath("..\\..\\..\\CodeAnalyzerTests\\TestInputFiles\\TestInputFile.cs");
            string fileName = "TestInputFile.cs";
            string fileText = "";

            if (File.Exists(filePath))
                fileText = File.ReadAllText(filePath);

            expectedProgramFile = new ProgramFile(filePath, fileName, fileText);
            tempProgramClassType = new ProgramInterface("IAnimalActions", empty, empty);
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramInterface("IHumanActions", empty, empty);
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Animal", empty, empty);
            tempProgramClassType.ChildList.Add(new ProgramFunction("Animal", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Move", empty, empty, empty, empty, empty));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Pet", empty, empty);
            tempProgramClassType.ChildList.Add(new ProgramFunction("Pet", empty, empty, empty, empty, empty));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Dog", empty, empty);
            tempProgramClassType.ChildList.Add(new ProgramFunction("Dog", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Talk", empty, empty, empty, empty, empty));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Human", empty, empty);
            tempProgramClassType.ChildList.Add(new ProgramFunction("Human", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Talk", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("Move", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GoToSchool", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GraduateSchool", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("GoToWork", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyPet", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyDog", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("BuyCar", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("SellCar", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("FillCarFuelTank", empty, empty, empty, empty, empty));
            expectedProgramFile.ChildList.Add(tempProgramClassType);
            expectedProgramClassTypeCollection.Add(tempProgramClassType);
            tempProgramClassType = new ProgramClass("Car", empty, empty);
            tempProgramClassType.ChildList.Add(new ProgramFunction("Car", empty, empty, empty, empty, empty));
            tempProgramClassType.ChildList.Add(new ProgramFunction("FillTank", empty, empty, empty, empty, empty));
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

        public static void CheckAllChildLists(ProgramType expected, ProgramType actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.GetType(), actual.GetType());
            Assert.AreEqual(expected.ChildList.Count, actual.ChildList.Count);

            for (int i = 0; i < expected.ChildList.Count; i++)
                CheckAllChildLists(expected.ChildList[i], actual.ChildList[i]);
        }

        [TestMethod]
        public void TestCodeProcessor_ProcessFileCode_FunctionData()
        {
            ProgramClassTypeCollection programClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor;
            FileProcessor fileProcessor;
            ProgramFile programFile;

            List<ProgramFunction> expectedFunctions = new List<ProgramFunction>();
            List<string> repeatedFunctionNames = new List<string>();
            List<string> empty = new List<string>();

            string filePath = Path.GetFullPath("..\\..\\..\\CodeAnalyzerTests\\TestInputFiles\\TestInputFile.cs");
            string fileName = "TestInputFile.cs";
            string fileText = "";

            if (File.Exists(filePath))
                fileText = File.ReadAllText(filePath);

            repeatedFunctionNames.Add("Talk");
            repeatedFunctionNames.Add("Move");

            ProgramFunction Animal = new ProgramFunction("Animal", empty, empty, empty, empty, empty);
            ProgramFunction Animal_Move = new ProgramFunction("Move", empty, empty, empty, empty, empty);
            ProgramFunction Pet = new ProgramFunction("Pet", empty, empty, empty, empty, empty);
            ProgramFunction Dog = new ProgramFunction("Dog", empty, empty, empty, empty, empty);
            ProgramFunction Dog_Talk = new ProgramFunction("Talk", empty, empty, empty, empty, empty);
            ProgramFunction Human = new ProgramFunction("Human", empty, empty, empty, empty, empty);
            ProgramFunction Human_Talk = new ProgramFunction("Talk", empty, empty, empty, empty, empty);
            ProgramFunction Human_Move = new ProgramFunction("Move", empty, empty, empty, empty, empty);
            ProgramFunction GoToSchool = new ProgramFunction("GoToSchool", empty, empty, empty, empty, empty);
            ProgramFunction GraduateSchool = new ProgramFunction("GraduateSchool", empty, empty, empty, empty, empty);
            ProgramFunction GoToWork = new ProgramFunction("GoToWork", empty, empty, empty, empty, empty);
            ProgramFunction BuyPet = new ProgramFunction("BuyPet", empty, empty, empty, empty, empty);
            ProgramFunction BuyDog = new ProgramFunction("BuyDog", empty, empty, empty, empty, empty);
            ProgramFunction BuyCar = new ProgramFunction("BuyCar", empty, empty, empty, empty, empty);
            ProgramFunction SellCar = new ProgramFunction("SellCar", empty, empty, empty, empty, empty);
            ProgramFunction FillCarFuelTank = new ProgramFunction("FillCarFuelTank", empty, empty, empty, empty, empty);
            ProgramFunction Car = new ProgramFunction("Car", empty, empty, empty, empty, empty);
            ProgramFunction FillTank = new ProgramFunction("FillTank", empty, empty, empty, empty, empty);

            string[] Animal_Move_TextDataArray = { " ", "if", "(", "direction", "==", "0", ")", "//", "move", "north", " ", "LocationY",
                "+=", "seconds", "*", "Speed", ";", " ", "else", "if", "(", "direction", "==", "1", ")", "//", "move", "east", " ",
                "LocationX", "+=", "seconds", "*", "Speed", ";", " ", "else", "if", "(", "direction", "==", "2", ")", "//", "move",
                "south", " ", "LocationY", "-=", "seconds", "*", "Speed", ";", " ", "else", "if", "(", "direction", "==", "3", ")", "//",
                "move", "west", " ", "LocationX", "-=", "seconds", "*", "Speed", ";", " ", "return", "(", "LocationX", ",", "LocationY",
                ")", ";", " ", "}" };

            string[] Human_Move_TextDataArray = { " ", "int", "speed", "=", "Speed", ";", " ", "if", "(", "isDriving", "&&", "car", "!=",
                "null", ")", "speed", "=", "car", ".", "Speed", ";", " ", "if", "(", "direction", "==", "0", ")", "//", "move", "north",
                " ", "LocationY", "+=", "seconds", "*", "speed", ";", " ", "else", "if", "(", "direction", "==", "1", ")", "//", "move",
                "east", " ", "LocationX", "+=", "seconds", "*", "speed", ";", " ", "else", "if", "(", "direction", "==", "2", ")", "//",
                "move", "south", " ", "LocationY", "-=", "seconds", "*", "speed", ";", " ", "else", "if", "(", "direction", "==", "3", ")",
                "//", "move", "west", " ", "LocationX", "-=", "seconds", "*", "speed", ";", " ", "return", "(", "LocationX", ",", "LocationY",
                ")", ";", " ", "}" };

            string[] Dog_Talk_TextDataArray = { "\"", "Woof", "!", "\"", ";" };

            string[] Human_Talk_TextDataArray = { "\"", "Hello", "\"", ";" };

            Animal_Move.TextData.AddRange(Animal_Move_TextDataArray);
            Dog_Talk.TextData.AddRange(Dog_Talk_TextDataArray);
            Human_Talk.TextData.AddRange(Human_Talk_TextDataArray);
            Human_Move.TextData.AddRange(Human_Move_TextDataArray);

            Animal.Size = 3; Animal.Complexity = 0;
            Animal_Move.Size = 9; Animal_Move.Complexity = 4;
            Pet.Size = 1; Pet.Complexity = 0;
            Dog.Size = 1; Dog.Complexity = 0;
            Dog_Talk.Size = 1; Dog_Talk.Complexity = 0;
            Human.Size = 2; Human.Complexity = 0;
            Human_Talk.Size = 1; Human_Talk.Complexity = 0;
            Human_Move.Size = 11; Human_Move.Complexity = 5;
            GoToSchool.Size = 6; GoToSchool.Complexity = 1;
            GraduateSchool.Size = 5; GraduateSchool.Complexity = 1;
            GoToWork.Size = 4; GoToWork.Complexity = 1;
            BuyPet.Size = 7; BuyPet.Complexity = 1;
            BuyDog.Size = 4; BuyDog.Complexity = 1;
            BuyCar.Size = 8; BuyCar.Complexity = 1;
            SellCar.Size = 5; SellCar.Complexity = 1;
            FillCarFuelTank.Size = 8; FillCarFuelTank.Complexity = 2;
            Car.Size = 5; Car.Complexity = 0;
            FillTank.Size = 7; FillTank.Complexity = 1;

            expectedFunctions.Add(Animal);
            expectedFunctions.Add(Animal_Move);
            expectedFunctions.Add(Pet);
            expectedFunctions.Add(Dog);
            expectedFunctions.Add(Dog_Talk);
            expectedFunctions.Add(Human);
            expectedFunctions.Add(Human_Talk);
            expectedFunctions.Add(Human_Move);
            expectedFunctions.Add(GoToSchool);
            expectedFunctions.Add(GraduateSchool);
            expectedFunctions.Add(GoToWork);
            expectedFunctions.Add(BuyPet);
            expectedFunctions.Add(BuyDog);
            expectedFunctions.Add(BuyCar);
            expectedFunctions.Add(SellCar);
            expectedFunctions.Add(FillCarFuelTank);
            expectedFunctions.Add(Car);
            expectedFunctions.Add(FillTank);

            programFile = new ProgramFile(filePath, fileName, fileText);
            fileProcessor = new FileProcessor(programFile);
            fileProcessor.ProcessFile();
            codeProcessor = new CodeProcessor(programFile, programClassTypeCollection);
            codeProcessor.ProcessFileCode();

            CheckAllChildListsFunctionData(expectedFunctions, repeatedFunctionNames, programFile);
        }

        public static void CheckAllChildListsFunctionData(List<ProgramFunction> expectedFunctions, List<string> repeatedFunctionNames, ProgramType programType)
        {
            Console.WriteLine(programType.Name);
            if (programType.GetType() == typeof(ProgramFunction))
            {
                bool repeatedName = false;

                foreach (string name in repeatedFunctionNames)
                    if (programType.Name.Equals(name))
                        repeatedName = true;

                for (int i = 0; i < expectedFunctions.Count; i++)
                {
                    if (programType.Name.Equals(expectedFunctions[i].Name))
                    {
                        if (repeatedName)
                        {
                            if (((ProgramFunction)programType).TextData.Equals(expectedFunctions[i].TextData))
                            {
                                (int actualSize, int actualComplexity) = (((ProgramFunction)programType).Size, ((ProgramFunction)programType).Complexity);
                                Assert.AreEqual(expectedFunctions[i].Size, actualSize);
                                Assert.AreEqual(expectedFunctions[i].Complexity, actualComplexity);
                                break;
                            }
                        }
                        else
                        {
                            (int actualSize, int actualComplexity) = (((ProgramFunction)programType).Size, ((ProgramFunction)programType).Complexity);
                            Assert.AreEqual(expectedFunctions[i].Size, actualSize);
                            Assert.AreEqual(expectedFunctions[i].Complexity, actualComplexity);
                            break;
                        }
                    }
                }
            }

            foreach (ProgramType child in programType.ChildList)
                CheckAllChildListsFunctionData(expectedFunctions, repeatedFunctionNames, child);
        }

        [TestMethod]
        public void TestFunctionSignature_A()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "public", "override", "sealed", 
                "(", "int", ",", "int", ")", "Test", "(", ")", " ", "{", " ", "return", "(", "0", ",", "0", ")", 
                ";", " ", "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "Test";
            List<string> empty = new List<string>();
            List<string> ModA = new List<string>();
            List<string> RetA = new List<string>();

            ModA.Add("public"); ModA.Add("override"); ModA.Add("sealed");
            RetA.Add("int"); RetA.Add(","); RetA.Add("int");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, ModA, RetA, empty, empty, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];
            
            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        [TestMethod]
        public void TestFunctionSignature_B()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "void", "Test", "<", "V", 
                ",", "W", ">", "(", "int", "x", ")", " ", "{", " ", "return", "(", "0", ",", "0", ")",
                ";", " ", "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "Test";
            List<string> empty = new List<string>();
            List<string> RetB = new List<string>();
            List<string> GenB = new List<string>();
            List<string> ParB = new List<string>();

            RetB.Add("void");
            GenB.Add("V"); GenB.Add(","); GenB.Add("W");
            ParB.Add("int"); ParB.Add("x");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, empty, RetB, GenB, ParB, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];

            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        [TestMethod]
        public void TestFunctionSignature_E()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "(", "int", ",", "int", ",", 
                "int", ",", "int", ",", "int", ")", "Test", "<", "PType", ",", "X", ">", "(", ")", " ", "{", 
                " ", "return", "(", "0", ",", "0", ")", ";", " ", "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "Test";
            List<string> empty = new List<string>();
            List<string> RetE = new List<string>();
            List<string> GenE = new List<string>();

            RetE.Add("int"); RetE.Add(","); RetE.Add("int"); RetE.Add(","); RetE.Add("int"); RetE.Add(","); RetE.Add("int"); RetE.Add(","); RetE.Add("int");
            GenE.Add("PType"); GenE.Add(","); GenE.Add("X");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, empty, RetE, GenE, empty, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];

            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        [TestMethod]
        public void TestFunctionSignature_F()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "public", "virtual", "(", 
                "List", "<", "int", ">", ",", "bool", ",", "float", ")", "Test", "<", "Object", ">", "(", 
                "double", "n", ",", "double", "m", ")", " ", "{", " ", "return", "(", "0", ",", "0", ")",
                ";", " ", "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "Test";
            List<string> empty = new List<string>();
            List<string> ModF = new List<string>();
            List<string> RetF = new List<string>();
            List<string> GenF = new List<string>();
            List<string> ParF = new List<string>();

            ModF.Add("public"); ModF.Add("virtual");
            RetF.Add("List"); RetF.Add("<"); RetF.Add("int"); RetF.Add(">"); RetF.Add(","); RetF.Add("bool"); RetF.Add(","); RetF.Add("float");
            GenF.Add("Object");
            ParF.Add("double"); ParF.Add("n"); ParF.Add(","); ParF.Add("double"); ParF.Add("m");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, ModF, RetF, GenF, ParF, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];

            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        [TestMethod]
        public void TestFunctionSignature_K()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "List", "<", "int", ">", 
                "F", "(", "int", "a", ")", " ", "{", " ", "return", "(", "0", ",", "0", ")", ";", " ", 
                "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "F";
            List<string> empty = new List<string>();
            List<string> RetK = new List<string>();
            List<string> ParK = new List<string>();

            RetK.Add("List"); RetK.Add("<"); RetK.Add("int"); RetK.Add(">");
            ParK.Add("int"); ParK.Add("a");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, empty, RetK, empty, ParK, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];

            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        [TestMethod]
        public void TestFunctionSignature_O()
        {
            ProgramFile programFile = new ProgramFile("path", "name", "text");
            ProgramClassTypeCollection expectedClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualClassTypeCollection = new ProgramClassTypeCollection();
            CodeProcessor codeProcessor = new CodeProcessor(programFile, actualClassTypeCollection);
            ProgramClass expectedProgramClass;
            ProgramClass actualProgramClass;
            ProgramFunction expectedProgramFunction;
            ProgramFunction actualProgramFunction;

            string[] fileTextDataArray = { "class", "ClassName", " ", "{", " ", "public", "static", 
                "Test", ".", "NewType", "TestFunc", "(", ")", " ", "{", " ", "return", "(", "0", 
                ",", "0", ")", ";", " ", "}", " ", "}" };

            programFile.FileTextData.AddRange(fileTextDataArray);

            string name = "TestFunc";
            List<string> empty = new List<string>();
            List<string> ModO = new List<string>();
            List<string> RetO = new List<string>();

            ModO.Add("public"); ModO.Add("static");
            RetO.Add("Test"); RetO.Add("."); RetO.Add("NewType");

            expectedProgramClass = new ProgramClass("ClassName", empty, empty);
            expectedProgramFunction = new ProgramFunction(name, ModO, RetO, empty, empty, empty);

            expectedProgramClass.ChildList.Add(expectedProgramFunction);
            expectedClassTypeCollection.Add(expectedProgramClass);

            codeProcessor.ProcessFileCode();

            CollectionAssert.AreEqual(expectedClassTypeCollection, actualClassTypeCollection);

            actualProgramClass = (ProgramClass)actualClassTypeCollection[0];

            Assert.AreEqual(expectedProgramClass.ChildList.Count, actualProgramClass.ChildList.Count);

            actualProgramFunction = (ProgramFunction)actualProgramClass.ChildList[0];

            Assert.AreEqual(expectedProgramFunction.Name, actualProgramFunction.Name);
            CollectionAssert.AreEqual(expectedProgramFunction.Modifiers, actualProgramFunction.Modifiers);
            CollectionAssert.AreEqual(expectedProgramFunction.ReturnTypes, actualProgramFunction.ReturnTypes);
            CollectionAssert.AreEqual(expectedProgramFunction.Generics, actualProgramFunction.Generics);
            CollectionAssert.AreEqual(expectedProgramFunction.Parameters, actualProgramFunction.Parameters);
        }

        /*
        [TestMethod]
        public void TestFunctionSignature_O()
        {
            CodeProcessor codeProcessor = new CodeProcessor(new ProgramFile("path", "name", "text"), new ProgramClassTypeCollection());

            List<string> empty = new List<string>();
            List<string> ModO = new List<string>();
            List<string> RetO = new List<string>();

            ModO.Add("public"); ModO.Add("static");
            RetO.Add("Test"); RetO.Add("."); RetO.Add("NewType");

            (bool O, ProgramFunction FunO) = codeProcessor.CheckIfFunctionTest(new string[] { "public", "static", "Test", ".", "NewType", "TestFunc", "(", ")" });

            Assert.AreEqual(true, O);
            Assert.AreEqual("TestFunc", FunO.Name);
            CollectionAssert.AreEqual(ModO, FunO.Modifiers);
            CollectionAssert.AreEqual(RetO, FunO.ReturnTypes);
            CollectionAssert.AreEqual(empty, FunO.Generics);
            CollectionAssert.AreEqual(empty, FunO.Parameters);
        }*/
    }

    [TestClass]
    public class RelationshipProcessorTests
    {
        [TestMethod]
        public void TestRelationshipProcessor_ProcessRelationships()
        {
            ProgramFile programFile;
            CodeProcessor codeProcessor;
            FileProcessor fileProcessor;
            ProgramClassTypeCollection expectedProgramClassTypeCollection = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypeCollection = new ProgramClassTypeCollection();
            List<string> empty = new List<string>();

            string filePath = Path.GetFullPath("..\\..\\..\\CodeAnalyzerTests\\TestInputFiles\\TestInputFile.cs");
            string fileName = "TestInputFile.cs";
            string fileText = "";

            if (File.Exists(filePath))
                fileText = File.ReadAllText(filePath);

            ProgramInterface IAnimalActions = new ProgramInterface("IAnimalActions", empty, empty);
            ProgramInterface IHumanActions = new ProgramInterface("IHumanActions", empty, empty);
            ProgramClass Animal = new ProgramClass("Animal", empty, empty);
            ProgramClass Pet = new ProgramClass("Pet", empty, empty);
            ProgramClass Dog = new ProgramClass("Dog", empty, empty);
            ProgramClass Human = new ProgramClass("Human", empty, empty);
            ProgramClass Car = new ProgramClass("Car", empty, empty);

            IAnimalActions.SubClasses.Add(Dog);
            IAnimalActions.SubClasses.Add(Human);
            IHumanActions.SubClasses.Add(Human);
            Animal.SubClasses.Add(Pet);
            Animal.SubClasses.Add(Human);
            Pet.SuperClasses.Add(Animal);
            Pet.SubClasses.Add(Dog);
            Pet.OwnedByClasses.Add(Human);
            Pet.UsedByClasses.Add(Human);
            Dog.SuperClasses.Add(Pet);
            Dog.SuperClasses.Add(IAnimalActions);
            Dog.OwnedByClasses.Add(Human);
            Human.SuperClasses.Add(Animal);
            Human.SuperClasses.Add(IAnimalActions);
            Human.SuperClasses.Add(IHumanActions);
            Human.OwnedClasses.Add(Car);
            Human.OwnedClasses.Add(Pet);
            Human.OwnedClasses.Add(Dog);
            Human.UsedClasses.Add(Pet);
            Car.OwnedByClasses.Add(Human);

            expectedProgramClassTypeCollection.Add(IAnimalActions);
            expectedProgramClassTypeCollection.Add(IHumanActions);
            expectedProgramClassTypeCollection.Add(Animal);
            expectedProgramClassTypeCollection.Add(Pet);
            expectedProgramClassTypeCollection.Add(Dog);
            expectedProgramClassTypeCollection.Add(Human);
            expectedProgramClassTypeCollection.Add(Car);

            programFile = new ProgramFile(filePath, fileName, fileText);
            fileProcessor = new FileProcessor(programFile);
            codeProcessor = new CodeProcessor(programFile, actualProgramClassTypeCollection);

            fileProcessor.ProcessFile();
            codeProcessor.ProcessFileCode();

            foreach (ProgramClassType programClassType in actualProgramClassTypeCollection)
            {
                RelationshipProcessor relationshipProcessor = new RelationshipProcessor(programClassType, actualProgramClassTypeCollection);
                relationshipProcessor.ProcessRelationships();
            }

            CollectionAssert.AreEquivalent(expectedProgramClassTypeCollection, actualProgramClassTypeCollection);

            foreach (ProgramClassType expectedProgramClassType in expectedProgramClassTypeCollection)
            {
                if (actualProgramClassTypeCollection.Contains(expectedProgramClassType.Name))
                {
                    ProgramClassType actualProgramClassType = actualProgramClassTypeCollection[expectedProgramClassType.Name];
                    CollectionAssert.AreEquivalent(expectedProgramClassType.SubClasses, actualProgramClassType.SubClasses);
                    CollectionAssert.AreEquivalent(expectedProgramClassType.SuperClasses, actualProgramClassType.SuperClasses);

                    if (expectedProgramClassType.GetType() == typeof(ProgramClass))
                    {
                        ProgramClass expectedProgramClass = (ProgramClass)expectedProgramClassType;
                        ProgramClass actualProgramClass = (ProgramClass)actualProgramClassType;
                        CollectionAssert.AreEquivalent(expectedProgramClass.OwnedClasses, actualProgramClass.OwnedClasses);
                        CollectionAssert.AreEquivalent(expectedProgramClass.OwnedByClasses, actualProgramClass.OwnedByClasses);
                        CollectionAssert.AreEquivalent(expectedProgramClass.UsedClasses, actualProgramClass.UsedClasses);
                        CollectionAssert.AreEquivalent(expectedProgramClass.UsedByClasses, actualProgramClass.UsedByClasses);
                    }
                }
            }
        }
    }
}
