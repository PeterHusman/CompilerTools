using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class TypeChecker
    {
        public void TypeCheck(NonterminalNode<ThingType> root)
        {
            ScopeStack scopes = new ScopeStack();
            Dictionary<string, TypeTypes> symbols = new Dictionary<string, TypeTypes>();

            var node = TypeTypes.GetChild(root, "CompilationUnit");
            foreach (NonterminalNode<ThingType> nonterm in node.Children)
            {
                TypeTypes typeTypes = new TypeTypes();
                typeTypes.Scan(nonterm);
                symbols.Add(((Terminal<ThingType>)nonterm.Children.First(a => a is Terminal<ThingType> b && b.TokenType == ThingType.Identifier)).TokenValue, typeTypes);
            }

            foreach(string className in symbols.Keys)
            {

            }
        }

        public CaulType TypeCheckExpression(Node root)
        {
            if(root is Terminal<ThingType> t)
            {
                switch(t.TokenType)
                {
                    case ThingType.IntLiteral:
                        return CaulType.Int;
                    case ThingType.StringLiteral:
                        return CaulType.String;
                    case ThingType.BoolLiteral:
                        return CaulType.Bool;
                    default:
                        throw new Exception();
                }
            }
            NonterminalNode<ThingType> nonterm = root as NonterminalNode<ThingType>;
            if (nonterm.Name == "AddSubExpression")
            {
                CaulType leftType = TypeCheckExpression(nonterm.Children[0]);
                CaulType rightType = TypeCheckExpression(nonterm.Children[2]);

            }
        }
    }
}
