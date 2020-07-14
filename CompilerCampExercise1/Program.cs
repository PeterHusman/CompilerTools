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
        MinusOperator,
        Whitespace,
        Comment,
        OpenParenthesis,
        CloseParenthesis,
        Comma,
        EqualsOperator,
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

            List<KeyValuePair<string, ThingType>> thingies = new Tokenizer<ThingType>(CauliflowerThings.TokenDefinitions, CauliflowerThings.IgnoredTokens, a => (int)a).Tokenize(input);

            foreach (var v in thingies)
            {
                Console.WriteLine($"({v.Key}, {v.Value.ToString()})");
            }

            Phase2 phase2 = new Phase2(thingies);
            CompilationUnit unit = phase2.Parse();


            Console.ReadKey(true);
        }
    }
}
