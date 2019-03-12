using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class ReferenceSymbol
    {
        public ReferenceSymbolKind Kind { get; }
        public TextRange Range { get; }
        public IBaseNode Node { get; }
        public ReferenceSymbol(ReferenceSymbolKind kind, TextRange range, IBaseNode node)
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
