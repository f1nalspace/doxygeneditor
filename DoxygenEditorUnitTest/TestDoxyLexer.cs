using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor
{
    [TestClass]
    public class TestDoxyLexer
    {
        class ExpectToken
        {
            public DoxygenTokenKind Kind { get; }
            public int MinLength { get; }
            public int MaxLength { get; }
            public string Value { get; }
            public ExpectToken(DoxygenTokenKind kind, int minLength, int maxLength, string value = null)
            {
                Kind = kind;
                MinLength = MaxLength = minLength;
                Value = value;
            }
            public ExpectToken(DoxygenTokenKind kind, int length, string value = null) : this(kind, length, length, value)
            {
            }
            public ExpectToken(DoxygenTokenKind kind, string value) : this(kind, value.Length, value.Length, value)
            {
            }
        }

        private void Lex(string source, params ExpectToken[] expectedTokens)
        {
            using (DoxygenBlockLexer lexer = new DoxygenBlockLexer(source, 0, source.Length, new TextPosition(0)))
            {
                IEnumerable<DoxygenToken> tokens = lexer.Tokenize();
                if (expectedTokens.Length > 0)
                {
                    Assert.AreEqual(expectedTokens.Length, tokens.Count());
                    int index = 0;
                    foreach (DoxygenToken token in tokens)
                    {
                        ExpectToken et = expectedTokens[index];
                        Assert.AreEqual(et.Kind, token.Kind);
                        Assert.IsTrue(token.Length >= et.MinLength, $"Expect min token length of {et.MinLength} but got {token.Length}");
                        Assert.IsTrue(token.Length <= et.MaxLength, $"Expect max token length of {et.MaxLength} but got {token.Length}");
                        if (et.Value != null)
                            Assert.AreEqual(et.Value, token.Value);
                        ++index;
                    }
                }
            }
        }

        [TestMethod]
        public void TestEmptyBlocks()
        {
            Lex($"//!");
            Lex($"//!{Environment.NewLine}");
            Lex($"//!{Environment.NewLine}//!");
            Lex($"/*");
            Lex($"/**/");
            Lex($"/**/{Environment.NewLine}");
            Lex($"/**/{Environment.NewLine}/**/");
            Lex($"/*Hello*/{Environment.NewLine}/*world*/");
        }

        [TestMethod]
        public void TestBlocks()
        {
            Lex($"//!@bri{Environment.NewLine}//!");
        }

        [TestMethod]
        public void ParseFPLDocs()
        {
            string docs = TSP.DoxygenEditor.Properties.Resources.final_platform_layer_docs;
            using (DoxygenBlockLexer lexer = new DoxygenBlockLexer(docs, 0, docs.Length, new TextPosition()))
            {
                IEnumerable<DoxygenToken> tokens = lexer.Tokenize();
                Assert.IsNotNull(tokens);
                Assert.IsTrue(tokens.Any());
            }
        }
    }
}
