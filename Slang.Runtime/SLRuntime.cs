using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SLang.Runtime
{
    /// <summary>
    /// The core runtime class, handling Variables & Functions...
    /// </summary>
    public class SLRuntime
    {
        public Dictionary<string, object> Variables { get; set; }
        public Dictionary<string, Func<object[], object>> Functions { get; set; }

        public Parser Parser { get; set; }

        public SLRuntime() 
        {
            Variables = new();
            Functions = new();
            Parser = new() { parentRuntime = this };

            Functions["printrt"] = PrintRTContent;
            Functions["printc"] = PrintConcat;
            Functions["load"] = Load;
        }

        ~SLRuntime()
        {
            Functions = null;
            Variables = null;
        }


        // ----------------------
        // Checks
        // ----------------------
        public bool IsWhileLoop(string line)
        {
            return line.StartsWith("while ");
        }
        public bool IsForEachLoop(string line)
        {
            return line.StartsWith("foreach ");
        }
        public bool IsForLoop(string line)
        {
            return line.StartsWith("for ");
        }

        public bool IsFunctionCall(string line)
        {
            return line.Contains("(") && line.EndsWith(")");
        }

        public bool IsUserFunctionDefinition(string line)
        {
            return line.StartsWith("def ");
        }

        public bool IsValidLineSyntax(string line)
        {
            return line.EndsWith(";");
        }

        public bool IsAssignment(string line)
        {
            return line.Contains("=");
        }

        public bool IsIfStatement(string line)
        {
            return line.StartsWith("if ");
        }

        // ----------------------
        // Utilities
        // ----------------------
        public string RemoveEndingSemicolon(string line)
        {
            return line.Substring(0, line.Length - 1);
        }

        // ----------------------
        // Execution Functions
        // ----------------------
        public void ExecuteFile(string path)
        {
            string[] file = File.ReadAllLines(path);

            StringBuilder multiLineBuffer = new StringBuilder();
            bool isMultiLine = false;

            for (int i = 0; i < file.Length; i++)
            {
                string line = file[i];

                if (line.EndsWith("{"))
                {
                    isMultiLine = true;
                    multiLineBuffer.Append(line + " ");
                }
                else if (line.EndsWith("}") || line.EndsWith("};"))
                {
                    isMultiLine = false;
                    multiLineBuffer.Append(line);
                    string singleLineCode = multiLineBuffer.ToString();
                    multiLineBuffer.Clear();

                    ExecuteLine(singleLineCode);
                }
                else
                {
                    if (isMultiLine)
                    {
                        multiLineBuffer.Append(line + " ");
                    }
                    else
                    {
                        ExecuteLine(line);  // Assuming slRuntime is your SLRuntime instance
                    }
                }
            }
        }
        public void ExecuteIfElseStatement(string inlineStatement)
        {
            string[] parts = inlineStatement.Split(new[] { "elseif", "else" }, StringSplitOptions.None);

            foreach (string part in parts)
            {
                if (part.StartsWith("if"))
                {
                    if (ExecuteIfStatement(part))
                    {
                        return;
                    }
                }
                else if (part.StartsWith("else"))
                {
                    ExecuteBlock(Parser.ExtractBlock(part), out object temp);
                    return;
                }
                else  // this is an "elseif"
                {
                    if (ExecuteIfStatement("if" + part))
                    {
                        return;
                    }
                }
            }
        }
        public void ExecuteForEachLoop(string line)
        {
            // Extracting the condition and the loop body from the line
            string condition = line.Substring(8, line.IndexOf("{") - 8).Trim(); // Everything between "foreach" and "{"
            string loopBody = line.Substring(line.IndexOf("{") + 1, line.LastIndexOf("}") - line.IndexOf("{") - 1).Trim();

            // Using regex to parse the condition into variable and collection
            Regex regex = new Regex(@"\s*(var\s+)?(?<variable>\w+)\s+in\s+(?<collection>\w+)\s*");
            Match match = regex.Match(condition);

            if (!match.Success)
            {
                throw new Exception("Invalid foreach loop syntax");
            }

            string variableName = match.Groups["variable"].Value;
            string collectionName = match.Groups["collection"].Value;

            if (!Variables.ContainsKey(collectionName))
            {
                throw new Exception($"The collection '{collectionName}' is not defined.");
            }

            object collection = Variables[collectionName];
            if (!(collection is IEnumerable))
            {
                throw new Exception($"The variable '{collectionName}' is not a collection.");
            }

            foreach (object item in (IEnumerable)collection)
            {
                // Assign the current item to the loop variable
                Variables[variableName] = item;

                // Execute the loop body
                ExecuteBlock(loopBody, out object temp, true);
            }
        }
        public void ExecuteForLoop(string line)
        {
            // Clean up the line for easier parsing
            line = line.Trim();
            if (line.EndsWith(";"))
                line = line.Substring(0, line.Length - 1);  // Remove the trailing semicolon

            Debug.WriteLine($"Raw line: {line}"); 

            int startOfForCondition = line.IndexOf("(") + 1;
            int endOfForCondition = line.IndexOf(")") - startOfForCondition;
            string contentInsideParens = line.Substring(startOfForCondition, endOfForCondition).Trim();

            Debug.WriteLine($"Content Inside Parens: {contentInsideParens}");

            string[] parts = contentInsideParens.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()).ToArray();
            Debug.WriteLine($"Parts: {parts[0]} |  {parts[1]} |  {parts[2]}");

            string init = parts[0];
            string condition = parts[1];
            string postop = parts[2];

            Debug.WriteLine($"slrt: forloop: init: '{init}', condition: '{condition}', postop: '{postop}'");

            ExecuteLine(init + ";");  // Initialization operation

            while (Parser.IsConditionTrue(condition))
            {
                // Extracting loop body, taking care of spaces and the semicolon
                string loopBody = line.Substring(line.IndexOf("{") + 1).Trim();
                loopBody = loopBody.Substring(0, loopBody.LastIndexOf("}")).Trim();
                Debug.WriteLine($"Loop Body: {loopBody}");

                ExecuteBlock(loopBody, out object temp);
                ExecuteLine(postop + ";");  // Post-operation
            }
        }

        public bool ExecuteIfStatement(string ifStatement)
        {
            string condition = Parser.ExtractCondition(ifStatement);
            if (Parser.IsConditionTrue(condition))
            {
                ExecuteBlock(Parser.ExtractBlock(ifStatement), out object temp);
                return true;
            }
            return false;
        }
        public void ExecuteBlock(string block, out object returnValue, bool disableValidLineRequirement = false)
        {
            returnValue = null; // Initialize to null
            var lines = block.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // If a "return" statement is encountered, set the returnValue and break
                if (line.StartsWith("return "))
                {
                    string returnExpression = line.Substring(7).Trim(); // Remove "return " keyword
                    returnValue = Parser.ParseValue(returnExpression); // Assume EvaluateExpression returns an object
                    break; // Exit the block after a return statement
                }
                else
                {
                    ExecuteLine(line + ";", disableValidLineRequirement);
                }
            }
        }

        public void ExecuteAssignment(string line)
        {
            string[] tokens = line.Split('=', StringSplitOptions.TrimEntries);
            if (tokens.Length == 2)
            {
                string variableName = tokens[0].Trim();
                string value = tokens[1].Trim();
                Variables[variableName] = Parser.ParseValue(value);
            }
            else
            {
                throw new("Value isn't recognized");
            }
        }
        public void ExecuteLine(string line, bool disableValidLineRequirement = false)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (line.StartsWith("//")) return;

            if (!IsValidLineSyntax(line) && !disableValidLineRequirement)
                throw new Exception("Lines must finish with ';' character.");

            Debug.WriteLine($"slrt: Executing line '{line}'...");

            line = RemoveEndingSemicolon(line);

            if (IsAssignment(line) && !IsWhileLoop(line) && !IsIfStatement(line) && !IsForLoop(line) && !IsForEachLoop(line))
            {
                ExecuteAssignment(line);
            }
            else if (IsFunctionCall(line))
            {
                ExecuteFunction(line);
            }
            else if (IsUserFunctionDefinition(line))
            {
                DefineFunction(line);
            }
            else if (IsWhileLoop(line))
            {
                ExecuteWhileLoop(line);
            }
            else if (IsIfStatement(line))
            {
                ExecuteIfStatement(line);
            }
            else if (IsForEachLoop(line))
            {
                ExecuteForEachLoop(line);
            }
            else if (IsForLoop(line))
            {
                ExecuteForLoop(line);
            }
            else
            {
                throw new Exception("Instruction isn't recognized");
            }
        }

        public void ExecuteWhileLoop(string line)
        {
            Debug.WriteLine("while loop");
            int openParen = line.IndexOf("(");
            int closeParen = line.IndexOf(")");
            int firstBrace = line.IndexOf("{");
            int lastBrace = line.LastIndexOf("}");

            // Check for syntax errors
            if (openParen == -1 || closeParen == -1 || firstBrace == -1 || lastBrace == -1)
            {
                throw new Exception("Syntax error in while loop.");
            }

            string condition = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
            string loopBody = line.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

            Debug.WriteLine($"slrt: while loop: condition: {condition}, body: {loopBody}");
            while (Parser.IsConditionTrue(condition))
            {
                ExecuteBlock(loopBody, out object temp);
            }
        }

        public void DefineFunction(string line)
        {
            string header = line.Substring(4, line.IndexOf("{") - 4).Trim();
            string body = line.Substring(line.IndexOf("{") + 1, line.LastIndexOf("}") - line.IndexOf("{") - 1).Trim();
            string[] headerParts = header.Split('(', ')');
            string funcName = headerParts[0].Trim();
            string[] paramsList = headerParts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteLine($"slrt: Starting definition of function '{funcName}'...");

            for (int i = 0; i < paramsList.Length; i++)
            {
                paramsList[i] = paramsList[i].Trim();
            }

            Debug.WriteLine($"slrt: func '{funcName}': Function body : {body}");

            Functions[funcName] = (args) =>
            {
                object returnValue = null;  // Initialize a returnValue to null

                if (args.Length != paramsList.Length)
                {
                    throw new($"Argument length mismatch...");
                    // existing code
                }

                Dictionary<string, object> tempVariables = new();
                for (int i = 0; i < paramsList.Length; i++)
                {
                    tempVariables[paramsList[i]] = args[i];
                }

                var originalVariables = Variables;
                Variables = tempVariables;

                // Execute body
                ExecuteBlock(body, out returnValue);

                Variables = originalVariables;

                return returnValue;  // Return the value
            };

            Debug.WriteLine($"slrt: Sucessfully defined function '{funcName}'");
        }

        public object ExecuteFunction(string line)
        {
            string functionName = line.Substring(0, line.IndexOf('(')).Trim();
            string argsString = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
            List<string> argsList = new List<string>();
            StringBuilder currentArg = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(argsString))
            {
                bool insideString = false;

                for (int i = 0; i < argsString.Length; i++)
                {
                    char c = argsString[i];

                    if (c == '"' && (i == 0 || argsString[i - 1] != '\\'))
                    {
                        insideString = !insideString;
                    }

                    if (c == ',' && !insideString)
                    {
                        argsList.Add(currentArg.ToString().Trim());
                        currentArg.Clear();
                    }
                    else
                    {
                        currentArg.Append(c);
                    }
                }
                if (currentArg.Length > 0)
                {
                    argsList.Add(currentArg.ToString().Trim());
                }
            }

            object[] args = argsList.Select(arg => Parser.ParseValue(arg)).ToArray();

            if (Functions.ContainsKey(functionName))
            {
                return Functions[functionName](args);
            }
            else
            {
                throw new Exception($"Function doesn't exist! Function name: {functionName}");
            }
        }

        // ----------------------
        // Core Functions
        // ----------------------
        private object Load(object[] args)
        {
            // TODO: add support for importing .NET classes into SLang (yes it's crazy)

            if (args.Length > 0)
            {
                if (args[0].GetType() == typeof(string))
                {
                    try
                    {
                        // Path checks
                        string path = (string)args[0];
                        Debug.WriteLine($"slrt: Starting loading of assembly '{path}'.");
                        if (!path.Contains(":\\")) {
                            Debug.WriteLine("slrt: Path isn't absolute, making it absolute.");
                            path = AppDomain.CurrentDomain.BaseDirectory + path;
                            Debug.WriteLine($"slrt: Path became '{path}'.");
                        }

                        Assembly a = Assembly.LoadFile(path);
                        bool IsSLLib = false;
                        Type slMeta = null;

                        // Check if it's a SLang library
                        foreach (Type type in a.GetTypes())
                        {
                            if (type.Name == "SLMetadata")
                            {
                                IsSLLib = true;
                                slMeta = type;
                                Debug.WriteLine($"slrt: S# Library detected! Library metadata type: {slMeta.FullName}");
                            }
                        }

                        // If it's a SLang library, then try running the function to load it
                        if (IsSLLib && slMeta != null)
                        {
                            object instance = null;
                            MethodInfo methodInfo = slMeta.GetMethod("LibLoad");
                            if (methodInfo != null)
                            {
                                if (!methodInfo.IsStatic)
                                    instance = Activator.CreateInstance(slMeta);

                                object[] parameters = { this }; // Put the runtime as parameter so the library can interact with the runtime

                                // Finally, invoke 'LibLoad' and check the result
                                bool result = (bool)methodInfo.Invoke(instance, parameters);
                                if (!result)
                                    throw new($"The SLang library '{a.GetName().Name}' failed to load.");
                            }
                            else
                            {
                                throw new($"Function 'LibLoad' isn't found in type '{slMeta.FullName}'");
                            }
                        }

                        // Say the library has been loaded, then exit this method.
                        Debug.WriteLine($"slrt: Loaded '{a.GetName().Name}', version {a.GetName().Version.ToString()}");
                        return null;
                    }
                    catch (Exception e)
                    {
                        throw new($"Failed assembly loading: {e.GetType().Name}: {e.Message}");
                    }
                }
            }

            throw new("Invalid arguments");

            return null;
        }

        private object PrintRTContent(object[] args)
        {
            Console.WriteLine("SLang Runtime\n\nVariables:");
            foreach (var v in Variables)
            {
                if (v.Value != null) 
                { 
                    Type t = v.Value.GetType();
                    if (t == typeof(int))
                        Console.WriteLine($"name: {v.Key}, type: {t.FullName}, value: {v.Value} (hex: 0x{((int)v.Value).ToString("X")})");
                    else if (t.IsArray)
                    {
                        object[] array = (object[])v.Value;
                        Console.Write($"name: {v.Key}, type: {t.FullName}, length: {array.Length}, values: ");
                        foreach (object av in array)
                        {
                            if (av != null)
                                Console.Write(av + " ");
                            else
                                Console.Write("null ");
                        }
                        Console.Write("\n");
                    }
                    else
                        Console.WriteLine($"name: {v.Key}, type: {t.FullName}, value: {v.Value}");
                }
                else
                    Console.WriteLine($"name: {v.Key}, type: System.Object, value: null");
            }
            foreach (var v in Functions)
            {
                Console.WriteLine($"name: {v.Key}, type: func, value: 0x{v.Value.Method.MethodHandle.Value.ToInt64().ToString("X")}");
            }
            return true;
        }
        private object PrintConcat(object[] args)
        {
            foreach (object arg in args)
                Console.Write(arg);
            Console.Write('\n');
            return true;
        }

    }
}