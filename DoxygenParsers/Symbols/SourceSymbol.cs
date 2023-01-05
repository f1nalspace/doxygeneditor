using System;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol : BaseSymbol
    {
        public SourceSymbolKind Kind { get; }
        public string Caption { get; }

        public SourceSymbol(LanguageKind lang, SourceSymbolKind kind, string name, string caption, TextRange range, IBaseNode node = null) : base(lang, name, range, node)
        {
            if (lang == LanguageKind.None)
                throw new ArgumentNullException("Expect language to be not none", nameof(lang));
            if (kind == SourceSymbolKind.Unknown)
                throw new ArgumentNullException("Expect kind to be not unknown", nameof(kind));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(caption))
                throw new ArgumentNullException(nameof(caption));
            if (range.Length == 0)
                throw new ArgumentException($"The range '{range}' is not valid", nameof(range));
            Kind = kind;
            Caption = caption;
        }

        public SourceSymbol(LanguageKind lang, SourceSymbolKind kind, string name, TextRange range, IBaseNode node = null) : base(lang, name, range, node)
        {
            if (lang == LanguageKind.None)
                throw new ArgumentNullException("Expect language to be not none", nameof(lang));
            if (kind == SourceSymbolKind.Unknown)
                throw new ArgumentNullException("Expect kind to be not unknown", nameof(kind));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (range.Length == 0)
                throw new ArgumentException($"The range '{range}' is not valid", nameof(range));
            Kind = kind;
            Caption = null;
        }

        public override string ToString()
        {
            return $"{Kind} => {base.ToString()}";
        }
    }
}
