using System;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Styles
{
    public readonly struct StyleEntry
    {
        public LanguageKind Lang { get; }
        public int Index { get; }
        public int Length { get; }
        public int Style { get; }

#if DEBUG
        public string Value { get; }
#endif

        public int End
        {
            get
            {
                int result = Index + Math.Max(0, Length - 1);
                return (result);
            }
        }

#if DEBUG
        public StyleEntry(LanguageKind lang, int index, int length, int style, string value = null)
        {
            Lang = lang;
            Index = index;
            Length = length;
            Style = style;
            Value = value;
        }

        public StyleEntry(LanguageKind lang, IBaseToken token, int style, string value = null) : this(lang, token.Index, token.Length, style, value)
        {
        }
#else
        public StyleEntry(LanguageKind lang, int index, int length, int style)
        {
            Lang = lang;
            Index = index;
            Length = length;
            Style = style;
        }

        public StyleEntry(LanguageKind lang, IBaseToken token, int style) : this(lang, token.Index, token.Length, style)
        {
        }
#endif

        public bool InterectsWith(StyleEntry other)
        {
            bool result = (Index <= other.End) && (End >= other.Index);
            return (result);
        }

        public override string ToString()
        {
#if DEBUG
            return $"{Index} => {Length} as {Lang} with style {Style} = {Value}";
#else
            return $"{Index} => {Length} as {Lang} with style {Style}";
#endif
        }
    }
}
