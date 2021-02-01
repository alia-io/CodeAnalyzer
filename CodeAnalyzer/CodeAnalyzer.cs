using System;
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

        private ProgramObjectType currentProgramObjectType = null;  // holds the ProgramObject to store text data within

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
            StringBuilder stringBuilder = new StringBuilder("");
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
                                        || (stringBuilder.ToString().Equals("=") || stringBuilder.ToString().Equals("*") || stringBuilder.ToString().Equals("!") || stringBuilder.ToString().Equals("%"))
                                            && ((char)enumerator.Current).Equals('=')
                                        || (stringBuilder.ToString().Equals("=") && ((char)enumerator.Current).Equals('>'))
                                        || (stringBuilder.ToString().Equals("&") && ((char)enumerator.Current).Equals('&'))
                                        || (stringBuilder.ToString().Equals("|") && ((char)enumerator.Current).Equals('|'))
                                        || (stringBuilder.ToString().Equals("\\") && (((char)enumerator.Current).Equals('"') || ((char)enumerator.Current).Equals('\''))))
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
                            if ((Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0])) && !((char)stringBuilder.ToString()[0]).Equals('_'))
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
            StringBuilder stringBuilder = new StringBuilder();  // used to hold text that occurs outside of a class
            string entry;

            for (int index = 0; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IgnoreEntry(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (entry.Equals("\"") || entry.Equals("'") || entry.Equals("//") || entry.Equals("/*"))
                {
                    scopeStack.Push(entry);
                    continue;
                }

                /* ---------- Check for the end of an existing scope ---------- */
                if (entry.Equals("}"))
                {
                    // TODO: uncomment
                    /*scopeStack.Pop();
                    if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                        || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("struct")
                        || scopeStack.Peek().Equals("enum") || scopeStack.Peek().Equals("function"))
                    {
                        scopeStack.Pop();
                        typeStack.Pop();
                    }*/
                }

                /* ---------- Check for a new namespace ---------- */
                if (entry.Equals("namespace"))
                {
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
                        if (!entry.Equals(" "))
                            stringBuilder.Append(entry);
                    }

                    ProgramNamespace programNamespace = new ProgramNamespace(stringBuilder.ToString());
                    stringBuilder.Clear();

                    // add new namespace to its parent's ChildList
                    if (typeStack.Count > 0)
                        typeStack.Peek().ChildList.Add(programNamespace);
                    else
                        programFile.ChildList.Add(programNamespace);
                    
                    typeStack.Push(programNamespace); // push the namespace onto typeStack
                    continue;
                }

                /* ---------- Check for a new class ---------- */
                if (entry.Equals("class"))
                {
                    string classModifiers = stringBuilder.ToString();   // get the class modifiers
                    List<string> classText = new List<string>();

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
                        if (stringBuilder.Length == 0)
                        {
                            if (!entry.Equals(" "))
                                stringBuilder.Append(entry); // the next entry after "class" will be the name
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
                        currentProgramObjectType = programClass;
                    }

                    // add new class to its parent's ChildList
                    if (typeStack.Count > 0)
                        typeStack.Peek().ChildList.Add(programClass);
                    else
                        programFile.ChildList.Add(programClass);

                    typeStack.Push(programClass); // push the class onto typeStack

                    index = this.ProcessObjectTypeData(stringBuilder, entry, index); // reads the rest of the class
                    continue;
                }

                // TODO .....









                /* ---------- Update stringBuilder ---------- */
                if (!entry.Equals(" "))
                {
                    if (entry.Equals(";") || entry.Equals("}"))
                    {
                        stringBuilder.Clear();
                        continue;
                    }
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(" ");
                    stringBuilder.Append(entry);
                }
            }
        }

        /* Processes data within a ObjectType (class, interface, etc) scope */
        public int ProcessObjectTypeData(StringBuilder stringBuilder, string entry, int i)
        {
            int index;
            for (index = i; index < programFile.FileTextData.Count; index++)
            {
                entry = programFile.FileTextData[index];

                if (populateObjectList) // add entry to object's text list
                    currentProgramObjectType.TextData.Add(entry);
                
                /* ---------- Determine whether to ignore the entry ---------- */
                if (this.IgnoreEntry(entry))
                    continue;

                /* ---------- If starting an area to ignore, push the entry ---------- */
                if (entry.Equals("\"") || entry.Equals("'") || entry.Equals("//") || entry.Equals("/*"))
                {
                    scopeStack.Push(entry);
                    continue;
                }

                /* ---------- Check for the end of an existing scope ---------- */
                if (entry.Equals("}"))
                {
                    // TODO: uncomment
                    /* scopeStack.Pop();
                    if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                        || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("struct")
                        || scopeStack.Peek().Equals("enum") || scopeStack.Peek().Equals("function"))
                    {
                        scopeStack.Pop();
                        typeStack.Pop();
                    } */
                }

                // TODO .........







                if (entry.Equals("{")) // open-brace outside of a function and without "class"/"interface"/etc. might be a function
                {
                    string[] possibleFunction = stringBuilder.ToString().Split(' ');
                    int nextRequirement = 0;    // the requirement to check next
                                                // (Optional: modifiers), [0]: return type, [1]: name, [2]: open parenthesis, (Optional: parameters), [3]: close parenthesis
                                                // [4]: all requirements fulfilled
                    string modifiers = "";
                    string returnType = "";
                    string name = "";
                    string parameters = "";

                    foreach (string text in possibleFunction)
                    {
                        switch (nextRequirement)
                        {
                            case 0:
                                if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                                {
                                    returnType = text;
                                    nextRequirement = 1;
                                    break;
                                }
                                nextRequirement = -1;
                                break;
                            case 1:
                                if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                                {
                                    name = text;
                                    nextRequirement = 2;
                                    break;
                                }
                                nextRequirement = -1;
                                break;
                            case 2:
                                if (text.Equals("("))
                                {
                                    parameters = text;
                                    nextRequirement = 3;
                                    break;
                                }
                                if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
                                {
                                    if (modifiers.Length > 0)
                                        modifiers += " ";
                                    modifiers += returnType;
                                    returnType = name;
                                    name = text;
                                    break;
                                }
                                nextRequirement = -1;
                                break;
                            case 3:
                                if (text.Equals(")"))
                                {
                                    parameters += text;
                                    nextRequirement = 4;
                                    break;
                                }
                                if ((!Char.IsSymbol((char)text[0]) && !Char.IsPunctuation((char)text[0])) 
                                    || ((char)text[0]).Equals('_') || text.Equals(','))
                                {
                                    if (!parameters[parameters.Length - 1].Equals('(') && !text.Equals(","))
                                        parameters += " ";
                                    parameters += text;
                                    break;
                                }
                                nextRequirement = -1;
                                break;
                            case 4:
                                if (!text.Equals(" "))
                                    nextRequirement = -1;
                                break;
                        }
                    }

                    if (nextRequirement == 4) // function signature detected
                    {
                        // TODO ......
                    }

                }

                /* ---------- Update stringBuilder ---------- */
                if (!entry.Equals(" "))
                {
                    if (entry.Equals(";") || entry.Equals("}"))
                    {
                        stringBuilder.Clear();
                        continue;
                    }
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(" ");
                    stringBuilder.Append(entry);
                }
            }

            return index;
        }

        /* Processes data within a Function scope */
        public int ProcessFunctionData(int index)
        {
            // TODO


            return index;
        }

        private bool IgnoreEntry(string entry)
        {
            if (scopeStack.Count() > 0)
            {
                if (scopeStack.Peek().Equals("\""))
                {
                    if (entry.Equals("\""))
                        scopeStack.Pop();
                    return true;
                }
                if (scopeStack.Peek().Equals("'"))
                {
                    if (entry.Equals("'"))
                        scopeStack.Pop();
                    return true;
                }
                if (scopeStack.Peek().Equals("//"))
                {
                    if (entry.Equals(" "))
                        scopeStack.Pop();
                    return true;
                }
                if (scopeStack.Peek().Equals("/*"))
                {
                    if (entry.Equals("*/"))
                        scopeStack.Pop();
                    return true;
                }
            }
            return false;
        }
    }

    class ClassProcessor
    {
        // TODO
    }
}
