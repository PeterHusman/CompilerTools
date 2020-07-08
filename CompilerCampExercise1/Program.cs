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
        OpenCurlyBrace,
        CloseCurlyBrace,
        AccessModifier,
        ReturnKeyword,
        StaticKeyword,
        ClassKeyword,
        ClassNameIdentifier,
        NamespaceKeyword,
        NamespaceIdentifier,
        Identifier,
        Semicolon,
        IntTypename,
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
                [ThingType.CloseParenthesis] = @"\)",
                [ThingType.Comma] = ",",
                [ThingType.EqualsOperator] = "=",
                [ThingType.PlusOperator] = @"\+",
                [ThingType.DivideOperator] = @"/",
                [ThingType.StaticKeyword] = "static"
            };

            #region dont
            Dictionary<ThingType, string> regexes = new Dictionary<ThingType, string>();
            foreach(ThingType type in notRealRegexes.Keys)
            {
                regexes.Add(type, $"^({notRealRegexes[type]})");
            }

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(input.ToCharArray());

            //string remaining = input;
            
            //Again, do not use
            while(chars.Length > 0)
            {
                string remaining = chars.ToString();
                //pls

                List<ThingType> types = regexes.Keys.Where(a => Regex.IsMatch(remaining, regexes[a])).OrderBy(a => (int)a).ToList();

                //dont

                if(types.Count <= 0)
                {
                    throw new Exception("lol invalid");
                }
                ThingType type = types[0];
                string match = Regex.Match(remaining, regexes[type]).Value;
                if(!useless.Contains(type))
                {
                    thingies.Add(new KeyValuePair<string, ThingType>(match, type));
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

            Console.ReadKey(true);

            Phase2 phase2 = new Phase2(thingies);
            phase2.Parse();
        }
    }
}
