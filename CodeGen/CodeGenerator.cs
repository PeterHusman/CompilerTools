using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TypeChecking;

namespace CodeGen
{
    public class CodeGenerator
    {
        public AssemblyBuilder AssemblyBuilder { get; }

        public AssemblyName AsmName { get; }

        public ModuleBuilder ModuleBuilder { get; }

        private string fileName;


        public CodeGenerator(string name, string moduleName, string fileName)
        {
            AsmName = new AssemblyName(name);
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(moduleName, fileName);
            this.fileName = fileName;
            //ModuleBuilder.DefineType(, TypeAttributes.Public | TypeAttributes.Class);
            //TypeTypes.
        }

        private Dictionary<TypeTypes, TypeBuilder> types = new Dictionary<TypeTypes, TypeBuilder>();
        private Dictionary<FieldInformation, FieldBuilder> fields = new Dictionary<FieldInformation, FieldBuilder>();
        private Dictionary<MethodInformation, MethodBuilder> methods = new Dictionary<MethodInformation, MethodBuilder>();
        private Dictionary<MethodInformation, ConstructorBuilder> ctors = new Dictionary<MethodInformation, ConstructorBuilder>();

        private MethodBuilder entryPoint;

        public void Gen(NonterminalNode<ThingType>[] classes)
        {
            RegisterTypes();
            RegisterFields();
            MakeMethodBuilders();
            GenMethods(classes);

            AssemblyBuilder.SetEntryPoint(entryPoint);

            AssemblyBuilder.Save(fileName);
        }


        public void RegisterTypes()
        {
            foreach (var type in TypeTypes.typeRefs)
            {
                if (TypeTypes.dotNetTypes.ContainsKey(type.Value))
                {
                    continue;
                }

                types.Add(type.Value, ModuleBuilder.DefineType(type.Key, TypeAttributes.Public | TypeAttributes.Class));
            }
        }

        private Type TypeFromTypeType(TypeTypes type)
        {
            if (types.ContainsKey(type))
            {
                return types[type];
            }

            return TypeTypes.dotNetTypes[type];
        }

        public void RegisterFields()
        {
            foreach (var type in TypeTypes.typeRefs)
            {
                if (TypeTypes.dotNetTypes.ContainsKey(type.Value))
                {
                    continue;
                }

                foreach (var fieldInfo in type.Value.Fields)
                {
                    fields.Add(fieldInfo.Value, types[type.Value].DefineField(fieldInfo.Key, TypeFromTypeType(fieldInfo.Value.Type), FieldAttributes.Public));
                }

                foreach (var fieldInfo in type.Value.StaticFields)
                {
                    fields.Add(fieldInfo.Value, types[type.Value].DefineField(fieldInfo.Key, TypeFromTypeType(fieldInfo.Value.Type), FieldAttributes.Public | FieldAttributes.Static));
                }
            }
        }

        public void MakeMethodBuilders()
        {
            foreach (var type in TypeTypes.typeRefs)
            {
                if (TypeTypes.dotNetTypes.ContainsKey(type.Value))
                {
                    continue;
                }

                foreach (var methInfo in type.Value.Methods)
                {
                    if (methInfo.Name == ".ctor")
                    {
                        ctors.Add(methInfo, types[type.Value].DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, methInfo.Parameters.Select(a => TypeFromTypeType(a.Type)).ToArray()));
                        continue;
                    }
                    methods.Add(methInfo, types[type.Value].DefineMethod(methInfo.Name, MethodAttributes.Public, TypeFromTypeType(methInfo.ReturnType), methInfo.Parameters.Select(a => TypeFromTypeType(a.Type)).ToArray()));
                }

                foreach (var methInfo in type.Value.StaticMethods)
                {
                    methods.Add(methInfo, types[type.Value].DefineMethod(methInfo.Name, MethodAttributes.Public | MethodAttributes.Static, TypeFromTypeType(methInfo.ReturnType), methInfo.Parameters.Select(a => TypeFromTypeType(a.Type)).ToArray()));
                }
            }
        }

        public void GenMethods(NonterminalNode<ThingType>[] classes)
        {
            foreach (NonterminalNode<ThingType> classNode in classes)
            {
                string name = classNode.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
                TypeTypes type = TypeTypes.typeRefs[name];

                foreach (NonterminalNode<ThingType> member in TypeTypes.GetChild(classNode, "ClassMembers").Children)
                {
                    if (member.Name != "MethodDecl" && member.Name != "CtorDecl")
                    {
                        continue;
                    }

                    MethodInformation method = TypeTypes.meths[member];
                    MethodBuilder builder = methods[method];
                    ILGenerator ilgen = builder.GetILGenerator();

                    GenFromStatements(TypeTypes.GetChild(member, "Statements"), ilgen, method, new ScopeStack<string, LocalBuilder>(), type);
                }

                types[type].CreateType();


            }
        }

