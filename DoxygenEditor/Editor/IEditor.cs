using System;
using System.Text;
using System.Windows.Forms;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Editor
{
    delegate void ParseEventHandler(IParseInfo parseInfo);
    delegate void FocusChangedEventHandler(IEditor sender, bool focused);
    delegate void JumpToEditorEventHandler(ISymbolTableId id, int position);

    interface IEditor : ISymbolTableId, IDisposable
    {
        TabPage Tab { get; }
        string Name { get; set; }
        string FilePath { get; set; }
        int TabIndex { get; }
        EditorFileType FileType { get; set; }
        Encoding FileEncoding { get; set; }

        bool IsChanged { get; set; }

        bool IsShowWhitespace { get; set; }

        IParseInfo ParseInfo { get; }

        Panel ContainerPanel { get; }

        event EventHandler TabUpdating;
        event FocusChangedEventHandler FocusChanged;
        event ParseEventHandler ParseCompleted;
        event ParseEventHandler ParseStarting;
        event JumpToEditorEventHandler JumpToEditor;

        void Stop();
        void Reparse();

        void ShowSearch();
        void ShowReplace();

        bool CanUndo();
        bool CanRedo();
        void Undo();
        void Redo();

        bool CanCut();
        bool CanCopy();
        bool CanPaste();
        void Cut();
        void Copy();
        void Paste();

        void Clear();
        void SelectAll();
        string GetText();
        void SetText(string text);
        void SetFocus();

        void GoToPosition(int position);
        void GoToLine(int lineIndex);
    }
}
