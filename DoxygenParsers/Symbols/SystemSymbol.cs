using System;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SystemSymbol : BaseSymbol
    {
        public SystemSymbolKind Kind { get; }

        public SystemSymbol(LanguageKind lang, SystemSymbolKind kind, string name) : base(lang, name, TextRange.Invalid)
        {
            if (lang == LanguageKind.None)
                throw new ArgumentNullException("Expect language to be not none", nameof(lang));
            if (kind == SystemSymbolKind.None)
                throw new ArgumentNullException("Expect kind to be not none", nameof(kind));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Kind = kind;
        }

        public override string ToString()
        {
            return $"{Kind} => {base.ToString()})";
        }
    }
}
