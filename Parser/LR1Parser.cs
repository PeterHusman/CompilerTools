using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{

    class LR0Item<T> where T : Enum
    {
        public Production<T> Production { get; set; }

        public int DotPosition { get; set; }
    }

    class LR1Item<T> : IEquatable<LR1Item<T>> where T : Enum
    {
        public Production<T> Production { get; set; }

        public int DotPosition { get; set; }

        public TerminalSymbol<T> LookAhead { get; set; }

        public Symbol<T> NextSymbol => DotPosition < Production.Symbols.Length ? Production.Symbols[DotPosition] : null;

        public override bool Equals(object obj)
        {
            return Equals(obj as LR1Item<T>);
        }

        public bool Equals(LR1Item<T> other)
        {
            return other != null &&
                   EqualityComparer<Production<T>>.Default.Equals(Production, other.Production) &&
                   DotPosition == other.DotPosition &&
                   EqualityComparer<Symbol<T>>.Default.Equals(LookAhead, other.LookAhead);
        }

        public override int GetHashCode()
        {
            var hashCode = -953855800;
            hashCode = hashCode * -1521134295 + EqualityComparer<Production<T>>.Default.GetHashCode(Production);
            hashCode = hashCode * -1521134295 + DotPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Symbol<T>>.Default.GetHashCode(LookAhead);
            return hashCode;
        }

        public LR1Item<T> PassSymbol(Symbol<T> symbol)
        {
            if (NextSymbol == null)
            {
                return null;
            }
            if (!symbol.Equals(NextSymbol))
            {
                return null;
            }

            return new LR1Item<T>() { Production = Production, LookAhead = LookAhead, DotPosition = DotPosition + 1 };
        }

        public override string ToString()
        {
            string str = $"{Production.Left.ToString()} = ";
            for(int i = 0; i < DotPosition; i++)
            {
                str = $"{str} {Production.Symbols[i].ToString()}";
            }
            str = $"{str} .";
            for (int i = DotPosition; i < Production.Symbols.Length; i++)
            {
                str = $"{str} {Production.Symbols[i].ToString()}";
            }
            str = $"{str}, {LookAhead.ToString()}";
            return str;
        }
    }


    class LR1ItemSet<T> where T : Enum
    {
        public HashSet<LR1Item<T>> Items { get; set; }

        public Grammar<T> Grammar { get; set; }

        public LR1ItemSet<T> PassSymbol(Symbol<T> symbol)
        {
            HashSet<LR1Item<T>> newItems = new HashSet<LR1Item<T>>();

            foreach (LR1Item<T> item in Items)
            {
                var v = item.PassSymbol(symbol);
                if (v == null)
                {
                    continue;
                }

                newItems.Add(v);
            }

            return new LR1ItemSet<T>(Grammar) { Items = newItems };
        }

        public LR1ItemSet(Grammar<T> grammar)
        {
            Grammar = grammar;
        }

        HashSet<TerminalSymbol<T>> First(Symbol<T>[] symbols)
        {
            HashSet<TerminalSymbol<T>> terms = new HashSet<TerminalSymbol<T>>();
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] is TerminalSymbol<T> term)
                {
                    if (term is TerminalEpsilon<T>)
                    {
                        continue;
                    }
                    terms.Add(term);

                    //Comment this out for magic
                    return terms;
                }
                else if (symbols[i] is NonterminalSymbol<T> nonterm)
                {
                    bool hasEp = false;
                    foreach (TerminalSymbol<T> term2 in Grammar.firsts[nonterm])
                    {
                        if (term2 is TerminalEpsilon<T>)
                        {
                            hasEp = true;
                            continue;
                        }
                        terms.Add(term2);
                    }

                    if (hasEp)
                    {
                        continue;
                    }

                    //Comment this out for magic
                    return terms;
                }
                else
                {
                    throw new Exception();
                }
            }

            //return terms;

            throw new Exception();
        }

        public void Closure()
        {
            List<LR1Item<T>> toAdd = new List<LR1Item<T>>();
            while (true)
            {
                foreach (LR1Item<T> item in Items)
                {
                    var nextSym = item.NextSymbol;
                    if (nextSym == null)
                    {
                        continue;
                    }

                    if(nextSym is TerminalSymbol<T>)
                    {
                        continue;
                    }

                    Symbol<T>[] symbs = new Symbol<T>[item.Production.Symbols.Length - item.DotPosition/* + 1*/];

                    for (int i = 0; i < symbs.Length - 1; i++)
                    {
                        symbs[i] = item.Production.Symbols[i + item.DotPosition + 1];
                    }

                    symbs[symbs.Length - 1] = item.LookAhead;

                    TerminalSymbol<T>[] firsts = First(symbs).ToArray();

                    foreach (Production<T> prod in Grammar.Productions.Where(a => a.Left.Equals(item.NextSymbol)))
                    {
                        foreach (TerminalSymbol<T> symbol in firsts)
                        {
                            toAdd.Add(new LR1Item<T>() { DotPosition = 0, LookAhead = symbol, Production = prod });
                        }
                    }
                }

                bool anyAdded = false;

                foreach (var item in toAdd)
                {
                    bool b = Items.Add(item);
                    anyAdded = b || anyAdded;
                }

                if (!anyAdded)
                {
                    break;
                }

                toAdd.Clear();
            }
        }

        public bool EqualsOtherItemSet(LR1ItemSet<T> other)
        {
            if (other.Items.Count != Items.Count)
            {
                return false;
            }

            foreach (LR1Item<T> item in Items)
            {
                if (!other.Items.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public enum ParserActionOption
    {
        Shift,
        Reduce,
        Accept
    }

    public struct ParserAction
    {
        public ParserActionOption Type;
        public int Parameter;

        public override string ToString()
        {
            return Type == ParserActionOption.Accept ? "Acc" : (Type == ParserActionOption.Reduce ? $"r{Parameter}" : $"s{Parameter}");
        }
    }


    public class LR1Parser<T> where T : Enum
    {
        public AugmentedGrammar<T> Grammar { get; }

        private T EndOfStream;

        public Dictionary<(int, T), ParserAction> ParseTable;

        public Dictionary<(int, string), int> GotoTable;

        public LR1Parser(AugmentedGrammar<T> grammar, T endOfStream)
        {
            Grammar = grammar;
            EndOfStream = endOfStream;

            (ParseTable, GotoTable) = InternalParserStuff.MakeParseTable(grammar, endOfStream, false);
        }

        public LR1Parser(Dictionary<(int, T), ParserAction> parseTable, Dictionary<(int, string), int> gotoTable)
        {
            this.ParseTable = parseTable;
            this.GotoTable = gotoTable;
        }

        public NonterminalNode<T> Parse(List<KeyValuePair<string, T>> tokens)
        {
            Stack<int> states = new Stack<int>();
            Stack<Node> nodes = new Stack<Node>();
            states.Push(0);
            int position = 0;
            KeyValuePair<string, T> token = tokens[position];
            if (!ParseTable.ContainsKey((states.Peek(), token.Value)))
            {
                T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                throw new Exception($"Unexpected {token.Value} token at start of file. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
            }
            ParserAction action = ParseTable[(states.Peek(), token.Value)];
            while(action.Type != ParserActionOption.Accept)
            {
                switch (action.Type)
                {
                    case ParserActionOption.Shift:
                        states.Push(action.Parameter);
                        nodes.Push(new Terminal<T>() { TokenType = token.Value, TokenValue = token.Key });
                        position++;
                        token = tokens[position];
                        break;
                    case ParserActionOption.Reduce:
                        Production<T> prod = Grammar.Productions[action.Parameter];
                        Node[] popped = new Node[prod.Symbols.Length];
                        for(int i = popped.Length - 1; i >= 0; i--)
                        {
                            popped[i] = nodes.Pop();
                            states.Pop();
                        }

                        nodes.Push(new NonterminalNode<T>(popped, prod.Left.Name, prod));
                        states.Push(GotoTable[(states.Peek(), prod.Left.Name)]);
                        break;
                    case ParserActionOption.Accept:
                        break;
                }
                if(!ParseTable.ContainsKey((states.Peek(), token.Value)))
                {
                    T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                    throw new Exception($"Unexpected {token.Value} token at token stream index {position}. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
                }
                action = ParseTable[(states.Peek(), token.Value)];
            }

            return (NonterminalNode<T>)nodes.Pop();
        }

        public NonterminalNode<T> Parse(List<KeyValuePair<string, T>> tokens, Dictionary<Production<T>, Func<Node[], Node>> ruleExecutions)
        {
            Stack<int> states = new Stack<int>();
            Stack<Node> nodes = new Stack<Node>();
            states.Push(0);
            int position = 0;
            KeyValuePair<string, T> token = tokens[position];
            if (!ParseTable.ContainsKey((states.Peek(), token.Value)))
            {
                T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                throw new Exception($"Unexpected {token.Value} token at start of file. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
            }
            ParserAction action = ParseTable[(states.Peek(), token.Value)];
            while (action.Type != ParserActionOption.Accept)
            {
                switch (action.Type)
                {
                    case ParserActionOption.Shift:
                        states.Push(action.Parameter);
                        nodes.Push(new Terminal<T>() { TokenType = token.Value, TokenValue = token.Key });
                        position++;
                        token = tokens[position];
                        break;
                    case ParserActionOption.Reduce:
                        Production<T> prod = Grammar.Productions[action.Parameter];
                        Node[] popped = new Node[prod.Symbols.Length];
                        for (int i = popped.Length - 1; i >= 0; i--)
                        {
                            popped[i] = nodes.Pop();
                            states.Pop();
                        }
                        Node nodeToPush;
                        if(ruleExecutions.ContainsKey(prod))
                        {
                            nodeToPush = ruleExecutions[prod](popped);
                        }
                        else
                        {
                            nodeToPush = new NonterminalNode<T>(popped, prod.Left.Name, prod);
                        }
                        nodes.Push(nodeToPush);
                        states.Push(GotoTable[(states.Peek(), prod.Left.Name)]);
                        break;
                    case ParserActionOption.Accept:
                        break;
                }
                if (!ParseTable.ContainsKey((states.Peek(), token.Value)))
                {
                    T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                    throw new Exception($"Unexpected {token.Value} token at token stream index {position}. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
                }
                action = ParseTable[(states.Peek(), token.Value)];
            }

            return (NonterminalNode<T>)nodes.Pop();
        }

        public object Parse(List<KeyValuePair<string, T>> tokens, Dictionary<Production<T>, Func<object[], object>> ruleExecutions)
        {
            Stack<int> states = new Stack<int>();
            Stack<object> nodes = new Stack<object>();
            states.Push(0);
            int position = 0;
            KeyValuePair<string, T> token = tokens[position];
            if (!ParseTable.ContainsKey((states.Peek(), token.Value)))
            {
                T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                throw new Exception($"Unexpected {token.Value} token at start of file. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
            }
            ParserAction action = ParseTable[(states.Peek(), token.Value)];
            while (action.Type != ParserActionOption.Accept)
            {
                switch (action.Type)
                {
                    case ParserActionOption.Shift:
                        states.Push(action.Parameter);
                        nodes.Push(new Terminal<T>() { TokenType = token.Value, TokenValue = token.Key });
                        position++;
                        token = tokens[position];
                        break;
                    case ParserActionOption.Reduce:
                        Production<T> prod = Grammar.Productions[action.Parameter];
                        object[] popped = new object[prod.Symbols.Length];
                        for (int i = popped.Length - 1; i >= 0; i--)
                        {
                            popped[i] = nodes.Pop();
                            states.Pop();
                        }
                        object nodeToPush;
                        if (ruleExecutions.ContainsKey(prod))
                        {
                            nodeToPush = ruleExecutions[prod](popped);
                        }
                        else
                        {
                            nodeToPush = popped;//new NonterminalNode<T>(popped, prod.Left.Name, prod);
                        }
                        nodes.Push(nodeToPush);
                        states.Push(GotoTable[(states.Peek(), prod.Left.Name)]);
                        break;
                    case ParserActionOption.Accept:
                        break;
                }
                if (!ParseTable.ContainsKey((states.Peek(), token.Value)))
                {
                    T[] acceptableTokens = ParseTable.Keys.Where(a => a.Item1 == states.Peek()).Select(a => a.Item2).ToArray();
                    throw new Exception($"Unexpected {token.Value} token at token stream index {position}. Acceptable tokens for this position are: {acceptableTokens.Select(a => "\n" + a.ToString()).Aggregate((a, b) => a + b)}");
                }
                action = ParseTable[(states.Peek(), token.Value)];
            }

            return nodes.Pop();
        }
    }
}
