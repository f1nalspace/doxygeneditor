using TSP.DoxygenEditor.SearchReplace;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Symbols;
using System.Threading;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Styles;
using System.Linq;
using ScintillaNET;

namespace TSP.DoxygenEditor.Editor
{
    class ScintillaEditor : IEditor, IDisposable
    {
        public TabPage Tab { get; }
        public int TabIndex { get; }
        public string FilePath { get; set; }
        public string Name { get; set; }
        public object SymbolTableId => FilePath;
        public EditorFileType FileType { get; set; }

        public bool IsChanged { get; set; }
        public Encoding FileEncoding { get; set; }

        public bool IsShowWhitespace
        {
            get { return _editor.ViewWhitespace != WhitespaceMode.Invisible; }
            set { _editor.ViewWhitespace = value ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible; }
        }
        
        public event EventHandler TabUpdating;
        public event FocusChangedEventHandler FocusChanged;
        public event ParseEventHandler ParseCompleted;
        public event ParseEventHandler ParseStarting;
        public event JumpToEditorEventHandler JumpToEditor;

        #region Parsing
        private ParseContext _parseState;
        public IParseInfo ParseInfo => _parseState;
        private IParseControl ParseControl => _parseState;
        #endregion

        private readonly IWin32Window _window;
        public Panel ContainerPanel { get; private set; }

        internal IVisualStyler VisualStyler => _visualStyler;

        private SearchReplaceControl _searchControl;
        private readonly Scintilla _editor;
        private int _maxLineNumberCharLength;
        private System.Windows.Forms.Timer _textChangedTimer;

        class StyleNeededState
        {
            private volatile int _value;
            public bool IsSet => _value > 0;
            public int StartPos { get; private set; }
            public int EndPos { get; private set; }
            public void Set(int startPos, int endPos)
            {
                Interlocked.Exchange(ref _value, 1);
                StartPos = StartPos;
                EndPos = endPos;
            }
            public void Reset()
            {
                Interlocked.Exchange(ref _value, 0);
            }
        }
        private StyleNeededState _styleNeededState;
        private readonly EditorStyler _editorStyler;
        private readonly IVisualStyler _visualStyler;

        public ScintillaEditor(IWin32Window window, WorkspaceModel workspace, string name, TabPage tab, int tabIndex)
        {
            // Mainform
            FilePath = null;
            Name = name;
            Tab = tab;
            IsChanged = false;
            FileEncoding = Encoding.UTF8;
            _window = window;

            // Editor and Parsing
            _editorStyler = new EditorStyler(workspace);

            // Parsing
            _parseState = new ParseContext(this, _editorStyler, workspace);
            _parseState.ParseCompleted += (s) =>
            {
                ParseCompleted?.Invoke(ParseInfo);
            };
            _parseState.ParseStarting += (s) =>
            {
                ParseStarting?.Invoke(ParseInfo);
            };

            // Editor
            _visualStyler = _editorStyler;
            _styleNeededState = new StyleNeededState();
            _editor = new Scintilla();
            SetupEditor(_editor);

            ContainerPanel = new Panel();
            ContainerPanel.Dock = DockStyle.Fill;
            ContainerPanel.Controls.Add(_editor);

            _searchControl = new SearchReplaceControl();
            _searchControl.Dock = DockStyle.Top;
            ContainerPanel.Controls.Add(_searchControl);

            _searchControl.Search += (s, direction) =>
            {
                SearchText(_window, _searchControl.SearchText, direction, _searchControl.MatchCase, _searchControl.MatchWords, _searchControl.IsRegex, _searchControl.IsWrap);
            };
            _searchControl.Replace += (s, mode) =>
            {
                ReplaceText(_window, _searchControl.SearchText, _searchControl.ReplaceText, mode, _searchControl.MatchCase, _searchControl.MatchWords, _searchControl.IsRegex, _searchControl.IsWrap);
            };
            _searchControl.FocusChanged += (s, focused) =>
            {
                FocusChanged?.Invoke(this, focused);
            };
            _searchControl.Hide();

            _maxLineNumberCharLength = 0;
            _textChangedTimer = new System.Windows.Forms.Timer() { Enabled = false, Interval = 250 };
            _textChangedTimer.Tick += (s, e) =>
            {
                if (!ParseControl.IsParsing())
                {
                    ParseControl.StartParsing(_editor.Text);
                    _textChangedTimer.Enabled = false;
                }
            };
            ParseCompleted += (s) =>
            {
                if (_styleNeededState.IsSet)
                    _editor.Colorize(_styleNeededState.StartPos, _styleNeededState.EndPos);
                else
                {
                    int firstLine = _editor.FirstVisibleLine;
                    int lastLine = firstLine + Math.Max(Math.Min(_editor.LinesOnScreen, _editor.Lines.Count) - 1, 0);
                    int start = _editor.Lines[firstLine].Position;
                    int end = _editor.Lines[lastLine].Position + Math.Max(_editor.Lines[lastLine].Length - 1, 0);
                    _editor.Colorize(start, end);
                }
            };
        }

