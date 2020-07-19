using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal static class InternalParserStuff
    {

        internal static bool TryAdd<T1, T2>(Dictionary<T1, T2> dict, T1 key, T2 value, bool useGLR)
        {
            if (dict.ContainsKey(key))
            {
                if (dict[key].Equals(value))
                {
                    return false;
                }

                if (!useGLR)
                {
                    throw new Exception("there is conflict in ur grammar"); }
                //return false;
            }

            dict.Add(key, value);
            return true;
        }

        internal static (Dictionary<(int, T), ParserAction> parseTable, Dictionary<(int, string), int> gotoTable) MakeParseTable<T>(AugmentedGrammar<T> grammar, T EndOfStream, bool useGLR) where T : Enum
        {
            Dictionary<(int, T), ParserAction> ParseTable = new Dictionary<(int, T), ParserAction>();
            Dictionary<(int, string), int> GotoTable = new Dictionary<(int, string), int>();
            List<LR1ItemSet<T>> sets = new List<LR1ItemSet<T>>();

            sets.Add(new LR1ItemSet<T>(grammar) { Items = new HashSet<LR1Item<T>>() { new LR1Item<T>() { DotPosition = 0, LookAhead = new TerminalSymbol<T>(EndOfStream), Production = grammar.Productions.Last() } } });

            ParseTable = new Dictionary<(int, T), ParserAction>();
            GotoTable = new Dictionary<(int, string), int>();

            sets[0].Closure();
            //sets.Add(sets[0].PassSymbol(Grammar.Productions.Last().Symbols[0]));
            //sets[1].Closure();

            bool actionDone = true;
            while (actionDone)
            {
                actionDone = false;

                for (int i = 0; i < sets.Count; i++)
                {
                    LR1ItemSet<T> set = sets[i];
                    if (set.Items.Any(a => a.DotPosition == 1 && a.Production == grammar.Productions.Last()))
                    {
                        actionDone = TryAdd(ParseTable, (i, EndOfStream), new ParserAction() { Type = ParserActionOption.Accept }, useGLR) || actionDone;
                    }
                    foreach (LR1Item<T> item in set.Items.Where(a => a.DotPosition == a.Production.Symbols.Length))
                    {
                        if (item.Production.Left.Equals(grammar.StartingSymbol))
                        {
                            continue;
                        }
                        actionDone = TryAdd(ParseTable, (i, item.LookAhead.TokenType), new ParserAction() { Type = ParserActionOption.Reduce, Parameter = grammar.Productions.IndexOf(item.Production) }, useGLR) || actionDone;
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

                            //Does this work?
                            if (setToMaybeAdd.EqualsOtherItemSet(setCheck))
                            {
                                foundOne = true;
                                if (!isTerm)
                                {
                                    actionDone = TryAdd(GotoTable, (i, ((NonterminalSymbol<T>)symbol).Name), j, useGLR) || actionDone;
                                    continue;
                                }

                                if (term is TerminalEpsilon<T>)
                                {
                                    throw new Exception();
                                }

                                //Delibrately NOT breaking after this because I want it to throw in case of shift-shift conflict
                                actionDone = TryAdd(ParseTable, (i, term.TokenType), new ParserAction() { Type = ParserActionOption.Shift, Parameter = j }, useGLR) || actionDone;
                            }
                        }

                        if (!foundOne)
                        {
                            if (isTerm)
                            {
                                TryAdd(ParseTable, (i, term.TokenType), new ParserAction() { Type = ParserActionOption.Shift, Parameter = sets.Count }, useGLR);
                            }
                            else
                            {
                                TryAdd(GotoTable, (i, ((NonterminalSymbol<T>)symbol).Name), sets.Count, useGLR);
                            }
                            sets.Add(setToMaybeAdd);
                            actionDone = true;
                        }
                    }
                }
            }

            return (ParseTable, GotoTable);
        }
    }
}
