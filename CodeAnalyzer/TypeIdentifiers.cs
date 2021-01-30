using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    public class File
    {
        public string FileName { get; set; }
        public ProgramTypeCollection ChildCollection { get; set; }
        public File()
        {
            this.ChildCollection = new ProgramTypeCollection();
        }
    }

    public class ProgramType
    {
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (ParentCollection != null) ParentCollection.NotifyNameChange(this, value);
                this.name = value;
            }
        }
        public ProgramTypeCollection ParentCollection { get; internal set; }
        public ProgramTypeCollection ChildCollection { get; set; }
        public ProgramType()
        {
            this.ChildCollection = new ProgramTypeCollection();
        }
    }

    public class Namespace : ProgramType { public Namespace() : base() { } }

    // TODO: do we have to check for primitive types for aggregation/using relationships?
    public class Class : ProgramType
    {
        public string Modifiers { get; set; }                       // public, private, protected, protected internal, private protected
        public Class Superclass { get; set; }                       // *Inheritance* - class that this class inherits from
        public ProgramTypeCollection Subclasses { get; set; }       // *Inheritance* - class(es) that this class is inherited by
        public ProgramTypeCollection OwnedClasses { get; set; }     // *Composition/Aggregation* - class(es) that are "part of" (owned by) this class
        public ProgramTypeCollection OwnedByClasses { get; set; }   // *Composition/Aggregation* - class(es) that this class is "part of" (owned by)
        public ProgramTypeCollection UsedClasses { get; set; }      // *Using* - class(es) that this class uses
        public ProgramTypeCollection UsedByClasses { get; set; }    // *Using* - class(es) that this class is used by

        public Class() : base() 
        {
            this.Superclass = new Class();
            this.Subclasses = new ProgramTypeCollection();
            this.OwnedClasses = new ProgramTypeCollection();
            this.OwnedByClasses = new ProgramTypeCollection();
            this.UsedClasses = new ProgramTypeCollection();
            this.UsedByClasses = new ProgramTypeCollection();
        }
    }

    public class Function : ProgramType
    {
        public int Size { get; set; }
        public int Complexity { get; set; }

        public Function() : base() { }
    }

    /* KeyedCollection for ProgramType */
    public class ProgramTypeCollection : KeyedCollection<string, ProgramType>
    {
        internal void NotifyNameChange(ProgramType programType, string newName) =>
            this.ChangeItemKey(programType, newName);
        protected override string GetKeyForItem(ProgramType item) => item.Name;
        protected override void InsertItem(int index, ProgramType item)
        {
            base.InsertItem(index, item);
            item.ParentCollection = this;
        }
        protected override void SetItem(int index, ProgramType item)
        {
            base.SetItem(index, item);
            item.ParentCollection = this;
        }
        protected override void RemoveItem(int index)
        {
            this[index].ParentCollection = null;
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            foreach (ProgramType programType in this) programType.ParentCollection = null;
            base.ClearItems();
        }
    }

}