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
        Dictionary<string, TypeTypes> symbols = new Dictionary<string, TypeTypes>();

        public void TypeCheck(NonterminalNode<ThingType> root)
        {
            ScopeStack scopes = new ScopeStack();


            var node = TypeTypes.GetChild(root, "CompilationUnit");
            foreach (NonterminalNode<ThingType> nonterm in node.Children)
            {
                TypeTypes typeTypes = new TypeTypes();
                typeTypes.Scan(nonterm);
                symbols.Add(((Terminal<ThingType>)nonterm.Children.First(a => a is Terminal<ThingType> b && b.TokenType == ThingType.Identifier)).TokenValue, typeTypes);
            }

            foreach (NonterminalNode<ThingType> classNode in node.Children)
            {
                indices = new Dictionary<string, int>();
                staticIndices = new Dictionary<string, int>();
                var classMembersNode = TypeTypes.GetChild(classNode, "ClassMembers");
                TypeTypes classType = symbols[((Terminal<ThingType>)classNode.Children.First(a => a is Terminal<ThingType> b && b.TokenType == ThingType.Identifier)).TokenValue];

                foreach(NonterminalNode<ThingType> memberNode in classMembersNode.Children)
                {
                    if(memberNode.Name == "FieldDecl")
                    {
                        TypeCheckFieldDecl(memberNode, classType);
                        continue;
                    }

                    if(memberNode.Name == "MethodDecl" || memberNode.Name == "CtorDecl")
                    {
                        TypeCheckMethodDecl(memberNode, classType);
                    }
                }
            }
        }


        Dictionary<string, int> indices = new Dictionary<string, int>();
        Dictionary<string, int> staticIndices = new Dictionary<string, int>();
        void TypeCheckMethodDecl(NonterminalNode<ThingType> memberNode, TypeTypes classType)
        {
            NonterminalNode<ThingType> statementsNode = (NonterminalNode<ThingType>)memberNode.Children.FirstOrDefault(a => a is NonterminalNode<ThingType> n && n.Name == "Statements");
            if (statementsNode == null)
            {
                return;
            }

            string name = ((Terminal<ThingType>)memberNode.Children.First(a => a is Terminal<ThingType> b && b.TokenType == ThingType.Identifier)).TokenValue;
            MethodInformation info;
            bool isStatic = memberNode.Children.Any(a => a is NonterminalNode<ThingType> n && n.Name == "OptionalStatic") && ((NonterminalNode<ThingType>)memberNode.Children.First(a => a is NonterminalNode<ThingType> n && n.Name == "OptionalStatic")).Children.Length > 0;
            bool isCtor = memberNode.Name == "CtorDecl";
            if(isCtor)
            {
                name = ".ctor";
            }

            if(isStatic)
            {
                classType.TryGetStaticMethodsByName(name, out var ms);
                if(!staticIndices.ContainsKey(name))
                {
                    staticIndices.Add(name, -1);
                }
                staticIndices[name]++;
                info = ms[staticIndices[name]];
            }
            else
            {
                classType.TryGetInstanceMethodsByName(name, out var ms);
                if (!indices.ContainsKey(name))
                {
                    indices.Add(name, -1);
                }
                indices[name]++;
                info = ms[indices[name]];
            }

            Dictionary<string, TypeTypes> parameterTypeInfo = new Dictionary<string, TypeTypes>();

            foreach(var v in info.Parameters.Select(a => new KeyValuePair<string, TypeTypes>(a.Name, a.Type)))
            {
                parameterTypeInfo.Add(v.Key, v.Value);
            }

            ScopeStack scopeStack = new ScopeStack();
            scopeStack.Push(new Scope() { Types = parameterTypeInfo });

            //TODO: Typecheck body
        }

        void TypeCheckFieldDecl(NonterminalNode<ThingType> memberNode, TypeTypes classType)
        {
            if (memberNode.Children.Length > 5)
            {
                TypeTypes tp = TypeCheckExpression(memberNode.Children[memberNode.Children.Length - 2]);
                string name = ((Terminal<ThingType>)memberNode.Children.First(a => a is Terminal<ThingType> b && b.TokenType == ThingType.Identifier)).TokenValue;
                FieldInformation info;
                if (((NonterminalNode<ThingType>)memberNode.Children.First(a => a is NonterminalNode<ThingType> n && n.Name == "OptionalStatic")).Children.Length > 0)
                {
                    info = classType.StaticFields[name];
                }
                else
                {
                    info = classType.Fields[name];
                }

                if (!tp.Equals(info.Type) || TypeTypes.Void.Equals(tp))
                {
                    throw new Exception();
                }
            }
        }

        public TypeTypes TypeCheckExpression(Node root)
        {
            if (root is Terminal<ThingType> t)
            {
                switch (t.TokenType)
                {
                    case ThingType.IntLiteral:
                        return TypeTypes.IntType;
                    case ThingType.StringLiteral:
                        return TypeTypes.StringType;
                    case ThingType.BoolLiteral:
                        return TypeTypes.BoolType;
                    default:
                        throw new Exception();
                }
            }

            //throw new NotImplementedException();

            NonterminalNode<ThingType> nonterm = root as NonterminalNode<ThingType>;
            if (nonterm.Name == "AddSubExpression" || nonterm.Name == "Term")
            {
                TypeTypes leftType = TypeCheckExpression(nonterm.Children[0]);
                TypeTypes rightType = TypeCheckExpression(nonterm.Children[2]);
                string thingthing = ((Terminal<ThingType>)nonterm.Children[1]).TokenType.ToString();
                //if (((Terminal<ThingType>)nonterm.Children[1]).TokenType == ThingType.PlusOperator)
                //{
                if((leftType.Equals(TypeTypes.IntType)) && leftType.Equals(rightType))
                {
                    return leftType;
                }

                if ((leftType.Equals(TypeTypes.StringType)) && leftType.Equals(rightType) && thingthing == "PlusOperator")
                {
                    return leftType;
                }

                if (leftType.TryGetStaticMethod(thingthing, new[] { leftType, rightType }, out MethodInformation methodInfo))
                {
                    if (methodInfo.Parameters.Count == 2 && methodInfo.Parameters[0].Type.Equals(leftType) && methodInfo.Parameters[1].Type.Equals(rightType))
                    {
                        return methodInfo.ReturnType;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
                //}
            }
            else
            {
                throw new Exception();
            }
            //else if(nonterm)
        }
    }
}
