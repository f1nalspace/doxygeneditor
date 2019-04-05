using ScintillaNET;
using static TSP.DoxygenEditor.Editor.EditorStyler;

namespace TSP.DoxygenEditor.Editor
{
    interface IVisualStyler
    {
        void CreateStyles(Scintilla editor);
        void Highlight(Scintilla editor, int startPos, int endPos);
        StyleEntry FindStyleFromPosition(int position);
    }
}
