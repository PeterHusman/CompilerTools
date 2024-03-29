﻿using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokenizer;

namespace Parser
{
    enum GrammarTokenType
    {
        Identifier,
        Equals,
        Pipe,
        Newline,
        Whitespace,
        Comment,
        Colon
        //AnythingElse
    }


    public class Grammar<T> where T : Enum
    {
        internal Dictionary<Symbol<T>, HashSet<Symbol<T>>> firsts { get; set; }

        internal void GetFirsts()
        {
            Queue<NonterminalSymbol<T>> nonterms = new Queue<NonterminalSymbol<T>>(Productions.Select(a => a.Left).Distinct());

            foreach (NonterminalSymbol<T> nont in nonterms)
            {
                firsts.Add(nont, new HashSet<Symbol<T>>());
            }

            while (nonterms.Count > 0)
            {
                NonterminalSymbol<T> symbol = nonterms.Dequeue();
                if (!firsts.ContainsKey(symbol))
                {
                    firsts.Add(symbol, new HashSet<Symbol<T>>());
                }

                int firstSize = firsts[symbol].Count;

                if (symbol.Productions.Any(a => a.Symbols.Length == 0))
                {
                    firsts[symbol].Add(new TerminalEpsilon<T>());
                }

                foreach (Production<T> p in symbol.Productions)
                {
                    for (int i = 0; i < p.Symbols.Length; i++)
                    {
                        if (p.Symbols[i] is TerminalSymbol<T> tSym)
                        {
                            if (tSym is TerminalEpsilon<T>)
                            {
                                break;
                            }
                            firsts[symbol].Add(tSym);
                            break;
                        }
                        else if (p.Symbols[i] is NonterminalSymbol<T> nontermSym)
                        {
                            bool hasEp = false;
                            foreach (Symbol<T> sym in firsts[nontermSym])
                            {
                                if (sym is TerminalEpsilon<T>)
                                {
                                    hasEp = true;
                                    continue;
                                }
                                firsts[symbol].Add(sym);
                            }
                            if (!hasEp)
                            {
                                break;
                            }
                        }
                    }
                }

                int lastSize = firsts[symbol].Count;
                if (lastSize == 0 || lastSize != firstSize)
                {
                    nonterms.Enqueue(symbol);
                }
            }

        }


        public List<Production<T>> Productions { get; }

        private static Tokenizer<GrammarTokenType> tokenizer = new Tokenizer<GrammarTokenType>(new Dictionary<GrammarTokenType, string>
        {
            [GrammarTokenType.Equals] = "=",
            [GrammarTokenType.Pipe] = @"\|",
            [GrammarTokenType.Identifier] = "([a-z]|[A-Z])([a-z]|[A-Z]|[0-9])*",
            [GrammarTokenType.Whitespace] = @"( |\t)+",
            [GrammarTokenType.Comment] = @"//[^\r\n]*",
            [GrammarTokenType.Newline] = @"[\r\n]+",
            [GrammarTokenType.Colon] = ":"
            //[GrammarTokenType.AnythingElse] = @"\S+"
        }, new HashSet<GrammarTokenType> { GrammarTokenType.Whitespace, GrammarTokenType.Comment }, a => (int)a, GrammarTokenType.Newline);

        public Grammar(List<Production<T>> prods)
        {
            Productions = prods;
        }

        public static Grammar<T> FromTextDefinition(string text)
        {
            List<Production<T>> prods = new List<Production<T>>();
            var tokenStream = tokenizer.Tokenize(text + "\n");
            NonterminalSymbol<T> currentLeft = null;
            List<Production<T>> leftProds = new List<Production<T>>();
            List<Symbol<T>> symbols = new List<Symbol<T>>();

            Dictionary<string, NonterminalSymbol<T>> nonterms = new Dictionary<string, NonterminalSymbol<T>>();

            int state = 0;

            foreach (var tokenThing in tokenStream)
            {
                switch (state)
                {
                    case 0:
                        if(tokenThing.Value == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenThing.Value != GrammarTokenType.Identifier && tokenThing.Value != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenThing.Value == GrammarTokenType.Identifier)
                        {
                            nonterms.Add(tokenThing.Key, new NonterminalSymbol<T>(null, tokenThing.Key));
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenThing.Value == GrammarTokenType.Newline)
                        {
                            state = 0;
                        }
                        break;
                }
            }

            state = 0;
            foreach (var token in tokenStream)
            {
                GrammarTokenType tokenType = token.Value;
                string tokenValue = token.Key;

                switch (state)
                {
                    case 0:
                        if(tokenType == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenType != GrammarTokenType.Identifier && tokenType != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            currentLeft = nonterms[tokenValue];
                            leftProds = new List<Production<T>>();
                            state = 2;
                            break;
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenType == GrammarTokenType.Newline)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 0;
                            break;
                        }

                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            if (nonterms.ContainsKey(tokenValue))
                            {
                                symbols.Add(nonterms[tokenValue]);
                            }
                            else
                            {
                                symbols.Add(new TerminalSymbol<T>((T)Enum.Parse(typeof(T), tokenValue)));
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case 2:
                        if(tokenType == GrammarTokenType.Equals)
                        {
                            state = 1;
                            break;
                        }
                        throw new Exception();
                }
            }

            return new Grammar<T>(prods);
        }

