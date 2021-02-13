using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /* Preprocesses file text into a list of strings */
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
        /* Saved references from input arguments */
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

                if (entry.Equals("class") || entry.Equals("interface")) // Check for a new class or interface
                {
                    index = this.NewProgramClassType(entry, index);
                    continue;
                }

                if (entry.Equals("{")) // Push scope opener onto scopeStack
                    scopeStack.Push(entry);

                this.UpdateStringBuilder(entry);
            }
        }

        /* Processes data within a ProgramClassType (class or interface) scope but outside of a function */
        private int ProcessProgramClassTypeData(string scopeType, int i)
        {
            string entry;
            int index;

            for (index = i; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                // Determine whether to ignore the entry (if it's part of a comment)
                if (this.IgnoreEntry(entry)) continue;

                this.UpdateTextData(entry); // Add entry to current ProgramDataType's text list for relationship analysis

                if (entry.Equals("}")) // Check for the end of an existing bracketed scope
                {
                    if (this.EndBracketedScope(scopeType))
                        return index;
                    continue;
                }

                if (entry.Equals("class") || entry.Equals("interface")) // Check for a new class or interface
                {
                    index = this.NewProgramClassType(entry, index);
                    continue;
                }

                if (entry.Equals("{"))
                    if (this.CheckIfFunction()) // Check if a new function is being started
                    {
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                    else // Push scope opener onto scopeStack
                        scopeStack.Push(entry);

                this.UpdateStringBuilder(entry);
            }

            return index;
        }

        /* Processes data within a Function scope */
        private int ProcessFunctionData(int i) // TODO: don't increment size by counting new lines if TextData is empty
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

                this.UpdateTextData(entry); // Add entry to current Function's text list for relationship analysis

                if (entry.Equals(" ")) this.IncrementFunctionSize(); // Check for a new line and update function data

                // Check for closing parenthesis
                if (entry.Equals(")") && scopeStack.Count > 0 && scopeStack.Peek().Equals("(")) scopeStack.Pop();

                if (entry.Equals("}")) // Check for the end of an existing bracketed scope
                {
                    if (this.EndBracketedScope("function"))
                        return index;
                    continue;
                }

                // Check control flow scope openers
                if (!entry.Equals(" ") && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction) && this.CheckScopes(entry))
                    scopeOpener = true;

                if (entry.Equals("(")) scopeStack.Push(entry); // Check for open parenthesis

                if (entry.Equals("{"))
                    if (!scopeOpener && this.CheckIfFunction()) // Check if a new function is being started
                    {
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                    else // Push scope opener onto scopeStack
                        scopeStack.Push(entry);

                if (entry.Equals(";")) this.EndBracketlessScope(); // Check for the end of an existing bracketless scope

                this.UpdateStringBuilder(entry);
            }

            return index;
        }

        /* Creates a new namespace object and adds it as a child to the current type */
        private int NewNamespace(int index)
        {
            string entry;

            scopeStack.Push("namespace"); // Push the namespace scope opener onto scopeStack
            stringBuilder.Clear();

            while (++index < programFile.FileTextData.Count) // Get the name of the namespace
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

            // Add new namespace to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programNamespace);
            else programFile.ChildList.Add(programNamespace);

            typeStack.Push(programNamespace);
            return index;
        }

        /* Creates a new class or interface object, adds it as a child to the current type, and sends it to its analyzer */
        private int NewProgramClassType(string type, int index)
        {
            ProgramClassType programClassType;
            string entry;
            string modifiers = stringBuilder.ToString();   // Get the modifiers
            List<string> textData = new List<string>();

            scopeStack.Push(type); // Push the type of scope opener (class or interface)
            stringBuilder.Clear();

            while (++index < programFile.FileTextData.Count) // Get the name of the class/interface
            {
                entry = programFile.FileTextData[index];

                // Determine whether to ignore the entry (if it's part of a comment)
                if (this.IgnoreEntry(entry)) continue;

                textData.Add(entry); // Add entry to class's/interface's TextData list

                if (entry.Equals("{"))
                {
                    scopeStack.Push("{"); // Push the scope opener bracket
                    break;
                }

                if (stringBuilder.Length == 0) // The next entry after "class" or "interface" will be the name
                    if (!entry.Equals(" ")) stringBuilder.Append(entry);
            }

            // Create the new class or interface object
            if (type.Equals("class"))
                programClassType = this.NewClass(modifiers);
            else
                programClassType = this.NewInterface(modifiers);

            // Add text/inheritance data, and add class/interface to general ProgramClassType list
            programClassType.TextData = textData;
            programClassTypes.Add(programClassType);

            // Add new class/interface to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programClassType);
            else programFile.ChildList.Add(programClassType);

            typeStack.Push(programClassType);

            return this.ProcessProgramClassTypeData(type, ++index); // Send to method to analyze inside of a class/interface
        }

        /* Creates a new class object and adds it as a child to the current type */
        private ProgramClass NewClass(string modifiers)
        {
            ProgramClass programClass = new ProgramClass(stringBuilder.ToString(), modifiers);
            stringBuilder.Clear();
            return programClass;
        }

        /* Creates a new interface object and adds it as a child to the current type */
        private ProgramInterface NewInterface(string modifiers)
        {
            ProgramInterface programInterface = new ProgramInterface(stringBuilder.ToString(), modifiers);
            stringBuilder.Clear();
            return programInterface;
        }

        private void NewFunction(string[] functionIdentifier, string name, string modifiers, string returnType, List<string> parameters, string baseParameters)
        {
            this.RemoveFunctionSignatureFromTextData(functionIdentifier.Length);
            stringBuilder.Clear();

            ProgramFunction programFunction = new ProgramFunction(name, modifiers, returnType, parameters, baseParameters);

            // Add new function to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programFunction);
            else programFile.ChildList.Add(programFunction);

            // Add the function and scope to scopeStack
            scopeStack.Push("function");
            scopeStack.Push("{");

            typeStack.Push(programFunction);
        }

        /* Detects the syntax for a normal function signature */
        private bool CheckIfFunction()
        {
            string[] functionIdentifier = stringBuilder.ToString().Split(' ');

            // The function requirement to check next. If this ends at 4, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;
            
            string modifiers = "";
            string returnType = "";
            string name = "";
            List<string> parameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int brackets = 0;
            int periods = 0; // Used for formatting

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("[") || text.Equals("<")) brackets++;
                else if (text.Equals(".")) periods++;

                // Test the current requirement
                functionRequirement = this.TestFunctionRequirement(functionRequirement, text, ref modifiers, ref returnType, ref name, ref parameters, brackets, parentheses, periods);

                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 4) // Function signature detected - create a new function
            {
                this.NewFunction(functionIdentifier, name, modifiers, returnType, parameters, "");
                return true;
            }
            // If it failed normal function requirements, check rules for constructors and deconstructors
            else if (this.CheckIfConstructor(functionIdentifier)) return true;
            else if (this.CheckIfDeconstructor(functionIdentifier)) return true;

            return false;
        }

        /* Detects the syntax for a Constructor function */
        private bool CheckIfConstructor(string[] functionIdentifier)
        {
            // The constructor requirement to check next. If this ends at 3 or 7, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string modifiers = "";
            string name = "";
            List<string> parameters = new List<string>();
            string baseParameters = "";

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int brackets = 0;
            int periods = 0; // Used for formatting

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("[") || text.Equals("<")) brackets++;
                else if (text.Equals(".")) periods++;

                // Test the current requirement
                functionRequirement = this.TestConstructorRequirement(functionRequirement, text, ref modifiers, ref name, ref parameters, ref baseParameters, brackets, parentheses, periods);

                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 3 || functionRequirement == 7) // Constructor signature detected - create a new function
            {
                this.NewFunction(functionIdentifier, name, modifiers, "", parameters, baseParameters);
                return true;
            }

            return false;
        }

        /* Detects the syntax for a Deconstructor function */
        private bool CheckIfDeconstructor(string[] functionIdentifier)
        {
            // The deconstructor requirement to check next. If this ends at 4, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                // Test the current requirement
                functionRequirement = this.TestDeconstructorRequirement(functionRequirement, text, ref name);
            }

            if (functionRequirement == 4) // Deconstructor signature detected - create a new function
            {
                this.NewFunction(functionIdentifier, name, "", "", new List<string>(), "");
                return true;
            }

            return false;
        }

        /* Tests each step of normal function requirements */
        private int TestFunctionRequirement(int functionRequirement, string text, ref string modifiers, ref string returnType, ref string name, ref List<string> parameters, int brackets, int parentheses, int periods)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find text entry that could be a return type.
                    functionRequirement = this.FunctionStep0(text, ref returnType);
                    break;
                case 1: // To pass: Find text entry that could be a name.
                    functionRequirement = this.FunctionStep1(text, ref returnType, ref name, brackets, periods);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = this.FunctionStep2(text, ref modifiers, ref returnType, ref name, brackets, periods);
                    break;
                case 3: // To pass: Find closing parenthesis.
                    functionRequirement = this.FunctionStep3(text, ref parameters, parentheses);
                    break;
                case 4: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = this.FunctionStep4(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests each step of constructor function requirements */
        private int TestConstructorRequirement(int functionRequirement, string text, ref string modifiers, ref string name, ref List<string> parameters, ref string baseParameters, int brackets, int parentheses, int periods)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: The name of function equals name of the class.
                    functionRequirement = this.ConstructorStep0(text, ref name);
                    break;
                case 1: // To pass: Find opening parenthesis.
                    functionRequirement = this.ConstructorStep1(text, ref modifiers, ref name, brackets, periods);
                    break;
                case 2: // To pass: Find closing parenthesis.
                    functionRequirement = this.ConstructorStep2(text, ref parameters, parentheses);
                    break;
                case 3: // To continue: Find colon.
                    functionRequirement = this.ConstructorStep3(text, ref baseParameters);
                    break;
                case 4: // To pass: Find "base".
                    functionRequirement = this.ConstructorStep4(text, ref baseParameters);
                    break;
                case 5: // To pass: Find opening parenthesis.
                    functionRequirement = this.ConstructorStep5(text, ref baseParameters);
                    break;
                case 6: // To pass: Find closing parenthesis.
                    functionRequirement = this.ConstructorStep6(text, ref baseParameters);
                    break;
                case 7: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = this.ConstructorStep7(text);
                    break;
            }

            return functionRequirement;
        }

        /* Tests each step of deconstructor requirements */
        private int TestDeconstructorRequirement(int functionRequirement, string text, ref string name)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find tilde.
                    functionRequirement = this.DeconstructorStep0(text, ref name);
                    break;
                case 1: // To pass: The name of function equals name of the class.
                    functionRequirement = this.DeconstructorStep1(text, ref name);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = this.DeconstructorStep2(text);
                    break;
                case 3: // To pass: Find closing parenthesis.
                    functionRequirement = this.DeconstructorStep3(text);
                    break;
                case 4: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = this.DeconstructorStep4(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests step 0 of function syntax: Find text entry that could be a return type (1 = success, -1 = fail) */
        private int FunctionStep0(string text, ref string returnType)
        {
            if (text.Equals(" ")) return 0;

            if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                returnType = text;
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of function syntax: Find text entry that could be a name (2 = success, -1 = fail) */
        private int FunctionStep1(string text, ref string returnType, ref string name, int brackets, int periods)
        {
            if (text.Equals(" ")) return 1;

            if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                name = text;
                return 2;
            }

            if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals("<") || text.Equals("[") || text.Equals(">") || text.Equals("]"))
            {
                if (name.Length == 0)
                    returnType += text;
                else
                    name += text;
                return 1;
            }

            return -1;
        }

        /* Tests step 2 of function syntax: Find opening parenthesis (3 = success, -1 = fail) */
        private int FunctionStep2(string text, ref string modifiers, ref string returnType, ref string name, int brackets, int periods)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals("("))
                return 3;

            if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                if (modifiers.Length > 0) modifiers += " ";
                modifiers += returnType;
                returnType = name;
                name = text;
                return 2;
            }

            if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals(">") || text.Equals("]"))
            {
                if (name.Length == 0)
                    returnType += text;
                else
                    name += text;
                return 2;
            }

            return -1;
        }

        /* Tests step 3 of function syntax: Find closing parenthesis (4 = success) */
        private int FunctionStep3(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(")") && parentheses == 0)
                return 4;

            parameters.Add(text);
            return 3;
        }

        /* Tests step 4 of function syntax: Find no more text after closing parenthesis (-1 = fail) */
        private int FunctionStep4(string text)
        {
            if (text.Equals(" ")) return 4;
            return -1;
        }

        /* Tests step 0 of constructor syntax: The name of function equals name of the class (1 = success, -1 = fail) */
        private int ConstructorStep0(string text, ref string name)
        {
            if (text.Equals(" ")) return 0;

            if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                name = text;
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of constructor syntax: Find opening parenthesis (2 = success, -1 = fail) */
        private int ConstructorStep1(string text, ref string modifiers, ref string name, int brackets, int periods)
        {
            if (text.Equals(" ")) return 1;

            if (text.Equals("("))
            {
                if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(name))
                    return 2;
                return -1;
            }

            if (brackets == 0 && periods == 0 && (!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                if (modifiers.Length > 0) modifiers += " ";
                modifiers += name;
                name = text;
                return 1;
            }

            if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals(">") || text.Equals("]"))
            {
                name += text;
                return 1;
            }

            return -1;
        }

        /* Tests step 2 of constructor syntax: Find closing parenthesis (-1 = fail) */
        private int ConstructorStep2(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals(")") && parentheses == 0)
                return 3;

            parameters.Add(text);
            return 2;
        }

        /* Tests step 3 of constructor syntax: Find colon (4 = success) */
        private int ConstructorStep3(string text, ref string baseParameters)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(":"))
            {
                baseParameters = text;
                return 4;
            }

            return -1;
        }

        /* Tests step 4 of constructor syntax: Find "base" (5 = success, -1 = fail) */
        private int ConstructorStep4(string text, ref string baseParameters)
        {
            if (text.Equals(" ")) return 4;

            if (text.Equals("base"))
            {
                baseParameters = " " + text;
                return 5;
            }

            return -1;
        }

        /* Tests step 5 of constructor syntax: Find opening parenthesis (6 = success, -1 = fail) */
        private int ConstructorStep5(string text, ref string baseParameters)
        {
            if (text.Equals(" ")) return 5;

            if (text.Equals("("))
            {
                baseParameters += text;
                return 6;
            }

            return -1;
        }

        /* Tests step 6 of constructor syntax: Find closing parenthesis (7 = success) */
        private int ConstructorStep6(string text, ref string baseParameters)
        {
            if (text.Equals(" ")) return 6;

            if (text.Equals(")"))
            {
                baseParameters += text;
                return 7;
            }

            if (!baseParameters[baseParameters.Length - 1].Equals('(') && !text.Equals(","))
                baseParameters += " ";
            baseParameters += text;
            return 6;
        }

        /* Tests step 7 of constructor syntax: Find no more text after closing parenthesis (-1 = fail) */
        private int ConstructorStep7(string text)
        {
            if (text.Equals(" ")) return 7;
            return -1;
        }

        /* Tests step 0 of deconstructor syntax: Find tilde (1 = success, -1 = fail) */
        private int DeconstructorStep0(string text, ref string name)
        {
            if (text.Equals(" ")) return 0;

            if (text.Equals("~"))
            {
                name = text;
                return 1;
            }
            
            return -1;
        }

        /* Tests step 1 of deconstructor syntax: The name of function equals name of the class (2 = success, -1 = fail) */
        private int DeconstructorStep1(string text, ref string name)
        {
            if (text.Equals(" ")) return 1;

            if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(text))
            {
                name += text;
                return 2;
            }
            
            return -1;
        }

        /* Tests step 2 of deconstructor syntax: Find opening parenthesis (3 = success, -1 = fail) */
        private int DeconstructorStep2(string text)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals("("))
                return 3;
            
            return -1;
        }

        /* Tests step 3 of deconstructor syntax: Find closing parenthesis (4 = success, -1 = fail) */
        private int DeconstructorStep3(string text)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(")"))
                return 4;
            
            return -1;
        }

        /* Tests step 4 of deconstructor syntax: Find no more text after closing parenthesis (-1 = fail) */
        private int DeconstructorStep4(string text)
        {
            if (text.Equals(" ")) return 4;
            return -1;
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
            bool isScopeType = false;

            if (scopeStack.Count > 0) // Pop the scope opener
                if (scopeStack.Peek().Equals("{")) scopeStack.Pop();

            if (scopeStack.Count > 0)
            {
                if (scopeStack.Peek().Equals("function") && typeStack.Count > 0 && ((ProgramFunction)typeStack.Peek()).Size > 0)
                    ((ProgramFunction)typeStack.Peek()).Size--; // The last line of a function is usually just the closing bracket

                if (scopeStack.Peek().Equals(scopeType))
                    isScopeType = true;

                // If there is a different ProgramType-level scope identifier, pop the identifier and the type
                if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("function"))
                {
                    scopeStack.Pop();
                    if (typeStack.Count > 0) typeStack.Pop();
                }
                else // If ending at least one other named scope
                    while (scopeStack.Count > 0 && (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                            || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                            || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch")))
                        scopeStack.Pop();
            }

            stringBuilder.Clear();
            return isScopeType;
        }

        /* Maintains the stack when a bracketless scope ends; returns true if the scope is a function */
        private bool EndBracketlessScope()
        {
            bool isFunction = false;
            if (forScope == 0 && scopeStack.Count > 0 && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
            {
                while (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                    || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                    || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch") || scopeStack.Peek().Equals("function"))
                {
                    scopeStack.Pop();
                    if (scopeStack.Peek().Equals("function"))
                    {
                        if (typeStack.Count > 0) typeStack.Pop();
                        isFunction = true;
                    }
                }
            }
            stringBuilder.Clear();
            return isFunction;
        }

        /* Add entry to current ProgramDataType's text list for classes, interfaces, and functions */
        private void UpdateTextData(string entry)
        {
            if (typeStack.Count > 0 && (typeStack.Peek().GetType() == typeof(ProgramClass)
                    || typeStack.Peek().GetType() == typeof(ProgramInterface) 
                    || typeStack.Peek().GetType() == typeof(ProgramFunction)))
                ((ProgramDataType)typeStack.Peek()).TextData.Add(entry);
        }

        /* Increments the current function's size, if possible and appropriate */
        private void IncrementFunctionSize()
        {
            if (typeStack.Peek().GetType() == typeof(ProgramFunction) && ((ProgramFunction)typeStack.Peek()).TextData.Count > 0)
                ((ProgramFunction)typeStack.Peek()).Size++;
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
