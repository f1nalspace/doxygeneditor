using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class ReferenceSymbol : BaseSymbol
    {
        public ReferenceSymbolKind Kind { get; }
        public ReferenceSymbol(ReferenceSymbolKind kind, string name, TextRange range, IBaseNode node) : base(name, range, node)
        {
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind} => {base.ToString()}";
        }
    }
}
