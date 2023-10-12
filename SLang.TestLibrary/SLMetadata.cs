using SLang.Runtime;

namespace SLang.TestLibrary
{
    public class SLMetadata
    {
        public bool LibLoad(SLRuntime rt)
        {
            rt.Variables["newvar"] = "Library sucessfully loaded!";

            return true;
        }
    }
}