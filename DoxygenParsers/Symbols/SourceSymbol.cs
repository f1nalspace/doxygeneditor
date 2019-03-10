using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SourceSymbol
    {
        public IBaseNode Node { get; }
        public IBaseToken Token { get; }
        public SourceSymbolKind Kind { get; }

        public enum SymbolType
        {
       
        }

        public SourceSymbol(IBaseNode node, IBaseToken token, SourceSymbolKind kind)
        {
            Node = node;
            Token = token;
            Kind = kind;
        }
        public override string ToString()
        {
            return Node.ToString();
        }
    }
}
