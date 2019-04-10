using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol : BaseSymbol
    {
        public SourceSymbolKind Kind { get; }

        public SourceSymbol(LanguageKind lang, SourceSymbolKind kind, string name, TextRange range, IBaseNode node = null) : base(lang, name, range, node)
        {
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind} => {base.ToString()})";
        }
    }
}
