using System;
using System.Collections.Generic;
using CompilerCampExercise1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void BasicTests()
        {
            List<KeyValuePair<string, ThingType>> thingies = new Tokenizer(DSharpThings.TokenDefinitions, DSharpThings.IgnoredTokens).Tokenize("== = ><+-identifier class namespace int Class");

            List<KeyValuePair<string, ThingType>> correct = new List<KeyValuePair<string, ThingType>> {
                new KeyValuePair<string, ThingType>("==", ThingType.Equality),
                new KeyValuePair<string, ThingType>("=", ThingType.EqualsOperator),
                new KeyValuePair<string, ThingType>(">", ThingType.GreaterThan),
                new KeyValuePair<string, ThingType>("<", ThingType.LessThan),
                new KeyValuePair<string, ThingType>("+", ThingType.PlusOperator),
                new KeyValuePair<string, ThingType>("-", ThingType.MinusOperator),
                new KeyValuePair<string, ThingType>("identifier", ThingType.Identifier),
                new KeyValuePair<string, ThingType>("class", ThingType.ClassKeyword),
                new KeyValuePair<string, ThingType>("namespace", ThingType.NamespaceKeyword),
                new KeyValuePair<string, ThingType>("int", ThingType.Identifier),
                new KeyValuePair<string, ThingType>("Class", ThingType.Identifier),
            };

            Assert.AreEqual(thingies.Count, correct.Count);

            for (int i = 0; i < thingies.Count; i++)
            {
                Assert.AreEqual(thingies[i], correct[i]);
            }
        }

        [TestMethod]
        public void EdgeCases()
        {
            List<KeyValuePair<string, ThingType>> thingies = new Tokenizer(DSharpThings.TokenDefinitions, DSharpThings.IgnoredTokens).Tokenize("=== \"\\\"\"  \"\\\\\"");

            List<KeyValuePair<string, ThingType>> correct = new List<KeyValuePair<string, ThingType>> {
                new KeyValuePair<string, ThingType>("==", ThingType.Equality),
                new KeyValuePair<string, ThingType>("=", ThingType.EqualsOperator),
                new KeyValuePair<string, ThingType>("\"\\\"\"", ThingType.StringLiteral),
                new KeyValuePair<string, ThingType>("\"\\\\\"", ThingType.StringLiteral),
            };

            Assert.AreEqual(thingies.Count, correct.Count);

            for (int i = 0; i < thingies.Count; i++)
            {
                Assert.AreEqual(thingies[i], correct[i]);
            }
        }
    }
}
