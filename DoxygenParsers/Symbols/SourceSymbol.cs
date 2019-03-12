using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol
    {
        public TextRange Range { get; }
        public SourceSymbolKind Kind { get; }
        public IBaseNode Node { get; }

        public enum SymbolType
        {
       
        }

        public SourceSymbol(SourceSymbolKind kind, TextRange range, IBaseNode node = null)
        {
            Kind = kind;
            Range = range;
            Node = node;
        }
        public override string ToString()
        {
            return $"{Range} as {Kind} => {Node}";
        }
    }
}
