using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Symbols
{
    public class NonterminalSymbol<T> : Symbol<T>, IEquatable<NonterminalSymbol<T>> where T : Enum
    {
        public Production<T>[] Productions { get; set; }
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

        public override bool Equals(object obj)
        {
            if(!(obj is NonterminalSymbol<T> nonterm))
            {
                return false;
            }

            return nonterm.Name == Name;
        }

        public bool Equals(NonterminalSymbol<T> other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
