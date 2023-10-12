using SLang.Runtime.Types;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SLang.Runtime
{
    /// <summary>
    /// The core runtime class, handling Variables & Functions...
    /// </summary>
    public class SLRuntime
    {
        public List<SLVariable> Variables { get; set; }
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
            return line.Contains("=") || line.Contains("++") || line.Contains("--");
        }
        public bool DoesFileHaveMFTW(string filePath)
        {
            string zoneIdentifierPath = filePath + ":Zone.Identifier";
            return File.Exists(zoneIdentifierPath);
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
            if (DoesFileHaveMFTW(path))
            {
                Console.WriteLine("WARNING: The file you are trying to open has Mark of the Web.\nThe execution of this file could lead to problems.\nDo you still want to execute this file? (y/n)");
                if (Console.ReadLine().ToLower() != "y")
                    return;
            }

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

            object collection = Variables.GetFromKey(collectionName);
            if (!(collection is IEnumerable))
            {
                throw new Exception($"The variable '{collectionName}' is not a collection.");
            }

            foreach (object item in (IEnumerable)collection)
            {
                // Assign the current item to the loop variable
                Variables.SetKeyValue(variableName, item);

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
            if (line.Contains('='))
            {
                string[] tokens = line.Split('=', StringSplitOptions.TrimEntries);
                if (tokens.Length == 2)
                {
                    string variableName = tokens[0].Trim();
                    string value = tokens[1].Trim();
                    Variables.SetKeyValue(variableName, Parser.ParseValue(value));
                }
                else
                {
                    throw new("Value isn't recognized");
                }
            }
            else if (line.EndsWith("++") || line.EndsWith("--"))
            {
                string variableName = line.Remove(line.Length - 2, 2);
                if (Variables.ContainsKey(variableName))
                {
                    object value = Variables.GetFromKey(variableName);
                    if (value is int)
                    {
                        int intValue = (int)value;
                        if (line.EndsWith("++"))
                            intValue++;
                        else
                            intValue--;

                        Variables.SetKeyValue(variableName, intValue);
                    }
                    else if (value is double)
                    {
                        double doubleValue = (double)value;
                        if (line.EndsWith("++"))
                            doubleValue++;
                        else
                            doubleValue--;

                        Variables.SetKeyValue(variableName, doubleValue);
                    }
                    else if (value is float)
                    {
                        float floatValue = (float)value;
                        if (line.EndsWith("++"))
                            floatValue++;
                        else
                            floatValue--;

                        Variables.SetKeyValue(variableName, floatValue);
                    }
                    else
                    {
                        // Handle the case where the variable is not one of the expected number types
                        throw new Exception($"Variable '{variableName}' is not a supported numeric type for increment/decrement operations.");
                    }
                }
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

                List<SLVariable> tempVariables = new();
                for (int i = 0; i < paramsList.Length; i++)
                {
                    tempVariables.SetKeyValue(paramsList[i], args[i]);
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
            if (args.Length > 0)
            {
                if (args[0].GetType() == typeof(string))
                {
                    string name = (string)args[0];
                    return Loader.Load(this, name);
                }
            }

            throw new("Invalid arguments");
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
                        Console.WriteLine($"name: {v.Name}, type: {v.Type.FullName}, value: {v.Value} (hex: 0x{((int)v.Value).ToString("X")})");
                    else if (t.IsArray)
                    {
                        object[] array = (object[])v.Value;
                        Console.Write($"name: {v.Name}, type: {v.Type.FullName}, length: {array.Length}, values: ");
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
                        Console.WriteLine($"name: {v.Name}, type: {v.Type.FullName}, value: {v.Value}");
                }
                else
                    Console.WriteLine($"name: {v.Name}, type: System.Object, value: null");
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