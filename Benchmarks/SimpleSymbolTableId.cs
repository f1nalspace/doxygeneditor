using TSP.DoxygenEditor.Symbols;

namespace Benchmarks
{
    class SimpleSymbolTableId : ISymbolTableId
    {
        public object SymbolTableId { get; }

        public SimpleSymbolTableId(int id)
        {
            SymbolTableId = id;
        }
    }
}
