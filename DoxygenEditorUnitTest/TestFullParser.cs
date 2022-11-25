using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor
{
    [TestClass]
    public class TestFullParser
    {
        private void Parse(BaseToken token)
        {
            
        }

        private void Parse(string source)
        {
            List<CppToken> documentationBlocks = new List<CppToken>();
            using (CppLexer cppLexer = new CppLexer(source, source.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = cppLexer.Tokenize();
                foreach (CppToken token in tokens)
                {
                    if (token.Kind == CppTokenKind.MultiLineCommentDoc ||
                        token.Kind == CppTokenKind.SingleLineCommentDoc)
                        documentationBlocks.Add(token);
                }
            }

            foreach (CppToken documentationBlock in documentationBlocks)
            {
                string blockSource = documentationBlock.Value;
                using (DoxygenBlockLexer cppLexer = new DoxygenBlockLexer(blockSource, blockSource.Length, new TextPosition()))
                {
                    IEnumerable<DoxygenToken> tokens = cppLexer.Tokenize();
                    foreach (DoxygenToken token in tokens)
                    {
                        if (token.Kind == DoxygenTokenKind.Code)
                        {
                            string code = token.Value;
                            using (CppLexer codeLexer = new CppLexer(code, code.Length, new TextPosition(), LanguageKind.Cpp))
                            {
                                IEnumerable<CppToken> codeTokens = codeLexer.Tokenize();
                                Assert.IsNotNull(codeTokens);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ParseFPLSources()
        {
            string headerSource = TSP.DoxygenEditor.Properties.Resources.final_platform_layer_h;
            string docsSource = TSP.DoxygenEditor.Properties.Resources.final_platform_layer_docs;
            Parse(headerSource);
            Parse(docsSource);
        }
    }
}
