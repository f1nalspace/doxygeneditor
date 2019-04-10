using System.Windows.Forms;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Editor
{
    public interface IEditorId : ISymbolTableId
    {
        TabPage Tab { get; }
        string Name { get; }
        string FilePath { get; }
        int TabIndex { get; }
    }
}
