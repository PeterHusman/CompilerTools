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
        MultiplyOperator
    }
    class Program
    {
        //World's worst tokenizer, do not use
        static void Main(string[] args)
        {
            //C:\Users\Peter.Husman\source\repos\CompilerCampExercise1\CompilerCampExercise1\
            string input = File.ReadAllText(@"../../Test.cs");

            var tokenizer = new Tokenizer<ThingType>(CauliflowerThings.TokenDefinitions, CauliflowerThings.IgnoredTokens, a => (int)a);

            List<KeyValuePair<string, ThingType>> thingies = tokenizer.Tokenize(input);

            Span<KeyValuePair<string, ThingType>> tokens = new Span<KeyValuePair<string, ThingType>>(thingies.ToArray());

            foreach (var v in thingies)
            {
                Console.WriteLine($"({v.Key}, {v.Value.ToString()})");
            }

            //Grammar<ThingType> grammar = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../GrammarDefinition.txt"));

            //BottomUpParser<ThingType> parser = new BottomUpParser<ThingType>(grammar);

            //NonterminalNode<ThingType> node = parser.Parse(tokenizer.Tokenize("1 + 2 - 3*4/5 + 6"));

            Phase2 phase2 = new Phase2(thingies);
            CompilationUnit unit = phase2.Parse();


            Console.ReadKey(true);
        }
    }
}
