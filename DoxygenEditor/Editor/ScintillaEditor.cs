/*
    This entire class is not MVVM/Solid conform at all, clean it up!

    - EditorViewModel
    - Remove SearchReplaceViewModel from here
    - It should be nothing more than a wrapper for Scintilla <-> IEditor
*/

using DoxygenEditor.Controls;
using DoxygenEditor.Lexer;
using DoxygenEditor.MVVM;
using DoxygenEditor.Services;
using DoxygenEditor.ViewModels;
using ScintillaNET;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DoxygenEditor.Editor
{
    class ScintillaEditor : IEditor
    {
        private readonly Panel _container;
        private readonly Panel _searchPanel;
        private readonly SearchReplaceControl _searchControl;

        // @TODO(final): Remove this, this is not MVVM conform!
        private readonly MainViewModel _mainViewModel;
        private readonly SearchReplaceViewModel _searchReplaceViewModel;

        private readonly ScintillaNET.Scintilla _editor;
        private readonly IDoxygenLexer _lexer;
        private readonly Timer _editorChangeTimer;
        private readonly Timer _selectionCheckTimer;
        private readonly IFileHandler _fileHandler;
        private int maxLineNumberCharLength;

        public event EditorChangedEventHandler DelayedTextChanged;
        public event StyleChangedEventHandler StyleChanged;
        public event EventHandler FocusChanged;

        public ScintillaEditor(IFileHandler fileHandler, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _container = new Panel() { Dock = DockStyle.None };

            _editor = new ScintillaNET.Scintilla();
            _editor.Dock = DockStyle.Fill;
            _editor.WrapMode = ScintillaNET.WrapMode.None;
            _editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            _editor.CaretLineVisible = true;
            _editor.CaretLineBackColorAlpha = 50;
            _editor.CaretLineBackColor = Color.CornflowerBlue;
            _editor.TabWidth = _editor.IndentWidth = 4;
            _editor.Margins[0].Width = 16;
            _editor.ViewWhitespace = WhitespaceMode.Invisible;
            _editor.UseTabs = true;

            _editor.TextChanged += _editor_TextChanged;
            _editor.UpdateUI += _editor_UpdateUI;
            _editor.InsertCheck += _editor_InsertCheck;
            _editor.Delete += _editor_Delete;
            _editor.StyleNeeded += _editor_StyleNeeded;
            _editor.KeyDown += _editor_KeyDown;
            _editor.GotFocus += (s, e) => FocusChanged?.Invoke(this, new EventArgs());
            _editor.LostFocus += (s, e) => FocusChanged?.Invoke(this, new EventArgs());

            _container.Controls.Add(_editor);

            _searchControl = new SearchReplaceControl();
            _searchReplaceViewModel = _searchControl.ViewModel;
            _searchPanel = new Panel() { Visible = true, AutoSize = true, Dock = DockStyle.Top };
            _searchControl.Dock = DockStyle.Top;
            _searchPanel.Controls.Add(_searchControl);
            _container.Controls.Add(_searchPanel);
            _searchPanel.Hide();

            _searchReplaceViewModel.SearchExecuted += (s, e) =>
            {
                SearchText(_searchReplaceViewModel.SearchText, e, _searchReplaceViewModel.MatchCase, _searchReplaceViewModel.WholeWord, _searchReplaceViewModel.IsRegex, _searchReplaceViewModel.Wrap);
            };
            _searchReplaceViewModel.ReplaceExecuted += (s, e) =>
            {
                ReplaceText(_searchReplaceViewModel.ReplaceText, e);
            };

            _lexer = new DoxygenLexer(_editor);
            _editorChangeTimer = new Timer() { Interval = 1000, Enabled = false };
            _editorChangeTimer.Tick += _editorChangeTimer_Tick;

            var editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);
            _editor.StyleResetDefault();
            _editor.Styles[ScintillaNET.Style.Default].Font = editorFont.Name;
            _editor.Styles[ScintillaNET.Style.Default].Size = (int)editorFont.SizeInPoints;
            _editor.StyleClearAll();
            _editor.Lexer = ScintillaNET.Lexer.Container;
            _lexer.Init();

            _selectionCheckTimer = new Timer() { Interval = 500, Enabled = true };
            _selectionCheckTimer.Tick += _selectionCheckTimer_Tick;

            _fileHandler = fileHandler;
        }

        public void SetShowWhitespaces(bool value)
        {
            if (value)
                _editor.ViewWhitespace = WhitespaceMode.VisibleAlways;
            else
                _editor.ViewWhitespace = WhitespaceMode.Invisible;
        }

        private void _selectionCheckTimer_Tick(object sender, EventArgs e)
        {
            _mainViewModel.CopyCommand.RaiseCanExecuteChanged();
            _mainViewModel.CutCommand.RaiseCanExecuteChanged();
        }

        private Match GetSearchMatch(string text, string searchText, int matchStart, SearchReplaceViewModel.SearchDirection direction, bool matchCase, bool wholeWord, bool isRegex, bool wrap)
        {
            Debug.Assert(text != null);
            string rexPattern;
            if (!isRegex)
            {
                string searchTextEscaped = Regex.Escape(searchText);
                rexPattern = searchTextEscaped;
            }
            else
                rexPattern = searchText;
            if (wholeWord)
                rexPattern = $"\\b({rexPattern})\\b";
            RegexOptions rexOptions = RegexOptions.Compiled;
            if (!matchCase)
                rexOptions |= RegexOptions.IgnoreCase;
            if (direction == SearchReplaceViewModel.SearchDirection.Prev)
                rexOptions |= RegexOptions.RightToLeft;
            Regex rex = new Regex(rexPattern, rexOptions);
            Match match = rex.Match(text, matchStart);
            return (match);
        }

        private bool SearchText(string searchText, SearchReplaceViewModel.SearchDirection direction, bool matchCase, bool wholeWord, bool isRegex, bool wrap)
        {
            Debug.Assert(searchText != null);
            string text = _editor.Text;
            if (!string.IsNullOrEmpty(text))
            {
                int selectionStart = _editor.SelectionStart;
                int selectionLength = 0;
                if (_editor.SelectionStart < _editor.SelectionEnd)
                {
                    selectionLength = _editor.SelectionEnd - _editor.SelectionStart;
                    if (direction == SearchReplaceViewModel.SearchDirection.Next)
                        selectionStart += selectionLength;
                }
                Match match = GetSearchMatch(text, searchText, selectionStart, direction, matchCase, wholeWord, isRegex, wrap);
                if (!match.Success && wrap)
                {
                    if (direction == SearchReplaceViewModel.SearchDirection.Next)
                        match = GetSearchMatch(text, searchText, 0, direction, matchCase, wholeWord, isRegex, wrap);
                    else
                        match = GetSearchMatch(text, searchText, text.Length - 1, direction, matchCase, wholeWord, isRegex, wrap);
                }
                if (match.Success)
                {
                    _editor.SelectionStart = match.Index;
                    _editor.SelectionEnd = match.Index + match.Length;
                    _editor.ScrollCaret();
                    _editor.Focus();
                    return (true);
                }
                else
                {
                    // No match found
                    var msgService = IOCContainer.Default.Get<IMessageBoxService>();
                    msgService.Show($"No match for '{searchText}' found.", "No match", MsgBoxButtons.OK, MsgBoxIcon.Exclamation);
                }
            }
            return (false);
        }
        private void ReplaceText(string replacementText, SearchReplaceViewModel.ReplaceMode mode)
        {
            Debug.Assert(replacementText != null);
            int replacementLength = replacementText.Length;
            string searchText = _searchReplaceViewModel.SearchText;
            if (searchText != null)
            {
                bool matchCase = _searchReplaceViewModel.MatchCase;
                bool wholeWord = _searchReplaceViewModel.WholeWord;
                bool isRegex = _searchReplaceViewModel.IsRegex;
                bool wrap = _searchReplaceViewModel.Wrap;
                if (mode == SearchReplaceViewModel.ReplaceMode.Next)
                {
                    if (_editor.SelectionStart == _editor.SelectionEnd)
                        SearchText(searchText, SearchReplaceViewModel.SearchDirection.Next, matchCase, wholeWord, isRegex, wrap);
                    if (_editor.SelectionStart < _editor.SelectionEnd)
                    {
                        int selStart = _editor.SelectionStart;
                        _editor.ReplaceSelection(replacementText);
                        _editor.ClearSelections();
                        _editor.GotoPosition(selStart + replacementLength);
                        _editor.Focus();
                    }
                }
                else
                {
                    Match match;
                    int searchStart = 0;
                    string text = _editor.Text;
                    while ((match = GetSearchMatch(text, searchText, searchStart, SearchReplaceViewModel.SearchDirection.Next, matchCase, wholeWord, isRegex, false)) != null)
                    {
                        if (!match.Success)
                            break;
                        text = text.Insert(match.Index + match.Length, replacementText);
                        text = text.Remove(match.Index, match.Length);
                        searchStart = match.Index + replacementLength;
                    }
                    _editor.Text = text;
                }
            }
        }

        private void _editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && (!e.Alt && !e.Shift))
            {
                e.SuppressKeyPress = true;
                if (e.KeyCode == Keys.Home || e.KeyCode == Keys.Up)
                    GoToPosition(0);
                else if (e.KeyCode == Keys.End || e.KeyCode == Keys.Down)
                    GoToPosition(_editor.TextLength);
            }
            else if (!e.Alt && !e.Shift)
            {
                if (e.KeyCode == Keys.F3)
                {
                    if (_searchReplaceViewModel.IsShown)
                        _searchReplaceViewModel.SearchExecutedCommand.Execute(SearchReplaceViewModel.SearchDirection.Next);
                }
                else if (e.KeyCode == Keys.Escape)
                    _searchReplaceViewModel.HideCommand.Execute(null);
            }
        }

        public bool CanUndo()
        {
            bool result = _editor.Focused && _editor.CanUndo;
            return (result);
        }
        public bool CanRedo()
        {
            bool result = _editor.Focused && _editor.CanRedo;
            return (result);
        }
        public void Undo()
        {
            _editor.Undo();
        }
        public void Redo()
        {
            _editor.Redo();
        }

        public bool CanCut()
        {
            bool result = _editor.Focused && (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanCopy()
        {
            bool result = _editor.Focused && (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanPaste()
        {
            bool result = _editor.Focused && _editor.CanPaste;
            return (result);
        }
        public void Cut()
        {
            _editor.Cut();
        }
        public void Copy()
        {
            _editor.Copy();
        }
        public void Paste()
        {
            _editor.Paste();
        }

        public Control GetControl()
        {
            return _container;
        }

        private void _editor_StyleNeeded(object sender, ScintillaNET.StyleNeededEventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            int startPos = _editor.GetEndStyled();
            int endPos = e.Position;
            _lexer.Style(startPos, endPos);
            w.Stop();
            StyleChanged?.Invoke(this, w.Elapsed);
        }

        private void _editor_Delete(object sender, ScintillaNET.ModificationEventArgs e)
        {
        }

        private void _editor_InsertCheck(object sender, InsertCheckEventArgs e)
        {
            if ((e.Text.EndsWith("\n")))
            {
                var curLine = _editor.LineFromPosition(e.Position);
                var curLineText = _editor.Lines[curLine].Text;
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < curLineText.Length; ++i)
                {
                    char c = curLineText[i];
                    if ((c != '\n') && char.IsWhiteSpace(c))
                        s.Append(c);
                    else
                        break;
                }
                e.Text += s.ToString();
            }
        }

        private void _editor_UpdateUI(object sender, ScintillaNET.UpdateUIEventArgs e)
        {
        }

        private void _editor_TextChanged(object sender, EventArgs e)
        {
            _editorChangeTimer.Stop();
            _editorChangeTimer.Start();
            if (!_fileHandler.IsChanged)
                _fileHandler.IsChanged = true;
        }

        private void _editorChangeTimer_Tick(object sender, EventArgs e)
        {
            // We dont want to trigger another change tick
            _editorChangeTimer.Stop();

            // Autofit left-margin to fit-in line count number
            int maxLineNumberCharLength = _editor.Lines.Count.ToString().Length;
            if (maxLineNumberCharLength != this.maxLineNumberCharLength)
            {
                const int padding = 2;
                _editor.Margins[0].Width = _editor.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
                this.maxLineNumberCharLength = maxLineNumberCharLength;
            }

            // Invoke delayed text change event
            string text = _editor.Text;
            bool? res = DelayedTextChanged?.Invoke(this, text);

            // Restart timer, when a expensive process is still going
            if (res.HasValue && !res.Value)
                _editorChangeTimer.Start();
        }

        public void Clear()
        {
            _editor.ClearAll();
        }

        public void SetText(string text)
        {
            _editor.ClearAll();
            _editor.Text = text;
            _editor.EmptyUndoBuffer();
        }

        public string GetText()
        {
            string result = _editor.Text;
            return (result);
        }

        public void GoToLine(int lineIndex)
        {
            var line = _editor.Lines[lineIndex];
            GoToPosition(line.Position);
        }

        public void GoToPosition(int position)
        {
            int line = _editor.LineFromPosition(position);
            _editor.GotoPosition(position);
            int firstVisible = _editor.FirstVisibleLine;
            if (line > firstVisible)
            {
                int delta = firstVisible - line;
                _editor.LineScroll(-delta, 0);
            }
            _editor.Focus();
        }

        public void StopTimers()
        {
            _editorChangeTimer.Stop();
        }

        public void SelectAll()
        {
            _editor.SelectAll();
        }

        public void ShowQuickSearch()
        {
            _searchPanel.Show();
            _searchReplaceViewModel.ShowSearchCommand.Execute(null);
        }

        public void ShowQuickReplace()
        {
            _searchPanel.Show();
            _searchReplaceViewModel.ShowReplaceCommand.Execute(null);
        }
    }
}
