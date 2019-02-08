using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lexers.Cpp;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lexers.Html;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.SearchReplace;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Parsers.Doxygen;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Lists;

namespace TSP.DoxygenEditor.Editor
{
    class EditorState
    {
        private static int _parseCounter = 0;
        private readonly IWin32Window _window;
        private Panel _containerPanel;
        private SearchReplaceControl _searchControl;
        private readonly Scintilla _editor;
        private int _maxLineNumberCharLength;
        private System.Windows.Forms.Timer _textChangedTimer;
        private BackgroundWorker _parseWorker;
        private BaseTree _doxyTree;
        private readonly List<BaseToken> _tokens = new List<BaseToken>();
        private readonly EditorStyler _styler = new EditorStyler();
        public BaseTree DoxyTree { get { return _doxyTree; } }
        public object Tag { get; set; }
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool IsChanged { get; set; }
        public Encoding FileEncoding { get; set; }
        public Panel Container { get { return _containerPanel; } }
        public bool IsShowWhitespace
        {
            get { return _editor.ViewWhitespace != WhitespaceMode.Invisible; }
            set { _editor.ViewWhitespace = value ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible; }
        }

        public delegate void ParseEventHandler(object sender, bool allDone);
        public event EventHandler TabUpdating;
        public event ParseEventHandler ParseComplete;

