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

        private HashSet<Production<T>> possibleSRConflictProds { get; }

        public BottomUpParser(Grammar<T> grammar)
        {
            Grammar = grammar;
            possibleSRConflictProds = GetPossibleSRConflictProds();
        }

        private static bool SequenceContains<T2>(T2[] maybeContainer, T2[] maybeContainee)
        {
            bool contains = false;
            for (int i = 0; i < maybeContainer.Length - maybeContainee.Length + 1; i++)
            {
                contains = true;
                for (int j = 0; j < maybeContainee.Length; j++)
                {
                    if (!maybeContainer[j + i].Equals(maybeContainee[j]))
                    {
                        contains = false;
                        break;
                    }
                }
                if (contains)
                {
                    return true;
                }
            }

            return false;
        }

        private HashSet<Production<T>> GetPossibleSRConflictProds()
        {
            HashSet<Production<T>> conflictProds = new HashSet<Production<T>>();

            foreach (Production<T> prod in Grammar.Productions)
            {
                //Todo: make this work well instead of whatever the heck this is
                //conflictProds.Add(prod);

                foreach (Production<T> prod2 in Grammar.Productions)
                {
                    if(prod2 == prod)
                    {
                        continue;
                    }
                    if (SequenceContains(prod2.Symbols, prod.Symbols))
                    {
                        conflictProds.Add(prod);
                        conflictProds.Add(prod2);
                    }
                }

            }

            return conflictProds;
        }

        class NodeStack
        {
            public List<Node> Nodes { get; set; }

            public int Position { get; set; }
        }

        public NonterminalNode<T> Parse(List<KeyValuePair<string, T>> tokens)
        {
            //Maybe scan across whole thing every step?
            //No
            //Let's try extreme GLR -- Make a new option every step
            //

            List<NodeStack> nodeStacks = new List<NodeStack>() { new NodeStack { Nodes = new List<Node>(), Position = 0 } };
            List<NodeStack> toAdd = new List<NodeStack>();

            while (nodeStacks[0].Position < tokens.Count)
            {
                foreach (NodeStack stack in nodeStacks)
                {
                    stack.Nodes.Add(new Terminal<T> { TokenType = tokens[stack.Position].Value, TokenValue = tokens[stack.Position].Key });
                    stack.Position++;
                    bool keepGoing = true;
                    while (keepGoing)
                    {
                        keepGoing = false;
                        foreach (Production<T> prod in Grammar.Productions)
                        {
                            if (prod.Symbols.Length > stack.Nodes.Count)
                            {
                                continue;
                            }
                            bool valid = true;
                            Node[] parameters = new Node[prod.Symbols.Length];
                            for (int i = 0; i < prod.Symbols.Length; i++)
                            {
                                Symbol<T> prodSymbol = prod.Symbols[i];
                                Node stackNode = stack.Nodes[stack.Nodes.Count - (prod.Symbols.Length - i)];
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
                            if (possibleSRConflictProds.Contains(prod))
                            {
                                toAdd.Add(new NodeStack() { Nodes = stack.Nodes.ToList(), Position = stack.Position });
                            }
                            stack.Nodes.RemoveRange(stack.Nodes.Count - prod.Symbols.Length, prod.Symbols.Length);
                            stack.Nodes.Add(new NonterminalNode<T>(parameters, prod.Left.Name, prod));
                            keepGoing = keepGoing || true;
                            break;
                        }
                    }
                }

                foreach(NodeStack stack in toAdd)
                {
                    if(!nodeStacks.Any(a => a.Nodes.SequenceEqual(stack.Nodes)))
                    {
                        nodeStacks.Add(stack);
                    }
                }
                toAdd.Clear();
            }

            List<NodeStack> viable = nodeStacks.Where(a => a.Nodes.Count == 1).ToList();

            if (viable.Count != 1)
            {
                throw new Exception();
            }

            return (NonterminalNode<T>)viable[0].Nodes[0];
        }
    }
}
