using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class Scope
    {
        public Dictionary<string, CaulType> Types { get; set; } = new Dictionary<string, CaulType>();
    }
}
