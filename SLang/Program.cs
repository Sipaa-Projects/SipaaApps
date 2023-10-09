using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using SLang.Runtime;

namespace SLang
{
    class Program
    {
        static SLRuntime rt;

        static void Main(string[] args)
        {
            Console.WriteLine($"SLang R.E.P.L. (Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}, Runtime version {typeof(SLRuntime).Assembly.GetName().Version.ToString()})");
            rt = new();
            StringBuilder multiLineBuffer = new StringBuilder();
            bool isMultiLine = false;

            while (true)
            {
                Console.Write(isMultiLine ? "... " : ">>> "); // Put 3 of '>' so we don't confuse the programmer.
                string line = Console.ReadLine();

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

                    try
                    {
                        rt.ExecuteLine(singleLineCode);  // Assuming slRuntime is your SLRuntime instance
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    if (isMultiLine)
                    {
                        multiLineBuffer.Append(line + " ");
                    }
                    else
                    {
                        try
                        {
                            rt.ExecuteLine(line);  // Assuming slRuntime is your SLRuntime instance
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }
            }
        }

    }
}
