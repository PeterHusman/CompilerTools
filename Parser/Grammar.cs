using Parser.Symbols;
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
        //AnythingElse
    }


    public class Grammar<T> where T : Enum
    {
        public List<Production<T>> Productions { get; }

        private static Tokenizer<GrammarTokenType> tokenizer = new Tokenizer<GrammarTokenType>(new Dictionary<GrammarTokenType, string>
        {
            [GrammarTokenType.Equals] = "=",
            [GrammarTokenType.Pipe] = @"\|",
            [GrammarTokenType.Identifier] = "([a-z]|[A-Z])([a-z]|[A-Z]|[0-9])*",
            [GrammarTokenType.Whitespace] = @"( |\t|\n|\r|\r\n)+",
            [GrammarTokenType.Comment] = @"//[^\r\n]*\n",
            [GrammarTokenType.Newline] = @"[\r\n]+",
            //[GrammarTokenType.AnythingElse] = @"\S+"
        }, new HashSet<GrammarTokenType> { GrammarTokenType.Whitespace, GrammarTokenType.Comment }, a => (int)a);

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
            foreach(var tokenThing in tokenStream)
            {
                throw new NotImplementedException("TODO: Implement nonterm searching");
            }

            int state = 0;
            foreach(var tokenThing in tokenStream)
            {
                GrammarTokenType tokenType = tokenThing.Value;
                string tokenValue = tokenThing.Key;

                switch(state)
                {
                    case 0:
                        if(tokenType != GrammarTokenType.Identifier && tokenType != GrammarTokenType.Pipe)
                        {
                            throw new Exception();
                        }
                        if(tokenType == GrammarTokenType.Identifier)
                        {
                            currentLeft = nonterms[tokenValue];
                            leftProds = new List<Production<T>>();
                        }
                        state = 1;
                        break;
                    case 1:
                        if(tokenType == GrammarTokenType.Newline)
                        {
                            var prod = new Production<T>(symbols.ToArray(), currentLeft);
                            leftProds.Add(prod);
                            prods.Add(prod);
                            currentLeft.Productions = leftProds.ToArray();
                            symbols = new List<Symbol<T>>();
                            state = 0;
                            break;
                        }

                        if(tokenType == GrammarTokenType.Identifier)
                        {
                            if(nonterms.ContainsKey(tokenValue))
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
                }
            }

            throw new NotImplementedException();
        }
    }
}
