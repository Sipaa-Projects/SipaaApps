using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLang.Runtime
{
    public class Parser
    {
        public SLRuntime parentRuntime;

        private string UnescapeString(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\' && i < input.Length - 1)
                {
                    char nextChar = input[i + 1];
                    switch (nextChar)
                    {
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case '\"':
                            sb.Append('\"');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        // Add more escape sequences here
                        default:
                            sb.Append('\\');
                            sb.Append(nextChar);
                            break;
                    }
                    i++; // Skip the next character as we have already processed it
                }
                else
                {
                    sb.Append(input[i]);
                }
            }
            return sb.ToString();
        }

        public object ParseValue(string valueString)
        {
            if (string.IsNullOrEmpty(valueString))
            {
                return null;
            }

            var trimmedValue = valueString.Trim();
            Debug.WriteLine($"slrt: Trying parsing value `{trimmedValue}`");

            // Check if the value contains expressions
            if (trimmedValue.Contains('+') || trimmedValue.Contains('-') || trimmedValue.Contains('*') || trimmedValue.Contains('/'))
                return EvaluateExpression(trimmedValue);

            if (trimmedValue.EndsWith("f") || trimmedValue.EndsWith("F"))
                if (float.TryParse(trimmedValue.Remove(trimmedValue.Length - 1, 1), out float fltValue))
                    return fltValue;
                else
                    return 0f;
            else if (trimmedValue.EndsWith("D") || trimmedValue.EndsWith("d"))
                if (double.TryParse(trimmedValue.Remove(trimmedValue.Length - 1, 1), out double dblValue))
                    return dblValue;
                else
                    return 0.5;
            else if (int.TryParse(trimmedValue, out int intValue))
                return intValue;
            else if (trimmedValue.StartsWith("0x"))
                if (int.TryParse(trimmedValue.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int hexValue))
                    return hexValue;
                else
                    throw new("Hexadecimal value isn't recognized");
            else if (trimmedValue.StartsWith("\"") && trimmedValue.EndsWith("\""))
                return UnescapeString(valueString.Substring(1, trimmedValue.Length - 2));
            else if (trimmedValue == "true")
                return true;
            else if (trimmedValue == "false")
                return false;
            else if (trimmedValue == "null")
                return null;
            else if (parentRuntime != null)
                if (parentRuntime.Variables.ContainsKey(trimmedValue))
                    return parentRuntime.Variables[trimmedValue];
                else if (parentRuntime.IsFunctionCall(trimmedValue))
                    return parentRuntime.ExecuteFunction(trimmedValue);
                else
                    throw new("Value isn't recognized");
            else
                throw new("Value isn't recognized");
        }

        public string ExtractBlock(string statement)
        {
            int start = statement.IndexOf('{');
            int end = statement.LastIndexOf('}');
            if (start != -1 && end != -1)
            {
                return statement.Substring(start + 1, end - start - 1).Trim();
            }
            throw new("Block extraction failed. Invalid statement syntax.");
        }

        public string ExtractCondition(string statement)
        {
            int start = statement.IndexOf('(');
            int end = statement.IndexOf(')');
            if (start != -1 && end != -1)
            {
                return statement.Substring(start + 1, end - start - 1).Trim();
            }
            throw new("Condition extraction failed. Invalid statement syntax.");
        }

        public bool IsConditionTrue(string condition)
        {
            bool negate = false;

            // Check for negation
            if (condition.StartsWith("!"))
            {
                negate = true;
                condition = condition.Substring(1).Trim();
            }

            if (condition == "true" || condition == "1")
                return true;
            else if (condition == "false" || condition == "0")
                return false;

            if (condition.Contains("&&"))
            {
                string[] parts = condition.Split(new[] { "&&" }, StringSplitOptions.None);
                return IsConditionTrue(parts[0].Trim()) && IsConditionTrue(parts[1].Trim());
            }
            else if (condition.Contains("||"))
            {
                string[] parts = condition.Split(new[] { "||" }, StringSplitOptions.None);
                return IsConditionTrue(parts[0].Trim()) || IsConditionTrue(parts[1].Trim());
            }
            else if (condition.Contains("=="))
            {
                string[] parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                return ParseValue(parts[0].Trim()).Equals(ParseValue(parts[1].Trim()));
            }
            else if (condition.Contains("!="))
            {
                string[] parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
                return !ParseValue(parts[0].Trim()).Equals(ParseValue(parts[1].Trim()));
            }
            else if (condition.Contains(">="))
            {
                string[] parts = condition.Split(new[] { ">=" }, StringSplitOptions.None);
                return Convert.ToDouble(ParseValue(parts[0].Trim())) >= Convert.ToDouble(ParseValue(parts[1].Trim()));
            }
            else if (condition.Contains("<="))
            {
                string[] parts = condition.Split(new[] { "<=" }, StringSplitOptions.None);
                return Convert.ToDouble(ParseValue(parts[0].Trim())) <= Convert.ToDouble(ParseValue(parts[1].Trim()));
            }
            else if (condition.Contains(">"))
            {
                string[] parts = condition.Split(new[] { ">" }, StringSplitOptions.None);
                return Convert.ToDouble(ParseValue(parts[0].Trim())) > Convert.ToDouble(ParseValue(parts[1].Trim()));
            }
            else if (condition.Contains("<"))
            {
                string[] parts = condition.Split(new[] { "<" }, StringSplitOptions.None);
                return Convert.ToDouble(ParseValue(parts[0].Trim())) < Convert.ToDouble(ParseValue(parts[1].Trim()));
            }

            if (parentRuntime != null) { 
                if (parentRuntime.Variables.ContainsKey(condition))
                {
                    object val = parentRuntime.Variables[condition];
                    if (val is bool b)
                    {
                        return negate ? !b : b;
                    }
                    else if (val is int i)
                    {
                        return negate ? (i == 0) : (i != 0);
                    }
                    // Add more type checks as needed
                }
            };

            // Fallback
            return false;
        }

        public object EvaluateExpression(string expr)
        {
            char[] operators = { '+', '-', '*', '/' };
            string[] logicOps = { "==", "||", "&&", "!=", ">=", "<=", ">", "<" };

            foreach (char op in operators)
            {
                if (expr.Contains(op))
                {
                    string[] operands = expr.Split(op);
                    if (operands.Length < 2)
                    {
                        throw new("Invalid expression");
                    }

                    object firstVal = ParseValue(operands[0].Trim());

                    // If first value is a string, we assume all are strings for concatenation.
                    if (firstVal is string && op == '+')
                    {
                        string result = firstVal as string;

                        for (int i = 1; i < operands.Length; i++)
                        {
                            string nextVal = ParseValue(operands[i].Trim()) as string;
                            result += nextVal;
                        }

                        return result;
                    }
                    // If first value is a number, we assume all are numbers for arithmetic operations.
                    else if (firstVal is double || firstVal is float || firstVal is int)
                    {
                        double result = Convert.ToDouble(firstVal);

                        for (int i = 1; i < operands.Length; i++)
                        {
                            double nextVal = Convert.ToDouble(ParseValue(operands[i].Trim()));

                            switch (op)
                            {
                                case '+':
                                    result += nextVal;
                                    break;
                                case '-':
                                    result -= nextVal;
                                    break;
                                case '*':
                                    result *= nextVal;
                                    break;
                                case '/':
                                    if (nextVal == 0)
                                    {
                                        throw new("Division by zero");
                                    }
                                    result /= nextVal;
                                    break;
                            }
                        }

                        return result;
                    }
                    else
                    {
                        throw new("Incompatible types for the operation");
                    }
                }
            }
            
            foreach (string op in logicOps)
            {
                if (expr.Contains(op))
                {
                    return IsConditionTrue(expr) ? true : false;
                }
            }

            throw new("Operator not recognized");
        }

    }
}
