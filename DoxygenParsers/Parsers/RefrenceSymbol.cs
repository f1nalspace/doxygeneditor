using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public class RefrenceSymbol
    {
        public IBaseNode Node { get; }
        public TextRange Range { get; }
        public RefrenceSymbol(IBaseNode node, TextRange range)
        {
            Node = node;
            Range = range;
        }
        public override string ToString()
        {
            return Node.ToString();
        }
    }
}
