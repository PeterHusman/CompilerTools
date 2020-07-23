using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class TypeTypes
    {
        public CaulType Self { get; set; }

        public Dictionary<string, MethodInformation> Methods { get; set; } = new Dictionary<string, MethodInformation>();

        public Dictionary<string, MethodInformation> StaticMethods { get; set; } = new Dictionary<string, MethodInformation>();

        public Dictionary<string, FieldInformation> Fields { get; set; } = new Dictionary<string, FieldInformation>();

        public Dictionary<string, FieldInformation> StaticFields { get; set; } = new Dictionary<string, FieldInformation>();

        public bool TryGetStaticMethod(string name, out MethodInformation method)
        {
            if(StaticMethods.ContainsKey(name))
            {
                method = StaticMethods[name];
                return true;
            }
            method = null;
            return false;
        }

        public void Scan(NonterminalNode<ThingType> nonterm)
        {
            NonterminalNode<ThingType> members = (NonterminalNode<ThingType>)nonterm.Children.FirstOrDefault(a => a is NonterminalNode<ThingType> b && b.Name == "ClassMembers");
            if (members == null)
            {
                return;
            }
            foreach (NonterminalNode<ThingType> member in members.Children.Select(a => (NonterminalNode<ThingType>)a))
            {
                if (member.Name == "FieldDecl")
                {
                    FieldInformation field = FieldInformation.FromNonterminal(member);
                    if (((NonterminalNode<ThingType>)member.Children[1]).Children.Length > 0)
                    {
                        StaticFields.Add(field.Name, field);
                    }
                    else
                    {
                        Fields.Add(field.Name, field);
                    }
                }
                else if (member.Name == "MethodDecl")
                {
                    //string name = member.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
                    MethodInformation method = MethodInformation.FromNonterminal(member);
                    if (((NonterminalNode<ThingType>)member.Children[1]).Children.Length > 0)
                    {
                        StaticMethods.Add(method.Name, method);
                    }
                    else
                    {
                        Methods.Add(method.Name, method);
                    }
                }
                else if (member.Name == "CtorDecl")
                {
                    string name = ".ctor";//member.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
                    MethodInformation method = MethodInformation.FromNonterminal(member, CaulType.Void);
                    Methods.Add(name, method);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        internal static NonterminalNode<ThingType> GetChild(NonterminalNode<ThingType> node, string name)
        {
            return (NonterminalNode<ThingType>)node.Children.FirstOrDefault(a => a is NonterminalNode<ThingType> b && b.Name == name);
        }
    }
}