        public void GenFromStatements(NonterminalNode<ThingType> statementsNode, ILGenerator generator, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type2)
        {
            if (statementsNode == null)
            {
                return;
            }

            locals.PushNew();
            Scope<string, LocalBuilder> currentScope = locals.Current;

            foreach (NonterminalNode<ThingType> statement in statementsNode.Children)
            {
                if (statement.Name == "VarDecl")
                {
                    TypeTypes type = TypeTypes.FromTypeNameNonterminal((NonterminalNode<ThingType>)statement.Children[0]);
                    LocalBuilder local = generator.DeclareLocal(TypeFromTypeType(type));
                    if (statement.Children.Length == 5)
                    {
                        GenExpression(statement.Children[3] as NonterminalNode<ThingType>, generator, inf, locals, type2);
                        currentScope.Types.Add((statement.Children[1] as Terminal<ThingType>).TokenValue, local);
                        generator.Emit(OpCodes.Stloc, local);
                    }
                    else if (statement.Children.Length == 3)
                    {
                        currentScope.Types.Add((statement.Children[1] as Terminal<ThingType>).TokenValue, local);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if(statement.Name == "FunctionCall")
                {
                    if(GenFunctionCall(statement, generator, inf, locals, type2).Equals(TypeTypes.Void))
                    {

                    }
                    else
                    {
                        generator.Emit(OpCodes.Pop);
                    }
                }
            }
        }

        private TypeTypes GenFunctionCall(NonterminalNode<ThingType> nonterminalNode, ILGenerator generator, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type)
        {
            throw new NotImplementedException();
        }

        private static void EmitNot(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldc_I4, 0);
            gen.Emit(OpCodes.Ceq);
        }

        static string FromStringToken(string value)
        {
            List<char> chars = new List<char>(value.Length);
            int state = 0;
            value = value.Substring(1, value.Length - 2);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (state)
                {
                    case 0:
                        if (c == '\\')
                        {
                            state = 1;
                            break;
                        }

                        chars.Add(c);
                        break;
                    case 1:
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '\'':
                                chars.Add(c);
                                break;
                            case 'a':
                                chars.Add('\a');
                                break;
                            case 'b':
                                chars.Add('\b');
                                break;
                            case 'f':
                                chars.Add('\f');
                                break;
                            case 'n':
                                chars.Add('\n');
                                break;
                            case 'r':
                                chars.Add('\r');
                                break;
                            case 't':
                                chars.Add('\t');
                                break;
                            case 'v':
                                chars.Add('\v');
                                break;
                            default:
                                throw new Exception();
                        }
                        break;
                }
            }

