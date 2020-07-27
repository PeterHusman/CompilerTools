using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class TypeTypes
    {
        public static TypeTypes IntType { get; } = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", ReturnType = StringType, Parameters = new List<ParameterInformation>() } } };//FromDotNetType(typeof(int));

        public static TypeTypes StringType { get; } = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", ReturnType = StringType, Parameters = new List<ParameterInformation>() } } };//FromDotNetType(typeof(string));

        public static TypeTypes BoolType { get; } = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", ReturnType = StringType, Parameters = new List<ParameterInformation>() } } };//FromDotNetType(typeof(bool));

        public static TypeTypes Void { get; } = new TypeTypes();

        private static void PopulateDotNetType(Type type, TypeTypes typeTypes)
        {
            foreach(System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                typeTypes.Fields.Add(inf.Name, new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) });
            }

            foreach (System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                typeTypes.StaticFields.Add(inf.Name, new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) });
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if(inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                typeTypes.Methods.Add(new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() });
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                if (inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                typeTypes.StaticMethods.Add(new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() });
            }
        }

        public static TypeTypes FromTypeNameNonterminal(NonterminalNode<ThingType> nonterminalNode)
        {
            string typeName = ((Terminal<ThingType>)((NonterminalNode<ThingType>)nonterminalNode.Children[0]).Children.Last()).TokenValue;
            if(!typeRefs.ContainsKey(typeName))
            {
                var v = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes.Where(b => b.Name == typeName));
                ConservativelyLoadDotNetType(v.First());
            }
            return typeRefs[typeName];
            throw new NotImplementedException();
        }

        private static Dictionary<string, TypeTypes> typeRefs = new Dictionary<string, TypeTypes> { ["int"] = IntType, ["Int32"] = IntType, ["bool"] = BoolType, ["Boolean"] = BoolType, ["string"] = StringType, ["String"] = StringType, ["void"] = Void };

        private static void ConservativelyLoadDotNetType(Type type)
        {
            typeRefs.Add(type.Name, new TypeTypes());


            foreach (System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.FieldType.Name)))
            {
                typeRefs[type.Name].Fields.Add(inf.Name, new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) });
            }

            foreach (System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.FieldType.Name)))
            {
                typeRefs[type.Name].StaticFields.Add(inf.Name, new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) });
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.ReturnType.Name) && a.GetParameters().All(b => typeRefs.ContainsKey(b.ParameterType.Name))))
            {
                if (inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                typeRefs[type.Name].Methods.Add(new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() });
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.ReturnType.Name) && a.GetParameters().All(b => typeRefs.ContainsKey(b.ParameterType.Name))))
            {
                if (inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                typeRefs[type.Name].StaticMethods.Add(new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() });
            }

        }

        private static TypeTypes FromDotNetType(Type type)
        {
            if (typeRefs == null)
            {
                typeRefs = new Dictionary<string, TypeTypes>();
            }

            if (typeRefs.ContainsKey(type.Name))
            {
                return typeRefs[type.Name];
            }
            
            typeRefs.Add(type.Name, new TypeTypes());
            PopulateDotNetType(type, typeRefs[type.Name]);
            return typeRefs[type.Name];
        }

        public List<MethodInformation> Methods { get; set; } = new List<MethodInformation>();

        public List<MethodInformation> StaticMethods { get; set; } = new List<MethodInformation>();

        public Dictionary<string, FieldInformation> Fields { get; set; } = new Dictionary<string, FieldInformation>();

        public Dictionary<string, FieldInformation> StaticFields { get; set; } = new Dictionary<string, FieldInformation>();

        public bool TryGetStaticMethod(string name, TypeTypes[] parameters, out MethodInformation method)
        {
            method = StaticMethods.Where(a => a.Name == name).FirstOrDefault(a => a.Parameters.Select(b => b.Type).SequenceEqual(parameters));
            return method != null;
        }

        public bool TryGetStaticMethodsByName(string name, out MethodInformation[] methods)
        {
            methods = StaticMethods.Where(a => a.Name == name).ToArray();
            return methods.Any();
        }

        public bool TryGetInstanceMethodsByName(string name, out MethodInformation[] methods)
        {
            methods = Methods.Where(a => a.Name == name).ToArray();
            return methods.Any();
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
                        StaticMethods.Add(method);
                    }
                    else
                    {
                        Methods.Add(method);
                    }
                }
                else if (member.Name == "CtorDecl")
                {
                    string name = ".ctor";//member.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
                    MethodInformation method = MethodInformation.FromNonterminal(member, TypeTypes.Void);
                    AttemptAddMethod(method);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        void AttemptAddMethod(MethodInformation inf)
        {
            if(Methods.Contains(inf))
            {
                throw new Exception();
            }

            Methods.Add(inf);
        }

        void AttemptAddStaticMethod(MethodInformation inf)
        {
            if (StaticMethods.Contains(inf))
            {
                throw new Exception();
            }

            StaticMethods.Add(inf);
        }

        internal static NonterminalNode<ThingType> GetChild(NonterminalNode<ThingType> node, string name)
        {
            return (NonterminalNode<ThingType>)node.Children.FirstOrDefault(a => a is NonterminalNode<ThingType> b && b.Name == name);
        }
    }
}
