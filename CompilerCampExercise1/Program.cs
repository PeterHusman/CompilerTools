#define LR1Generator
using CauliflowerSpecifics;
using Parser;
using ParseTreeExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tokenizer;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using ObjectExaminer;

namespace CompilerCampExercise1
{   
    class Program
    {
        static Node MakeFlatList(Node[] nodes)
        {
            var root = nodes[0] as NonterminalNode<ThingType>;

            Node[] newChildren = new Node[root.Children.Length + nodes.Length - 1];
            root.Children.CopyTo(newChildren, 0);
            for(int i = root.Children.Length; i < newChildren.Length; i++)
            {
                newChildren[i] = nodes[i - root.Children.Length + 1];
            }

            root.Children = newChildren;
            return root;
        }

        static Node Identity(Node[] nodes)
        {
            return nodes[0];
        }

        static Node Second(Node[] nodes)
        {
            return nodes[1];
        }

        static object Identity(object[] nodes)
        {
            return nodes[0];
        }

        static object Second(object[] nodes)
        {
            return nodes[1];
        }

        static IEnumerable<Func<T[], T>> GetFuncs<T>()
        {
            IEnumerable<MethodInfo> methods = typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            IEnumerable<MethodInfo> methodsOfRightType = methods.Where(a => a.GetParameters().Length == 1 && a.GetParameters()[0].ParameterType == typeof(T[]) && a.ReturnType == typeof(T));

            return methodsOfRightType.Select(a => (Func<T[], T>)a.CreateDelegate(typeof(Func<T[], T>)));
        }

        //World's worst tokenizer, do not use
        static void Main(string[] args)
        {

            //C:\Users\Peter.Husman\source\repos\CompilerCampExercise1\CompilerCampExercise1\
            string input = File.ReadAllText(@"../../Test.cs");

            var tokenizer = new Tokenizer<ThingType>(CauliflowerThings.TokenDefinitions, CauliflowerThings.IgnoredTokens, a => (int)a, ThingType.EndOfStream);

            List<KeyValuePair<string, ThingType>> thingies = tokenizer.Tokenize(input);

            Span<KeyValuePair<string, ThingType>> tokens = new Span<KeyValuePair<string, ThingType>>(thingies.ToArray());

            foreach (var v in thingies)
            {
                Console.WriteLine($"({v.Key}, {v.Value.ToString()})");
            }

            var availableFuncs = GetFuncs<Node>();

            (Grammar<ThingType> grammar, var funcs) = Grammar<ThingType>.FromTextDefinitionWithProductionNodeFunctions(File.ReadAllText(@"../../CauliflowerGrammarDefinition.txt"), availableFuncs.ToDictionary(a => a.Method.Name));
            AugmentedGrammar<ThingType> augmentedGrammar = new AugmentedGrammar<ThingType>(grammar);

            LR1Parser<ThingType> parser = new LR1Parser<ThingType>(augmentedGrammar, ThingType.EndOfStream);
            object root = parser.Parse(thingies, funcs);

            //RenderNode(root);

            /*
            var parseTreeExplorer = new ParseTreeExplorer.ParseTreeExplorer();
            parseTreeExplorer.LoadParseTree(root);
            parseTreeExplorer.ShowDialog();
            */

            var objectExplorer = new ObjectExaminerForm();
            objectExplorer.LoadObject(root);
            objectExplorer.ShowDialog();

            //Console.ReadKey(true);

#if false
            Grammar<ThingType> grammar = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../GrammarDefinition.txt"));

            AugmentedGrammar<ThingType> augGrammar = new AugmentedGrammar<ThingType>(grammar);

            LR1Parser<ThingType> parser = new LR1Parser<ThingType>(augGrammar, ThingType.EndOfStream);

            //Pls figure it out when you're less tired.
            string inputParserTest = "1 + a.b.c(efg).d - 3 * 4 / 5 + 6";

            NonterminalNode<ThingType> node = parser.Parse(tokenizer.Tokenize(inputParserTest));

            //Console.WriteLine();
            RenderNode(node);

            Console.ReadKey(true);

            Grammar<ThingType> grammar2 = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../GrammarDefinition2.txt"));
            LR1Parser<ThingType> parser2 = new LR1Parser<ThingType>(new AugmentedGrammar<ThingType>(grammar2), ThingType.EndOfStream);
            NonterminalNode<ThingType> node2 = parser2.Parse(tokenizer.Tokenize("int Function(a b) {}"));
            Console.WriteLine();
            RenderNode(node2);
            Console.ReadKey(true);

            Grammar<ThingType> grammar3 = Grammar<ThingType>.FromTextDefinition(File.ReadAllText(@"../../ClassGrammarDefinition.txt"));
            LR1Parser<ThingType> parser3 = new LR1Parser<ThingType>(new AugmentedGrammar<ThingType>(grammar3), ThingType.EndOfStream);
            NonterminalNode<ThingType> node3 = parser3.Parse(tokenizer.Tokenize("public class Name {}"));
            Console.WriteLine();
            RenderNode(node3);
            Console.ReadKey(true);
#endif

#if Specific


            Phase2 phase2 = new Phase2(thingies);
            CompilationUnit unit = phase2.Parse();

#endif
            Console.ReadKey(true);
        }

        static ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Blue };

        static void RenderNode(Node node, int indentation = 0)
        {
            //Console.Write(new string('|', indentation));
            for(int i = 0; i < indentation; i++)
            {
                Console.ForegroundColor = colors[i % colors.Length];
                Console.Write("| ");
            }
            Console.ForegroundColor = ConsoleColor.White;
            if (node is Terminal<ThingType> terminal)
            {
                Console.WriteLine($"{terminal.TokenType.ToString()}: {terminal.TokenValue}");
            }
            else if(node is NonterminalNode<ThingType> nonterm)
            {
                Console.WriteLine($"{nonterm.Name}");
                for(int i = 0; i < nonterm.Children.Length; i++)
                {
                    RenderNode(nonterm.Children[i], indentation + 1);
                }
            }
        }
    }
}
