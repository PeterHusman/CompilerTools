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
            foreach(var type in TypeTypes.typeRefs)
            {
                if(TypeTypes.dotNetTypes.ContainsKey(type.Value))
                {
                    continue;
                }

                types.Add(type.Value, ModuleBuilder.DefineType(type.Key, TypeAttributes.Public | TypeAttributes.Class));
            }
        }

        private Type TypeFromTypeType(TypeTypes type)
        {
            if(types.ContainsKey(type))
            {
                return types[type];
            }

            return TypeTypes.dotNetTypes[type];
        }

        public void RegisterFields()
        {
            foreach(var type in TypeTypes.typeRefs)
            {
                if (TypeTypes.dotNetTypes.ContainsKey(type.Value))
                {
                    continue;
                }

                foreach (var fieldInfo in type.Value.Fields)
                {
                    types[type.Value].DefineField(fieldInfo.Key, TypeFromTypeType(fieldInfo.Value.Type), FieldAttributes.Public);
                }

                foreach (var fieldInfo in type.Value.StaticFields)
                {
                    types[type.Value].DefineField(fieldInfo.Key, TypeFromTypeType(fieldInfo.Value.Type), FieldAttributes.Public | FieldAttributes.Static);
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
                    if(methInfo.Name == ".ctor")
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
            foreach(NonterminalNode<ThingType> classNode in classes)
            {
                string name = classNode.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
                TypeTypes type = TypeTypes.typeRefs[name];

                foreach(var method in type.Methods)
                {
                    method.
                }

                types[type].CreateType();
            }
        }
    }
}
