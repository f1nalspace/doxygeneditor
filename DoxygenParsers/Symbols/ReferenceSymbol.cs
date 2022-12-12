using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class ReferenceSymbol : BaseSymbol
    {
        public ReferenceSymbolKind Kind { get; internal set; }
        public ReferenceSymbol(LanguageKind lang, ReferenceSymbolKind kind, string name, TextRange range, IBaseNode node) : base(lang, name, range, node)
        {
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind} => {base.ToString()}";
        }
    }
}
