using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public abstract class BaseSymbol
    {
        public LanguageKind Lang { get; }
        public string Name { get; }
        public TextRange Range { get; }
        public IBaseNode Node { get; }
        public BaseSymbol(LanguageKind lang, string name, TextRange range, IBaseNode node = null)
        {
            Lang = lang;
            Name = name;
            Range = range;
            Node = node;
        }

        public override string ToString()
        {
            return $"{Name} ({Range})";
        }
    }
}
