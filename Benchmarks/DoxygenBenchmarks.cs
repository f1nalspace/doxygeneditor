using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Languages.Doxygen;

namespace Benchmarks
{
    [MinColumn, MaxColumn, MedianColumn]
    public class DoxygenBenchmarks
    {
        [ParamsSource(nameof(DocBlocks))]
        public string BlockSource { get; set; }

        public string[] DocBlocks { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            BlockSource = global::Benchmarks.Properties.Resources.final_platform_layer_docs;

            var blocks = new List<string>();

            using (CppLexer lexer = new CppLexer(BlockSource, BlockSource.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = lexer.Tokenize();
                IEnumerable<CppToken> docTokens = tokens.Where(t => t.Kind == CppTokenKind.MultiLineCommentDoc);
                foreach (CppToken docToken in docTokens)
                    blocks.Add(docToken.Value);
            }

            DocBlocks = blocks.ToArray();
        }

        [Benchmark]
        public int LexDoxygen()
        {
            using (DoxygenBlockLexer lexer = new DoxygenBlockLexer(BlockSource, BlockSource.Length, new TextPosition()))
            {
                IEnumerable<DoxygenToken> tokens = lexer.Tokenize();
                return tokens.Count();
            }
        }
    }
}
