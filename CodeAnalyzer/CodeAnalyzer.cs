/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  CodeAnalyzer.cs - Analyzes all C# code input from a file, stores analysis data   ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeAnalyzer
{
    /* Pre-processor of file text into a list of strings */
    public class FileProcessor
    {
        private ProgramFile programFile;
        private StringBuilder stringBuilder = new StringBuilder();

        public FileProcessor(ProgramFile programFile) => this.programFile = programFile;

        /* Puts the file next into a string list, dividing elements logically */
        public void ProcessFile()
        {
            IEnumerator enumerator;

            // Split text by line
            string[] programLines = programFile.FileText.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < programLines.Length; i++)
            {
                enumerator = programLines[i].GetEnumerator();

                while (enumerator.MoveNext()) // Read the line char by char
                {
                    if (Char.IsWhiteSpace((char)enumerator.Current)) // Add the element to the FileTextData list
                    {
                        this.AddEntryToFileTextData();
                        continue;
                    }

                    // Check special cases
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
                programFile.FileTextData.Add(" "); // Marker for a new line
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

    /* Processor of the file data (except relationship data), filling all internal Child ProgramType lists */
    public class CodeProcessor
    {
        /* Saved references from input arguments */
        private ProgramClassTypeCollection programClassTypes;
        private ProgramFile programFile;

        /* Stacks to keep track of the current scope */
        private readonly Stack<string> scopeStack = new Stack<string>();
        private readonly Stack<ProgramType> typeStack = new Stack<ProgramType>();

        private readonly StringBuilder stringBuilder = new StringBuilder("");

        /* Scope syntax rules to check for */
        int ifScope = 0;
        int elseIfScope = 0;
        int elseScope = 0;
        int forScope = 0;
        int forEachScope = 0;
        int whileScope = 0;
        int doWhileScope = 0;
        int switchScope = 0;

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
                        scopeStack.Push(entry);
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                    else // Push scope opener onto scopeStack
                        scopeStack.Push(entry);

                if (entry.Equals("=>"))
                {
                    if (this.CheckIfFunction()) // Check if a new lambda function is being started
                    {
                        index = this.ProcessFunctionData(++index);
                        continue;
                    }
                }

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

                this.UpdateTextData(entry); // Add entry to current Function's text list for relationship analysis

                if (entry.Equals(" ")) this.IncrementFunctionSize(); // Check for a new line and update function data

                if (this.CheckScopeClosersWithinFunction(entry, ref index)) // Check for some closing scopes: ")", "}", ";"
                    return index; // Closing scope was the end of this function

                // Check control flow scope openers
                if (!entry.Equals(" ") && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction) && this.CheckControlFlowScopes(entry))
                    scopeOpener = true;

                this.CheckScopeOpenersWithinFunction(entry, scopeOpener, ref index); // Check for some opening scopes: "(", "{", "=>"

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

            // Gather the ProgramClassType data
            (int newIndex, string name, List<string> textData, List<string> modifiers, List<string> generics) = this.GetClassTypeData(type, index);

            // Create the new class or interface object
            if (type.Equals("class")) programClassType = this.NewClass(name, modifiers, generics);
            else programClassType = this.NewInterface(name, modifiers, generics);

            if (programClassTypes.Contains(name))
            {
                Console.WriteLine("\n\nError: Cannot determine data with two classes with the same name.\n\n");
                Environment.Exit(1);
            }

            // Add text/inheritance data, and add class/interface to general ProgramClassType list
            programClassType.TextData = textData;
            programClassTypes.Add(programClassType);

            // Add new class/interface to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programClassType);
            else programFile.ChildList.Add(programClassType);

            typeStack.Push(programClassType);

            return this.ProcessProgramClassTypeData(type, newIndex); // Send to method to analyze inside of a class/interface
        }

        /* Creates a new class object and adds it as a child to the current type */
        private ProgramClass NewClass(string name, List<string> modifiers, List<string> generics)
        {
            ProgramClass programClass = new ProgramClass(name, modifiers, generics);
            stringBuilder.Clear();
            return programClass;
        }

        /* Creates a new interface object and adds it as a child to the current type */
        private ProgramInterface NewInterface(string name, List<string> modifiers, List<string> generics)
        {
            ProgramInterface programInterface = new ProgramInterface(name, modifiers, generics);
            stringBuilder.Clear();
            return programInterface;
        }

        /* Creates a new function object and adds it as a child to the current type */
        private void NewFunction(string[] functionIdentifier, string name, List<string> modifiers, List<string> returnTypes, List<string> generics, List<string> parameters, List<string> baseParameters)
        {
            this.RemoveFunctionSignatureFromTextData(functionIdentifier.Length);
            stringBuilder.Clear();

            ProgramFunction programFunction = new ProgramFunction(name, modifiers, returnTypes, generics, parameters, baseParameters);

            // Add new function to its parent's ChildList
            if (typeStack.Count > 0) typeStack.Peek().ChildList.Add(programFunction);
            else programFile.ChildList.Add(programFunction);

            // Add the function and scope to scopeStack
            scopeStack.Push("function");

            typeStack.Push(programFunction);
        }

        /* Finds all data required to create a new class or interface */
        private (int index, string name, List<string> textData, List<string> modifiers, List<string> generics) GetClassTypeData(string type, int index)
        {
            string entry;
            string name = "";
            List<string> textData = new List<string>();
            string[] modifiersArray = stringBuilder.ToString().Split(' ');
            List<string> modifiers = new List<string>();
            List<string> generics = new List<string>();
            int brackets = 0;

            foreach (string modifier in modifiersArray) // Get the modifiers
                if (modifier.Length > 0) modifiers.Add(modifier);

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

                if (entry.Equals("<")) brackets++;
                else if (entry.Equals(">")) brackets--;
                else if (brackets > 0) generics.Add(entry); // Save any generic types

                if (name.Length == 0) // The next entry after "class" or "interface" will be the name
                    if (!entry.Equals(" ")) name = entry;
            }

            return (++index, name, textData, modifiers, generics);
        }

        /* Detects the syntax for a normal function signature */
        private bool CheckIfFunction()
        {
            string[] functionIdentifier = stringBuilder.ToString().Split(' ');

            // The function requirement to check next. If this ends at 4 or 7, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            List<string> modifiers = new List<string>();
            List<string> returnTypes = new List<string>();
            List<string> generics = new List<string>();
            List<string> parameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int squareBrackets = 0;
            int angleBrackets = 0;

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals("[")) squareBrackets++;
                else if (text.Equals("<")) angleBrackets++;

                // Test the current requirement
                functionRequirement = this.TestFunctionRequirement(functionRequirement, text, ref name, ref modifiers, ref returnTypes, ref generics, ref parameters, squareBrackets, angleBrackets, parentheses/*, periods*/);

                if (text.Equals(")")) parentheses--;
                else if (text.Equals("]")) squareBrackets--;
                else if (text.Equals(">")) angleBrackets--;
            }

            if (functionRequirement == 4 || functionRequirement == 7) // Function signature detected - create a new function
            {
                this.NewFunction(functionIdentifier, name, modifiers, returnTypes, generics, parameters, new List<string>());
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

            List<string> modifiers = new List<string>();
            string name = "";
            List<string> parameters = new List<string>();
            List<string> baseParameters = new List<string>();

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
                this.NewFunction(functionIdentifier, name, modifiers, new List<string>(), new List<string>(), parameters, baseParameters);
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
                this.NewFunction(functionIdentifier, name, new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>());
                return true;
            }

            return false;
        }

        /* Tests each step of normal function requirements */
        private int TestFunctionRequirement(int functionRequirement, string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> generics, ref List<string> parameters, int squareBrackets, int angleBrackets, int parentheses)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find text entry that could be a return type.
                    functionRequirement = this.FunctionStep0(text, ref returnTypes, parentheses);
                    break;
                case 1: // To pass: Find text entry that could be a name.
                    functionRequirement = this.FunctionStep1(text, ref name, ref modifiers, ref returnTypes, parentheses, angleBrackets + squareBrackets);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = this.FunctionStep2(text, ref name, ref modifiers, ref returnTypes, ref generics, squareBrackets, angleBrackets);
                    break;
                case 3: // To pass: Find closing parenthesis.
                    functionRequirement = this.FunctionStep3(text, ref parameters, parentheses);
                    break;
                case 4: // To pass: Find no more text after closing parenthesis, or continue to test for function with multiple return types.
                    functionRequirement = this.FunctionStep4(text, ref name, ref modifiers, ref returnTypes, ref parameters);
                    break;
                case 5: // To pass: Find opening parenthesis.
                    functionRequirement = this.FunctionStep5(text, ref generics, angleBrackets);
                    break;
                case 6: // To pass: Find closing parenthesis.
                    functionRequirement = this.FunctionStep6(text, ref parameters, parentheses);
                    break;
                case 7: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = this.FunctionStep7(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests each step of constructor function requirements */
        private int TestConstructorRequirement(int functionRequirement, string text, ref List<string> modifiers, ref string name, ref List<string> parameters, ref List<string> baseParameters, int brackets, int parentheses, int periods)
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
        private int FunctionStep0(string text, ref List<string> returnTypes, int parentheses)
        {
            if (text.Equals(" ")) return 0;

            if (parentheses > 0) // Function has multiple return types
            {
                if (parentheses == 1 && text.Equals(")")) return 1;
                if (!text.Equals("(")) returnTypes.Add(text);
                return 0;
            }

            if ((!Char.IsSymbol(text[0]) && !Char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                returnTypes.Add(text);
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of function syntax: Find text entry that could be a name (2 = success, -1 = fail) */
        private int FunctionStep1(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, int parentheses, int brackets)
        {
            if (text.Equals(" ")) return 1;

            if (parentheses > 0) // Function has multiple return types
            {
                if (text.Equals("("))
                {
                    foreach (string entry in returnTypes)
                        modifiers.Add(entry);
                    returnTypes.Clear();
                }
                else if (!text.Equals(")")) returnTypes.Add(text);
                return 1;
            }

            if (brackets > 0 || text.Equals(".") || (returnTypes.Count > 0 && returnTypes[returnTypes.Count - 1].Equals("."))) // Part of return type
            {
                returnTypes.Add(text);
                return 1;
            }

            if ((!Char.IsSymbol(text[0]) && !Char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                name = text;
                return 2;
            }

            return -1;
        }

        /* Tests step 2 of function syntax: Find opening parenthesis (3 = success, -1 = fail) */
        private int FunctionStep2(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> generics, int squareBrackets, int angleBrackets)
        {
            if (text.Equals(" ")) return 2;

            if (angleBrackets > 0) // Function has a generic type attached
            {
                if (!text.Equals("<") && !text.Equals(">")) generics.Add(text);
            }

            else if (squareBrackets > 0 || text.Equals(".") || (returnTypes.Count > 0 && returnTypes[returnTypes.Count - 1].Equals("."))) // Still reading return type
            {
                if (text.Equals("[") || (squareBrackets == 0 && returnTypes.Count == 1 && name.Length > 0))
                {
                    foreach (string entry in returnTypes) modifiers.Add(entry);
                    returnTypes.Clear();
                    returnTypes.Add(name);
                    name = "";
                }
                returnTypes.Add(text);
            }

            else if ((!Char.IsSymbol(text[0]) && !Char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                if (name.Length == 0) // Need to find name again
                    name = text;
                else if (returnTypes.Count == 1) // Shift everything for a new name
                {
                    foreach (string entry in returnTypes) modifiers.Add(entry);
                    returnTypes.Clear();
                    returnTypes.Add(name);
                    name = text;
                }
            }

            else if (text.Equals("(")) return 3;

            else return -1;

            return 2;
        }

        /* Tests step 3 of function syntax: Find closing parenthesis (4 = success) */
        private int FunctionStep3(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(")") && parentheses == 1)
                return 4;
            
            parameters.Add(text);
            return 3;
        }

        /* Tests step 4 of function syntax: Find no more text after closing parenthesis, or (for multiple return types) find a new name (5 = pass to multiple return types, -1 = fail) */
        private int FunctionStep4(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> parameters)
        {
            if (text.Equals(" ")) return 4;

            // Check for a function signature with multiple return types
            if (parameters.Count > 1 && returnTypes.Count > 0 && ((!Char.IsSymbol(text[0]) && !Char.IsPunctuation(text[0])) || text[0].Equals('_')))
            {
                foreach (string entry in returnTypes) modifiers.Add(entry);
                modifiers.Add(name);
                returnTypes.Clear();
                name = text;
                foreach (string entry in parameters) returnTypes.Add(entry);
                parameters.Clear();
                return 5;
            }

            return -1;
        }

        /* Tests step 5 of function syntax: Find opening parenthesis (6 = success, -1 = fail) */
        private int FunctionStep5(string text, ref List<string> generics, int angleBrackets)
        {
            if (text.Equals(" ")) return 5;

            if (angleBrackets > 0) // Function has a generic type attached
            {
                if (!text.Equals("<") && !text.Equals(">")) generics.Add(text);
                return 5;
            }

            if (text.Equals("(")) return 6;

            return -1;
        }

        /* Tests step 6 of function syntax: Find closing parenthesis (7 = success) */
        private int FunctionStep6(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 6;

            if (text.Equals(")") && parentheses == 1) return 7;

            parameters.Add(text);
            return 6;
        }

        /* Tests step 7 of function syntax: Find no more text after closing parenthesis (-1 = fail) */
        private int FunctionStep7(string text)
        {
            if (text.Equals(" ")) return 7;
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
        private int ConstructorStep1(string text, ref List<string> modifiers, ref string name, int brackets, int periods)
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
                modifiers.Add(name);
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
        private int ConstructorStep3(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(":"))
            {
                baseParameters.Add(text);
                return 4;
            }

            return -1;
        }

        /* Tests step 4 of constructor syntax: Find "base" (5 = success, -1 = fail) */
        private int ConstructorStep4(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 4;

            if (text.Equals("base"))
            {
                baseParameters.Add(text);
                return 5;
            }

            return -1;
        }

        /* Tests step 5 of constructor syntax: Find opening parenthesis (6 = success, -1 = fail) */
        private int ConstructorStep5(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 5;

            if (text.Equals("("))
            {
                baseParameters.Add(text);
                return 6;
            }

            return -1;
        }

        /* Tests step 6 of constructor syntax: Find closing parenthesis (7 = success) */
        private int ConstructorStep6(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 6;

            if (text.Equals(")"))
            {
                baseParameters.Add(text);
                return 7;
            }

            baseParameters.Add(text);
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

        /* Check for control flow scope openers within functions: if, else if, else, for, for each, while, do while, switch */
        private bool CheckControlFlowScopes(string entry)
        {
            bool scopeOpener = false;

            if (ifScope > 0)
                if (this.CheckIfScope(entry)) scopeOpener = true;

            if (elseIfScope > 0)
                if (this.CheckElseIfScope(entry)) scopeOpener = true;

            if (elseScope > 0)
                if (this.CheckElseScope(entry)) scopeOpener = true;

            if (forScope > 0)
                if (this.CheckForScope(entry)) scopeOpener = true;

            if (forEachScope > 0)
                if (this.CheckForEachScope(entry)) scopeOpener = true;

            if (whileScope > 0)
                if (this.CheckWhileScope(entry)) scopeOpener = true;

            if (doWhileScope > 0)
                if (this.CheckDoWhileScope(entry)) scopeOpener = true;

            if (switchScope > 0)
                if (this.CheckSwitchScope(entry)) scopeOpener = true;

            /* Recheck the scopes with no rules passed, because "entry" could be the first word inside a bracketless scope
            * that was just opened. Need to check for the beginning of another scope directly after. */

            if (ifScope == 0 && this.CheckIfScope(entry)) scopeOpener = true;
            if (elseIfScope == 0 && this.CheckElseIfScope(entry)) scopeOpener = true;
            if (elseScope == 0 && this.CheckElseScope(entry)) scopeOpener = true;
            if (forScope == 0 && this.CheckForScope(entry)) scopeOpener = true;
            if (forEachScope == 0 && this.CheckForEachScope(entry)) scopeOpener = true;
            if (whileScope == 0 && this.CheckWhileScope(entry)) scopeOpener = true;
            if (doWhileScope == 0 && this.CheckDoWhileScope(entry)) scopeOpener = true;
            if (switchScope == 0 && this.CheckSwitchScope(entry)) scopeOpener = true;

            return scopeOpener;
        }
        
        /* Tests rules for "if" statement syntax */
        private bool CheckIfScope(string entry)
        {
            switch (ifScope)
            {
                case 0: // To pass: Find "if" and elseIfScope == 0.
                    if (entry.Equals("if") && elseIfScope == 0)
                    {
                        ifScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) ifScope = 2;
                    else ifScope = 0;
                    break;
                case 2: // To pass: Find at least one entry inside parentheses.
                    ifScope = 3;
                    break;
                case 3: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count)
                        ifScope = 4;
                    break;
                case 4: // Add new "if" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("if");
                    stringBuilder.Clear();
                    ifScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }
        
        /* Tests rules for "if else" statement syntax */
        private bool CheckElseIfScope(string entry)
        {
            switch (elseIfScope)
            {
                case 0: // To pass: Find "else".
                    if (entry.Equals("else"))
                    {
                        elseIfScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find "if".
                    if (entry.Equals("if")) elseIfScope = 2;
                    else elseIfScope = 0;
                    break;
                case 2: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) elseIfScope = 3;
                    else elseIfScope = 0;
                    break;
                case 3: // To pass: Find at least one entry inside parentheses.
                    elseIfScope = 4;
                    break;
                case 4: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) elseIfScope = 5;
                    break;
                case 5: // Add new "else if" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("else if");
                    stringBuilder.Clear();
                    elseIfScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "else" statement syntax */
        private bool CheckElseScope(string entry)
        {
            switch (elseScope)
            {
                case 0: // To pass: Find "else".
                    if (entry.Equals("else")) elseScope = 1;
                    break;
                case 1: // To pass: Anything except "if".
                    if (entry.Equals("if"))
                    {
                        elseScope = 0;
                        break;
                    }
                    // Add new "else" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("else");
                    stringBuilder.Clear();
                    elseScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "for" loop syntax */
        private bool CheckForScope(string entry)
        {
            switch (forScope)
            {
                case 0: // To pass: Find "for".
                    if (entry.Equals("for"))
                    {
                        forScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) forScope = 2;
                    else forScope = 0;
                    break;
                case 2: // To pass: Find at least one entry.
                    forScope = 3;
                    break;
                case 3: // To pass: Find semicolon.
                    if (entry.Equals(";")) forScope = 4;
                    break;
                case 4: // To pass: Find at least one entry.
                    forScope = 5;
                    break;
                case 5: // To pass: Find semicolon.
                    if (entry.Equals(";")) forScope = 6;
                    break;
                case 6: // To pass: Find at least one entry.
                    forScope = 7;
                    break;
                case 7: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) forScope = 8;
                    break;
                case 8: // Add new "for" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("for");
                    stringBuilder.Clear();
                    forScope = 0; // Reset rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "foreach" loop syntax */
        private bool CheckForEachScope(string entry)
        {
            switch (forEachScope)
            {
                case 0: // To pass: Find "foreach".
                    if (entry.Equals("foreach"))
                    {
                        forEachScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) forEachScope = 2;
                    else forEachScope = 0;
                    break;
                case 2: // To pass: Find at least one entry inside parentheses.
                    forEachScope = 3;
                    break;
                case 3: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) forEachScope = 4;
                    break;
                case 4: // Add new "foreach" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("foreach");
                    stringBuilder.Clear();
                    forEachScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "while" loop syntax */
        private bool CheckWhileScope(string entry)
        {
            switch (whileScope)
            {
                case 0: // To pass: Find "while".
                    if (entry.Equals("while"))
                    {
                        whileScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) whileScope = 2;
                    else whileScope = 0;
                    break;
                case 2: // To pass: Find at least one entry inside parentheses.
                    whileScope = 3;
                    break;
                case 3: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) whileScope = 4;
                    break;
                case 4: // To pass: Anything except semicolon.
                    if (entry.Equals(";"))
                    {
                        whileScope = 0;
                        break;
                    }
                    // Add new "while" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("while");
                    stringBuilder.Clear();
                    whileScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "do while" loop syntax */
        private bool CheckDoWhileScope(string entry)
        {
            switch (doWhileScope)
            {
                case 0: // To pass: Find "do".
                    if (entry.Equals("do")) doWhileScope = 1;
                    break;
                case 1: // Add new "do while" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("do while");
                    stringBuilder.Clear();
                    doWhileScope = 0; // Reset the rule
                    return true;
            }
            return false;
        }

        /* Tests rules for "switch" statement syntax */
        private bool CheckSwitchScope(string entry)
        {
            switch (switchScope)
            {
                case 0: // To pass: Find "switch".
                    if (entry.Equals("switch"))
                    {
                        switchScope = 1;
                        savedScopeStackCount = scopeStack.Count;
                    }
                    break;
                case 1: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) switchScope = 2;
                    else switchScope = 0;
                    break;
                case 2: // To pass: Find at least one entry inside parentheses.
                    switchScope = 3;
                    break;
                case 3: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) switchScope = 4;
                    break;
                case 4: // To pass: Find opening bracket.
                    if (!entry.Equals("{"))
                    {
                        switchScope = 0;
                        break;
                    }
                    // Add new "switch" scope
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("switch");
                    stringBuilder.Clear();
                    switchScope = 0; // Reset the rule
                    return true;
            }
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
            if (forScope == 0)
            {
                while (scopeStack.Count > 0 && scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                    || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                    || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch") || scopeStack.Peek().Equals("function"))
                {
                    if (scopeStack.Peek().Equals("function"))
                    {
                        if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                        {
                            if (((ProgramFunction)typeStack.Peek()).Size == 0)
                                ((ProgramFunction)typeStack.Peek()).Size++; // Functions are at least one line
                            typeStack.Pop();
                        }
                        isFunction = true;
                    }
                    scopeStack.Pop();
                }
            }
            stringBuilder.Clear();
            return isFunction;
        }

        /* Maintains the stack when there is a scope closer; returns true if a function is ending */
        private bool CheckScopeClosersWithinFunction(string entry, ref int index)
        {
            // Check for closing parenthesis
            if (entry.Equals(")") && scopeStack.Count > 0 && scopeStack.Peek().Equals("(")) scopeStack.Pop();

            if (entry.Equals("}")) // Check for the end of an existing bracketed scope
            {
                if (this.EndBracketedScope("function"))
                    return true;
            }

            if (entry.Equals(";")) // Check for the end of an existing bracketless scope
                if (this.EndBracketlessScope()) // True if bracketless scope was a function
                {
                    index++;
                    return true;
                }

            return false;
        }

        /* Maintains the stack when there is a scope opener */
        private void CheckScopeOpenersWithinFunction(string entry, bool scopeOpener, ref int index)
        {
            if (entry.Equals("(")) scopeStack.Push(entry); // Check for open parenthesis

            if (entry.Equals("{"))
                if (!scopeOpener && this.CheckIfFunction()) // Check if a new function is being started
                {
                    scopeStack.Push(entry);
                    index = this.ProcessFunctionData(++index);
                }
                else // Push scope opener onto scopeStack
                    scopeStack.Push(entry);

            if (entry.Equals("=>"))
                if (this.CheckIfFunction()) // Check if a new lambda function is being started
                    index = this.ProcessFunctionData(++index);
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

        /* Maintains stack for commented areas; returns true if text is within a comment */
        private bool IgnoreEntry(string entry)
        {
            // Determine entry is within a comment, and check for the end of the comment
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

            // If starting a commented area, push the entry
            if (entry.Equals("\"") || entry.Equals("'") || entry.Equals("//") || entry.Equals("/*"))
            {
                scopeStack.Push(entry);
                return true;
            }

            return false;
        }

        /* Remove the function signature from the current class's or interface's text (for relationship analysis) */
        private void RemoveFunctionSignatureFromTextData(int size)
        {
            if (typeStack.Count > 0 && (typeStack.Peek().GetType() == typeof(ProgramClass)
                || typeStack.Peek().GetType() == typeof(ProgramInterface) || typeStack.Peek().GetType() == typeof(ProgramFunction)))
            {
                int textDataIndex = ((ProgramDataType)typeStack.Peek()).TextData.Count - 1; // Last index of TextData

                // Get the index of the last closing parentheses
                while (textDataIndex >= 0 && !((ProgramDataType)typeStack.Peek()).TextData[textDataIndex].Equals(")"))
                    textDataIndex--;

                // Update the size of the function signature
                size += ((ProgramDataType)typeStack.Peek()).TextData.Count - textDataIndex - 1;

                // Remove the function signature
                ((ProgramDataType)typeStack.Peek()).TextData = ((ProgramDataType)typeStack.Peek()).TextData.GetRange(0, ((ProgramDataType)typeStack.Peek()).TextData.Count - size);
            }
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

        /* This is a test method - used in unit testing, not in the application. It has the same functionality as CheckIfFunction. */
        public (bool, ProgramFunction) CheckIfFunctionTest(string[] functionIdentifier)
        {
            // The function requirement to check next. If this ends at 4 or 7, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            List<string> modifiers = new List<string>();
            List<string> returnTypes = new List<string>();
            List<string> generics = new List<string>();
            List<string> parameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int squareBrackets = 0;
            int angleBrackets = 0;

            for (int i = 0; i < functionIdentifier.Length; i++)
            {
                string text = functionIdentifier[i];
                if (text.Length < 1) continue;

                if (text.Equals("(")) parentheses++;
                else if (text.Equals("[")) squareBrackets++;
                else if (text.Equals("<")) angleBrackets++;

                // Test the current requirement
                functionRequirement = this.TestFunctionRequirement(functionRequirement, text, ref name, ref modifiers, ref returnTypes, ref generics, ref parameters, squareBrackets, angleBrackets, parentheses/*, periods*/);

                if (text.Equals(")")) parentheses--;
                else if (text.Equals("]")) squareBrackets--;
                else if (text.Equals(">")) angleBrackets--;
            }

            if (functionRequirement == 4 || functionRequirement == 7) // Function signature detected - create a new function
            {
                this.NewFunction(functionIdentifier, name, modifiers, returnTypes, generics, parameters, new List<string>());
                return (true, (ProgramFunction)typeStack.Peek());
            }
            // If it failed normal function requirements, check rules for constructors and deconstructors
            else if (this.CheckIfConstructor(functionIdentifier)) return (true, (ProgramFunction)typeStack.Peek());
            else if (this.CheckIfDeconstructor(functionIdentifier)) return (true, (ProgramFunction)typeStack.Peek());

            return (false, null);
        }
    }

    /* Processor of all class and interface relationship data, filling all internal relationship lists */
    public class RelationshipProcessor
    {
        ProgramClassType programClassType;
        ProgramClassTypeCollection programClassTypeCollection;

        public RelationshipProcessor(ProgramClassType programClassType, ProgramClassTypeCollection programClassTypeCollection)
        {
            this.programClassType = programClassType;
            this.programClassTypeCollection = programClassTypeCollection;
        }
        
        /* Starts the relationship processor for a class or interface */
        public void ProcessRelationships()
        {
            this.SetInheritanceRelationships(); // Get the superclass/subclass data from the beginning of the class text

            if (programClassType.GetType() != typeof(ProgramClass)) return; // Interfaces only collect inheritance data

             /* (1) Get the aggregation data from the class text and text of all children
              * (2) Get the using data from the parameters fields of all child functions */
            this.SetAggregationAndUsingRelationships(programClassType);
        }

        /* Populates superclass and subclass lists related to this class/interface */
        private void SetInheritanceRelationships()
        {
            string entry;
            int index;
            bool hasSuperclasses = false;
            int brackets = 0;

            for (index = 0;  index < programClassType.TextData.Count; index++)
            {
                entry = programClassType.TextData[index];

                if (!hasSuperclasses && entry.Equals(":")) // Look for a colon (signifies that the class/interface has a superclass)
                {
                    hasSuperclasses = true;
                    continue;
                }

                if (entry.Equals("{")) // End the search at the first opening bracket, remove the text that has already been searched
                {
                    programClassType.TextData = programClassType.TextData.GetRange(++index, programClassType.TextData.Count - index);
                    return;
                }

                if (entry.Equals("[") || entry.Equals("<"))
                    brackets++;

                if (brackets > 0) // Ignore text within brackets
                {
                    if (entry.Equals("]") || entry.Equals(">"))
                        brackets--;
                    continue;
                }

                if (hasSuperclasses) // Entry might be a superclass - search the class list
                    if (programClassType.Name != entry && programClassTypeCollection.Contains(entry))
                    {
                        // Add to each other's lists
                        ProgramClassType super = programClassTypeCollection[entry];
                        super.SubClasses.Add(programClassType);
                        programClassType.SuperClasses.Add(super);
                        programClassType.TextData.RemoveAt(index);
                    }
            }
        }

        /* Populates all relationship lists related to this class, except inheritance */
        private void SetAggregationAndUsingRelationships(ProgramDataType programDataType)
        {
            // Find and set the aggregation data
            this.SetAggregationRelationships(programDataType);

            // Find and set the using data
            this.SetUsingRelationships(programDataType);

            // Repeat recursively for each child class and function
            foreach (ProgramDataType child in programDataType.ChildList)
            {
                if (child.GetType() == typeof(ProgramClass) || child.GetType() == typeof(ProgramFunction))
                {
                    this.SetAggregationAndUsingRelationships(child);
                }
            }
        }

        /* Populates the aggregation lists related to this class */
        private void SetAggregationRelationships(ProgramDataType programDataType)
        {
            foreach (string entry in programDataType.TextData)
            {
                // Check that "entry" is a different class/interface
                if (!programClassType.Name.Equals(entry) && programClassTypeCollection.Contains(entry))
                {
                    ProgramClassType owned = programClassTypeCollection[entry];

                    // Check that "owned" is a class and is not already in this class's OwnedClasses list
                    if (!((ProgramClass)programClassType).OwnedClasses.Contains(owned) && owned.GetType() == typeof(ProgramClass))
                    {
                        // Add each to the other's list
                        ((ProgramClass)owned).OwnedByClasses.Add(programClassType);
                        ((ProgramClass)programClassType).OwnedClasses.Add(owned);
                    }
                }
            }
        }

        /* Populates the using lists related to this class */
        private void SetUsingRelationships(ProgramDataType programDataType)
        {
            // Check that "programDataType" is a function with parameters
            if (programDataType.GetType() == typeof(ProgramFunction) && ((ProgramFunction)programDataType).Parameters.Count > 0)
            {
                foreach (string parameter in ((ProgramFunction)programDataType).Parameters) // Search the parameters
                {
                    // Check that "parameter" is a different class/interface
                    if (!parameter.Equals(",") && !parameter.Equals(".") && !parameter.Equals("[") && !parameter.Equals("]") && !parameter.Equals("<") && !parameter.Equals(">") 
                        && !programClassType.Name.Equals(parameter) && programClassTypeCollection.Contains(parameter))
                    {
                        ProgramClassType used = programClassTypeCollection[parameter];

                        // Check that "used" is a class and is not already in this class's UsedClasses list
                        if (!((ProgramClass)programClassType).UsedClasses.Contains(used) && used.GetType() == typeof(ProgramClass))
                        {
                            // Add each to the other's lists
                            ((ProgramClass)used).UsedByClasses.Add(programClassType);
                            ((ProgramClass)programClassType).UsedClasses.Add(used);
                        }
                    }
                }
            }
        }
    }
}
