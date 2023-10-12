using SLang.Runtime;
using SLang.Runtime.Types;

namespace SLang.TestLibrary
{
    public class SLMetadata
    {
        public bool LibLoad(SLRuntime rt)
        {
            rt.Variables.SetKeyValue("newvar", "Library sucessfully loaded!");

            return true;
        }
    }
}