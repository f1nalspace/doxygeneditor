using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public class SourceSymbol
    {
        public IBaseNode Node { get; }
        public TextRange Range { get; }
        public SymbolKind Kind { get; }

        public enum SymbolType
        {
       
        }

        public SourceSymbol(IBaseNode node, TextRange range, SymbolKind kind)
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
