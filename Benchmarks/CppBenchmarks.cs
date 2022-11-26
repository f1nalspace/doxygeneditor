using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace Benchmarks
{
    [MinColumn, MaxColumn, MedianColumn]
    public class CppBenchmarks
    {
        public string HeaderSource { get; set; }

        public ImmutableArray<CppToken> HeaderTokens { get; set; }

        public ISymbolTableId SymbolTable { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            SymbolTable = new SimpleSymbolTableId(42);

            HeaderSource = global::Benchmarks.Properties.Resources.final_platform_layer_h;

            using (CppLexer lexer = new CppLexer(HeaderSource, HeaderSource.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = lexer.Tokenize();
                HeaderTokens = tokens.ToImmutableArray();
            }
        }

        [Benchmark]
        public int LexCpp()
        {
            using (CppLexer lexer = new CppLexer(HeaderSource, HeaderSource.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = lexer.Tokenize();
                return tokens.Count();
            }
        }

        [Benchmark]
        public int ParseCpp()
        {
            using (CppParser parser = new CppParser(SymbolTable, new CppParser.CppConfiguration()))
            {
                parser.ParseTokens(HeaderSource, HeaderTokens);

                IEnumerable<TextError> errors = parser.ParseErrors;
                return errors.Count();
            }
        }

        [Benchmark]
        public int LexAndParseCpp()
        {
            int totalErrorCount = 0;

            using (CppLexer lexer = new CppLexer(HeaderSource, HeaderSource.Length, new TextPosition(), LanguageKind.Cpp))
            {
                IEnumerable<CppToken> tokens = lexer.Tokenize();

                using (CppParser parser = new CppParser(SymbolTable, new CppParser.CppConfiguration()))
                {
                    parser.ParseTokens(HeaderSource, HeaderTokens);

                    IEnumerable<TextError> errors = parser.ParseErrors;

                    totalErrorCount += errors.Count();
                }
            }

            return totalErrorCount;
        }

        
    }
}
