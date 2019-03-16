using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol : BaseSymbol
    {
        public SourceSymbolKind Kind { get; }

        public SourceSymbol(SourceSymbolKind kind, string name, TextRange range, IBaseNode node = null) : base(name, range, node)
        {
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind} => {base.ToString()})";
        }
    }
}
