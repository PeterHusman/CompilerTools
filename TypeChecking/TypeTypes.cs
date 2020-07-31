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
        public static TypeTypes IntType { get; }//FromDotNetType(typeof(int));

        public static TypeTypes StringType { get; }//FromDotNetType(typeof(string));

        public static TypeTypes BoolType { get; }//FromDotNetType(typeof(bool));

        public static TypeTypes Void { get; }

        static TypeTypes()
        {
            StringType = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", Parameters = new List<ParameterInformation>() } } };
            StringType.Methods[0].ReturnType = StringType;
            IntType = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", ReturnType = StringType, Parameters = new List<ParameterInformation>() } }, StaticMethods = new List<MethodInformation>() { new MethodInformation() { Name = "Parse", Parameters = new List<ParameterInformation>() } } };
            BoolType = new TypeTypes() { Methods = new List<MethodInformation>() { new MethodInformation() { Name = "ToString", ReturnType = StringType, Parameters = new List<ParameterInformation>() } } }; ;
            IntType.StaticMethods[0].Parameters.Add(new ParameterInformation() { Name = "value", Type = StringType });
            IntType.StaticMethods[0].ReturnType = IntType;

            Void = new TypeTypes();

            typeRefs = new Dictionary<string, TypeTypes> { ["int"] = IntType, ["Int32"] = IntType, ["bool"] = BoolType, ["Boolean"] = BoolType, ["string"] = StringType, ["String"] = StringType, ["void"] = Void };
            dotNetTypes = new Dictionary<TypeTypes, Type> { [IntType] = typeof(int), [BoolType] = typeof(bool), [StringType] = typeof(string), [Void] = typeof(void) };
            dotNetFieldInfos = new Dictionary<FieldInformation, FieldInfo>();
            dotNetMethodInfos = new Dictionary<MethodInformation, MethodInfo> { [IntType.Methods[0]] = typeof(int).GetMethod("ToString", new Type[0]), [BoolType.Methods[0]] = typeof(bool).GetMethod("ToString", new Type[0]), [StringType.Methods[0]] = typeof(string).GetMethod("ToString", new Type[0]) };
        }

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
        }

        public static Dictionary<string, TypeTypes> typeRefs;
        public static Dictionary<NonterminalNode<ThingType>, MethodInformation> meths = new Dictionary<NonterminalNode<ThingType>, MethodInformation>();
        public static Dictionary<TypeTypes, Type> dotNetTypes;
        public static Dictionary<FieldInformation, FieldInfo> dotNetFieldInfos;
        public static Dictionary<MethodInformation, MethodInfo> dotNetMethodInfos;

        private static void ConservativelyLoadDotNetType(Type type)
        {
            typeRefs.Add(type.Name, new TypeTypes());


            foreach (System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.FieldType.Name)))
            {
                FieldInformation toAdd = new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) };
                typeRefs[type.Name].Fields.Add(inf.Name, toAdd);
                dotNetFieldInfos.Add(toAdd, inf);
            }

            foreach (System.Reflection.FieldInfo inf in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.FieldType.Name)))
            {
                FieldInformation toAdd = new FieldInformation() { Name = inf.Name, Type = FromDotNetType(inf.FieldType) };
                typeRefs[type.Name].StaticFields.Add(inf.Name, toAdd);
                dotNetFieldInfos.Add(toAdd, inf);
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.ReturnType.Name) && a.GetParameters().All(b => typeRefs.ContainsKey(b.ParameterType.Name))))
            {
                if (inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                MethodInformation toAdd = new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() };
                typeRefs[type.Name].Methods.Add(toAdd);
                dotNetMethodInfos.Add(toAdd, inf);
            }

            foreach (System.Reflection.MethodInfo inf in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(a => typeRefs.ContainsKey(a.ReturnType.Name) && a.GetParameters().All(b => typeRefs.ContainsKey(b.ParameterType.Name))))
            {
                if (inf.GetParameters().Any(a => a.IsOut))
                {
                    continue;
                }
                MethodInformation toAdd = new MethodInformation() { Name = inf.Name, ReturnType = FromDotNetType(inf.ReturnType), Parameters = inf.GetParameters().Select(a => new ParameterInformation() { Name = a.Name, Type = FromDotNetType(a.ParameterType) }).ToList() };
                typeRefs[type.Name].StaticMethods.Add(toAdd);
                dotNetMethodInfos.Add(toAdd, inf);
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
                throw new Exception();
                //return;
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
                    meths.Add(member, method);
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
                    //string name = ".ctor";//member.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
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

        public static NonterminalNode<ThingType> GetChild(NonterminalNode<ThingType> node, string name)
        {
            return (NonterminalNode<ThingType>)node.Children.FirstOrDefault(a => a is NonterminalNode<ThingType> b && b.Name == name);
        }
    }
}
