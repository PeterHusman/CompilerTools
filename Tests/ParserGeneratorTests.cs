using CompilerCampExercise1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokenizer;

namespace Tests
{
    [TestClass]
    public class ParserGeneratorTests
    {
        [TestMethod]
        public void TestArithmetic()
        {
            LR1Parser<ThingType> parser = new LR1Parser<ThingType>(new AugmentedGrammar<ThingType>(Grammar<ThingType>.FromTextDefinition(@"Expression = Expression PlusOperator Term
		   | Expression MinusOperator Term
		   | Term

Factor = OpenParenthesis Expression CloseParenthesis
       | IntLiteral

Term = Term MultiplyOperator Factor
	 | Term DivideOperator Factor
	 | Factor")), ThingType.EndOfStream);

            List<KeyValuePair<string, ThingType>> thingies = new Tokenizer<ThingType>(CauliflowerThings.TokenDefinitions, CauliflowerThings.IgnoredTokens, a => (int)a, ThingType.EndOfStream).Tokenize("1 + 2 - 3 * 4/5 + 6");

            NonterminalNode<ThingType> root = parser.Parse(thingies);

            Assert.AreEqual(root.Producer.ToString(), "Expression = Expression PlusOperator Term");
            Assert.AreEqual(root.Children.Length, 3);
            Assert.AreEqual(root.Children[2].FullToString, "IntLiteral");
            Assert.AreEqual(((NonterminalNode<ThingType>)root.Children[0]).Producer.ToString(), "Expression = Expression MinusOperator Term");
            Assert.AreEqual(((NonterminalNode<ThingType>)(((NonterminalNode<ThingType>)root.Children[0]).Children[0])).Producer.ToString(), "Expression = Expression PlusOperator Term");
            Assert.AreEqual(((NonterminalNode<ThingType>)((NonterminalNode<ThingType>)root.Children[0]).Children[0]).Children[0].FullToString, "IntLiteral");

            Assert.AreEqual(((NonterminalNode<ThingType>)(((NonterminalNode<ThingType>)root.Children[0]).Children[2])).Producer.ToString(), "Term = Term DivideOperator Factor");
            Assert.AreEqual(((NonterminalNode<ThingType>)((NonterminalNode<ThingType>)((NonterminalNode<ThingType>)root.Children[0]).Children[2]).Children[0]).Producer.ToString(), "Term = Term MultiplyOperator Factor");
        }
    }
}
