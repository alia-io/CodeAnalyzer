/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  TypeIdentifier.cs - Defines C# types, stores data within types                   ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CodeAnalyzer
{

    public abstract class ProgramType
    {
        public virtual string Name { get; set; }
        public List<ProgramType> ChildList { get; set; }
        public ProgramType(string name)
        {
            this.ChildList = new List<ProgramType>();
            this.Name = name;
        }
    }

    public abstract class ProgramDataType : ProgramType   // ProgramDataType includes classes, interfaces, and functions (the types that hold data)
    {
        public List<string> TextData { get; set; }
        public string Modifiers { get; }
        public ProgramDataType(string name, string modifiers) : base(name)
        {
            this.Modifiers = modifiers;
            this.TextData = new List<string>();
        }
    }

    public abstract class ProgramClassType : ProgramDataType     // ProgramClassType includes all classes and interfaces
    {
        public ProgramClassTypeCollection ProgramClassCollection { get; internal set; }
        public List<ProgramClassType> SubClasses { get; }       // *Inheritance* - ProgramClass(es) that this class is inherited by
        public List<ProgramClassType> SuperClasses { get; }     // *Inheritance* - ProgramClass(es) that this class inherits from
        public List<ProgramClassType> UsedByClasses { get; }    // *Using* - ProgramClass(es) that this ProgramClass is used by

        public ProgramClassType(string name, string modifiers) : base(name, modifiers) 
        { 
            this.SubClasses = new List<ProgramClassType>();
            this.SuperClasses = new List<ProgramClassType>();
            this.UsedByClasses = new List<ProgramClassType>();
        }

        public override string Name
        {
            get { return base.Name; }
            set
            {
                if (ProgramClassCollection != null) ProgramClassCollection.NotifyNameChange(this, value);
                base.Name = value;
            }
        }
    }

    public class ProgramFile : ProgramType
    {
        public string FilePath { get; }
        public string FileText { get; }
        public List<string> FileTextData { get; }
        public ProgramFile(string filePath, string fileName, string fileText) : base(fileName)
        {
            this.FilePath = filePath;
            this.FileText = fileText;
            this.FileTextData = new List<string>();
        }
    }

    public class ProgramNamespace : ProgramType { public ProgramNamespace(string name) : base(name) { } }

    public class ProgramClass : ProgramClassType 
    {
        public List<ProgramClassType> OwnedClasses { get; }     // *Composition/Aggregation* - ProgramClass(es) that are "part of" (owned by) this ProgramClass
        public List<ProgramClassType> OwnedByClasses { get; }   // *Composition/Aggregation* - ProgramClass(es) that this ProgramClass is "part of"
        public List<ProgramClassType> UsedClasses { get; }      // *Using* - ProgramClass(es) that this ProgramClass uses
        public ProgramClass(string name, string modifiers) : base(name, modifiers) 
        {
            this.OwnedClasses = new List<ProgramClassType>();
            this.OwnedByClasses = new List<ProgramClassType>();
            this.UsedClasses = new List<ProgramClassType>();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ProgramClass)) return false;
            return (base.Name).Equals(((ProgramClass)obj).Name);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public class ProgramInterface : ProgramClassType 
    { 
        public ProgramInterface(string name, string modifiers) : base(name, modifiers) { }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ProgramInterface)) return false;
            return (base.Name).Equals(((ProgramInterface)obj).Name);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public class ProgramFunction : ProgramDataType
    {
        public string ReturnType { get; }
        public List<string> Parameters { get; set; }
        public string BaseParameters { get; }
        public int Size { get; set; }
        public int Complexity { get; set; }
        public ProgramFunction(string name, string modifiers, string returnType, List<string> parameters, string baseParameters) : base(name, modifiers)
        {
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.BaseParameters = baseParameters;
            this.Size = 0;
            this.Complexity = 0;
        }
    }

    /* KeyedCollection for ProgramClassType */
    public class ProgramClassTypeCollection : KeyedCollection<string, ProgramClassType>
    {
        internal void NotifyNameChange(ProgramClassType programClassType, string newName) =>
            this.ChangeItemKey(programClassType, newName);
        protected override string GetKeyForItem(ProgramClassType item) => item.Name;
        protected override void InsertItem(int index, ProgramClassType item)
        {
            base.InsertItem(index, item);
            item.ProgramClassCollection = this;
        }
        protected override void SetItem(int index, ProgramClassType item)
        {
            base.SetItem(index, item);
            item.ProgramClassCollection = this;
        }
        protected override void RemoveItem(int index)
        {
            this[index].ProgramClassCollection = null;
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            foreach (ProgramClassType programClassType in this) programClassType.ProgramClassCollection = null;
            base.ClearItems();
        }
    }

}
