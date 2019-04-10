using ScintillaNET;
using static TSP.DoxygenEditor.Styles.EditorStyler;

namespace TSP.DoxygenEditor.Styles
{
    interface IVisualStyler
    {
        void ApplyStyles(Scintilla editor);
        void Highlight(Scintilla editor, int startPos, int endPos);
        StyleEntry FindStyleFromPosition(int position);
    }
}
