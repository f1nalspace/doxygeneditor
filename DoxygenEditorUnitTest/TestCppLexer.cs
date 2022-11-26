using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor
{
    [TestClass]
    public class TestCppLexer
    {
        [TestMethod]
        public void ParseFPLHeaderFile()
        {
            string headerFile = TSP.DoxygenEditor.Properties.Resources.final_platform_layer_h;
            using (CppLexer lexer = new CppLexer(headerFile, 0, headerFile.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = lexer.Tokenize();
                Assert.IsNotNull(tokens);
                Assert.IsTrue(tokens.Any());
            }
        }
    }
}