using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLang.Runtime.Types
{
    public class SLVariable
    {
        public string Name { get; set; } = "var";
        public object Value { get; set; } = null;
        public Type Type { get; set; } = null;

        public SLVariable(string name, object value)
        {
            Name = name;
            Value = value;
            Type = value.GetType();
        }
    }
}
