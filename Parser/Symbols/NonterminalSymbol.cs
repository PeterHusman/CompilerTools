using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Symbols
{
    public class NonterminalSymbol<T> : Symbol<T> where T : Enum
    {
        public Production<T>[] Productions { get; }
        public string Name { get; }

        public NonterminalSymbol(Production<T>[] prods, string name)
        {
            Productions = prods;
            Name = name;
        }

        public NonterminalNode<T> TryMatch(List<KeyValuePair<string, T>> tokens, ref int position)
        {
            NonterminalNode<T> node = null;
            foreach(Production<T> prod in Productions)
            {
                node = prod.TryMatch(tokens, ref position);
                if(node != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
