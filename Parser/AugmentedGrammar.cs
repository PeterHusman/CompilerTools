using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class AugmentedGrammar<T> : Grammar<T> where T : Enum
    {
        public NonterminalSymbol<T> StartingSymbol { get; }

        public AugmentedGrammar(List<Production<T>> prods, NonterminalSymbol<T> startingSymbol = null) : base(prods)
        {
            if (startingSymbol == null)
            {
                startingSymbol = prods[0].Left;
            }
            StartingSymbol = new NonterminalSymbol<T>(null, "<>start");
            Productions.Add(new Production<T>(new[] { startingSymbol }, StartingSymbol));
            StartingSymbol.Productions = new[] { Productions[Productions.Count - 1] };

            firsts = new Dictionary<Symbol<T>, HashSet<Symbol<T>>>();
            GetFirsts();
        }

        public AugmentedGrammar(Grammar<T> grammar, NonterminalSymbol<T> startingSymbol = null) : this(grammar.Productions, startingSymbol) { }
    }
}
