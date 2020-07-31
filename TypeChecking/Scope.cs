using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class Scope<T1, T2>
    {
        public Dictionary<T1, T2> Types { get; set; } = new Dictionary<T1, T2>();
    }
}
