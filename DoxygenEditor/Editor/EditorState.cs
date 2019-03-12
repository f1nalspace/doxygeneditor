using TSP.DoxygenEditor.SearchReplace;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using TSP.DoxygenEditor.Extensions;
using System.Linq;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Html;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.Models;

namespace TSP.DoxygenEditor.Editor
{
    class EditorState : IDisposable
    {
        private readonly IWin32Window _window;
        private Panel _containerPanel;
        private SearchReplaceControl _searchControl;
        private readonly Scintilla _editor;
        private int _maxLineNumberCharLength;
        private System.Windows.Forms.Timer _textChangedTimer;
        private BackgroundWorker _parseWorker;
        private IBaseNode _doxyTree;
        private IBaseNode _cppTree;
        private readonly List<IBaseToken> _tokens = new List<IBaseToken>();
        private readonly List<TextError> _errors = new List<TextError>();
        private readonly List<PerformanceItemModel> _performanceItems = new List<PerformanceItemModel>();
        private readonly EditorStyler _styler = new EditorStyler();
        public IEnumerable<TextError> Errors => _errors;
        public IEnumerable<PerformanceItemModel> PerformanceItems => _performanceItems;
        public IBaseNode DoxyTree { get { return _doxyTree; } }
        public IBaseNode CppTree { get { return _cppTree; } }
        public object Tag { get; }
        public int TabIndex { get; }
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool IsChanged { get; set; }
        public Encoding FileEncoding { get; set; }
        public Panel Container { get { return _containerPanel; } }
        struct StyleNeededState
        {
            public int Has { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
        }
        private StyleNeededState _styleNeededState;
        public bool IsShowWhitespace
        {
            get { return _editor.ViewWhitespace != WhitespaceMode.Invisible; }
            set { _editor.ViewWhitespace = value ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible; }
        }

        public delegate void ParseEventHandler(EditorState sender);
        public delegate void FocusChangedEventHandler(EditorState sender, bool focused);
        public event EventHandler TabUpdating;
        public event ParseEventHandler ParseComplete;
        public event ParseEventHandler ParseStarting;
        public event FocusChangedEventHandler FocusChanged;

