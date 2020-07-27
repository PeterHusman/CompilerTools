using CauliflowerSpecifics;
using Parser;
using System;

namespace TypeChecking
{
    public class ParameterInformation
    {
        public TypeTypes Type { get; set; }
        public string Name { get; set; }

        public static ParameterInformation FromNontermin\u0061l(NonterminalNode<ThingType> nonterm)
        {
            return new ParameterInformation() { Name = (nonterm.Children[1] as Terminal<ThingType>).TokenValue, Type = TypeTypes.FromTypeNameNonterminal(nonterm.Children[0] as NonterminalNode<ThingType>) };
        }
    }
}