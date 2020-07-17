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
            if (symbol != NextSymbol)
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

            return terms;

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

    enum ParserActionOption
    {
        Shift,
        Reduce,
        Accept
    }

    struct ParserAction
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

        private Dictionary<(int, T), ParserAction> parseTable;

        private Dictionary<(int, string), int> gotoTable;

        public LR1Parser(AugmentedGrammar<T> grammar, T endOfStream)
        {
            Grammar = grammar;
            EndOfStream = endOfStream;

            SetupParseTable();
        }

        static bool TryAdd<T1,T2>(Dictionary<T1,T2> dict, T1 key, T2 value)
        {
            if(dict.ContainsKey(key))
            {
                if(dict[key].Equals(value))
                {
                    return false;
                }

                throw new Exception("there is conflict in ur grammar");
                //return false;
            }

            dict.Add(key, value);
            return true;
        }

        public NonterminalNode<T> Parse(List<KeyValuePair<string, T>> tokens)
        {
            Stack<int> states = new Stack<int>();
            Stack<Node> nodes = new Stack<Node>();
            states.Push(0);
            int position = 0;
            KeyValuePair<string, T> token = tokens[position];
            ParserAction action = parseTable[(states.Peek(), token.Value)];
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
                        states.Push(gotoTable[(states.Peek(), prod.Left.Name)]);
                        break;
                    case ParserActionOption.Accept:
                        break;
                }
                if(!parseTable.ContainsKey((states.Peek(), token.Value)))
                {
                    throw new Exception($"Unexpected {token.Value} token");
                }
                action = parseTable[(states.Peek(), token.Value)];
            }

            return (NonterminalNode<T>)nodes.Pop();
        }



        private void SetupParseTable()
        {
            List<LR1ItemSet<T>> sets = new List<LR1ItemSet<T>>();

            sets.Add(new LR1ItemSet<T>(Grammar) { Items = new HashSet<LR1Item<T>>() { new LR1Item<T>() { DotPosition = 0, LookAhead = new TerminalSymbol<T>(EndOfStream), Production = Grammar.Productions.Last() } } });

            parseTable = new Dictionary<(int, T), ParserAction>();
            gotoTable = new Dictionary<(int, string), int>();

            sets[0].Closure();
            //sets.Add(sets[0].PassSymbol(Grammar.Productions.Last().Symbols[0]));
            //sets[1].Closure();

            bool actionDone = true;
            while(actionDone)
            {
                actionDone = false;

                for (int i = 0; i < sets.Count; i++)
                {
                    LR1ItemSet<T> set = sets[i];
                    if (set.Items.Any(a => a.DotPosition == 1 && a.Production == Grammar.Productions.Last()))
                    {
                        actionDone = TryAdd(parseTable, (i, EndOfStream), new ParserAction() { Type = ParserActionOption.Accept }) || actionDone;
                    }
                    foreach (LR1Item<T> item in set.Items.Where(a => a.DotPosition == a.Production.Symbols.Length))
                    {
                        if(item.Production.Left.Equals(Grammar.StartingSymbol))
                        {
                            continue;
                        }
                        actionDone = TryAdd(parseTable, (i, item.LookAhead.TokenType), new ParserAction() { Type = ParserActionOption.Reduce, Parameter = Grammar.Productions.IndexOf(item.Production) }) || actionDone;
                    }
                    foreach (Symbol<T> symbol in set.Items.Select(a => a.NextSymbol).Where(a => a != null).Distinct())
                    {
                        LR1ItemSet<T> setToMaybeAdd = set.PassSymbol(symbol);
                        setToMaybeAdd.Closure();

                        TerminalSymbol<T> term = symbol as TerminalSymbol<T>;
                        bool isTerm = term != null;


                        bool foundOne = false;
                        for (int j = 0; j < sets.Count; j++)
                        {
                            LR1ItemSet<T> setCheck = sets[j];
                            if (setToMaybeAdd.EqualsOtherItemSet(setCheck))
                            {
                                foundOne = true;
                                if (!isTerm)
                                {
                                    actionDone = TryAdd(gotoTable, (i, ((NonterminalSymbol<T>)symbol).Name), j) || actionDone;
                                    continue;
                                }

                                if (term is TerminalEpsilon<T>)
                                {
                                    throw new Exception();
                                }

                                //Delibrately NOT breaking after this because I want it to throw in case of shift-shift conflict
                                actionDone = TryAdd(parseTable, (i, term.TokenType), new ParserAction() { Type = ParserActionOption.Shift, Parameter = j }) || actionDone;
                            }
                        }

                        if (!foundOne)
                        {
                            if (isTerm)
                            {
                                TryAdd(parseTable, (i, term.TokenType), new ParserAction() { Type = ParserActionOption.Shift, Parameter = sets.Count });
                            }
                            else
                            {
                                TryAdd(gotoTable, (i, ((NonterminalSymbol<T>)symbol).Name), sets.Count);
                            }
                            sets.Add(setToMaybeAdd);
                            actionDone = true;
                        }
                    }
                }
            }
        }

        //private List<LR0Item>
    }
}