        public EditorState(IWin32Window window, string name, object tag, int tabIndex)
        {
            FilePath = null;
            Name = name;
            Tag = tag;
            IsChanged = false;
            FileEncoding = Encoding.UTF8;

            _styleNeededState = new StyleNeededState();

            _window = window;
            TabIndex = tabIndex;

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
            _searchControl.FocusChanged += (s, focused) =>
            {
                FocusChanged?.Invoke(this, focused);
            };
            _searchControl.Hide();

            _maxLineNumberCharLength = 0;
            _textChangedTimer = new System.Windows.Forms.Timer() { Enabled = false, Interval = 250 };
            _textChangedTimer.Tick += (s, e) =>
            {
                if (!_parseWorker.IsBusy)
                {
                    _textChangedTimer.Enabled = false;
                    ParseStarting?.Invoke(this);
                    _parseWorker.RunWorkerAsync(_editor.Text);
                }
            };
            _parseWorker = new BackgroundWorker();
            _parseWorker.WorkerSupportsCancellation = true;
            _parseWorker.DoWork += (s, e) =>
            {
                // @TODO(final): Support for incremental parsing, so only changes are applied
                string text = (string)e.Argument;
                Tokenize(text);
                Parse(text);
            };
            _parseWorker.RunWorkerCompleted += (s, e) =>
            {
                // @TODO(final): Dont colorize everything, just re-colorize the changes -> See "Support for continuous tokenization"
                if (_styleNeededState.Has > 0)
                    _editor.Colorize(_styleNeededState.StartPos, _styleNeededState.EndPos);
                else
                {
                    int firstLine = _editor.FirstVisibleLine;
                    int lastLine = firstLine + _editor.LinesOnScreen;
                    int start = _editor.Lines[firstLine].Index;
                    int end = _editor.Lines[lastLine].Index;
                    _editor.Colorize(start, end);
                }
                ParseComplete?.Invoke(this);
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

        class TokenizerTimingStats
        {
            public TimeSpan CppDuration = new TimeSpan();
            public TimeSpan DoxyDuration = new TimeSpan();
            public TimeSpan HtmlDuration = new TimeSpan();
            public TimeSpan InsertDuration = new TimeSpan();

            public static TokenizerTimingStats operator +(TokenizerTimingStats a, TokenizerTimingStats b)
            {
                TokenizerTimingStats result = new TokenizerTimingStats();
                result.CppDuration = a.CppDuration + b.CppDuration;
                result.DoxyDuration = a.DoxyDuration + b.DoxyDuration;
                result.HtmlDuration = a.HtmlDuration + b.HtmlDuration;
                result.InsertDuration = a.InsertDuration + b.InsertDuration;
                return (result);
            }
        }

        class TokenizeResult : IDisposable
        {
            private readonly List<IBaseToken> _tokens = new List<IBaseToken>();
            private readonly List<TextError> _errors = new List<TextError>();
            public IEnumerable<IBaseToken> Tokens => _tokens;
            public IEnumerable<TextError> Errors => _errors;
            public TokenizerTimingStats Stats { get; }
            public TokenizeResult()
            {
                Stats = new TokenizerTimingStats();
            }
            public void AddErrors(IEnumerable<TextError> errors)
            {
                _errors.AddRange(errors);
            }
            public void AddTokens(IEnumerable<IBaseToken> inTokens)
            {
                Stopwatch w = Stopwatch.StartNew();
                _tokens.AddRange(inTokens);
                w.Stop();
                Stats.InsertDuration += w.Elapsed;
            }
            public void AddToken(IBaseToken token)
            {
                Stopwatch w = Stopwatch.StartNew();
                _tokens.Add(token);
                w.Stop();
                Stats.InsertDuration += w.Elapsed;
            }

            public void Dispose()
            {
                _tokens.Clear();
                _errors.Clear();
            }
        }

        private TokenizeResult TokenizeCpp(string text, TextPosition pos, int length, bool allowDoxy)
        {
            TokenizeResult result = new TokenizeResult();
            Stopwatch timer = new Stopwatch();
            timer.Restart();
            List<CppToken> cppTokens = new List<CppToken>();
            using (CppLexer cppLexer = new CppLexer(text, pos, length))
            {
                cppTokens.AddRange(cppLexer.Tokenize());
                result.AddErrors(cppLexer.LexErrors);
            }
            timer.Stop();
            result.Stats.CppDuration += timer.Elapsed;
            foreach (CppToken token in cppTokens)
            {
                if (allowDoxy && (token.Kind == CppTokenKind.MultiLineCommentDoc || token.Kind == CppTokenKind.SingleLineCommentDoc))
                {
                    result.AddToken(token);
                    using (TokenizeResult doxyRes = TokenizeDoxy(text, token.Position, token.Length))
                    {
                        result.Stats.DoxyDuration += doxyRes.Stats.DoxyDuration;
                        result.Stats.HtmlDuration += doxyRes.Stats.HtmlDuration;
                        result.AddTokens(doxyRes.Tokens);
                        result.AddErrors(doxyRes.Errors);
                    }
                }
                else result.AddToken(token);
            }
            return (result);
        }

        private TokenizeResult TokenizeHtml(string text, TextPosition pos, int length)
        {
            TokenizeResult result = new TokenizeResult();
            Stopwatch timer = Stopwatch.StartNew();
            using (HtmlLexer htmlLexer = new HtmlLexer(text, pos, length))
            {
                IEnumerable<HtmlToken> htmlTokens = htmlLexer.Tokenize();
                if (htmlTokens.FirstOrDefault(d => !d.IsEOF) != null)
                    result.AddTokens(htmlTokens);
                result.AddErrors(htmlLexer.LexErrors);
            }
            timer.Stop();
            result.Stats.HtmlDuration += timer.Elapsed;
            return (result);
        }

        class CommandStartState
        {
            public TextPosition StartPosition { get; set; }
            public string CommandName { get; }
            public List<DoxygenToken> ArgTokens { get; }
            public DoxygenToken CommandToken { get; }

            public CommandStartState(DoxygenToken commandToken, string commandName)
            {
                CommandToken = commandToken;
                CommandName = commandName;
                ArgTokens = new List<DoxygenToken>();
                StartPosition = commandToken.Position;
            }
        }

        private TokenizeResult TokenizeDoxy(string text, TextPosition pos, int length)
        {
            TokenizeResult result = new TokenizeResult();

            Stopwatch timer = new Stopwatch();
            timer.Restart();
            List<DoxygenToken> doxyTokens = new List<DoxygenToken>();
            using (DoxygenLexer doxyLexer = new DoxygenLexer(text, pos, length))
            {
                doxyTokens.AddRange(doxyLexer.Tokenize());
                result.AddErrors(doxyLexer.LexErrors);
            }
            timer.Stop();
            result.Stats.DoxyDuration += timer.Elapsed;

            Stack<CommandStartState> startStates = new Stack<CommandStartState>();
            Stack<DoxygenToken> textStartTokens = new Stack<DoxygenToken>();
            var doxyTokenList = new LinkedList<DoxygenToken>(doxyTokens);
            var curLink = doxyTokenList.First;
            while (curLink != null)
            {
                var doxyToken = curLink.Value;
                if (doxyToken.Kind == DoxygenTokenKind.CommandStart)
                {
                    result.AddToken(doxyToken);
                    string commandName = text.Substring(doxyToken.Index + 1, doxyToken.Length - 1);
                    List<DoxygenToken> argTokens = new List<DoxygenToken>();
                    if (curLink.Next != null)
                    {
                        LinkedListNode<DoxygenToken> nextLink = curLink.Next;
                        while (nextLink != null)
                        {
                            if (nextLink.Value.IsArgument)
                            {
                                argTokens.Add(nextLink.Value);
                            }
                            else
                                break;
                            nextLink = nextLink.Next;
                        }
                        curLink = nextLink;
                    }
                    else
                        curLink = null;

                    result.AddTokens(argTokens);

                    CommandStartState startState = new CommandStartState(doxyToken, commandName);
                    startState.ArgTokens.AddRange(argTokens);
                    startStates.Push(startState);

                    if (argTokens.Count > 0)
                    {
                        var last = argTokens.Last();
                        startState.StartPosition = new TextPosition(last.End, last.Position.Line, last.Position.Column);
                    }
                    else
                        startState.StartPosition = new TextPosition(doxyToken.End, doxyToken.Position.Line, doxyToken.Position.Column);

                    continue;
                }

                if (doxyToken.Kind == DoxygenTokenKind.CommandEnd)
                {
                    string commandName = text.Substring(doxyToken.Index + 1, doxyToken.Length - 1);
                    CommandStartState topStartState = startStates.Count > 0 ? startStates.Peek() : null;
                    if (topStartState != null)
                    {
                        var rule = DoxygenSyntax.GetCommandRule(commandName);
                        Debug.Assert(rule != null && rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock);
                        DoxygenSyntax.EndBlockCommandRule endRule = rule as DoxygenSyntax.EndBlockCommandRule;
                        Debug.Assert(endRule != null);
                        if (endRule.StartCommandNames.Contains(topStartState.CommandName))
                        {
                            TextPosition commandContentStart = topStartState.StartPosition;
                            TextPosition commandContentEnd = doxyToken.Position;
                            Debug.Assert(commandContentEnd.Index >= commandContentStart.Index);
                            int commandContentLength = commandContentEnd.Index - commandContentStart.Index;

                            // Special handling for code block
                            if ("code".Equals(topStartState.CommandName))
                            {
                                string codeType = null;
                                DoxygenToken firstArgToken = topStartState.ArgTokens.FirstOrDefault();
                                if (firstArgToken != null && firstArgToken.Kind == DoxygenTokenKind.ArgumentCaption)
                                    codeType = text.Substring(firstArgToken.Index, firstArgToken.Length);
                                if ("{.c}".Equals(codeType, StringComparison.InvariantCultureIgnoreCase) || "{.cpp}".Equals(codeType, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    using (TokenizeResult cppRes = TokenizeCpp(text, commandContentStart, commandContentLength, false))
                                    {
                                        result.AddTokens(cppRes.Tokens);
                                        result.AddErrors(cppRes.Errors);
                                    }
                                }
                            }
                            startStates.Pop();
                        }
                    }
                    else
                    {
                        // @TODO(final): Print error (Command end without command start)
                    }
                    result.AddToken(doxyToken);
                }
                else if (doxyToken.Kind == DoxygenTokenKind.TextStart)
                {
                    textStartTokens.Push(doxyToken);
                    result.AddToken(doxyToken);
                }
                else if (doxyToken.Kind == DoxygenTokenKind.TextEnd)
                {
                    if (textStartTokens.Count > 0)
                    {
                        DoxygenToken textStartToken = textStartTokens.Pop();
                        Debug.Assert(doxyToken.Index >= textStartToken.Index);
                        int textContentLen = doxyToken.Index - textStartToken.Index;
                        using (TokenizeResult htmlRes = TokenizeHtml(text, textStartToken.Position, textContentLen))
                        {
                            result.AddTokens(htmlRes.Tokens);
                            result.AddErrors(htmlRes.Errors);
                            result.Stats.HtmlDuration += htmlRes.Stats.HtmlDuration;
                        }
                    }
                    result.AddToken(doxyToken);
                }
                else
                    result.AddToken(doxyToken);
                curLink = curLink.Next;
            }
            return (result);
        }

        private void GiveTokensBackToPool()
        {
            CppTokenPool.Release(_tokens.Where(t => typeof(CppToken).Equals(t.GetType())).Select(t => (CppToken)t));
            DoxygenTokenPool.Release(_tokens.Where(t => typeof(DoxygenToken).Equals(t.GetType())).Select(t => (DoxygenToken)t));
            HtmlTokenPool.Release(_tokens.Where(t => typeof(HtmlToken).Equals(t.GetType())).Select(t => (HtmlToken)t));
        }

        private void Tokenize(string text)
        {
            Stopwatch timer = new Stopwatch();

            // Push back all tokens to to pools
            GiveTokensBackToPool();

            // Clear tokens & errors
            _tokens.Clear();
            _errors.Clear();
            _performanceItems.Clear();
            SymbolCache.Clear(this);

            Stopwatch totalLexTimer = Stopwatch.StartNew();

            // C++ lexing -> Doxygen (Code -> Cpp) -> (Text -> Html)
            TokenizerTimingStats totalStats = new TokenizerTimingStats();
            using (TokenizeResult cppRes = TokenizeCpp(text, new TextPosition(0), text.Length, true))
            {
                totalStats += cppRes.Stats;
                _tokens.AddRange(cppRes.Tokens);
                _errors.AddRange(cppRes.Errors);
            }
            totalLexTimer.Stop();
            Debug.WriteLine($"Lexing done (Tokens: {_tokens.Count}, Total: {totalLexTimer.Elapsed.ToMilliseconds()} ms, Insert: {totalStats.InsertDuration.ToMilliseconds()}, C++: {totalStats.CppDuration.ToMilliseconds()} ms, Doxygen: {totalStats.DoxyDuration.ToMilliseconds()} ms, Html: {totalStats.HtmlDuration.ToMilliseconds()} ms)");

            int countCppTokens = _tokens.Count(t => typeof(CppToken).Equals(t.GetType()));
            int countHtmlTokens = _tokens.Count(t => typeof(HtmlToken).Equals(t.GetType()));
            int countDoxyTokens = _tokens.Count(t => typeof(DoxygenToken).Equals(t.GetType()));
            _performanceItems.Add(new PerformanceItemModel(this, TabIndex, $"{text.Length} chars", $"{countCppTokens} tokens", "C++ lexer", totalStats.CppDuration));
            _performanceItems.Add(new PerformanceItemModel(this, TabIndex, $"{text.Length} chars", $"{countDoxyTokens} tokens", "Doxygen lexer", totalStats.DoxyDuration));
            _performanceItems.Add(new PerformanceItemModel(this, TabIndex, $"{text.Length} chars", $"{countHtmlTokens} tokens", "Html lexer", totalStats.HtmlDuration));

            timer.Restart();
            _styler.Refresh(_tokens);
            timer.Stop();
            Debug.WriteLine($"Styler done, took {timer.Elapsed.ToMilliseconds()} ms");
        }

        private void Parse(string text)
        {
            // Clear stream from all invalid tokens
            _tokens.RemoveAll(d => d.IsEOF || (!d.IsMarker && d.Length == 0));

            // @FIXME(final): Right know, the tokens are not in a valid range
            // Reason is: No tokens gets replaced by another range.
            // Also there are zero-length tokens or start/end tokens
#if false
            // Validate stream
            {
                LinkedListStream<BaseToken> tokenStream = new LinkedListStream<BaseToken>(_tokens);
                while (!tokenStream.IsEOF)
                {
                    var tokenNode = tokenStream.CurrentNode;
                    if (tokenNode.Next != null)
                    {
                        int endIndex = tokenNode.Value.Index;
                        int startIndex = tokenNode.Next.Value.Index;
                        Debug.Assert(startIndex >= endIndex);
                    }
                    tokenStream.Next();
                }
            }
#endif

            Stopwatch timer = new Stopwatch();

            // Doxygen parsing
            int doxyNodeCount = 0;
            timer.Restart();
            using (DoxygenParser doxyParser = new DoxygenParser(this, text))
            {
                doxyParser.ParseTokens(_tokens);
                _errors.InsertRange(0, doxyParser.ParseErrors);
                _doxyTree = doxyParser.Root;
                doxyNodeCount = doxyParser.TotalNodeCount;
            }
            timer.Stop();
            Debug.WriteLine($"Doxygen parse done, took {timer.Elapsed.ToMilliseconds()} ms");
            _performanceItems.Add(new PerformanceItemModel(this, TabIndex, $"{_tokens.Count} tokens", $"{doxyNodeCount} nodes", "Doxygen parser", timer.Elapsed));

            // C++ parsing
            timer.Restart();
            int cppNodeCount = 0;
            using (CppParser cppParser = new CppParser(this))
            {
                cppParser.GetDocumentationNode += (token) =>
                {
                    IBaseNode result = _doxyTree.FindNodeByRange(token.Range);
                    return (result);
                };
                cppParser.ParseTokens(_tokens);
                _errors.InsertRange(0, cppParser.ParseErrors);
                _cppTree = cppParser.Root;
                cppNodeCount = cppParser.TotalNodeCount;
            }
            timer.Stop();
            Debug.WriteLine($"C++ parse done, took {timer.Elapsed.ToMilliseconds()} ms");
            _performanceItems.Add(new PerformanceItemModel(this, TabIndex, $"{_tokens.Count} tokens", $"{cppNodeCount} nodes", "C++ parser", timer.Elapsed));
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
                if (!_parseWorker.IsBusy)
                {
                    _styler.Highlight(thisEditor, startPos, endPos);
                    _styleNeededState.Has = 0;
                }
                else
                {
                    _styleNeededState.StartPos = startPos;
                    _styleNeededState.EndPos = endPos;
                    _styleNeededState.Has++;
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

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls
        public void Dispose()
        {
            if (!_disposed)
            {
                GiveTokensBackToPool();
                _disposed = true;
            }
        }
        #endregion
    }
}
