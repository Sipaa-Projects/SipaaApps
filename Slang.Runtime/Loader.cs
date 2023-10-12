using SLang.Runtime.Types;
using System.Diagnostics;
using System.Reflection;

namespace SLang.Runtime
{
    public class Loader
    {
        public static Type GetTypeInLoadedAssemblies(string typeName)
        {
            foreach (Assembly s in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in s.GetTypes())
                {
                    if (t.FullName == typeName)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        public static bool Load(SLRuntime rt, string name)
        {
            try
            {
                if (name.EndsWith(".dll"))
                {
                    string path = name;
                    // Path checks
                    Debug.WriteLine($"slrt: Starting loading of assembly '{path}'.");
                    if (!path.Contains(":\\"))
                    {
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

                            object[] parameters = { rt }; // Put the runtime as parameter so the library can interact with the runtime

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
                    return true;
                }
                else
                {
                    Type typeToImport = GetTypeInLoadedAssemblies(name);

                    if (typeToImport == null)
                        throw new($".NET Type '{name}' doesn't exists. Maybe the type is located in a unloaded assembly?");

                    object instance = null;

                    if (!typeToImport.IsSealed && !typeToImport.IsAbstract)
                        instance = Activator.CreateInstance(typeToImport);

                    foreach (MethodInfo method in typeToImport.GetMethods())
                    {
                        if (method.IsPrivate)
                            continue;

                        if (!rt.Functions.ContainsKey(method.Name))
                        {
                            rt.Functions[typeToImport.Name + "." + method.Name] = (args) =>
                            {
                                MethodInfo correctOverload = null;
                                foreach (var overload in typeToImport.GetMethods().Where(m => m.Name == method.Name))
                                {
                                    if (overload.GetParameters().Length == args.Length)
                                    {
                                        bool match = true;
                                        for (int i = 0; i < args.Length; i++)
                                        {
                                            if (!overload.GetParameters()[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                                            {
                                                match = false;
                                                break;
                                            }
                                        }
                                        if (match)
                                        {
                                            correctOverload = overload;
                                            break;
                                        }
                                    }
                                }

                                if (correctOverload == null)
                                {
                                    throw new Exception($"No matching overload found for {method.Name} with {args.Length} arguments.");
                                }

                                object tempInst = null;
                                if (!correctOverload.IsStatic)
                                    tempInst = instance;

                                return correctOverload.Invoke(tempInst, args);
                            };
                        }
                    }

                    // Importing public instance properties and fields as variables
                    foreach (PropertyInfo prop in typeToImport.GetProperties())
                    {
                        MethodInfo method = prop.GetGetMethod() ?? prop.GetSetMethod();
                        object tempInst = null;

                        if (!method.IsStatic)
                            tempInst = instance;

                        rt.Variables.SetKeyValue(typeToImport.Name + "_" + prop.Name, prop.GetValue(tempInst));
                    }

                    foreach (FieldInfo field in typeToImport.GetFields())
                    {
                        if (field.IsPrivate)
                            continue;

                        object tempInst = null;

                        if (!field.IsStatic)
                            tempInst = instance;

                        rt.Variables.SetKeyValue(typeToImport.Name + "_" + field.Name, field.GetValue(tempInst));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                throw new($"Failed assembly loading: {e.GetType().Name}: {e.Message}");
            }
        }
    }
}
