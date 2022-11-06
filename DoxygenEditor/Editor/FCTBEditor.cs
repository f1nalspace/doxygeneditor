using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Models;

namespace TSP.DoxygenEditor.Editor
{
    class FCTBEditor : IEditor, IDisposable
    {
        private readonly IWin32Window _window;
        private readonly WorkspaceModel _workspace;
        private readonly Panel _containerPanel;
        private readonly FastColoredTextBox _editor;

        private readonly ParseContext _parseState;

        public Panel ContainerPanel => _containerPanel;

        public TabPage Tab { get; }
        public int TabIndex { get; }
        public string FilePath { get; set; }
        public string Name { get; set; }
        public object SymbolTableId => FilePath;
        public EditorFileType FileType { get; set; }

        public bool IsShowWhitespace { get; set; }

        public bool IsChanged { get; set; }
        public Encoding FileEncoding { get; set; }

        public event EventHandler TabUpdating;
        public event FocusChangedEventHandler FocusChanged;
        public event ParseEventHandler ParseCompleted;
        public event ParseEventHandler ParseStarting;
        public event JumpToEditorEventHandler JumpToEditor;

        public IParseInfo ParseInfo => _parseState;
        private IParseControl ParseControl => _parseState;

        public FCTBEditor(IWin32Window window, WorkspaceModel workspace, string name, TabPage tab, int tabIndex)
        {
            _window = window;
            _workspace = workspace;

            Name = name;
            Tab = tab;
            TabIndex = tabIndex;

            _editor = new FastColoredTextBox()
            {
                Dock = DockStyle.Fill,
                ShowLineNumbers = true,
                TabLength = 4,
                SyntaxHighlighter = null,
                VirtualSpace = true,
                Font = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular),
                DelayedTextChangedInterval = 250,
            };
            _containerPanel = new Panel()
            {
                Dock = DockStyle.Fill,
            };
            _containerPanel.Controls.Add(_editor);

            _parseState = new ParseContext(this, workspace);
            _parseState.ParseCompleted += (s) =>
            {
                // @TODO(final): Style text!

                ParseCompleted?.Invoke(ParseInfo);
            };
            _parseState.ParseStarting += (s) =>
            {
                ParseStarting?.Invoke(ParseInfo);
            };

            _editor.TextChangedDelayed += (s, e) =>
            {
                if (!_parseState.IsParsing())
                    _parseState.StartParsing(_editor.Text);
            };
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _parseState.Dispose();
                _editor.Dispose();
                _isDisposed = true;
            }
        }

        public void Stop()
        {
            if (_parseState.IsParsing())
                _parseState.StopParsing();
        }

        public void Reparse()
        {
            _parseState.StopParsing();
            _parseState.StartParsing(GetText());
        }

        public void ShowSearch() => _editor.ShowFindDialog();
        public void ShowReplace() => _editor.ShowReplaceDialog();

        public bool CanUndo() => _editor.UndoEnabled;
        public bool CanRedo() => _editor.RedoEnabled;

        public void Undo() => _editor.Undo();
        public void Redo() => _editor.Redo();

        public bool CanCut() => _editor.Selection.Length > 0;
        public bool CanCopy() => _editor.Selection.Length > 0;
        public bool CanPaste() => Clipboard.ContainsText();

        public void Cut() => _editor.Cut();
        public void Copy() => _editor.Copy();
        public void Paste() => _editor.Paste();
        public void Clear() => _editor.Clear();
        public void SelectAll() => _editor.SelectAll();

        public string GetText() => _editor.Text;
        public void SetText(string text) => _editor.Text = text;

        public void SetFocus() => _editor.Focus();

        public void GoToPosition(int position)
        {
            throw new NotImplementedException();
        }

        public void GoToLine(int lineIndex) => _editor.SetSelectedLine(lineIndex);

        public int StyleTokens(IEnumerable<IBaseToken> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
