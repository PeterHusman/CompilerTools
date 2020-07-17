using Parser.Symbols;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Production<T> where T : Enum
    {
        public Symbol<T>[] Symbols { get; }

        public NonterminalSymbol<T> Left { get; }

        public Production(Symbol<T>[] symbols, NonterminalSymbol<T> left)
        {
            Symbols = symbols;
            Left = left;
        }

        public NonterminalNode<T> TryMatch(List<KeyValuePair<string, T>> tokens, ref int position)
        {
            int fakePos = position;
            List<Node> wip = new List<Node>();
            for(int i = 0; i < Symbols.Length; i++)
            {
                if(Symbols[i] is TerminalSymbol<T> term)
                {
                    if(tokens[fakePos].Value.Equals(term.TokenType))
                    {
                        wip.Add(new Terminal<T> { TokenType = term.TokenType, TokenValue = tokens[fakePos].Key });
                        fakePos++;
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                }

                var nonterm = (NonterminalSymbol<T>)Symbols[i];

                var node = nonterm.TryMatch(tokens, ref fakePos);

                if(node == null)
                {
                    return null;
                }

                wip.Add(node);
            }

            position = fakePos;
            return new NonterminalNode<T>(wip.ToArray(), Left.Name, this);
        }

        public override string ToString()
        {
            return $"{Left.ToString()} = {(Symbols.Length == 0 ? string.Empty : Symbols.Select(a => a.ToString()).Aggregate((a, b) => $"{a} {b}"))}";
        }
    }
}
