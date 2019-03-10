using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol
    {
        public IBaseNode Node { get; }
        public TextRange Range { get; }
        public SourceSymbolKind Kind { get; }

        public enum SymbolType
        {
       
        }

        public SourceSymbol(IBaseNode node, TextRange range, SourceSymbolKind kind)
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