        public void Reparse()
        {
            ParseControl.StopParsing();
            ParseControl.StartParsing(GetText());
        }

        #region Editor implementation
        public void ShowSearch()
        {
            _searchControl.ShowSearchOnly(true);
        }
        public void ShowReplace()
        {
            _searchControl.ShowSearchAndReplace(true);
        }

        public bool CanUndo()
        {
            bool result = !_searchControl.IsFocused() && _editor.CanUndo;
            return (result);
        }
        public bool CanRedo()
        {
            bool result = !_searchControl.IsFocused() && _editor.CanRedo;
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
            bool result = !_searchControl.IsFocused() && (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanCopy()
        {
            bool result = !_searchControl.IsFocused() && (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanPaste()
        {
            bool result = !_searchControl.IsFocused() && _editor.CanPaste;
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
        public void SelectAll()
        {
            _editor.SelectAll();
        }

        public void GoToPosition(int position)
        {
            int line = _editor.LineFromPosition(position);
            _editor.GotoPosition(position);

            // @TODO(final): Include element comment in the view if possible
            int firstVisible = _editor.FirstVisibleLine;
            if (line > firstVisible)
            {
                int delta = firstVisible - line;
                _editor.LineScroll(-delta, 0);
            }
            _editor.Focus();
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

        public void SetFocus()
        {
            if (_editor.CanFocus)
                _editor.Focus();
        }

        public string GetText()
        {
            string result = _editor.Text;
            return (result);
        }

        public void GoToLine(int lineIndex)
        {
            Line line = _editor.Lines[lineIndex];
            GoToPosition(line.Position);
        }

        private Match GetSearchMatch(string text, string searchText, int matchStart, SearchDirection direction, bool matchCase, bool wholeWord, bool isRegex, bool wrap)
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
            if (direction == SearchDirection.Prev)
                rexOptions |= RegexOptions.RightToLeft;
            Regex rex = new Regex(rexPattern, rexOptions);
            Match match = rex.Match(text, matchStart);
            return (match);
        }

        private bool SearchText(IWin32Window window, string searchText, SearchDirection direction, bool matchCase, bool wholeWord, bool isRegex, bool wrap)
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
                    if (direction == SearchDirection.Next)
                        selectionStart += selectionLength;
                }
                Match match = GetSearchMatch(text, searchText, selectionStart, direction, matchCase, wholeWord, isRegex, wrap);
                if (!match.Success && wrap)
                {
                    if (direction == SearchDirection.Next)
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
                    MessageBox.Show(window, $"No match for '{searchText}' found.", "No match", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            return (false);
        }

        private void ReplaceText(IWin32Window window, string searchText, string replacementText, ReplaceMode mode, bool matchCase, bool wholeWord, bool isRegex, bool wrap)
        {
            if (searchText != null && replacementText != null)
            {
                int replacementLength = replacementText.Length;
                if (mode == ReplaceMode.Next)
                {
                    if (_editor.SelectionStart == _editor.SelectionEnd)
                        SearchText(window, searchText, SearchDirection.Next, matchCase, wholeWord, isRegex, wrap);
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
                    while ((match = GetSearchMatch(text, searchText, searchStart, SearchDirection.Next, matchCase, wholeWord, isRegex, false)) != null)
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

        /// <summary>
        /// Called before removing this state
        /// </summary>
        public void Stop()
        {
            // Stop timer and parsing if needed
            _textChangedTimer.Enabled = false;
            if (ParseControl.IsParsing())
                ParseControl.StopParsing();
        }



        private Tuple<string, StyleEntry> FindTextStyleFromPosition(int position)
        {
            StyleEntry style = VisualStyler.FindStyleFromPosition(position);
            if (style.Style != 0)
            {
                string text = _editor.GetTextRange(style.Index, style.Length);
                if (text.Contains("(") && text.Contains(")"))
                    text = text.Substring(0, text.IndexOf("("));
                return new Tuple<string, StyleEntry>(text, style);
            }
            return (null);
        }

        private bool isShownIndicators = false;
        private void ShowIndicators(Point mouse)
        {
            if (!isShownIndicators)
                isShownIndicators = true;
            _editor.IndicatorClearRange(0, _editor.TextLength);
            Point p = _editor.PointToClient(mouse);
            int c = _editor.CharPositionFromPoint(p.X, p.Y);
            Tuple<string, StyleEntry> textStyle = FindTextStyleFromPosition(c);
            if (textStyle != null)
            {
                string symbolName = textStyle.Item1;
                StyleEntry style = textStyle.Item2;
                SymbolTable innerTable = GlobalSymbolCache.GetTable(this);
                SourceSymbol source = innerTable?.GetSource(symbolName);
                if (source == null)
                {
                    Tuple<SourceSymbol, ISymbolTableId> sourceTuple = GlobalSymbolCache.FindSource(symbolName);
                    if (sourceTuple != null)
                        source = sourceTuple.Item1;
                }
                if (source != null)
                {
                    TextRange symbolRange = new TextRange(new TextPosition(style.Index), style.Length);
                    _editor.IndicatorCurrent = 0;
                    _editor.IndicatorFillRange(symbolRange.Index, symbolRange.Length);
                }
            }
        }

        private void HideIndicators()
        {
            if (isShownIndicators)
            {
                isShownIndicators = false;
                _editor.IndicatorClearRange(0, _editor.TextLength);
            }
        }

        private void JumpToIndicator(int position)
        {
            // Find text & style
            Tuple<string, StyleEntry> textStyle = FindTextStyleFromPosition(position);
            if (textStyle == null) return;

            string symbolName = textStyle.Item1;

            // Search for inner source symbol (Self)
            SymbolTable innerTable = GlobalSymbolCache.GetTable(this);
            SourceSymbol bestInnerSource = innerTable?.GetSource(symbolName);
            ISymbolTableId bestInnerSourceId = this;

            // Search for extern source symbol
            SourceSymbol bestExternSource = null;
            ISymbolTableId bestExternSourceId = null;
            Tuple<SourceSymbol, ISymbolTableId> externSourceTuple = GlobalSymbolCache.FindSource(symbolName, (t) => t != bestInnerSourceId);
            if (externSourceTuple != null)
            {
                bestExternSource = externSourceTuple.Item1;
                bestExternSourceId = externSourceTuple.Item2;
            }

            // Determine highest source symbol based on language weights (Cpp < DoxygenCode < Doxygen < Html)
            SourceSymbol source = null;
            ISymbolTableId sourceId = null;
            if (bestInnerSource != null)
            {
                source = bestInnerSource;
                sourceId = this;
                if (bestExternSource != null)
                {
                    if (bestExternSource.Lang < bestInnerSource.Lang)
                    {
                        source = bestExternSource;
                        sourceId = bestExternSourceId;
                    }
                }
            }
            else if (bestExternSource != null)
            {
                source = bestExternSource;
                sourceId = bestExternSourceId;
            }

            // Jump to inner or outer source
            if (source != null)
            {
                if (sourceId == this)
                    GoToPosition(source.Range.Index);
                else
                    JumpToEditor?.Invoke(sourceId, source.Range.Index);
            }
        }

        private void SetupEditor(Scintilla target)
        {
            target.Dock = DockStyle.Fill;

            target.WrapMode = ScintillaNET.WrapMode.None;
            target.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            target.CaretLineVisible = true;
            target.CaretLineBackColorAlpha = 50;
            target.CaretLineBackColor = Color.CornflowerBlue;
            target.TabWidth = target.IndentWidth = 4;
            target.Margins[0].Width = 16;
            target.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            target.SetWhitespaceForeColor(true, Color.LightGray);
            target.UseTabs = true;

            target.MouseSelectionRectangularSwitch = false;
            target.MultipleSelection = false;

            target.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && isShownIndicators)
                {
                    int position = _editor.CharPositionFromPoint(e.X, e.Y);
                    JumpToIndicator(position);
                }
                HideIndicators();
            };

            Font editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);
            target.StyleResetDefault();
            target.Styles[Style.Default].Font = editorFont.Name;
            target.Styles[Style.Default].Size = (int)editorFont.SizeInPoints;
            target.StyleClearAll();
            target.Lexer = Lexer.Container;

            VisualStyler.ApplyStyles(target);

            target.TextChanged += (s, e) =>
            {
                Scintilla thisEditor = (Scintilla)s;

                // Autofit left-margin to fit-in line count number
                int maxLineNumberCharLength = thisEditor.Lines.Count.ToString().Length;
                if (maxLineNumberCharLength != _maxLineNumberCharLength)
                {
                    thisEditor.Margins[0].Width = thisEditor.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1));
                    _maxLineNumberCharLength = maxLineNumberCharLength;
                }

                IsChanged = true;
                TabUpdating?.Invoke(this, new EventArgs());

                _textChangedTimer.Stop();
                _textChangedTimer.Start();
            };

            target.StyleNeeded += (s, e) =>
            {
                Scintilla thisEditor = (Scintilla)s;
                IEditor editor = (IEditor)thisEditor.Parent.Tag;
                int startPos = thisEditor.GetEndStyled();
                int endPos = Math.Min(e.Position, thisEditor.TextLength - 1);
                int startLine = thisEditor.LineFromPosition(startPos);
                int endLine = thisEditor.LineFromPosition(endPos);
                startPos = thisEditor.Lines[startLine].Position;
                endPos = thisEditor.Lines[endLine].Position + Math.Max(thisEditor.Lines[endLine].Length - 1, 0);
                if (!ParseControl.IsParsing())
                {
                    if (startPos < endPos)
                        VisualStyler.Highlight(thisEditor, startPos, endPos);
                    _styleNeededState.Reset();
                }
                else
                    _styleNeededState.Set(startPos, endPos);
            };

            target.KeyDown += (s, e) =>
            {
                if (e.Control && (!e.Alt && !e.Shift))
                {
                    e.SuppressKeyPress = true;
                    if (e.KeyCode == Keys.Home || e.KeyCode == Keys.Up)
                        GoToPosition(0);
                    else if (e.KeyCode == Keys.End || e.KeyCode == Keys.Down)
                        GoToPosition(_editor.TextLength);
                    else if (e.KeyCode == Keys.ControlKey)
                    {
                        Point mouse = Cursor.Position;
                        ShowIndicators(mouse);
                    }
                }
                else if (!e.Alt && !e.Shift)
                {
                    if (e.KeyCode == Keys.F3)
                    {
                        if (_searchControl.IsShown())
                            SearchText(_window, _searchControl.SearchText, SearchDirection.Next, _searchControl.MatchCase, _searchControl.MatchWords, _searchControl.IsRegex, _searchControl.IsWrap);
                    }
                    else if (e.KeyCode == Keys.Escape)
                    {
                        if (_searchControl.IsShown())
                            _searchControl.HideSearchReplace();
                    }
                }
            };

            target.KeyUp += (s, e) => HideIndicators();

            target.InsertCheck += (s, e) =>
            {
                if ((e.Text == "\n") || (e.Text == "\r") || (e.Text == "\r\n"))
                {
                    int curLine = _editor.LineFromPosition(e.Position);
                    string curLineText = _editor.Lines[curLine].Text;
                    StringBuilder addon = new StringBuilder();
                    for (int i = 0; i < curLineText.Length; ++i)
                    {
                        char c = curLineText[i];
                        if (c == '\t' || c == ' ')
                            addon.Append(c);
                        else
                            break;
                    }
                    e.Text += addon.ToString();
                }
            };
        }
        #endregion

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
            _parseState.Dispose();
            _editor.Dispose();
            _searchControl.Dispose();
        }
        protected virtual void DisposeUnmanaged()
        {
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
                DisposeManaged();
            DisposeUnmanaged();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ScintillaEditor()
        {
            Dispose(false);
        }
        #endregion
    }
}