        public static (Grammar<T>, Dictionary<Production<T>, Func<Node[], Node>>) FromTextDefinitionWithProductionNodeFunctions(string text, Dictionary<string, Func<Node[], Node>> functionsToUse)
        {
            Dictionary<Production<T>, Func<Node[], Node>> funcs = new Dictionary<Production<T>, Func<Node[], Node>>();
            List<Production<T>> prods = new List<Production<T>>();
            var tokenStream = tokenizer.Tokenize(text + "\n");
            NonterminalSymbol<T> currentLeft = null;
            List<Production<T>> leftProds = new List<Production<T>>();
            List<Symbol<T>> symbols = new List<Symbol<T>>();

            Dictionary<string, NonterminalSymbol<T>> nonterms = new Dictionary<string, NonterminalSymbol<T>>();

            int state = 0;

            foreach (var tokenThing in tokenStream)
            {
                switch (state)
                {
                    case 0:
                        if (tokenThing.Value == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenThing.Value != GrammarTokenType.Identifier && tokenThing.Value != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenThing.Value == GrammarTokenType.Identifier)
                        {
                            nonterms.Add(tokenThing.Key, new NonterminalSymbol<T>(null, tokenThing.Key));
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenThing.Value == GrammarTokenType.Newline)
                        {
                            state = 0;
                        }
                        break;
                }
            }

            state = 0;
            foreach (var token in tokenStream)
            {
                GrammarTokenType tokenType = token.Value;
                string tokenValue = token.Key;

                switch (state)
                {
                    case 0:
                        if (tokenType == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenType != GrammarTokenType.Identifier && tokenType != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            currentLeft = nonterms[tokenValue];
                            leftProds = new List<Production<T>>();
                            state = 2;
                            break;
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenType == GrammarTokenType.Newline)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 0;
                            break;
                        }

                        if(tokenType == GrammarTokenType.Colon)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 3;
                            break;
                        }

                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            if (nonterms.ContainsKey(tokenValue))
                            {
                                symbols.Add(nonterms[tokenValue]);
                            }
                            else
                            {
                                symbols.Add(new TerminalSymbol<T>((T)Enum.Parse(typeof(T), tokenValue)));
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case 2:
                        if (tokenType == GrammarTokenType.Equals)
                        {
                            state = 1;
                            break;
                        }
                        throw new Exception();
                    case 3:
                        if(tokenType != GrammarTokenType.Identifier)
                        {
                            throw new Exception();
                        }

                        funcs.Add(prods[prods.Count - 1], functionsToUse[tokenValue]);
                        state = 4;
                        break;
                    case 4:
                        if (tokenType != GrammarTokenType.Newline)
                        {
                            throw new Exception();
                        }
                        state = 0;
                        break;
                }
            }

            return (new Grammar<T>(prods), funcs);
        }

        public static (Grammar<T>, Dictionary<Production<T>, Func<object[], object>>) FromTextDefinitionWithProductionObjectFunctions(string text, Dictionary<string, Func<object[], object>> functionsToUse)
        {
            Dictionary<Production<T>, Func<object[], object>> funcs = new Dictionary<Production<T>, Func<object[], object>>();
            List<Production<T>> prods = new List<Production<T>>();
            var tokenStream = tokenizer.Tokenize(text + "\n");
            NonterminalSymbol<T> currentLeft = null;
            List<Production<T>> leftProds = new List<Production<T>>();
            List<Symbol<T>> symbols = new List<Symbol<T>>();

            Dictionary<string, NonterminalSymbol<T>> nonterms = new Dictionary<string, NonterminalSymbol<T>>();

            int state = 0;

            foreach (var tokenThing in tokenStream)
            {
                switch (state)
                {
                    case 0:
                        if (tokenThing.Value == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenThing.Value != GrammarTokenType.Identifier && tokenThing.Value != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenThing.Value == GrammarTokenType.Identifier)
                        {
                            nonterms.Add(tokenThing.Key, new NonterminalSymbol<T>(null, tokenThing.Key));
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenThing.Value == GrammarTokenType.Newline)
                        {
                            state = 0;
                        }
                        break;
                }
            }

            state = 0;
            foreach (var token in tokenStream)
            {
                GrammarTokenType tokenType = token.Value;
                string tokenValue = token.Key;

                switch (state)
                {
                    case 0:
                        if (tokenType == GrammarTokenType.Newline)
                        {
                            break;
                        }
                        if (tokenType != GrammarTokenType.Identifier && tokenType != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            currentLeft = nonterms[tokenValue];
                            leftProds = new List<Production<T>>();
                            state = 2;
                            break;
                        }
                        state = 1;
                        break;
                    case 1:
                        if (tokenType == GrammarTokenType.Newline)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 0;
                            break;
                        }

                        if (tokenType == GrammarTokenType.Colon)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 3;
                            break;
                        }

                        if (tokenType == GrammarTokenType.Identifier)
                        {
                            if (nonterms.ContainsKey(tokenValue))
                            {
                                symbols.Add(nonterms[tokenValue]);
                            }
                            else
                            {
                                symbols.Add(new TerminalSymbol<T>((T)Enum.Parse(typeof(T), tokenValue)));
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case 2:
                        if (tokenType == GrammarTokenType.Equals)
                        {
                            state = 1;
                            break;
                        }
                        throw new Exception();
                    case 3:
                        if (tokenType != GrammarTokenType.Identifier)
                        {
                            throw new Exception();
                        }

                        funcs.Add(prods[prods.Count - 1], functionsToUse[tokenValue]);
                        state = 4;
                        break;
                    case 4:
                        if (tokenType != GrammarTokenType.Newline)
                        {
                            throw new Exception();
                        }
                        state = 0;
                        break;
                }
            }

            return (new Grammar<T>(prods), funcs);
        }
    }
}
