using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class BottomUpParser<T> where T : Enum
    {
        public Grammar<T> Grammar { get; }

        public BottomUpParser(Grammar<T> grammar)
        {
            Grammar = grammar;
        }

        public NonterminalNode<T> Parse(List<KeyValuePair<string, T>> tokens)
        {
            int position = 0;
            List<Node> nodes = new List<Node>();

            while (position < tokens.Count)
            {
                nodes.Add(new Terminal<T> { TokenType = tokens[position].Value, TokenValue = tokens[position].Key });
                position++;
                bool keepGoing = true;
                while (keepGoing)
                {
                    keepGoing = false;
                    foreach (Production<T> prod in Grammar.Productions)
                    {
                        if (prod.Symbols.Length > nodes.Count)
                        {
                            continue;
                        }
                        bool valid = true;
                        Node[] parameters = new Node[prod.Symbols.Length];
                        for (int i = 0; i < prod.Symbols.Length; i++)
                        {
                            Symbol<T> prodSymbol = prod.Symbols[i];
                            Node stackNode = nodes[nodes.Count - (prod.Symbols.Length - i)];
                            if (prodSymbol is TerminalSymbol<T> terminalSymbol)
                            {
                                if (stackNode is Terminal<T> terminalNode && terminalNode.TokenType.Equals(terminalSymbol.TokenType))
                                {
                                    parameters[i] = terminalNode;
                                    continue;
                                }
                                else
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            else if (prodSymbol is NonterminalSymbol<T> nonterminalSymbol)
                            {
                                if (stackNode is NonterminalNode<T> nonterminalNode && nonterminalNode.Name == nonterminalSymbol.Name)
                                {
                                    parameters[i] = nonterminalNode;
                                    continue;
                                }
                                else
                                {
                                    valid = false;
                                    break;
                                }
                            }
                            else
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (!valid)
                        {
                            continue;
                        }

                        nodes.RemoveRange(nodes.Count - prod.Symbols.Length, prod.Symbols.Length);
                        nodes.Add(new NonterminalNode<T>(parameters, prod.Left.Name, prod));
                        keepGoing = true;
                        break;
                    }
                }
            }

            if(nodes.Count != 1)
            {
                throw new Exception();
            }

            return (NonterminalNode<T>)nodes[0];
        }
    }
}
