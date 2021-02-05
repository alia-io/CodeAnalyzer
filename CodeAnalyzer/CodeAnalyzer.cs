﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class FileProcessor
    {
        /* Saved copies of input input arguments */
        private CodeAnalysisData codeAnalysisData;
        private ProgramFile programFile;
        bool populateObjectList;

        /* Stacks to keep track of the current scope */
        private readonly Stack<string> scopeStack = new Stack<string>();
        private readonly Stack<ProgramType> typeStack = new Stack<ProgramType>();

        private readonly List<ProgramObjectType> currentProgramObjects = new List<ProgramObjectType>();  // holds the current ProgramObjects to store text data within
        private StringBuilder stringBuilder = new StringBuilder("");

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

        public void ProcessFile(CodeAnalysisData codeAnalysisData, ProgramFile programFile, bool populateObjectList)
        {
            this.codeAnalysisData = codeAnalysisData;
            this.programFile = programFile;
            this.populateObjectList = populateObjectList;

            codeAnalysisData.ProcessedFiles.Add(programFile);   // add file to processed files list

            this.SetFileTextData();                  // preprocess the text into a list

            // send it off to have its structure and functional data fully processed
            this.ProcessFileData();
        }

        /* Puts the file next into a string array, dividing elements logically */
        private void SetFileTextData()
        {
            stringBuilder.Clear();
            string[] programLines = programFile.FileText.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < programLines.Length; i++)
            {
                IEnumerator enumerator = programLines[i].GetEnumerator();

                while (enumerator.MoveNext())
                {
                    if (Char.IsWhiteSpace((char)enumerator.Current))
                    {
                        if (stringBuilder.Length > 0)
                        {
                            programFile.FileTextData.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        continue;
                    }

                    if ((Char.IsPunctuation((char)enumerator.Current) || Char.IsSymbol((char)enumerator.Current)) && !((char)enumerator.Current).Equals('_'))
                    {
                        if (stringBuilder.Length > 0)
                        {
                            if (stringBuilder.Length == 1)
                            {
                                if (Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0]))
                                {
                                    // double-character symbols
                                    if ((stringBuilder.ToString().Equals("/") && 
                                            (((char)enumerator.Current).Equals('/') || ((char)enumerator.Current).Equals('*') || ((char)enumerator.Current).Equals('=')))
                                        || (stringBuilder.ToString().Equals("*") && ((char)enumerator.Current).Equals('/'))
                                        || (stringBuilder.ToString().Equals("+") && (((char)enumerator.Current).Equals('+') || ((char)enumerator.Current).Equals('=')))
                                        || (stringBuilder.ToString().Equals("-") && (((char)enumerator.Current).Equals('-') || ((char)enumerator.Current).Equals('=')))
                                        || (stringBuilder.ToString().Equals(">") && (((char)enumerator.Current).Equals('>') || ((char)enumerator.Current).Equals('=')))
                                        || (stringBuilder.ToString().Equals("<") && (((char)enumerator.Current).Equals('<') || ((char)enumerator.Current).Equals('=')))
                                        || (stringBuilder.ToString().Equals("=") || stringBuilder.ToString().Equals("*") 
                                            || stringBuilder.ToString().Equals("!") || stringBuilder.ToString().Equals("%"))
                                            && ((char)enumerator.Current).Equals('=')
                                        || (stringBuilder.ToString().Equals("=") && ((char)enumerator.Current).Equals('>'))
                                        || (stringBuilder.ToString().Equals("&") && ((char)enumerator.Current).Equals('&'))
                                        || (stringBuilder.ToString().Equals("|") && ((char)enumerator.Current).Equals('|'))
                                        || (stringBuilder.ToString().Equals("\\") && (((char)enumerator.Current).Equals('\\') 
                                            || ((char)enumerator.Current).Equals('"') || ((char)enumerator.Current).Equals('\''))))
                                    {
                                        stringBuilder.Append(enumerator.Current);
                                        programFile.FileTextData.Add(stringBuilder.ToString());
                                        stringBuilder.Clear();
                                        continue;
                                    }
                                }
                            }
                            programFile.FileTextData.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                    }
                    else
                    {
                        if (stringBuilder.Length == 1)
                        {
                            if ((Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0])) 
                                && !((char)stringBuilder.ToString()[0]).Equals('_'))
                            {
                                programFile.FileTextData.Add(stringBuilder.ToString());
                                stringBuilder.Clear();
                            }
                        }
                    }
                    stringBuilder.Append(enumerator.Current);
                }
                if (stringBuilder.Length > 0)
                {
                    programFile.FileTextData.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                }

                programFile.FileTextData.Add(" ");
            }
            
        }

        /* Fully processes the file data (except relationship data), fills all internal Child ProgramType lists */
        private void ProcessFileData()
        {
            stringBuilder.Clear();
            string entry;

            for (int index = 0; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IgnoreEntry(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (this.StartPlainTextArea(entry))
                    continue;

                /* ---------- Check for the end of an existing scope ---------- */
                if (entry.Equals("}"))
                {
                    if (scopeStack.Count > 0)
                        if (scopeStack.Peek().Equals("{")) scopeStack.Pop();
                    if (scopeStack.Count > 0)
                    {
                        if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                        || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("struct")
                        || scopeStack.Peek().Equals("enum") || scopeStack.Peek().Equals("function"))
                        {
                            scopeStack.Pop();
                            if (typeStack.Count > 0) typeStack.Pop();
                        }
                    }
                    stringBuilder.Clear();
                    continue;
                }

                /* ---------- Check for a new namespace ---------- */
                if (entry.Equals("namespace"))
                {
                    index = this.NewNamespace(index);
                    continue;
                }

                /* ---------- Check for a new class ---------- */
                if (entry.Equals("class"))
                {
                    index = this.NewClass(index);
                    continue;
                }

                /* ---------- Check if other type of open brace ---------- */
                if (entry.Equals("{"))
                    scopeStack.Push(entry);

                /* ---------- Update stringBuilder ---------- */
                stringBuilder = this.UpdateStringBuilder(stringBuilder, entry);
            }
        }

        /* Processes data within a ObjectType (class, interface, etc) scope */
        private int ProcessObjectTypeData(int i)
        {
            string entry;
            int index;

            for (index = i; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                if (populateObjectList) // add entry to current objects' text lists
                    foreach (ProgramObjectType programObject in currentProgramObjects)
                        programObject.TextData.Add(entry);
                
                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IgnoreEntry(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (this.StartPlainTextArea(entry))
                    continue;

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
                            if (currentProgramObjects.Count > 0) currentProgramObjects.RemoveAt(currentProgramObjects.Count - 1);
                            stringBuilder.Clear();
                            return index;
                        }
                        if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("interface")              // ending a different named scope
                        || scopeStack.Peek().Equals("struct") || scopeStack.Peek().Equals("enum") || scopeStack.Peek().Equals("function"))
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
                stringBuilder = this.UpdateStringBuilder(stringBuilder, entry);
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

                if (populateObjectList) // add entry to current objects' text lists
                    foreach (ProgramObjectType programObject in currentProgramObjects)
                        programObject.TextData.Add(entry);

                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IgnoreEntry(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (this.StartPlainTextArea(entry))
                    continue;

                /* ---------- Check for a new line ---------- */
                if (!populateObjectList && entry.Equals(" ") && typeStack.Peek().GetType() == typeof(ProgramFunction))
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
                        if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                        || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("struct") || scopeStack.Peek().Equals("enum"))
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
                if (!populateObjectList && !entry.Equals(" ") && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                {
                    /* ---------- Check "if" scope ---------- */
                    if (this.CheckIfScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "else if" scope ---------- */
                    if (this.CheckElseIfScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "else" scope ---------- */
                    if (this.CheckElseScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "for" scope ---------- */
                    if (this.CheckForScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "foreach" scope ---------- */
                    if (this.CheckForEachScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "while" scope ---------- */
                    if (this.CheckWhileScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "do" scope ---------- */
                    if (this.CheckDoWhileScope(entry))
                        scopeOpener = true;

                    /* ---------- Check "switch" scope ---------- */
                    if (this.CheckSwitchScope(entry))
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
                if (!populateObjectList && entry.Equals(";") && forScope == 0 && scopeStack.Count > 0 && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                {
                    while (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                        || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("foreach") || scopeStack.Peek().Equals("while")
                        || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch"))
                    {
                        scopeStack.Pop();
                    }
                }

                /* ---------- Update stringBuilder ---------- */
                stringBuilder = this.UpdateStringBuilder(stringBuilder, entry);
            }

            return index;
        }


        /* -------------------- Helper Methods -------------------- */
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
                classText.Add(entry); // save any inheritance data
            }

            ProgramClass programClass = new ProgramClass(stringBuilder.ToString(), classModifiers);
            stringBuilder.Clear();

            if (populateObjectList) // add text/inheritance data, and add class to general ObjectType list
            {
                programClass.TextData = classText;
                codeAnalysisData.AddObject(programClass);
                currentProgramObjects.Add(programClass);
            }

            // add new class to its parent's ChildList
            if (typeStack.Count > 0)  typeStack.Peek().ChildList.Add(programClass);
            else programFile.ChildList.Add(programClass);

            typeStack.Push(programClass); // push the class onto typeStack

            return this.ProcessObjectTypeData(++index); // reads the rest of the class
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
            string parameters = "";
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
                            parameters = text;
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
                            parameters += text;
                            functionRequirement = 4;
                            break;
                        }
                        if (brackets == 0 && periods == 0 && !parameters[parameters.Length - 1].Equals('(') && !text.Equals(",")) 
                            parameters += " ";
                        parameters += text;
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
            string parameters = "";
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
                            parameters = text;
                            functionRequirement = 3;
                            break;
                        }
                        functionRequirement = -1; // failed deconstructor syntax
                        break;
                    case 3:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")"))
                        {
                            parameters += text;
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
            // (Optional: modifiers), [0-1]: return type, [1-2]: name, [2-3]: open parenthesis & name == ClassName,
            // (Optional: parameters), [3-4]: close parenthesis, OPTIONAL: [4-5]: colon, [5-6]: "base" keyword, [6-7]: open paranthesis,
            // (Optional: parameters), [7-8]: close parenthesis, [8]: all requirements fulfilled for *constructor* functions

            string modifiers = "";
            string returnType = "";
            string name = "";
            string parameters = "";
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
                        functionRequirement = -1; // failed constructor syntax
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
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 2:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            parameters = text;
                            if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass))
                            {
                                string className = typeStack.Peek().Name;
                                if (className.Contains("<"))
                                    className = className.Substring(0, className.IndexOf("<") + 1);
                                else if (className.Contains("["))
                                    className = className.Substring(0, className.IndexOf("[") + 1);
                                if (className.Equals(name))
                                {
                                    functionRequirement = 3;
                                    break;
                                }
                            }
                            functionRequirement = -1;
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
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 3:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")") && parentheses == 0)
                        {
                            parameters += text;
                            functionRequirement = 4;
                            break;
                        }
                        if (brackets == 0 && periods == 0 && !parameters[parameters.Length - 1].Equals('(') && !text.Equals(","))
                            parameters += " ";
                        parameters += text;
                        break;
                    case 4:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(":"))
                        {
                            baseParameters = text;
                            functionRequirement = 5;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 5:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("base"))
                        {
                            baseParameters = " " + text;
                            functionRequirement = 6;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 6:
                        if (text.Equals(" ")) continue;
                        if (text.Equals("("))
                        {
                            baseParameters += text;
                            functionRequirement = 7;
                            break;
                        }
                        functionRequirement = -1; // failed constructor syntax
                        break;
                    case 7:
                        if (text.Equals(" ")) continue;
                        if (text.Equals(")"))
                        {
                            baseParameters += text;
                            functionRequirement = 8;
                            break;
                        }
                        if (!baseParameters[baseParameters.Length - 1].Equals('(') && !text.Equals(",")) parameters += " ";
                        baseParameters += text;
                        break;
                    case 8:
                        if (text.Equals(" ")) continue;
                        functionRequirement = -1; // failed constructor syntax
                        break;
                }
                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 4 || functionRequirement == 8) // function signature detected
            {
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
                    if (entry.Equals(")") && savedScopeStackCount == scopeStack.Count) ifScope = 4;
                    break;
                case 4:
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push("if");
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
                    switchScope = 0; // reset the switchScope rule
                    return true;
            }
            return false;
        }


        /* ---------- General Upkeep ---------- */

        private bool IgnoreEntry(string entry)
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

        private StringBuilder UpdateStringBuilder(StringBuilder stringBuilder, string entry)
        {
            if (!entry.Equals(" "))
            {
                if (entry.Equals(";") || entry.Equals("}") || entry.Equals("{"))
                {
                    stringBuilder.Clear();
                    return stringBuilder;
                }
                if (stringBuilder.Length > 0) stringBuilder.Append(" ");
                stringBuilder.Append(entry);
            }
            return stringBuilder;
        }
    }

    class RelationshipProcessor
    {

        ProgramObjectType programObjectType;
        CodeAnalysisData codeAnalysisData;
        
        public void ProcessRelationships(ProgramObjectType programObjectType, CodeAnalysisData codeAnalysisData)
        {
            this.programObjectType = programObjectType;
            this.codeAnalysisData = codeAnalysisData;

            int index = this.SetSuperclasses(); // get the superclass info from the beginning of the class text

            // TODO
        }

        private int SetSuperclasses()
        {
            string entry;
            int index;
            bool hasSuperclasses = false;
            int brackets = 0;

            for (index = 0;  index < programObjectType.TextData.Count; index++)
            {
                entry = programObjectType.TextData[index];

                if (entry.Equals(" ")) continue;

                if (!hasSuperclasses)
                {
                    if (entry.Equals(":"))
                    {
                        hasSuperclasses = true;
                        continue;
                    }
                    return index;
                }

                if (entry.Equals("{"))
                    return ++index;

                if (entry.Equals(","))
                    continue;

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

                if (codeAnalysisData.ProgramObjectTypes.Contains(entry))
                {
                    ProgramObjectType super = codeAnalysisData.ProgramObjectTypes[entry];
                    super.SubObjects.Add(programObjectType);
                    programObjectType.SuperObjects.Add(super);
                }
            }

            return index;
        }


    }
}
