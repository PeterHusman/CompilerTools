using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{
    public static class CauliflowerThings
    {
        public static Dictionary<ThingType, string> TokenDefinitions = new Dictionary<ThingType, string>
        {
            [ThingType.EntrypointKeyword] = "entrypoint\\b",
            [ThingType.ClassKeyword] = "class\\b",
            [ThingType.AccessModifier] = "public\\b|private\\b",
            [ThingType.NamespaceKeyword] = "namespace\\b",
            [ThingType.ReturnKeyword] = "return\\b",
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
            [ThingType.MultiplyOperator] = @"\*",
            [ThingType.StaticKeyword] = @"static\b",
            [ThingType.Equality] = "==",
            [ThingType.BoolLiteral] = "false\\b|true\\b",
            [ThingType.StringLiteral] = @"""(\\[^\r\n]|[^""\\\r\n])*""",
            [ThingType.WhileKeyword] = "while\\b",
            [ThingType.MinusOperator] = "-",
            [ThingType.DotOperator] = @"\.",
            [ThingType.GreaterThan] = ">",
            [ThingType.NotEquals] = "!=",
            [ThingType.Not] = "!"
        };

        public static HashSet<ThingType> IgnoredTokens = new HashSet<ThingType> { ThingType.Whitespace, ThingType.Comment };

    }
}
