using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;
using System.Collections.Generic;

/* 
 * Professor Kaya - I wasn't really sure what to test for in these classes,
 * since they are mainly just object definitions. I just made tests to make sure
 * the overridden and inherited Equals and GetHashCode methods work, and that the
 * overriden Name method updates the KeyedCollection. Everything else is implicitly 
 * tested by the CodeAnalyzer tests, which uses all of these objects heavily.
 */

namespace CodeAnalyzerTests
{
    [TestClass]
    public class ProgramClassTypeTests
    {
        [TestMethod]
        public void TestProgramClassType_1()
        {
            string name1 = "TestClass";
            string name2 = "TestClass";
            List<string> list = new List<string>();
            List<string> modifiers1 = new List<string>();
            List<string> modifiers2 = new List<string>();
            List<string> returnTypes = new List<string>();
            List<string> modifiers = new List<string>();

            ProgramClass programClass1;
            ProgramClass programClass2;

            modifiers1.Add("class");
            modifiers1.Add("1");
            modifiers1.Add("modifiers");
            modifiers2.Add("class");
            modifiers2.Add("2");
            modifiers2.Add("modifiers");
            returnTypes.Add("int");
            modifiers.Add("public");

            programClass1 = new ProgramClass(name1, modifiers1, list);
            programClass2 = new ProgramClass(name2, modifiers2, list);

            programClass1.ChildList.Add(new ProgramFunction("FunctionName", list, returnTypes, list, list, list));
            programClass2.OwnedClasses.Add(new ProgramClass("ClassName", modifiers, list));

            Assert.AreEqual(programClass1, programClass2);
            Assert.AreEqual(programClass1.GetHashCode(), programClass2.GetHashCode());
        }

        [TestMethod]
        public void TestProgramClassType_2()
        {
            string name = "TestInterface";
            List<string> list = new List<string>();
            List<string> modifiers1 = new List<string>();
            List<string> modifiers2 = new List<string>();
            List<string> returnTypes = new List<string>();
            List<string> modifiers = new List<string>();

            ProgramInterface programInterface1;
            ProgramInterface programInterface2;

            modifiers1.Add("interface");
            modifiers1.Add("1");
            modifiers1.Add("modifiers");
            modifiers2.Add("interface");
            modifiers2.Add("2");
            modifiers2.Add("modifiers");
            returnTypes.Add("int");
            modifiers.Add("public");

            programInterface1 = new ProgramInterface(name, modifiers1, list);
            programInterface2 = new ProgramInterface(name, modifiers2, list);

            programInterface1.ChildList.Add(new ProgramFunction("FunctionName", list, returnTypes, list, list, list));
            programInterface2.SubClasses.Add(new ProgramClass("ClassName", modifiers, list));

            Assert.AreEqual(programInterface1, programInterface2);
            Assert.AreEqual(programInterface1.GetHashCode(), programInterface2.GetHashCode());
        }

        [TestMethod]
        public void TestProgramClassType_3()
        {
            string name = "TestName";
            List<string> modifiers = new List<string>();
            List<string> list = new List<string>();
            ProgramClass programClass;
            ProgramInterface programInterface;
            ProgramClass subclass;

            modifiers.Add("private");

            subclass = new ProgramClass("Subclass", list, list);

            programClass = new ProgramClass(name, modifiers, list);
            programInterface = new ProgramInterface(name, modifiers, list);

            programClass.SubClasses.Add(subclass);
            programInterface.SubClasses.Add(subclass);

            Assert.AreNotEqual(programClass, programInterface);
            Assert.AreNotEqual(programClass.GetHashCode(), programInterface.GetHashCode());
        }
    }

    [TestClass]
    public class ProgramClassTypeCollectionTests
    {

        [TestMethod]
        public void TestProgramClassTypeCollection_1()
        {
            ProgramClassTypeCollection collection = new ProgramClassTypeCollection();
            ProgramClass testProgramClass;
            ProgramInterface testProgramInterface;
            List<string> list = new List<string>();
            List<string> testClassModifiers = new List<string>();
            List<string> testClassGenerics = new List<string>();
            List<string> testInterfaceModifiers = new List<string>();
            List<string> testInterfaceGenerics = new List<string>();

            testClassModifiers.Add("x");
            testClassGenerics.Add("y");
            testInterfaceModifiers.Add("x");
            testInterfaceGenerics.Add("y");

            testProgramClass = new ProgramClass("item2", testClassModifiers, testClassGenerics);
            testProgramInterface = new ProgramInterface("item2", testInterfaceModifiers, testInterfaceGenerics);

            collection.Add(new ProgramClass("item0", list, list));
            collection.Add(new ProgramInterface("item1", list, list));
            collection.Add(new ProgramClass("item2", list, list));
            collection.Add(new ProgramClass("item3", list, list));

            Assert.AreEqual(collection[2], collection["item2"]);
            Assert.AreEqual(collection[2], testProgramClass);
            Assert.AreEqual(collection["item2"], testProgramClass);
            Assert.AreNotEqual(collection[2], testProgramInterface);
            Assert.AreNotEqual(collection["item2"], testProgramInterface);
        }

        [TestMethod]
        public void TestProgramClassTypeCollection_2()
        {
            ProgramClassTypeCollection collection = new ProgramClassTypeCollection();
            List<string> list = new List<string>();
            List<string> testClassModifiers = new List<string>();
            ProgramClass testProgramClass;

            testClassModifiers.Add("x");

            testProgramClass = new ProgramClass("item2", testClassModifiers, list);

            collection.Add(new ProgramClass("item0", list, list));
            collection.Add(new ProgramInterface("item1", list, list));
            collection.Add(new ProgramClass("item2", list, list));
            collection.Add(new ProgramClass("item3", list, list));

            collection[2].Name = "item2_newName";

            Assert.ThrowsException<KeyNotFoundException>(() => collection["item2"]);
            Assert.AreEqual(collection[2], collection["item2_newName"]);
            Assert.AreNotEqual(collection[2], testProgramClass);
            Assert.AreNotEqual(collection["item2_newName"], testProgramClass);
        }
    }
}
