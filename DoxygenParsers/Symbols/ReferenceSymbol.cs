using System;
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
            if (lang == LanguageKind.None)
                throw new ArgumentNullException("Expect language to be not none", nameof(lang));
            if (kind == ReferenceSymbolKind.Unknown)
                throw new ArgumentNullException("Expect kind to be not unknown", nameof(kind));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (range.Length == 0)
                throw new ArgumentException($"The range '{range}' is not valid", nameof(range));
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind} => {base.ToString()}";
        }
    }
}