        public EditorState(IWin32Window window)
        {
            FilePath = null;
            Name = null;
            IsChanged = false;
            FileEncoding = Encoding.UTF8;

            _window = window;

            _editor = new Scintilla();
            SetupEditor(_editor);

            _containerPanel = new Panel();
            _containerPanel.Dock = DockStyle.Fill;
            _containerPanel.Controls.Add(_editor);

            _searchControl = new SearchReplaceControl();
            _searchControl.Dock = DockStyle.Top;
            _containerPanel.Controls.Add(_searchControl);

            _searchControl.Search += (s, direction) =>
            {
                SearchText(_window, _searchControl.SearchText, direction, _searchControl.MatchCase, _searchControl.MatchWords, _searchControl.IsRegex, _searchControl.IsWrap);
            };
            _searchControl.Replace += (s, mode) =>
            {
                ReplaceText(_window, _searchControl.SearchText, _searchControl.ReplaceText, mode, _searchControl.MatchCase, _searchControl.MatchWords, _searchControl.IsRegex, _searchControl.IsWrap);
            };
            _searchControl.Hide();

            _maxLineNumberCharLength = 0;
            _textChangedTimer = new System.Windows.Forms.Timer() { Enabled = false, Interval = 250 };
            _textChangedTimer.Tick += (s, e) =>
            {
                if (!_parseWorker.IsBusy)
                {
                    _textChangedTimer.Enabled = false;
                    _parseWorker.RunWorkerAsync(_editor.Text);
                }
            };
            _parseWorker = new BackgroundWorker();
            _parseWorker.DoWork += (s, e) =>
            {
                Interlocked.Increment(ref _parseCounter);
                string text = (string)e.Argument;
                Tokenize(text);
                Parse(text);
            };
            _parseWorker.RunWorkerCompleted += (s, e) =>
            {
                _editor.Colorize(0, _editor.TextLength - 1);
                bool allDone = Interlocked.Decrement(ref _parseCounter) == 0;
                ParseComplete?.Invoke(this, allDone);
            };
        }

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
            bool result = _editor.CanUndo;
            return (result);
        }
        public bool CanRedo()
        {
            bool result = _editor.CanRedo;
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
            bool result = (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanCopy()
        {
            bool result = (_editor.SelectionStart < _editor.SelectionEnd);
            return (result);
        }
        public bool CanPaste()
        {
            bool result = _editor.CanPaste;
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
            var line = _editor.Lines[lineIndex];
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

        public void Stop()
        {
            _textChangedTimer.Enabled = false;
            if (_parseWorker.IsBusy)
                _parseWorker.CancelAsync();
        }

        public void Start()
        {
            Debug.Assert(!_parseWorker.IsBusy);
            _parseWorker.RunWorkerAsync(_editor.Text);
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

        private void InsertTokens(BaseToken rootToken, SourceBuffer sourceBuffer, IEnumerable<BaseToken> newTokens)
        {
            if (rootToken != null)
            {
                int index = _tokens.IndexOf(rootToken);
                _tokens.InsertRange(index + 1, newTokens);
            }
            else
                _tokens.AddRange(newTokens);
        }

        class TokenTimingStats
        {
            public long CppDuration = 0;
            public long DoxyDuration = 0;
            public long HtmlDuration = 0;
        }

        class DescendedIntComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                int delta = y - x;
                return (delta);
            }
        }

        class TokenizeResult
        {
            private readonly Dictionary<BaseToken, TokenizeResult> _map = new Dictionary<BaseToken, TokenizeResult>();
            public Dictionary<BaseToken, TokenizeResult> Map { get { return _map; } }
            public IEnumerable<BaseToken> Tokens { get; }
            public BaseToken RootToken { get; }
            public TokenTimingStats Stats { get; }
            public TokenizeResult(BaseToken rootToken, IEnumerable<BaseToken> tokens)
            {
                RootToken = rootToken;
                Tokens = tokens;
                Stats = new TokenTimingStats();
            }
            public void AddToMap(BaseToken token, TokenizeResult result)
            {
                _map.Add(token, result);
            }
        }

        private TokenizeResult TokenizeCpp(BaseToken rootToken, string text, int index, int length)
        {
            Stopwatch timer = new Stopwatch();
            timer.Restart();
            SourceBuffer sourceBuffer = new StringSourceBuffer(text, index, length);
            CppLexer cppLexer = new CppLexer(sourceBuffer);
            IEnumerable<CppToken> cppTokens = cppLexer.Tokenize();
            timer.Stop();
            TokenizeResult result = new TokenizeResult(rootToken, cppTokens);
            result.Stats.CppDuration += timer.ElapsedMilliseconds;
            return (result);
        }

        private TokenizeResult TokenizeHtml(BaseToken rootToken, string text, int index, int length)
        {
            Stopwatch timer = new Stopwatch();
            timer.Restart();
            SourceBuffer sourceBuffer = new StringSourceBuffer(text, index, length);
            HtmlLexer htmlLexer = new HtmlLexer(sourceBuffer);
            IEnumerable<HtmlToken> htmlTokens = htmlLexer.Tokenize();
            timer.Stop();
            TokenizeResult result = new TokenizeResult(rootToken, htmlTokens);
            result.Stats.HtmlDuration += timer.ElapsedMilliseconds;
            return (result);
        }

        private TokenizeResult TokenizeDoxy(BaseToken rootToken, string text, int index, int length)
        {
            Stopwatch timer = new Stopwatch();
            timer.Restart();
            SourceBuffer sourceBuffer = new StringSourceBuffer(text, index, length);
            DoxygenLexer doxyLexer = new DoxygenLexer(sourceBuffer);
            IEnumerable<DoxygenToken> doxyTokens = doxyLexer.Tokenize();
            TokenizeResult result = new TokenizeResult(rootToken, doxyTokens);
            timer.Stop();
            result.Stats.DoxyDuration += timer.ElapsedMilliseconds;

            DoxygenToken prevToken = null;
            foreach (DoxygenToken doxyToken in doxyTokens)
            {
                if (doxyToken.Type == DoxygenTokenType.CodeBlock)
                {
                    string codeType = null;
                    if (prevToken != null)
                    {
                        if (prevToken.Type == DoxygenTokenType.CodeType)
                            codeType = text.Substring(prevToken.Index, prevToken.Length);
                    }
                    if ("{.c}".Equals(codeType, StringComparison.InvariantCultureIgnoreCase) || "{.cpp}".Equals(codeType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var cppRes = TokenizeCpp(doxyToken, text, doxyToken.Index, doxyToken.Length);
                        result.AddToMap(doxyToken, cppRes);
                    }
                }
                else if (doxyToken.Type == DoxygenTokenType.Text)
                {
                    var htmlRes = TokenizeHtml(doxyToken, text, doxyToken.Index, doxyToken.Length);
                    result.AddToMap(doxyToken, htmlRes);
                }

                prevToken = doxyToken;
            }
            return (result);
        }

        private void ExpandTokenResults(IEnumerable<TokenizeResult> results, List<BaseToken> outTokens)
        {
            foreach (var result in results)
            {
                int index = outTokens.IndexOf(result.RootToken);
                Debug.Assert(index != -1);
                outTokens.InsertRange(index + 1, result.Tokens);
                foreach (var p in result.Map)
                {
                    ExpandTokenResults(new[] { p.Value }, outTokens);
                }
            }
        }

        private void Tokenize(string text)
        {
            Stopwatch timer = new Stopwatch();

            // Clear all tokens
            Debug.Assert(_tokens.Count == 0);

            Stopwatch totalLexTimer = Stopwatch.StartNew();

            // C++ lexing
            TokenTimingStats stats = new TokenTimingStats();
            var cppRes = TokenizeCpp(null, text, 0, text.Length);
            _tokens.AddRange(cppRes.Tokens);

            // Doxy lexing
            List<TokenizeResult> tokenResults = new List<TokenizeResult>();
            object forTokensLock = new object();
            Parallel.ForEach(cppRes.Tokens,
                // Local init
                () => new List<TokenizeResult>(),
                // Body
                (token, state, local) =>
                {
                    CppToken cppToken = (CppToken)token;
                    if (cppToken.Type == CppTokenType.MultiLineCommentDoc || cppToken.Type == CppTokenType.SingleLineCommentDoc)
                    {
                        var doxyRes = TokenizeDoxy(token, text, token.Index, token.Length);
                        local.Add(doxyRes);
                    }
                    return (local);
                },
                // Local finally
                (local) =>
                {
                    lock (forTokensLock)
                    {
                        foreach (var res in local)
                            tokenResults.Add(res);
                    }
                }
            );
            totalLexTimer.Stop();
            Debug.WriteLine($"Lexing done (Total: {totalLexTimer.ElapsedMilliseconds} ms, C++: {stats.CppDuration} ms, Doxygen: {stats.DoxyDuration} ms, Html: {stats.HtmlDuration} ms)");

            timer.Restart();
            List<TokenizeResult> expandedTokenResults = new List<TokenizeResult>();
            ExpandTokenResults(tokenResults, _tokens);
            timer.Stop();
            Debug.WriteLine($"Expand done, took {timer.ElapsedMilliseconds} ms");

            timer.Restart();
            _styler.Refresh(_tokens);
            timer.Stop();
            Debug.WriteLine($"Styler done, took {timer.ElapsedMilliseconds} ms");
        }

        private void Parse(string text)
        {
            // Doxygen parsing
            Stopwatch timer = Stopwatch.StartNew();

            DoxygenTree doxyTree = new DoxygenTree();
            doxyTree.GetTextRange += (s, l) => text.Substring(s, l);

            LinkedListStream<BaseToken> tokenStream = new LinkedListStream<BaseToken>(_tokens);
            while (!tokenStream.IsEOF)
            {
                BaseToken old = tokenStream.CurrentValue;
                if (!doxyTree.ParseToken(tokenStream))
                    tokenStream.Next();
                Debug.Assert(old != tokenStream.CurrentValue);
            }

            if (_doxyTree != null) _doxyTree.Dispose();
            _doxyTree = doxyTree;

            timer.Stop();
            Debug.WriteLine($"Doxygen parse took {timer.ElapsedMilliseconds} ms");
        }

        private void SetupEditor(Scintilla editor)
        {
            editor.Dock = DockStyle.Fill;

            editor.WrapMode = ScintillaNET.WrapMode.None;
            editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            editor.CaretLineVisible = true;
            editor.CaretLineBackColorAlpha = 50;
            editor.CaretLineBackColor = Color.CornflowerBlue;
            editor.TabWidth = editor.IndentWidth = 4;
            editor.Margins[0].Width = 16;
            editor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            editor.SetWhitespaceForeColor(true, Color.LightGray);
            editor.UseTabs = true;

            Font editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = editorFont.Name;
            editor.Styles[Style.Default].Size = (int)editorFont.SizeInPoints;
            editor.StyleClearAll();
            editor.Lexer = Lexer.Container;

            _styler.InitStyles(editor);

            editor.TextChanged += (s, e) =>
            {
                Scintilla thisEditor = (Scintilla)s;

                // Autofit left-margin to fit-in line count number
                int maxLineNumberCharLength = thisEditor.Lines.Count.ToString().Length;
                if (maxLineNumberCharLength != _maxLineNumberCharLength)
                {
                    thisEditor.Margins[0].Width = thisEditor.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1));
                    _maxLineNumberCharLength = maxLineNumberCharLength;
                }

                _tokens.Clear();

                IsChanged = true;
                TabUpdating?.Invoke(this, new EventArgs());

                _textChangedTimer.Stop();
                _textChangedTimer.Start();
            };

            editor.StyleNeeded += (s, e) =>
            {
                Scintilla thisEditor = (Scintilla)s;
                EditorState editorState = (EditorState)thisEditor.Parent.Tag;
                int startPos = thisEditor.GetEndStyled();
                int endPos = Math.Min(e.Position, thisEditor.TextLength - 1);
                int length = (endPos - startPos) + 1;
                if (!_parseWorker.IsBusy)
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    int styleCount = _styler.Highlight(thisEditor, startPos, endPos);
                    timer.Stop();
                    Debug.WriteLine($"Styled {styleCount} parts ({startPos} to {endPos}) took: {timer.Elapsed.TotalMilliseconds} ms");
                }
            };

            editor.KeyDown += (s, e) =>
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

            editor.InsertCheck += (s, e) =>
            {
                if ((e.Text.EndsWith("\n")))
                {
                    var curLine = _editor.LineFromPosition(e.Position);
                    var curLineText = _editor.Lines[curLine].Text;
                    StringBuilder addon = new StringBuilder();
                    for (int i = 0; i < curLineText.Length; ++i)
                    {
                        char c = curLineText[i];
                        if ((c != '\n') && char.IsWhiteSpace(c))
                            addon.Append(c);
                        else
                            break;
                    }
                    e.Text += addon.ToString();
                }
            };
        }
    }
}
