#define LR1Generator
using Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tokenizer;

namespace CompilerCampExercise1
{

    public enum ThingType
    {
        Equality,
        Increment,
        Decrement,
        NotEquals,
        Not,
        DotOperator,
        OpenCurlyBrace,
        BoolLiteral,
        CloseCurlyBrace,
        LessThan,
        GreaterThan,
        AccessModifier,
        EntrypointKeyword,
        ReturnKeyword,
        StaticKeyword,
        ClassKeyword,
        IfKeyword,
        WhileKeyword,
        ClassNameIdentifier,
        NamespaceKeyword,
        StringLiteral,
        Identifier,
        Semicolon,
        IntLiteral,
        Whitespace,
        Comment,
        OpenParenthesis,
        CloseParenthesis,
        Comma,
        EqualsOperator,
        MinusOperator,
        PlusOperator,
        DivideOperator,
        MultiplyOperator,
        EndOfStream
    }
    class Program
    {
        //World's worst tokenizer, do not use
        static void Main(string[] args)
        {

            //C:\Users\Peter.Husman\source\repos\CompilerCampExercise1\CompilerCampExercise1\
            string input = File.ReadAllText(@"../../Test.cs");

            var tokenizer = new Tokenizer<ThingType>(CauliflowerThings.TokenDefinitions, CauliflowerThings.IgnoredTokens, a => (int)a, ThingType.EndOfStream);

#if Specific
            List<KeyValuePair<string, ThingType>> thingies = tokenizer.Tokenize(input);

            Span<KeyValuePair<string, ThingType>> tokens = new Span<KeyValuePair<string, ThingType>>(thingies.ToArray());

            foreach (var v in thingies)
            {
                Console.WriteLine($"({v.Key}, {v.Value.ToString()})");
            }
#endif

#if LR1Generator
            Grammar<ThingType> grammar = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../GrammarDefinition.txt"));

            LR1Parser<ThingType> parser = new LR1Parser<ThingType>(new AugmentedGrammar<ThingType>(grammar), ThingType.EndOfStream);

            string inputParserTest = "1 + a.b.c().d - 3 * 4 / 5 + 6";

            NonterminalNode<ThingType> node = parser.Parse(tokenizer.Tokenize(inputParserTest));

            Console.WriteLine();
            RenderNode(node);

            Console.ReadKey(true);

            Grammar<ThingType> grammar2 = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../GrammarDefinition2.txt"));
            LR1Parser<ThingType> parser2 = new LR1Parser<ThingType>(new AugmentedGrammar<ThingType>(grammar2), ThingType.EndOfStream);
            NonterminalNode<ThingType> node2 = parser2.Parse(tokenizer.Tokenize("int Function() {}"));
            Console.WriteLine();
            RenderNode(node);
#endif

#if Specific


            Phase2 phase2 = new Phase2(thingies);
            CompilationUnit unit = phase2.Parse();

#endif
            Console.ReadKey(true);
        }

        static ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Blue };

        static void RenderNode(Node node, int indentation = 0)
        {
            //Console.Write(new string('|', indentation));
            for(int i = 0; i < indentation; i++)
            {
                Console.ForegroundColor = colors[i % colors.Length];
                Console.Write("| ");
            }
            Console.ForegroundColor = ConsoleColor.White;
            if (node is Terminal<ThingType> terminal)
            {
                Console.WriteLine($"{terminal.TokenType.ToString()}: {terminal.TokenValue}");
            }
            else if(node is NonterminalNode<ThingType> nonterm)
            {
                Console.WriteLine($"{nonterm.Name}");
                for(int i = 0; i < nonterm.Children.Length; i++)
                {
                    RenderNode(nonterm.Children[i], indentation + 1);
                }
            }
        }
    }
}
