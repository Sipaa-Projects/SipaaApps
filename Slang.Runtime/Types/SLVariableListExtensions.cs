using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLang.Runtime.Types
{
    public static class SLVariableListExtensions
    {
        public static bool ContainsKey(this List<SLVariable> list, string key)
        {
            foreach (SLVariable v in list)
            {
                if (v.Name == key)
                    return true;
            }
            return false;
        }
        public static SLVariable GetFromKey(this List<SLVariable> list, string key)
        {
            foreach (SLVariable v in list)
            {
                if (v.Name == key)
                    return v;
            }
            return null;
        }
        public static void SetKeyValue(this List<SLVariable> list, string key, object value)
        {
            var v = GetFromKey(list, key);
            if (GetFromKey(list, key) != null)
            {
                v.Value = value;
                v.Type = v.Value.GetType();
            }
            else
            {
                list.Add(new(key, value));
            }
        }
    }
}
