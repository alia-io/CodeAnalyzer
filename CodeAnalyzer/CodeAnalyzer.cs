using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /* Pre-processes file text into a list of strings */
    class FileProcessor
    {
        ProgramFile programFile;
        StringBuilder stringBuilder = new StringBuilder();

        public FileProcessor(ProgramFile programFile) => this.programFile = programFile;

        /* Puts the file next into a string list, dividing elements logically */
        public void ProcessFile()
        {
            IEnumerator enumerator;

            // split text by line
            string[] programLines = programFile.FileText.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < programLines.Length; i++)
            {
                enumerator = programLines[i].GetEnumerator();

                while (enumerator.MoveNext()) // read the line char by char
                {
                    if (Char.IsWhiteSpace((char)enumerator.Current)) // add the element to the FileTextData list
                    {
                        this.AddEntryToFileTextData();
                        continue;
                    }

                    /* ---------- Check special cases ---------- */
                    if ((Char.IsPunctuation((char)enumerator.Current) || Char.IsSymbol((char)enumerator.Current))
                        && !((char)enumerator.Current).Equals('_'))
                    {
                        // Detect double-character symbols
                        if (stringBuilder.Length == 1 && (Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0]))
                            && this.DetectDoubleCharacter((char)stringBuilder.ToString()[0], (char)enumerator.Current))
                        {
                            stringBuilder.Append(enumerator.Current);
                            this.AddEntryToFileTextData();
                            continue;
                        }
                        this.AddEntryToFileTextData();
                    }
                    else if (stringBuilder.Length == 1 && !((char)stringBuilder.ToString()[0]).Equals('_')
                            && (Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0])))
                        this.AddEntryToFileTextData();

                    stringBuilder.Append(enumerator.Current);
                }

                this.AddEntryToFileTextData();
                programFile.FileTextData.Add(" "); // mark for a new line
            }
        }

        /* Adds the current string in the StringBuilder to the FileTextData list, then clears the StringBuilder */
        private void AddEntryToFileTextData()
        {
            if (stringBuilder.Length > 0)
            {
                programFile.FileTextData.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
        }

        /* Tests for two-character sequences that have a combined syntactical meaning */
        private bool DetectDoubleCharacter(char previous, char current)
        {
            if (previous.Equals('/') && (current.Equals('/') || current.Equals('*') || current.Equals('=')))
                return true;
            if (previous.Equals('*') && current.Equals('/'))
                return true;
            if (previous.Equals('+') && (current.Equals('+') || current.Equals('=')))
                return true;
            if (previous.Equals('-') && (current.Equals('-') || current.Equals('=')))
                return true;
            if (previous.Equals('>') && (current.Equals('>') || current.Equals('=')))
                return true;
            if (previous.Equals('<') && (current.Equals('<') || current.Equals('=')))
                return true;
            if ((previous.Equals('*') || previous.Equals('!') || previous.Equals('%')) && current.Equals('='))
                return true;
            if (previous.Equals('=') && (current.Equals('>') || current.Equals('=')))
                return true;
            if (previous.Equals('&') && current.Equals('&'))
                return true;
            if (previous.Equals('|') && current.Equals('|'))
                return true;
            if (previous.Equals('\\') && (current.Equals('\\') || current.Equals('"') || current.Equals('\'')))
                return true;
            return false;
        }
    }

    /* Fully processes the file data (except relationship data), fills all internal Child ProgramType lists */
    class CodeProcessor
    {
        /* Saved copies of input input arguments */
        private ProgramClassTypeCollection programClassTypes;
        private ProgramFile programFile;

        /* Stacks to keep track of the current scope */
        private readonly Stack<string> scopeStack = new Stack<string>();
        private readonly Stack<ProgramType> typeStack = new Stack<ProgramType>();

        private readonly StringBuilder stringBuilder = new StringBuilder("");

        /* Scope syntax rules to check for */
        int ifScope = 0;        // [0-1]: "if" ONLY if elseIfScope == 0, [1-2]: "(", [2-3]: word(s), [3-4]: ")", [4-0]: "{" or bracketless
        int elseIfScope = 0;    // [0-1]: "else", [1-2]: "if", [2-3]: "(", [3-4]: word(s), [4-5]: ")", [5-0]: "{" or bracketless
        int elseScope = 0;      // [0-1]: "else", [1-0]: "{" or bracketless NOT "if"
        int forScope = 0;       // [0-1]: "for", [1-2]: "(", [2-3]: word(s), [3-4]: ";", [4-5]: word(s), [5-6]: ";", [6-7]: word(s), [7-8]: ")", [8-0]: "{" or bracketless
        int forEachScope = 0;   // [0-1]: "foreach", [1-2]: "(", [2-3]: word(s), [3-4]: ")", [4-0]: "{" or bracketless
        int whileScope = 0;     // [0-1]: "while", [1-2]: "(", [2-3]: word(s), [3-4]: ")", [4-0]: "{" or bracketless NOT ";"
        int doWhileScope = 0;   // [0-1]: "do", [1-0]: "{" or bracketless
        int switchScope = 0;    // [0-1]: "switch", [1-2]: "(", [2-3]: word(s), [3-4]: ")", [4-0]: "{"

        int savedScopeStackCount = 0;

        public CodeProcessor(ProgramFile programFile, ProgramClassTypeCollection programClassTypes)
        {
            this.programClassTypes = programClassTypes;
            this.programFile = programFile;
        }

        /* Analyzes all of the code outside of a class or interface */
        public void ProcessFileCode()
        {
            string entry;

            for (int index = 0; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                // Determine whether to ignore the entry (if it's part of a comment)
                if (this.IgnoreEntry(entry)) continue;

                if (entry.Equals("}")) // Check for the end of an existing bracketed scope
                {
                    this.EndBracketedScope("");
                    continue;
                }

                if (entry.Equals("namespace")) // Check for a new namespace
                {
                    index = this.NewNamespace(index);
                    continue;
                }

                if (entry.Equals("class")) // Check for a new class
                {
                    index = this.NewClass(index);
                    continue;
                }
                
                if (entry.Equals("interface")) // Check for a new interface
                {
                    index = this.NewInterface(index);
                    continue;
                }

                if (entry.Equals("{")) // Push scope opener onto scopeStack
                    scopeStack.Push(entry);

                this.UpdateStringBuilder(entry);
            }
        }

        /* Processes data within a ProgramClassType (class or interface) scope but outside of a function */
        private int ProcessProgramClassTypeData(int i)
        {
            string entry;
            int index;

            for (index = i; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                // Determine whether to ignore the entry (if it's part of a comment)
                if (this.IgnoreEntry(entry)) continue;

                /* ---------- Add entry to current ProgramDataType's text list ---------- */
                if (typeStack.Count > 0 && (typeStack.Peek().GetType() == typeof(ProgramClass)
                    || typeStack.Peek().GetType() == typeof(ProgramInterface) || typeStack.Peek().GetType() == typeof(ProgramFunction)))
                    ((ProgramDataType)typeStack.Peek()).TextData.Add(entry);

                /* ---------- Check for the end of an existing scope ---------- */
                if (entry.Equals("}"))
                {
                    if (scopeStack.Count > 0)
                        if (scopeStack.Peek().Equals("{")) scopeStack.Pop();
                    if (scopeStack.Count > 0)
                    {
                        if (scopeStack.Peek().Equals("class")) // return from the class
                        {
                            scopeStack.Pop();
                            if (typeStack.Count > 0) typeStack.Pop();
                            stringBuilder.Clear();
                            return index;
                        }
                        // ending a different named scope
                        if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("function"))
                        {
                            scopeStack.Pop();
                            if (typeStack.Count > 0) typeStack.Pop();
                        }
                    }
                    continue;
                }

                /* ---------- Check for a new class ---------- */
                if (entry.Equals("class"))
                {
                    index = this.NewClass(index);
                    continue;
                }

                /* ---------- Check for a new interface ---------- */
                if (entry.Equals("interface"))
                {
                    index = this.NewInterface(index);
                    continue;
                }

                /* ---------- Check if a new function is being started ---------- */
                if (entry.Equals("{"))
                {
                    if (this.CheckIfFunction()) // valid function or method syntax
                    {
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                    else /* ---------- Other type of open brace ---------- */
                        scopeStack.Push(entry);
                }

                /* ---------- Update stringBuilder ---------- */
                this.UpdateStringBuilder(entry);
            }

            return index;
        }

        /* Processes data within a Function scope */
        private int ProcessFunctionData(int i)
        {
            string entry;
            int index;
            bool scopeOpener;

            for (index = i; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];
                scopeOpener = false;

                // Determine whether to ignore the entry (if it's part of a comment)
                if (this.IgnoreEntry(entry)) continue;

                /* ---------- Add entry to current ProgramDataType's text list ---------- */
                if (typeStack.Count > 0 && (typeStack.Peek().GetType() == typeof(ProgramClass)
                    || typeStack.Peek().GetType() == typeof(ProgramInterface) || typeStack.Peek().GetType() == typeof(ProgramFunction)))
                    ((ProgramDataType)typeStack.Peek()).TextData.Add(entry);

                /* ---------- Check for a new line ---------- */
                if (entry.Equals(" ") && typeStack.Peek().GetType() == typeof(ProgramFunction))
                    ((ProgramFunction)typeStack.Peek()).Size++;

                /* ---------- Check for closing parenthesis ---------- */
                if (entry.Equals(")"))
                    if (scopeStack.Count > 0)
                        if (scopeStack.Peek().Equals("("))
                            scopeStack.Pop();

                /* ---------- Check for the end of an existing bracketed scope ---------- */
                if (entry.Equals("}"))
                {
                    if (scopeStack.Count > 0)
                        if (scopeStack.Peek().Equals("{")) scopeStack.Pop();
                    if (scopeStack.Count > 0)
                    {
                        if (scopeStack.Peek().Equals("function")) // return from the function
                        {
                            scopeStack.Pop();
                            if (typeStack.Count > 0) typeStack.Pop();
                            stringBuilder.Clear();
                            return index;
                        }
                        // ending a different ProgramType scope
                        if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class") || scopeStack.Peek().Equals("interface"))
                        {
                            scopeStack.Pop();
                            if (typeStack.Count > 0) typeStack.Pop();
                        }
                        else // ending another named scope
                            while (scopeStack.Count > 0 && (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                                || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                                || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch")))
                                scopeStack.Pop();
                    }
                    stringBuilder.Clear();
                    continue;
                }

                /* ---------- Check for opening scope statements ---------- */
                if (!entry.Equals(" ") && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                {
                    if (this.CheckScopes(entry))
                        scopeOpener = true;
                }

                /* ---------- Check for open parenthesis ---------- */
                if (entry.Equals("("))
                    scopeStack.Push("(");

                /* Push an opening bracket */
                if (entry.Equals("{"))
                {
                    /* ---------- Check if a new function is being started ---------- */
                    if (!scopeOpener && this.CheckIfFunction())
                    {
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                    else /* ---------- Other type of open bracket ---------- */
                        scopeStack.Push(entry);
                }

                /* ---------- Check for the end of an existing bracketless scope ---------- */
                if (entry.Equals(";") && forScope == 0 && scopeStack.Count > 0 && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                {
                    while (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                        || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                        || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch"))
                    {
                        scopeStack.Pop();
                    }
                }

                /* ---------- Update stringBuilder ---------- */
                this.UpdateStringBuilder(entry);
            }

            return index;
        }

        /* Maintains stack for commented areas; returns true if text is within a comment */
        private bool IgnoreEntry(string entry)
        {
            // Determine entry is within a comment, and check for the end of the comment
            if (this.IsComment(entry)) return true;

            // If starting a commented area, push the entry
            if (this.StartPlainTextArea(entry)) return true;

            return false;
        }

        /* Maintains stacks when a bracketed scope ends; returns true if the type of scope is equal to scopeType */
        private bool EndBracketedScope(string scopeType)
        {
            if (scopeStack.Count > 0) // Pop the scope opener
                if (scopeStack.Peek().Equals("{")) scopeStack.Pop();
            if (scopeStack.Count > 0) // If there is a scope identifier (type), pop the identifier and the type
                if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("function"))
                {
                    scopeStack.Pop();
                    if (typeStack.Count > 0) typeStack.Pop();
                }
            stringBuilder.Clear();
            return false;
        }

        private int NewNamespace(int index)
        {
            string entry;

            scopeStack.Push("namespace"); // push the type of the next scope opener onto scopeStack

            stringBuilder.Clear();
            while (++index < programFile.FileTextData.Count) // get the name of the namespace
            {
                entry = programFile.FileTextData[index];
                if (entry.Equals("{"))
                {
                    scopeStack.Push("{"); // push the new scope opener onto scopeStack
                    break;
                }
                if (!entry.Equals(" ")) stringBuilder.Append(entry);
            }

            ProgramNamespace programNamespace = new ProgramNamespace(stringBuilder.ToString());
            stringBuilder.Clear();

            // add new namespace to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programNamespace);
            else programFile.ChildList.Add(programNamespace);

            typeStack.Push(programNamespace); // push the namespace onto typeStack
            return index;
        }

        private int NewClass(int index)
        {
            string entry;
            string classModifiers = stringBuilder.ToString();   // get the class modifiers
            List<string> classText = new List<string>();
            int brackets = 0;

            scopeStack.Push("class"); // push the type of the next scope opener onto scopeStack

            stringBuilder.Clear();
            while (++index < programFile.FileTextData.Count) // get the name of the class
            {
                entry = programFile.FileTextData[index];

                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IsComment(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (this.StartPlainTextArea(entry))
                    continue;

                /* ---------- Add entry to class's TextData list ---------- */
                classText.Add(entry);

                if (entry.Equals("{"))
                {
                    scopeStack.Push("{"); // push the new scope opener onto scopeStack
                    break;
                }
                if (classText.Count == 0)
                {
                    if (entry.Equals("<") || entry.Equals("["))
                    {
                        brackets++;
                        stringBuilder.Append(entry);
                        continue;
                    }
                    else if (brackets != 0)
                    {
                        if (entry.Equals(">") || entry.Equals("]"))
                        {
                            brackets--;
                        }
                        stringBuilder.Append(entry);
                        continue;
                    }
                }
                if (stringBuilder.Length == 0)
                {
                    if (!entry.Equals(" ")) stringBuilder.Append(entry); // the next entry after "class" will be the name
                    continue;
                }
            }

            ProgramClass programClass = new ProgramClass(stringBuilder.ToString(), classModifiers);
            stringBuilder.Clear();

            // add text/inheritance data, and add class to general ProgramClassType list
            programClass.TextData = classText;
            programClassTypes.Add(programClass);

            // add new class to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programClass);
            else programFile.ChildList.Add(programClass);

            typeStack.Push(programClass); // push the class onto typeStack

            return this.ProcessProgramClassTypeData(++index); // reads the rest of the class
        }

        private int NewInterface(int index)
        {
            string entry;
            string interfaceModifiers = stringBuilder.ToString();   // get the interface modifiers
            List<string> interfaceText = new List<string>();
            int brackets = 0;

            scopeStack.Push("interface"); // push the type of the next scope opener onto scopeStack

            stringBuilder.Clear();
            while (++index < programFile.FileTextData.Count) // get the name of the interface
            {
                entry = programFile.FileTextData[index];

                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IsComment(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (this.StartPlainTextArea(entry))
                    continue;

                /* ---------- Add entry to interface's TextData list ---------- */
                interfaceText.Add(entry);

                if (entry.Equals("{"))
                {
                    scopeStack.Push("{"); // push the new scope opener onto scopeStack
                    break;
                }
                if (interfaceText.Count == 0)
                {
                    if (entry.Equals("<") || entry.Equals("["))
                    {
                        brackets++;
                        stringBuilder.Append(entry);
                        continue;
                    }
                    else if (brackets != 0)
                    {
                        if (entry.Equals(">") || entry.Equals("]"))
                        {
                            brackets--;
                        }
                        stringBuilder.Append(entry);
                        continue;
                    }
                }
                if (stringBuilder.Length == 0)
                {
                    if (!entry.Equals(" ")) stringBuilder.Append(entry); // the next entry after "interface" will be the name
                    continue;
                }
            }

            ProgramInterface programInterface = new ProgramInterface(stringBuilder.ToString(), interfaceModifiers);
            stringBuilder.Clear();

            // add text/inheritance data, and add interface to general ProgramClassType list
            programInterface.TextData = interfaceText;
            programClassTypes.Add(programInterface);

            // add new interface to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programInterface);
            else programFile.ChildList.Add(programInterface);

            typeStack.Push(programInterface); // push the interface onto typeStack

            return this.ProcessProgramClassTypeData(++index); // reads the rest of the interface
        }

        /* Used to detect the syntax for a function signature */
        private bool CheckIfFunction()
        {
            string[] functionIdentifier = stringBuilder.ToString().Split(' ');

            /* ---------- Determine whether new scope is a function ---------- */
            int functionRequirement = 0;    // the requirement to check next
                                            // (Optional: modifiers), [0-1]: return type, [1-2]: name, [2-3]: open parenthesis, (Optional: parameters), 
                                            // [3-4]: close parenthesis, [4]: all requirements fulfilled for *normal* functions
            string modifiers = "";
            string returnType = "";
            string name = "";
            List<string> parameters = new List<string>();
            string baseParameters = "";

            int parentheses = 0;            // ensure the same number of opening and closing parentheses
            int brackets = 0;
            int periods = 0;

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("[") || text.Equals("<")) brackets++;
                else if (text.Equals(".")) periods++;

                switch (functionRequirement)
                {
                    case 0:
                        if (text.Equals(" ")) continue;
                        if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                        {
                            returnType = text;
                            functionRequirement = 1;
                            break;
                        }
                        functionRequirement = -1; // failed function syntax
                        break;
                    case 1:
                        if (text.Equals(" ")) continue;
                        if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                        {
                            name = text;
                            functionRequirement = 2;
                            break;
                        }
                        if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals("<") || text.Equals("[") || text.Equals(">") || text.Equals("]"))
                        {
                            if (name.Length == 0)
                                returnType += text;
                            else
                                name += text;
                            break;
                        }
                        functionRequirement = -1; // failed function syntax
                        break;
                    case 2:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            functionRequirement = 3;
                            break;
                        }
                        if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                        {
                            if (modifiers.Length > 0) modifiers += " ";
                            modifiers += returnType;
                            returnType = name;
                            name = text;
                            break;
                        }
                        if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals("<") || text.Equals("[") || text.Equals(">") || text.Equals("]"))
                        {
                            if (name.Length == 0)
                                returnType += text;
                            else
                                name += text;
                            break;
                        }
                        functionRequirement = -1; // failed function syntax
                        break;
                    case 3:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")") && parentheses == 0)
                        {
                            functionRequirement = 4;
                            break;
                        }
                        parameters.Add(text);
                        break;
                    case 4:
                        if (text.Equals(" ")) continue;
                        functionRequirement = -1; // failed function syntax
                        break;
                }
                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 4) // function signature detected
            {
                this.RemoveFunctionSignatureFromTextData(functionIdentifier.Length);
                stringBuilder.Clear();

                ProgramFunction programFunction = new ProgramFunction(name, modifiers, returnType, parameters, baseParameters);

                // add new function to its parent's ChildList
                if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programFunction);
                else programFile.ChildList.Add(programFunction);

                // add the function and scope to scopeStack
                scopeStack.Push("function");
                scopeStack.Push("{");

                typeStack.Push(programFunction); // push the class onto typeStack
                return true;
            }
            else if (this.CheckIfConstructor(functionIdentifier))
                return true;
            else if (this.CheckIfDeconstructor(functionIdentifier))
                return true;

            return false;
        }

        /* Used to detect the syntax for a Deconstructor function */
        private bool CheckIfDeconstructor(string[] functionIdentifier)
        {
            int functionRequirement = 0;
            // [0-1]: ~, [1-2]: name == class.Name [2-3]: open parenthesis, [3-4] close paranethesis
            // [4]: all requirements fulfilled for *deconstructor* functions

            string modifiers = "";
            string returnType = "";
            string name = "";
            List<string> parameters = new List<string>();
            string baseParameters = "";

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                switch (functionRequirement)
                {
                    case 0:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("~"))
                        {
                            functionRequirement = 1;
                            break;
                        }
                        functionRequirement = -1; // failed deconstructor syntax
                        break;
                    case 1:
                        if (text.Equals(" ")) continue;
                        if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass))
                        {
                            string className = typeStack.Peek().Name;
                            if (className.Contains("<"))
                                className = className.Substring(0, className.IndexOf("<") + 1);
                            else if (className.Contains("["))
                                className = className.Substring(0, className.IndexOf("[") + 1);
                            if (className.Equals(text))
                            {
                                functionRequirement = 2;
                                break;
                            }
                        }
                        functionRequirement = -1; // failed deconstructor syntax
                        break;
                    case 2:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            functionRequirement = 3;
                            break;
                        }
                        functionRequirement = -1; // failed deconstructor syntax
                        break;
                    case 3:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")"))
                        {
                            functionRequirement = 4;
                            break;
                        }
                        functionRequirement = -1; // failed deconstructor syntax
                        break;
                    case 4:
                        if (text.Equals(" ")) continue;
                        functionRequirement = -1;
                        break;
                }
            }

            if (functionRequirement == 4) // deconstructor signature detected
            {
                this.RemoveFunctionSignatureFromTextData(functionIdentifier.Length);
                stringBuilder.Clear();

                ProgramFunction programFunction = new ProgramFunction(name, modifiers, returnType, parameters, baseParameters);

                // add new function to its parent's ChildList
                if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programFunction);
                else programFile.ChildList.Add(programFunction);

                // add the function and scope to scopeStack
                scopeStack.Push("function");
                scopeStack.Push("{");

                typeStack.Push(programFunction); // push the class onto typeStack
                return true;
            }

            return false;
        }

        /* Used to detect the syntax for a Constructor function */
        // TODO: redo to check for a single-word Constructor function
        private bool CheckIfConstructor(string[] functionIdentifier)
        {
            int functionRequirement = 0;
            // (Optional: modifiers), [0-1]: name, [1-2]: open parenthesis & name == ClassName, (Optional: parameters), 
            // [2-3]: close parenthesis, OPTIONAL: [3-4]: colon, [4-5]: "base" keyword, [5-6]: open paranthesis,
            // (Optional: parameters), [6-7]: close parenthesis, [7]: all requirements fulfilled for *constructor* functions

            string modifiers = "";
            string returnType = "";
            string name = "";
            List<string> parameters = new List<string>();
            string baseParameters = "";

            int parentheses = 0;            // ensure the same number of opening and closing parentheses
            int brackets = 0;
            int periods = 0;

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("[") || text.Equals("<")) brackets++;
                else if (text.Equals(".")) periods++;

                switch (functionRequirement)
                {
                    case 0:
                        if (text.Equals(" ")) continue;
                        if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                        {
                            name = text;
                            functionRequirement = 1;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 1:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass))
                            {
                                string className = typeStack.Peek().Name;
                                if (className.Contains("<"))
                                    className = className.Substring(0, className.IndexOf("<") + 1);
                                else if (className.Contains("["))
                                    className = className.Substring(0, className.IndexOf("[") + 1);
                                if (className.Equals(name))
                                {
                                    functionRequirement = 2;
                                    break;
                                }
                            }
                            functionRequirement = -1;
                            break;
                        }
                        if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                        {
                            if (modifiers.Length > 0) modifiers += " ";
                            modifiers += name;
                            name = text;
                            break;
                        }
                        if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals("<") || text.Equals("[") || text.Equals(">") || text.Equals("]"))
                        {
                            name += text;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 2:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")") && parentheses == 0)
                        {
                            functionRequirement = 3;
                            break;
                        }
                        parameters.Add(text);
                        break;
                    case 3:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(":"))
                        {
                            baseParameters = text;
                            functionRequirement = 4;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 4:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("base"))
                        {
                            baseParameters = " " + text;
                            functionRequirement = 5;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 5:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            baseParameters += text;
                            functionRequirement = 6;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 6:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")"))
                        {
                            baseParameters += text;
                            functionRequirement = 7;
                            break;
                        }
                        if (!baseParameters[baseParameters.Length - 1].Equals('(') && !text.Equals(",")) baseParameters += " ";
                        baseParameters += text;
                        break;
                    case 7:
                        if (text.Equals(" ")) continue;
                        functionRequirement = -1; // failed constructor syntax
                        break;
                }
                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 3 || functionRequirement == 7) // constructor signature detected
            {
                this.RemoveFunctionSignatureFromTextData(functionIdentifier.Length);
                stringBuilder.Clear();

                ProgramFunction programFunction = new ProgramFunction(name, modifiers, returnType, parameters, baseParameters);

                // add new function to its parent's ChildList
                if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programFunction);
                else programFile.ChildList.Add(programFunction);

                // add the function and scope to scopeStack
                scopeStack.Push("function");
                scopeStack.Push("{");

                typeStack.Push(programFunction); // push the class onto typeStack

                return true;
            }

            return false;
        }


        /* ---------- Scope Detectors (within functions) ---------- */

        private bool CheckScopes(string entry)
        {
            bool scopeOpener = false;

            bool ifScopeChecked = false;
            bool elseIfScopeChecked = false;
            bool elseScopeChecked = false;
            bool forScopeChecked = false;
            bool forEachScopeChecked = false;
            bool whileScopeChecked = false;
            bool doWhileScopeChecked = false;
            bool switchScopeChecked = false;

            /* ---------- Check "if" scope ---------- */
            if (ifScope > 0)
            {
                if (this.CheckIfScope(entry))
                    scopeOpener = true;
                ifScopeChecked = true;
            }

            /* ---------- Check "else if" scope ---------- */
            if (elseIfScope > 0)
            {
                if (this.CheckElseIfScope(entry))
                    scopeOpener = true;
                elseIfScopeChecked = true;
            }

            /* ---------- Check "else" scope ---------- */
            if (elseScope > 0)
            {
                if (this.CheckElseScope(entry))
                    scopeOpener = true;
                elseScopeChecked = true;
            }

            /* ---------- Check "for" scope ---------- */
            if (forScope > 0)
            {
                if (this.CheckForScope(entry))
                    scopeOpener = true;
                forScopeChecked = true;
            }

            /* ---------- Check "foreach" scope ---------- */
            if (forEachScope > 0)
            {
                if (this.CheckForEachScope(entry))
                    scopeOpener = true;
                forEachScopeChecked = true;
            }

            /* ---------- Check "while" scope ---------- */
            if (whileScope > 0)
            {
                if (this.CheckWhileScope(entry))
                    scopeOpener = true;
                whileScopeChecked = true;
            }

            /* ---------- Check "do while" scope ---------- */
            if (doWhileScope > 0)
            {
                if (this.CheckDoWhileScope(entry))
                    scopeOpener = true;
                doWhileScopeChecked = true;
            }

            /* ---------- Check "switch" scope ---------- */
            if (switchScope > 0)
            {
                if (this.CheckSwitchScope(entry))
                    scopeOpener = true;
                switchScopeChecked = true;
            }

            /* ---------- Check "if" scope ---------- */
            if (!ifScopeChecked)
                if (this.CheckIfScope(entry))
                    scopeOpener = true;

            /* ---------- Check "else if" scope ---------- */
            if (!elseIfScopeChecked)
                if (this.CheckElseIfScope(entry))
                    scopeOpener = true;

            /* ---------- Check "else" scope ---------- */
            if (!elseScopeChecked)
                if (this.CheckElseScope(entry))
                    scopeOpener = true;

            /* ---------- Check "for" scope ---------- */
            if (!forScopeChecked)
                if (this.CheckForScope(entry))
                    scopeOpener = true;

            /* ---------- Check "foreach" scope ---------- */
            if (!forEachScopeChecked)
                if (this.CheckForEachScope(entry))
                    scopeOpener = true;

            /* ---------- Check "while" scope ---------- */
            if (!whileScopeChecked)
                if (this.CheckWhileScope(entry))
                    scopeOpener = true;

            /* ---------- Check "do while" scope ---------- */
            if (!doWhileScopeChecked)
                if (this.CheckDoWhileScope(entry))
                    scopeOpener = true;

            /* ---------- Check "switch" scope ---------- */
            if (!switchScopeChecked)
                if (this.CheckSwitchScope(entry))
                    scopeOpener = true;

            return scopeOpener;
        }

        private bool CheckIfScope(string entry)
        {
            switch (ifScope)
            {
                case 0:
                    if (entry.Equals("if") && elseIfScope == 0)
                    {
                        ifScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("(")) ifScope = 2;
                    else ifScope = 0;
                    break;
                case 2:
                    ifScope = 3;
                    break;
                case 3:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count)
                    {

                        ifScope = 4;
                    }
                    break;
                case 4:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("if");
                    stringBuilder.Clear();
                    if (entry.Equals("if") && elseIfScope == 0) // check if the next statement starts an "if"
                    {
                        ifScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else ifScope = 0; // reset the ifScope rule
                    return true;
            }
            return false;
        }

        private bool CheckElseIfScope(string entry)
        {
            switch (elseIfScope)
            {
                case 0:
                    if (entry.Equals("else"))
                    {
                        elseIfScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("if")) elseIfScope = 2;
                    else elseIfScope = 0;
                    break;
                case 2:
                    if (entry.Equals("(")) elseIfScope = 3;
                    else elseIfScope = 0;
                    break;
                case 3:
                    elseIfScope = 4;
                    break;
                case 4:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) elseIfScope = 5;
                    break;
                case 5:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("else if");
                    stringBuilder.Clear();
                    if (entry.Equals("else")) // check if the next statement starts an "else if"
                    {
                        elseIfScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else elseIfScope = 0; // reset the elseIfScope rule
                    return true;
            }
            return false;
        }

        private bool CheckElseScope(string entry)
        {
            switch (elseScope)
            {
                case 0:
                    if (entry.Equals("else")) elseScope = 1;
                    break;
                case 1:
                    if (entry.Equals("if"))
                    {
                        elseScope = 0;
                        break;
                    }
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("else");
                    stringBuilder.Clear();
                    if (entry.Equals("else")) // check if the next statement starts an "else"
                    {
                        elseScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else elseScope = 0; // reset the elseScope rule
                    return true;
            }
            return false;
        }

        private bool CheckForScope(string entry)
        {
            switch (forScope)
            {
                case 0:
                    if (entry.Equals("for"))
                    {
                        forScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("(")) forScope = 2;
                    else forScope = 0;
                    break;
                case 2:
                    forScope = 3;
                    break;
                case 3:
                    if (entry.Equals(";")) forScope = 4;
                    break;
                case 4:
                    forScope = 5;
                    break;
                case 5:
                    if (entry.Equals(";")) forScope = 6;
                    break;
                case 6:
                    forScope = 7;
                    break;
                case 7:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) forScope = 8;
                    break;
                case 8:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("for");
                    stringBuilder.Clear();
                    if (entry.Equals("for")) // check if the next statement starts a "for"
                    {
                        forScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else forScope = 0; // reset the forScope rule
                    return true;
            }
            return false;
        }

        private bool CheckForEachScope(string entry)
        {
            switch (forEachScope)
            {
                case 0:
                    if (entry.Equals("foreach"))
                    {
                        forEachScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("(")) forEachScope = 2;
                    else forEachScope = 0;
                    break;
                case 2:
                    forEachScope = 3;
                    break;
                case 3:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) forEachScope = 4;
                    break;
                case 4:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("foreach");
                    stringBuilder.Clear();
                    if (entry.Equals("foreach")) // check if the next statement starts a "foreach"
                    {
                        forEachScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else forEachScope = 0; // reset the forEachScope rule
                    return true;
            }
            return false;
        }

        private bool CheckWhileScope(string entry)
        {
            switch (whileScope)
            {
                case 0:
                    if (entry.Equals("while"))
                    {
                        whileScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("(")) whileScope = 2;
                    else whileScope = 0;
                    break;
                case 2:
                    whileScope = 3;
                    break;
                case 3:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) whileScope = 4;
                    break;
                case 4:
                    if (entry.Equals(";"))
                    {
                        whileScope = 0;
                        break;
                    }
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("while");
                    stringBuilder.Clear();
                    if (entry.Equals("while")) // check if the next statement starts a "while"
                    {
                        whileScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    else whileScope = 0; // reset the whileScope rule
                    return true;
            }
            return false;
        }

        private bool CheckDoWhileScope(string entry)
        {
            switch (doWhileScope)
            {
                case 0:
                    if (entry.Equals("do")) doWhileScope = 1;
                    break;
                case 1:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("do while");
                    stringBuilder.Clear();
                    if (entry.Equals("do")) doWhileScope = 1; // check if the next statement starts a "do while"
                    else doWhileScope = 0; // reset the doWhileScope rule
                    return true;
            }
            return false;
        }

        private bool CheckSwitchScope(string entry)
        {
            switch (switchScope)
            {
                case 0:
                    if (entry.Equals("switch"))
                    {
                        switchScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1:
                    if (entry.Equals("(")) switchScope = 2;
                    else switchScope = 0;
                    break;
                case 2:
                    switchScope = 3;
                    break;
                case 3:
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) switchScope = 4;
                    break;
                case 4:
                    if (!entry.Equals("{"))
                    {
                        switchScope = 0;
                        break;
                    }
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("switch");
                    stringBuilder.Clear();
                    switchScope = 0; // reset the switchScope rule
                    return true;
            }
            return false;
        }


        /* ---------- General Upkeep ---------- */

        private bool IsComment(string entry)
        {
            if (scopeStack.Count() > 0)
            {
                if (scopeStack.Peek().Equals("\""))
                {
                    if (entry.Equals("\"")) scopeStack.Pop();
                    return true;
                }
                if (scopeStack.Peek().Equals("'"))
                {
                    if (entry.Equals("'")) scopeStack.Pop();
                    return true;
                }
                if (scopeStack.Peek().Equals("//"))
                {
                    if (entry.Equals(" "))
                    {
                        scopeStack.Pop();
                        return false;
                    }
                    return true;
                }
                if (scopeStack.Peek().Equals("/*"))
                {
                    if (entry.Equals("*/")) scopeStack.Pop();
                    return true;
                }
            }
            return false;
        }

        private bool StartPlainTextArea(string entry)
        {
            if (entry.Equals("\"") || entry.Equals("'") || entry.Equals("//") || entry.Equals("/*"))
            {
                scopeStack.Push(entry);
                return true;
            }
            return false;
        }

        /* Updates the StringBuilder in the case of a new statement or scope */
        private void UpdateStringBuilder(string entry)
        {
            if (!entry.Equals(" "))
            {
                if (entry.Equals(";") || entry.Equals("}") || entry.Equals("{"))
                {
                    stringBuilder.Clear();
                    return;
                }
                if (stringBuilder.Length > 0) stringBuilder.Append(" ");
                stringBuilder.Append(entry);
            }
        }

        private void RemoveFunctionSignatureFromTextData(int size)
        {
            if (typeStack.Count > 0 && (typeStack.Peek().GetType() == typeof(ProgramClass)
                || typeStack.Peek().GetType() == typeof(ProgramInterface) || typeStack.Peek().GetType() == typeof(ProgramFunction)))
            {
                int textDataIndex = ((ProgramDataType)typeStack.Peek()).TextData.Count - 1; // last index of TextData
                while (textDataIndex >= 0 && !((ProgramDataType)typeStack.Peek()).TextData[textDataIndex].Equals(")")) textDataIndex--; // get the index of the last closing parentheses
                size += ((ProgramDataType)typeStack.Peek()).TextData.Count - textDataIndex - 1;
                ((ProgramDataType)typeStack.Peek()).TextData
                    = ((ProgramDataType)typeStack.Peek()).TextData.GetRange(0, ((ProgramDataType)typeStack.Peek()).TextData.Count - size); // remove function signature
            }
        }
    }

    class RelationshipProcessor
    {
        ProgramClassType programClassType;
        ProgramClassTypeCollection programClassTypeCollection;

        public RelationshipProcessor(ProgramClassType programClassType, ProgramClassTypeCollection programClassTypeCollection)
        {
            this.programClassType = programClassType;
            this.programClassTypeCollection = programClassTypeCollection;
        }
        
        public void ProcessRelationships()
        {
            this.SetInheritanceRelationships(); // get the superclass/subclass data from the beginning of the class text

            if (programClassType.GetType() != typeof(ProgramClass)) return; // interfaces only collect inheritance data

            // (1) get the aggregation data from the class text and text of all children
            // (2) get the using data from the parameters fields of all child functions
            this.SetAggregationAndUsingRelationships(this.programClassType);
        }

        private void SetInheritanceRelationships()
        {
            string entry;
            int index;
            bool hasSuperclasses = false;
            int brackets = 0;

            for (index = 0;  index < programClassType.TextData.Count; index++)
            {
                entry = programClassType.TextData[index];

                if (!hasSuperclasses)
                {
                    if (entry.Equals(":"))
                    {
                        hasSuperclasses = true;
                        continue;
                    }
                }

                if (entry.Equals("{"))
                {
                    programClassType.TextData = programClassType.TextData.GetRange(++index, programClassType.TextData.Count - index);
                    return;
                }

                if (entry.Equals("[") || entry.Equals("<"))
                {
                    brackets++;
                    continue;
                }

                if (entry.Equals("]") || entry.Equals(">"))
                {
                    brackets--;
                    continue;
                }

                if (brackets > 0) continue;

                if (hasSuperclasses)
                {
                    if (programClassType.Name != entry && programClassTypeCollection.Contains(entry))
                    {
                        ProgramClassType super = programClassTypeCollection[entry];
                        super.SubClasses.Add(programClassType);
                        programClassType.SuperClasses.Add(super);
                        programClassType.TextData.RemoveAt(index);
                    }
                }
            }
        }

        private void SetAggregationAndUsingRelationships(ProgramDataType programDataType)
        {
            // get the aggregation data
            foreach (string entry in programDataType.TextData)
            {
                if (programClassType.Name != entry && programClassTypeCollection.Contains(entry))
                {
                    ProgramClassType owned = programClassTypeCollection[entry];
                    if (!((ProgramClass)programClassType).OwnedClasses.Contains(owned) && owned.GetType() == typeof(ProgramClass))
                    {
                        ((ProgramClass)owned).OwnedByClasses.Add(programClassType);
                        ((ProgramClass)programClassType).OwnedClasses.Add(owned);
                    }
                }
            }

            // get the using data
            if (programDataType.GetType() == typeof(ProgramFunction))
            {
                if (((ProgramFunction)programDataType).Parameters.Count > 0)
                {
                    foreach (string parameter in ((ProgramFunction)programDataType).Parameters)
                    {
                        if (!programClassType.Name.Equals(parameter) && programClassTypeCollection.Contains(parameter))
                        {
                            ProgramClassType used = programClassTypeCollection[parameter];
                            if (!((ProgramClass)programClassType).UsedClasses.Contains(used))
                            {
                                used.UsedByClasses.Add(programClassType);
                                ((ProgramClass)programClassType).UsedClasses.Add(used);
                            }
                        }
                    }
                }
            }

            // repeat for each child, grandchild, etc.
            foreach (ProgramType programType in programDataType.ChildList)
            {
                if (programType.GetType() == typeof(ProgramClass) || programType.GetType() == typeof(ProgramFunction))
                {
                    this.SetAggregationAndUsingRelationships((ProgramDataType)programType);
                }
            }
        }
    }
}
