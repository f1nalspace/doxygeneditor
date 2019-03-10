using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class ReferenceSymbol
    {
        public IBaseNode Node { get; }
        public TextRange Range { get; }
        public ReferenceSymbolKind Kind { get; }
        public ReferenceSymbol(IBaseNode node, TextRange range, ReferenceSymbolKind kind)
        {
            Node = node;
            Range = range;
            Kind = kind;
        }
        public override string ToString()
        {
            return Node.ToString();
        }
    }
}
