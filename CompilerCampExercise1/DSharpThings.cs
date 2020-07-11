using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{
    public static class DSharpThings
    {
        public static Dictionary<ThingType, string> TokenDefinitions = new Dictionary<ThingType, string>
        {
            [ThingType.EntrypointKeyword] = "entrypoint",
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
            [ThingType.IntLiteral] = @"[0-9]+",
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
            [ThingType.StringLiteral] = @"""(\\[^\r\n]|[^""\\\r\n])*""",
            [ThingType.WhileKeyword] = "while",
            [ThingType.MinusOperator] = "-",
            [ThingType.DotOperator] = @"\.",
            [ThingType.GreaterThan] = ">",
            [ThingType.NotEquals] = "!=",
            [ThingType.Not] = "!"
        };

        public static HashSet<ThingType> IgnoredTokens = new HashSet<ThingType> { ThingType.Whitespace, ThingType.Comment };

    }
}
