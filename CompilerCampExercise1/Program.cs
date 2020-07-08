using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{

    public enum ThingType
    {
        Equality,
        OpenCurlyBrace,
        BoolLiteral,
        CloseCurlyBrace,
        LessThan,
        AccessModifier,
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
        NumberLiteral,
        Whitespace,
        Comment,
        OpenParenthesis,
        CloseParenthesis,
        Comma,
        EqualsOperator,
        PlusOperator,
        DivideOperator
    }
    class Program
    {

        static HashSet<ThingType> useless = new HashSet<ThingType>(new[] { ThingType.Whitespace, ThingType.Comment });

        //World's worst tokenizer, do not use
        static void Main(string[] args)
        {
            //C:\Users\Peter.Husman\source\repos\CompilerCampExercise1\CompilerCampExercise1\
            string input = File.ReadAllText(@"../../Test.cs");

            List<KeyValuePair<string, ThingType>> thingies = new List<KeyValuePair<string, ThingType>>();

            Dictionary<ThingType, string> notRealRegexes = new Dictionary<ThingType, string> {
                [ThingType.ClassKeyword] = "class",
                [ThingType.AccessModifier] = "public|private",
                [ThingType.NamespaceKeyword] = "namespace",
                [ThingType.ReturnKeyword] = "return",
                [ThingType.Whitespace] = @"( |\t|\n|\r|\r\n)+",
                [ThingType.Comment] = @"//[^\r\n]*\n",
                [ThingType.Semicolon] = ";",
                [ThingType.OpenCurlyBrace] = @"\{",
                [ThingType.CloseCurlyBrace] = @"\}",
                [ThingType.Identifier] = "([a-z]|[A-Z])([a-z]|[A-Z]|[0-9])*",
                [ThingType.NumberLiteral] = @"[0-9]+",
                [ThingType.OpenParenthesis] = @"\(",
                [ThingType.LessThan] = "<",
                [ThingType.CloseParenthesis] = @"\)",
                [ThingType.Comma] = ",",
                [ThingType.EqualsOperator] = "=",
                [ThingType.PlusOperator] = @"\+",
                [ThingType.DivideOperator] = @"/",
                [ThingType.StaticKeyword] = "static",
                [ThingType.Equality] = "==",
                [ThingType.BoolLiteral] = "false|true",
                [ThingType.StringLiteral] = @"\"".*?\""",
                [ThingType.WhileKeyword] = "while"
            };

            #region dont
            List<KeyValuePair<ThingType, Regex>> regexes = new List<KeyValuePair<ThingType, Regex>>();
            foreach(ThingType type in notRealRegexes.Keys.OrderBy(a => (int)a))
            {
                regexes.Add(new KeyValuePair<ThingType, Regex>(type, new Regex($"^({notRealRegexes[type]})", RegexOptions.Compiled)));
            }

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(input.ToCharArray());

            //string remaining = input;
            
            //Again, do not use
            while(chars.Length > 0)
            {
                string remaining = chars.ToString();
                //pls

                KeyValuePair<ThingType, Regex>? type = regexes.FirstOrDefault(a => a.Value.IsMatch(remaining));

                //dont

                if(type == null || type.Value.Value == null)
                {
                    throw new Exception("lol invalid, get urself a real token and come back");
                }
                ThingType thingType = type.Value.Key;
                Regex regex = type.Value.Value;
                string match = regex.Match(remaining).Value;
                if(!useless.Contains(thingType))
                {
                    thingies.Add(new KeyValuePair<string, ThingType>(match, thingType));
                }

                //remaining = remaining.Remove(0, match.Length);
                if(match.Length == chars.Length)
                {
                    break;
                }
                chars = chars.Slice(match.Length);
            }
            #endregion

            foreach(var v in thingies)
            {
                Console.WriteLine($"({v.Key}, {v.Value.ToString()})");
            }

            Phase2 phase2 = new Phase2(thingies);
            CompilationUnit unit = phase2.Parse();


            Console.ReadKey(true);
        }
    }
}