            return new string(chars.ToArray());
        }

        private TypeTypes GenExpression(Node node, ILGenerator generator, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type)
        {
            if(node is Terminal<ThingType> term)
            {
                switch(term.TokenType)
                {
                    case ThingType.IntLiteral:
                        int val = int.Parse(term.TokenValue);
                        generator.Emit(OpCodes.Ldc_I4, val);
                        return TypeTypes.IntType;
                    case ThingType.BoolLiteral:
                        bool bVal = bool.Parse(term.TokenValue);
                        generator.Emit(OpCodes.Ldc_I4, bVal ? 1 : 0);
                        return TypeTypes.BoolType;
                    case ThingType.StringLiteral:
                        string sVal = FromStringToken(term.TokenValue);
                        generator.Emit(OpCodes.Ldstr, sVal);
                        return TypeTypes.BoolType;
                }

                throw new Exception();
            }

            NonterminalNode<ThingType> nonterminalNode = node as NonterminalNode<ThingType>;

            switch(nonterminalNode.Name)
            {
                case "Expression":
                    GenExpression(nonterminalNode.Children[0], generator, inf, locals, type);
                    GenExpression(nonterminalNode.Children[2], generator, inf, locals, type);
                    switch ((nonterminalNode.Children[1] as Terminal<ThingType>).TokenType)
                    {
                        case ThingType.NotEquals:
                            generator.Emit(OpCodes.Ceq);
                            EmitNot(generator);
                            break;
                        case ThingType.Equality:
                            generator.Emit(OpCodes.Ceq);
                            break;
                        case ThingType.LessThan:
                            generator.Emit(OpCodes.Clt);
                            break;
                        case ThingType.GreaterThan:
                            generator.Emit(OpCodes.Cgt);
                            break;
                    }
                    break;
                case "AddSubExpression":
                    GenExpression(nonterminalNode.Children[0], generator, inf, locals, type);
                    GenExpression(nonterminalNode.Children[2], generator, inf, locals, type);
                    switch ((nonterminalNode.Children[1] as Terminal<ThingType>).TokenType)
                    {
                        case ThingType.PlusOperator:
                            generator.Emit(OpCodes.Add);
                            break;
                        case ThingType.MinusOperator:
                            generator.Emit(OpCodes.Sub);
                            break;
                    }
                    break;
                case "Term":
                    GenExpression(nonterminalNode.Children[0], generator, inf, locals, type);
                    GenExpression(nonterminalNode.Children[2], generator, inf, locals, type);
                    switch ((nonterminalNode.Children[1] as Terminal<ThingType>).TokenType)
                    {
                        case ThingType.MultiplyOperator:
                            generator.Emit(OpCodes.Mul);
                            break;
                        case ThingType.DivideOperator:
                            generator.Emit(OpCodes.Div);
                            break;
                    }
                    break;
                case "FunctionCall":
                    GenFunctionCall(nonterminalNode, generator, inf, locals, type);
                    break;
                case "VarOrFieldReference":
                    GenGetFieldOrVar(nonterminalNode, generator, inf, locals, type);
                    break;
            }
            throw new NotImplementedException();
        }

        private TypeTypes GenGetFieldOrVar(NonterminalNode<ThingType> nonterminalNode, ILGenerator generator, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type)
        {
            
        }

        private (Variety variety, object reference) EmitNamespaced(NonterminalNode<ThingType> namespaced, ILGenerator generator, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type)
        {
            if(namespaced.Children.Length == 1)
            {
                (Variety variety, object reference) = FindIdent(((Terminal<ThingType>)namespaced.Children[0]).TokenValue, inf, locals, type);

                switch (variety)
                {
                    case Variety.Local:
                        generator.Emit(OpCodes.Ldloc, (LocalBuilder)reference);
                        break;
                    case Variety.Param:
                        generator.Emit(OpCodes.Ldarg, (LocalBuilder)reference);
                        break;
                    case Variety.Field:
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, (FieldBuilder)reference);
                        break;
                    case Variety.StaticField:
                        generator.Emit(OpCodes.Ldsfld, (FieldBuilder)reference);
                        break;
                    case Variety.Type:
                        break;
                    case Variety.Method:
                        break;
                    case Variety.StaticMethod:
                        break;
                }

                return (variety, reference);
            }


            (Variety variety1, object reference1) = EmitNamespaced(namespaced.Children[0] as NonterminalNode<ThingType>, generator, inf, locals, type);

            TypeTypes left;

            Variety returnVariety;
            object returnReference = null;

            switch (variety1)
            {
                case Variety.Local:
                    left = TypeTypeFromType(((LocalBuilder)reference1).LocalType);
                    break;
                case Variety.Param:
                    left = (inf.Parameters[type.Methods.Contains(inf) ? (int)reference1 - 1 : (int)reference1]).Type;
                    break;
                case Variety.Field:
                    left = TypeTypeFromType(((FieldBuilder)reference1).FieldType);
                    break;
                case Variety.StaticField:
                    left = TypeTypeFromType(((FieldBuilder)reference1).FieldType);
                    break;
                case Variety.Type:
                    left = TypeTypeFromType((Type)reference1);
                    break;
                case Variety.Method:
                case Variety.StaticMethod:
                    throw new Exception();
                default:
                    throw new Exception();
            }

            string name = ((Terminal<ThingType>)namespaced.Children[2]).TokenValue;

            foreach (var v in left.Fields)
            {
                if (v.Key == name)
                {
                    (returnVariety, returnReference) = (Variety.Field, fields[v.Value]);
                    break;
                }
            }

            if(returnReference == null)
            {
                foreach (var v in left.Methods)
                {
                    if (v.Name == name)
                    {
                        (returnVariety, returnReference) = (Variety.Method, methods[v]);
                        break;
                    }
                }
            }

            switch(returnVariety)
            {
                case Variety.Field:
                    generator.Emit(OpCodes.Ldfld, (FieldBuilder)returnReference);
                    break;
                case Variety.Method:
                    break;
            }
        }

        private TypeTypes TypeTypeFromType(Type t)
        {
            return TypeTypes.typeRefs[t.Name];
            //return types.First(a => a.Value == t).Key;
        }

        enum Variety
        {
            Local,
            Param,
            Field,
            StaticField,
            Type,
            Method,
            StaticMethod
        }

        private (Variety variety, object reference) FindIdent(string name, MethodInformation inf, ScopeStack<string, LocalBuilder> locals, TypeTypes type)
        {
            if(name == "this")
            {
                return (Variety.Param, 0);
            }

            if(locals.TrySearch(name, out LocalBuilder b))
            {
                return (Variety.Local, b);
            }

            for(int i = 0; i < inf.Parameters.Count; i++)
            {
                if(inf.Parameters[i].Name == name)
                {
                    if(type.Methods.Contains(inf))
                    {
                        return (Variety.Param, i + 1);
                    }
                    return (Variety.Param, i);
                }
            }

            foreach(var v in type.Fields)
            {
                if(v.Key == name)
                {
                    return (Variety.Field, fields[v.Value]);
                }
            }

            foreach (var v in type.StaticFields)
            {
                if (v.Key == name)
                {
                    return (Variety.StaticField, fields[v.Value]);
                }
            }

            foreach (var v in type.Methods)
            {
                if (v.Name == name)
                {
                    return (Variety.Method, methods[v]);
                }
            }

            foreach (var v in type.StaticMethods)
            {
                if (v.Name == name)
                {
                    return (Variety.StaticMethod, methods[v]);
                }
            }

            return (Variety.Type, types[TypeTypes.typeRefs[name]]);
        }
    }
}
