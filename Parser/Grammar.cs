using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Grammar<T> where T : Enum
    {
        public List<Production<T>> Productions { get; }

        public Grammar(List<Production<T>> prods)
        {
            Productions = prods;
        }
    }
}
