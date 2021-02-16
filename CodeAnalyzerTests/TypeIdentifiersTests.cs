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
        public void TestClassType_1()
        {
            string name = "TestClass";
            string modifiers1 = "modifiers for class 1";
            string modifiers2 = "modifiers for class 2";

            ProgramClass programClass1 = new ProgramClass(name, modifiers1);
            ProgramClass programClass2 = new ProgramClass(name, modifiers2);

            programClass1.ChildList.Add(new ProgramFunction("FunctionName", "", "int", new List<string>(), ""));
            programClass2.OwnedClasses.Add(new ProgramClass("ClassName", "public"));

            Assert.AreEqual(programClass1, programClass2);
            Assert.AreEqual(programClass1.GetHashCode(), programClass2.GetHashCode());
        }

        [TestMethod]
        public void TestClassType_2()
        {
            string name = "TestInterface";
            string modifiers1 = "modifiers for class 1";
            string modifiers2 = "modifiers for class 2";

            ProgramInterface programInterface1 = new ProgramInterface(name, modifiers1);
            ProgramInterface programInterface2 = new ProgramInterface(name, modifiers2);

            programInterface1.ChildList.Add(new ProgramFunction("FunctionName", "", "int", new List<string>(), ""));
            programInterface2.SubClasses.Add(new ProgramClass("ClassName", "public"));

            Assert.AreEqual(programInterface1, programInterface2);
            Assert.AreEqual(programInterface1.GetHashCode(), programInterface2.GetHashCode());
        }

        [TestMethod]
        public void TestClassType_3()
        {
            string name = "TestName";
            string modifiers = "private";
            ProgramClass subclass = new ProgramClass("Subclass", "");

            ProgramClass programClass = new ProgramClass(name, modifiers);
            ProgramInterface programInterface = new ProgramInterface(name, modifiers);

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
            ProgramClass testProgramClass = new ProgramClass("item2", "x");
            ProgramInterface testProgramInterface = new ProgramInterface("item2", "x");

            collection.Add(new ProgramClass("item0", ""));
            collection.Add(new ProgramInterface("item1", ""));
            collection.Add(new ProgramClass("item2", ""));
            collection.Add(new ProgramClass("item3", ""));

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
            ProgramClass testProgramClass = new ProgramClass("item2", "x");

            collection.Add(new ProgramClass("item0", ""));
            collection.Add(new ProgramInterface("item1", ""));
            collection.Add(new ProgramClass("item2", ""));
            collection.Add(new ProgramClass("item3", ""));

            collection[2].Name = "item2_newName";

            Assert.ThrowsException<KeyNotFoundException>(() => collection["item2"]);
            Assert.AreEqual(collection[2], collection["item2_newName"]);
            Assert.AreNotEqual(collection[2], testProgramClass);
            Assert.AreNotEqual(collection["item2_newName"], testProgramClass);
        }
    }
}
