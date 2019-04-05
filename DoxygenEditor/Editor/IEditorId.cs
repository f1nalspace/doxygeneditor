using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Editor
{
    public interface IEditorId : ISymbolTableId
    {
        object Tag { get; }
        string Name { get; }
        string FilePath { get; }
        int TabIndex { get; }
    }
}
