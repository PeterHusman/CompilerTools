using CauliflowerSpecifics;
using Parser;
using System.Linq;

namespace TypeChecking
{
    public class FieldInformation
    {
        public string Name { get; set; }
        public TypeTypes Type { get; set; }

        public static FieldInformation FromNonterminal(NonterminalNode<ThingType> nonterm)
        {
            return new FieldInformation() { Type = TypeTypes.FromTypeNameNonterminal(TypeTypes.GetChild(nonterm, "TypeName")), Name = nonterm.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First()};
        }

        public override string ToString()
        {
            return $"{Name}: {Type}";
        }
    }
}