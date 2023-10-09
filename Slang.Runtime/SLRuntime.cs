using System.Diagnostics;
using System.Text;

namespace SLang.Runtime
{
    /// <summary>
    /// The core runtime class, handling Variables & Functions...
    /// </summary>
    public class SLRuntime
    {
        public Dictionary<string, object> Variables { get; set; }
        public Dictionary<string, Action<object[]>> Functions { get; set; }
        public Dictionary<string, Func<object[], object>> UserFunctions { get; set; }
        public Parser parser;

        public SLRuntime() 
        {
            Variables = new();
            Functions = new();
            parser = new() { parentRuntime = this };

            Functions["printrt"] = PrintRTContent;
            Functions["printc"] = PrintConcat;
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
                    ExecuteBlock(parser.ExtractBlock(part));
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

        public bool ExecuteIfStatement(string ifStatement)
        {
            string condition = parser.ExtractCondition(ifStatement);
            if (IsConditionTrue(condition))
            {
                ExecuteBlock(parser.ExtractBlock(ifStatement));
                return true;
            }
            return false;
        }

        public void ExecuteBlock(string block)
        {
            var lines = block.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                ExecuteLine(line + ";");
            }
        }
        public void ExecuteAssignment(string line)
        {
            string[] tokens = line.Split('=', StringSplitOptions.TrimEntries);
            if (tokens.Length == 2)
            {
                string variableName = tokens[0].Trim();
                string value = tokens[1].Trim();
                Variables[variableName] = parser.ParseValue(value);
            }
            else
            {
                throw new("Value isn't recognized");
            }
        }
        public void ExecuteLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (line.StartsWith("//")) return;

            if (!IsValidLineSyntax(line))
                throw new Exception("Lines must finish with ';' character.");

            line = RemoveEndingSemicolon(line);

            if (IsAssignment(line) && !IsFunctionCall(line) && !IsWhileLoop(line) && !IsIfStatement(line))
            {
                ExecuteAssignment(line);
            }
            else if (IsFunctionCall(line))
            {
                ExecuteFunction(line);
            }
            else if (IsWhileLoop(line))
            {
                ExecuteWhileLoop(line);
            }
            else if (IsIfStatement(line))
            {
                ExecuteIfStatement(line);
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
            while (IsConditionTrue(condition))
            {
                ExecuteBlock(loopBody);
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
                // Validate args length, assign them to Variables, execute the body
                if (args.Length != paramsList.Length)
                {
                    throw new("Argument length mismatch");
                }

                Dictionary<string, object> tempVariables = new();
                for (int i = 0; i < paramsList.Length; i++)
                {
                    tempVariables[paramsList[i]] = args[i];
                }

                var originalVariables = Variables;
                Variables = tempVariables;

                // Execute body
                ExecuteBlock(body);

                Variables = originalVariables;

                //return null; // Or return value, if your language supports that
            };

            Debug.WriteLine($"slrt: Sucessfully defined function '{funcName}'");
        }
        public void ExecuteFunction(string line)
        {
            string functionName = line.Substring(0, line.IndexOf('('));
            string argsString = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);

            List<string> argsList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
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

            argsList.Add(currentArg.ToString().Trim());
            object[] args = argsList.Select(arg => parser.ParseValue(arg)).ToArray();

            if (Functions.ContainsKey(functionName))
            {
                Functions[functionName](args);
            }
            else
            {
                throw new Exception("Function doesn't exist");
            }
        }

        // ----------------------
        // Core Functions
        // ----------------------
        private void PrintRTContent(object[] args)
        {
            Console.WriteLine("SLang Runtime\n\nVariables:");
            foreach (var v in Variables)
            {
                if (v.Value != null) 
                { 
                    Type t = v.Value.GetType();
                    if (t == typeof(int))
                        Console.WriteLine($"name: {v.Key}, type: {t.FullName}, value: {v.Value} (hex: 0x{((int)v.Value).ToString("X")})");
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
        }
        private void PrintConcat(object[] args)
        {
            foreach (object arg in args)
                Console.Write(arg);
            Console.Write('\n');
        }

    }
}