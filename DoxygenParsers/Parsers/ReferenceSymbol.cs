using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public class ReferenceSymbol
    {
        public IBaseNode Node { get; }
        public TextRange Range { get; }
        public ReferenceTarget Target { get; }
        public ReferenceSymbol(IBaseNode node, TextRange range, ReferenceTarget target)
        {
            Node = node;
            Range = range;
            Target = target;
        }
        public override string ToString()
        {
            return Node.ToString();
        }
    }
}
