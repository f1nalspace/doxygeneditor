using DoxygenEditor.ViewModels;
using System;
using System.Windows.Forms;

namespace DoxygenEditor.Editor
{
    public delegate bool EditorChangedEventHandler(object sender, string text);
    public delegate void StyleChangedEventHandler(object sender, TimeSpan duration);
    interface IEditor
    {
        event EditorChangedEventHandler DelayedTextChanged;
        event StyleChangedEventHandler StyleChanged;
        event EventHandler FocusChanged;

        void Clear();
        void SetText(string text);
        string GetText();
        void GoToLine(int lineIndex);
        void GoToPosition(int position);
        void StopTimers();

        Control GetControl();

        void Undo();
        void Redo();
        bool CanUndo();
        bool CanRedo();

        void Cut();
        void Copy();
        void Paste();
        bool CanCut();
        bool CanCopy();
        bool CanPaste();

        void SelectAll();

        void ShowQuickSearch();
        void ShowQuickReplace();

        void SetShowWhitespaces(bool value);
    }
}
