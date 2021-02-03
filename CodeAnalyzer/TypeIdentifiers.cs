using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{

    public class ProgramType
    {
        public virtual string Name { get; set; }
        public List<ProgramType> ChildList { get; set; }
        public ProgramType(string name)
        {
            this.ChildList = new List<ProgramType>();
            this.Name = name;
        }
    }

    public class ProgramFile : ProgramType
    {
        public string FilePath { get; set; }
        public string FileText { get; set; }
        public List<string> FileTextData { get; }
        public ProgramFile(string filePath, string fileName, string fileText) : base(fileName)
        {
            this.FilePath = filePath;
            this.FileText = fileText;
            this.FileTextData = new List<string>();
        }
    }

    public class ProgramNamespace : ProgramType { public ProgramNamespace(string name) : base(name) { } }

    public class ProgramObjectType : ProgramType     // ProgramObject includes all classes, interfaces, structs(?), and enums(?)
    {
        public ProgramObjectTypeCollection ProgramObjectCollection { get; internal set; }
        public string Modifiers { get; }
        public List<string> TextData { get; set; }
        public List<ProgramObjectType> SuperObjects { get; }     // *Inheritance* - ProgramObject(s) that this class inherits from
        public List<ProgramObjectType> SubObjects { get; }       // *Inheritance* - ProgramObject(s) that this class is inherited by
        public List<ProgramObjectType> OwnedObjects { get; }     // *Composition/Aggregation* - ProgramObject(s) that are "part of" (owned by) this ProgramObject
        public List<ProgramObjectType> OwnedByObjects { get; }   // *Composition/Aggregation* - ProgramObject(s) that this class is "part of" ProgramObject
        public List<ProgramObjectType> UsedObjects { get; }      // *Using* - ProgramObject(s) that this ProgramObject uses
        public List<ProgramObjectType> UsedByObjects { get; }    // *Using* - ProgramObject(s) that this ProgramObject is used by

        public ProgramObjectType(string name, string modifiers) : base(name)
        {
            this.Modifiers = modifiers;
            this.SuperObjects = new List<ProgramObjectType>();
            this.SubObjects = new List<ProgramObjectType>();
            this.OwnedObjects = new List<ProgramObjectType>();
            this.OwnedByObjects = new List<ProgramObjectType>();
            this.UsedObjects = new List<ProgramObjectType>();
            this.UsedByObjects = new List<ProgramObjectType>();
        }

        public override string Name
        {
            get { return base.Name; }
            set
            {
                if (ProgramObjectCollection != null) ProgramObjectCollection.NotifyNameChange(this, value);
                base.Name = value;
            }
        }
    }

    public class ProgramClass : ProgramObjectType { public ProgramClass(string name, string modifiers) : base(name, modifiers) { } }
    //public class ProgramInterface : ProgramObjectType { public ProgramInterface(string name, string modifiers) : base(name, modifiers) { } }
    //public class ProgramStruct : ProgramObjectType { public ProgramStruct(string name, string modifiers) : base(name, modifiers) { } }
    //public class ProgramEnum : ProgramObjectType { public ProgramEnum(string name, string modifiers) : base(name, modifiers) { } }

    public class ProgramFunction : ProgramType
    {
        public string Modifiers { get; set; }
        public string ReturnType { get; set; }
        public string Parameters { get; set; }
        public string BaseParameters { get; set; }
        public int Size { get; set; }
        public int Complexity { get; set; }
        public ProgramFunction(string name, string modifiers, string returnType, string parameters, string baseParameters) : base(name)
        {
            this.Modifiers = modifiers;
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.BaseParameters = baseParameters;
            this.Size = 0;
            this.Complexity = 0;
        }
    }

    /* KeyedCollection for ProgramClassType */
    public class ProgramObjectTypeCollection : KeyedCollection<string, ProgramObjectType>
    {
        internal void NotifyNameChange(ProgramObjectType programObjectType, string newName) =>
            this.ChangeItemKey(programObjectType, newName);
        protected override string GetKeyForItem(ProgramObjectType item) => item.Name;
        protected override void InsertItem(int index, ProgramObjectType item)
        {
            base.InsertItem(index, item);
            item.ProgramObjectCollection = this;
        }
        protected override void SetItem(int index, ProgramObjectType item)
        {
            base.SetItem(index, item);
            item.ProgramObjectCollection = this;
        }
        protected override void RemoveItem(int index)
        {
            this[index].ProgramObjectCollection = null;
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            foreach (ProgramObjectType programObjectType in this) programObjectType.ProgramObjectCollection = null;
            base.ClearItems();
        }
    }

}