using SLang.Runtime;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SLang
{

    public class ConsoleTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    class Program
    {
        static bool Verbose = false; // Enable that for debugging purposes only
        static SLRuntime rt;

        static void Main(string[] args)
        {
            rt = new();
            bool isREPL = false;
            if (Verbose)
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
                Debug.AutoFlush = true;
            }
            try
            {
                if (args.Length > 0)
                {
                    // Assuming the first argument is a file
                    rt.ExecuteFile(args[0]);
                    Console.ReadKey();
                }
                else
                {
                    isREPL = true;
                    Console.WriteLine($"SLang R.E.P.L. (Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}, Runtime version {typeof(SLRuntime).Assembly.GetName().Version.ToString()})");
                    ExecuteREPL();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
                if (isREPL)
                    ExecuteREPL();
                else
                    Console.ReadKey();
            }
        }

        static void ExecuteREPL()
        {
            try
            {
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

                        rt.ExecuteLine(singleLineCode);
                    }
                    else
                    {
                        if (isMultiLine)
                        {
                            multiLineBuffer.Append(line + " ");
                        }
                        else
                        {
                            rt.ExecuteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
                ExecuteREPL();
            }
        }
    }
}
