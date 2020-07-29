using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmissionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyName = new AssemblyName("test");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("testModule",  "Test.exe");
            var typeBuilder = moduleBuilder.DefineType("testClass");
            Type type = typeBuilder as Type;
            var methodBuilder = typeBuilder.DefineMethod("Hi", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[0]);
            var ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldstr, "World, hi!");
            ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));
            ilGenerator.Emit(OpCodes.Ret);

            typeBuilder.CreateType();

            assemblyBuilder.SetEntryPoint(methodBuilder as MethodInfo);

            assemblyBuilder.Save("Test.exe");
        }
    }
}
