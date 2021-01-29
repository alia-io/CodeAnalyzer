using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    public class File
    {
        private string fileName;
        private List<ProgramType> topLevelTypes;

    }

    public class ProgramType
    {
        string name;
        List<ProgramType> programTypes;
    }

    public class Namespace : ProgramType { }

    // TODO: do we have to check for primitive types for aggregation/using relationships?
    public class Class : ProgramType
    {
        string modifiers; // public, private, protected, protected internal, private protected
        Class superclass;           // *Inheritance* - class that this class inherits from
        List<Class> subclasses;     // *Inheritance* - class(es) that this class is inherited by
        List<Class> ownedClasses;   // *Composition/Aggregation* - class(es) that are "part of" (owned by) this class
        List<Class> ownedByClasses; // *Composition/Aggregation* - class(es) that this class is "part of" (owned by)
        List<Class> usedClasses;    // *Using* - class(es) that this class uses
        List<Class> usedByClasses;  // *Using* - class(es) that this class is used by
    }

    public class Function : ProgramType
    {
        int size;
        int complexity;
    }

}