using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public abstract class BaseSymbol
    {
        public string Name { get; }
        public TextRange Range { get; }
        public IBaseNode Node { get; }
        public BaseSymbol(string name, TextRange range, IBaseNode node = null)
        {
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
