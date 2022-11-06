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
using System.Collections.Generic;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Html;
using TSP.DoxygenEditor.Lexers;

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

        #region Styler
        static int styleIndex = 100;

        static readonly int cppMultiLineCommentStyle = styleIndex++;
        static readonly int cppMultiLineCommentDocStyle = styleIndex++;
        static readonly int cppMultiLineCommentDocTextStyle = styleIndex++;
        static readonly int cppSingleLineCommentStyle = styleIndex++;
        static readonly int cppSingleLineCommentDocStyle = styleIndex++;
        static readonly int cppSingleLineCommentDocTextStyle = styleIndex++;

        static readonly int cppPreprocessorBasicStyle = styleIndex++;
        static readonly int cppPreprocessorKeywordStyle = styleIndex++;
        static readonly int cppPreprocessorDefineStyle = styleIndex++;
        static readonly int cppPreprocessorDefineArgumentStyle = styleIndex++;
        static readonly int cppPreprocessorIncludeStyle = styleIndex++;

        static readonly int cppReservedKeywordStyle = styleIndex++;
        static readonly int cppGlobalTypeKeywordStyle = styleIndex++;
        static readonly int cppUserTypeIdentStyle = styleIndex++;
        static readonly int cppMemberIdentStyle = styleIndex++;
        static readonly int cppFunctionIdentStyle = styleIndex++;

        static readonly int cppCharLiteralStyle = styleIndex++;
        static readonly int cppStringLiteralStyle = styleIndex++;
        static readonly int cppNumberLiteralStyle = styleIndex++;

        static readonly Dictionary<CppTokenKind, int> cppTokenTypeToStyleDict = new Dictionary<CppTokenKind, int>() {
            { CppTokenKind.MultiLineComment, cppMultiLineCommentStyle },
            { CppTokenKind.MultiLineCommentDoc, cppMultiLineCommentDocTextStyle },
            { CppTokenKind.SingleLineComment, cppSingleLineCommentStyle },
            { CppTokenKind.SingleLineCommentDoc, cppSingleLineCommentDocTextStyle },

            { CppTokenKind.PreprocessorStart, cppPreprocessorBasicStyle },
            { CppTokenKind.PreprocessorOperator, cppPreprocessorBasicStyle },
            { CppTokenKind.PreprocessorKeyword, cppPreprocessorKeywordStyle },
            { CppTokenKind.PreprocessorDefineSource, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorFunctionSource, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineUsage, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineMatch, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineArgument, cppPreprocessorDefineArgumentStyle },
            { CppTokenKind.PreprocessorInclude, cppPreprocessorIncludeStyle },

            { CppTokenKind.ReservedKeyword, cppReservedKeywordStyle },
            { CppTokenKind.GlobalTypeKeyword, cppGlobalTypeKeywordStyle },
            { CppTokenKind.FunctionIdent, cppFunctionIdentStyle },
            { CppTokenKind.UserTypeIdent, cppUserTypeIdentStyle },
            { CppTokenKind.MemberIdent, cppMemberIdentStyle },

            { CppTokenKind.StringLiteral, cppStringLiteralStyle },
            { CppTokenKind.CharLiteral, cppCharLiteralStyle },

            { CppTokenKind.IntegerLiteral, cppNumberLiteralStyle },
            { CppTokenKind.OctalLiteral, cppNumberLiteralStyle },
            { CppTokenKind.HexLiteral, cppNumberLiteralStyle },
            { CppTokenKind.IntegerFloatLiteral, cppNumberLiteralStyle },
            { CppTokenKind.HexadecimalFloatLiteral, cppNumberLiteralStyle },
        };

        static int doxygenBlockStyle = styleIndex++;
        static int doxygenCommandStyle = styleIndex++;
        static int doxygenInvalidCommandStyle = styleIndex++;
        static int doxygenIdentStyle = styleIndex++;
        static int doxygenQuoteStringStyle = styleIndex++;
        static int doxygenArgumentStyle = styleIndex++;

        static int doxygenConfigCommentStyle = styleIndex++;
        static int doxygenConfigKeyStyle = styleIndex++;
        static int doxygenConfigOpStyle = styleIndex++;
        static int doxygenConfigValueStyle = styleIndex++;

        static Dictionary<DoxygenTokenKind, int> doxygenTokenTypeToStyleDict = new Dictionary<DoxygenTokenKind, int>() {
            { DoxygenTokenKind.DoxyBlockStartSingle, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockStartMulti, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockEnd, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockChars, doxygenBlockStyle },
            { DoxygenTokenKind.Command, doxygenCommandStyle },
            { DoxygenTokenKind.InvalidCommand, doxygenInvalidCommandStyle },
            { DoxygenTokenKind.GroupStart, doxygenCommandStyle },
            { DoxygenTokenKind.GroupEnd, doxygenCommandStyle },
            { DoxygenTokenKind.ArgumentIdent, doxygenIdentStyle },
            { DoxygenTokenKind.ArgumentText, doxygenQuoteStringStyle },
            { DoxygenTokenKind.ArgumentCaption, doxygenArgumentStyle },
            { DoxygenTokenKind.ArgumentFile, doxygenArgumentStyle },
            { DoxygenTokenKind.CommandStart, doxygenCommandStyle },
            { DoxygenTokenKind.CommandEnd, doxygenCommandStyle },
            { DoxygenTokenKind.Code, Style.Default },

            { DoxygenTokenKind.ConfigComment, doxygenConfigCommentStyle },
            { DoxygenTokenKind.ConfigKey, doxygenConfigKeyStyle },
            { DoxygenTokenKind.ConfigOpAddAssign, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigOpAssign, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigOpAddLine, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigValue, doxygenConfigValueStyle },
        };

        static int htmlTagCharsStyle = styleIndex++;
        static int htmlTagNameStyle = styleIndex++;
        static int htmlAttrNameStyle = styleIndex++;
        static int htmlAttrValueStyle = styleIndex++;

        static Dictionary<HtmlTokenKind, int> htmlTokenTypeToStyleDict = new Dictionary<HtmlTokenKind, int>() {
            { HtmlTokenKind.TagChars, htmlTagCharsStyle },
            { HtmlTokenKind.TagName, htmlTagNameStyle },
            { HtmlTokenKind.AttrName, htmlAttrNameStyle },
            { HtmlTokenKind.AttrValue, htmlAttrValueStyle },
        };

        private readonly static HashSet<int> allowedMatchStyles = new HashSet<int> {
            cppPreprocessorDefineStyle,
            cppUserTypeIdentStyle,
            cppMemberIdentStyle,
            cppFunctionIdentStyle,
            doxygenIdentStyle,
            doxygenArgumentStyle,
        };

        public struct StyleEntry
        {
            public LanguageKind Lang { get; }
            public int Index { get; }
            public int Length { get; }
            public int Style { get; }
            public string Value { get; }

            public int End
            {
                get
                {
                    int result = Index + Math.Max(0, Length - 1);
                    return (result);
                }
            }

            public StyleEntry(LanguageKind lang, int index, int length, int style, string value)
            {
                Lang = lang;
                Index = index;
                Length = length;
                Style = style;
                Value = value;
            }

            public StyleEntry(LanguageKind lang, IBaseToken token, int style, string value) : this(lang, token.Index, token.Length, style, value)
            {
            }

            public bool InterectsWith(StyleEntry other)
            {
                bool result = (Index <= other.End) && (End >= other.Index);
                return (result);
            }

            public override string ToString()
            {
                return $"{Index} => {Length} as {Lang} with style {Style} = {Value}";
            }
        }

        private readonly List<StyleEntry> _entries = new List<StyleEntry>();
        #endregion

        public ScintillaEditor(IWin32Window window, WorkspaceModel workspace, string name, TabPage tab, int tabIndex)
        {
            // Mainform
            FilePath = null;
            Name = name;
            Tab = tab;
            IsChanged = false;
            FileEncoding = Encoding.UTF8;
            _window = window;

            // Parsing
            _parseState = new ParseContext(this, workspace);
            _parseState.ParseCompleted += (s) =>
            {
                ParseCompleted?.Invoke(ParseInfo);
            };
            _parseState.ParseStarting += (s) =>
            {
                ParseStarting?.Invoke(ParseInfo);
            };

            // Editor
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
            StyleEntry style = FindStyleFromPosition(position);
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

            ApplyStyles();

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
                        Highlight(startPos, endPos);
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

        #region Styler impl
        public StyleEntry FindStyleFromPosition(int position)
        {
            StyleEntry result = _entries.FirstOrDefault((e) => allowedMatchStyles.Contains(e.Style) && position >= e.Index && position <= e.End);
            return (result);
        }

        public int StyleTokens(IEnumerable<IBaseToken> tokens)
        {
            _entries.Clear();
            foreach (IBaseToken token in tokens)
            {
                if (token.Length == 0) continue;
                if (typeof(CppToken).Equals(token.GetType()))
                {
                    CppToken cppToken = (CppToken)token;
                    int style;
                    if (cppTokenTypeToStyleDict.TryGetValue(cppToken.Kind, out style))
                        _entries.Add(new StyleEntry(LanguageKind.Cpp, token, style, token.Value));
                }
                else if (typeof(DoxygenToken).Equals(token.GetType()))
                {
                    DoxygenToken doxygenToken = (DoxygenToken)token;
                    int style;
                    if (doxygenTokenTypeToStyleDict.TryGetValue(doxygenToken.Kind, out style))
                    {
                        LanguageKind styleKind = LanguageKind.Doxygen;
                        if (doxygenToken.Kind == DoxygenTokenKind.Code)
                            styleKind = LanguageKind.DoxygenCode;
                        _entries.Add(new StyleEntry(styleKind, token, style, doxygenToken.Value));
                    }
                }
                else if (typeof(HtmlToken).Equals(token.GetType()))
                {
                    HtmlToken htmlToken = (HtmlToken)token;
                    int style;
                    if (htmlTokenTypeToStyleDict.TryGetValue(htmlToken.Kind, out style))
                        _entries.Add(new StyleEntry(LanguageKind.Html, token, style, htmlToken.Value));
                }
            }
            return _entries.Count;
        }

        private void ApplyCppStyle(ColorTheme theme)
        {
            CppColorTheme cppTheme = theme.Cpp;

            _editor.Styles[cppMultiLineCommentStyle].Set(cppTheme[CppStyleKind.MultiLineComment]);
            _editor.Styles[cppMultiLineCommentDocStyle].Set(cppTheme[CppStyleKind.MultiLineCommentDoc]);
            _editor.Styles[cppMultiLineCommentDocTextStyle].Set(cppTheme[CppStyleKind.MultiLineCommentDocText]);
            _editor.Styles[cppSingleLineCommentStyle].Set(cppTheme[CppStyleKind.SingleLineComment]);
            _editor.Styles[cppSingleLineCommentDocStyle].Set(cppTheme[CppStyleKind.SingleLineCommentDoc]);
            _editor.Styles[cppSingleLineCommentDocTextStyle].Set(cppTheme[CppStyleKind.SingleLineCommentDocText]);

            _editor.Styles[cppPreprocessorBasicStyle].Set(cppTheme[CppStyleKind.PreprocessorBasic]);
            _editor.Styles[cppPreprocessorKeywordStyle].Set(cppTheme[CppStyleKind.PreprocessorKeyword]);
            _editor.Styles[cppPreprocessorDefineStyle].Set(cppTheme[CppStyleKind.PreprocessorDefine]);
            _editor.Styles[cppPreprocessorDefineArgumentStyle].Set(cppTheme[CppStyleKind.PreprocessorDefineArgument]);
            _editor.Styles[cppPreprocessorIncludeStyle].Set(cppTheme[CppStyleKind.PreprocessorInclude]);

            _editor.Styles[cppReservedKeywordStyle].Set(cppTheme[CppStyleKind.ReservedKeyword]);
            _editor.Styles[cppGlobalTypeKeywordStyle].Set(cppTheme[CppStyleKind.GlobalTypeKeyword]);
            _editor.Styles[cppUserTypeIdentStyle].Set(cppTheme[CppStyleKind.UserTypeKeyword]);
            _editor.Styles[cppMemberIdentStyle].Set(cppTheme[CppStyleKind.MemberKeyword]);
            _editor.Styles[cppFunctionIdentStyle].Set(cppTheme[CppStyleKind.FunctionKeyword]);

            _editor.Styles[cppStringLiteralStyle].Set(cppTheme[CppStyleKind.StringLiteral]);
            _editor.Styles[cppCharLiteralStyle].Set(cppTheme[CppStyleKind.CharLiteral]);
            _editor.Styles[cppNumberLiteralStyle].Set(cppTheme[CppStyleKind.NumberLiteral]);
        }

        private void ApplyDoxygenStyle()
        {
            // Block styles
            _editor.Styles[doxygenBlockStyle].ForeColor = Color.DarkViolet;
            _editor.Styles[doxygenCommandStyle].ForeColor = Color.Red;
            _editor.Styles[doxygenInvalidCommandStyle].ForeColor = Color.Red;
            _editor.Styles[doxygenInvalidCommandStyle].Underline = true;
            _editor.Styles[doxygenIdentStyle].ForeColor = Color.Blue;
            _editor.Styles[doxygenQuoteStringStyle].ForeColor = Color.Green;
            _editor.Styles[doxygenArgumentStyle].ForeColor = Color.Red;

            // Config styles
            _editor.Styles[doxygenConfigCommentStyle].ForeColor = Color.Gray;
            _editor.Styles[doxygenConfigKeyStyle].ForeColor = Color.Blue;
            _editor.Styles[doxygenConfigKeyStyle].Bold = false;
            _editor.Styles[doxygenConfigValueStyle].ForeColor = Color.Green;
            _editor.Styles[doxygenConfigOpStyle].ForeColor = Color.Black;
            _editor.Styles[doxygenConfigOpStyle].Bold = true;
        }

        private void ApplyHtmlStyle()
        {
            _editor.Styles[htmlTagCharsStyle].ForeColor = Color.DarkRed;
            _editor.Styles[htmlTagNameStyle].ForeColor = Color.DarkRed;
            _editor.Styles[htmlAttrNameStyle].ForeColor = Color.OrangeRed;
            _editor.Styles[htmlAttrValueStyle].ForeColor = Color.CornflowerBlue;
        }

        public void ApplyStyles()
        {
            ColorTheme theme = ColorThemeManager.Current;

            ApplyCppStyle(theme);
            ApplyDoxygenStyle();
            ApplyHtmlStyle();

            _editor.Indicators[0].Style = IndicatorStyle.FullBox;
            _editor.Indicators[0].ForeColor = Color.Red;
        }

        public void Highlight(int startPos, int endPos)
        {
            Debug.Assert(startPos < endPos);

            int length = (endPos - startPos) + 1;

            _editor.StartStyling(startPos);
            _editor.SetStyling(length, 0);

            StyleEntry rangeEntry = new StyleEntry(LanguageKind.None, startPos, length, 0, null);
            IEnumerable<StyleEntry> intersectingEntries = _entries.Where(r => r.InterectsWith(rangeEntry));

            foreach (StyleEntry entry in intersectingEntries)
            {
                int s = Math.Max(startPos, entry.Index);
                int e = Math.Min(entry.Index + (entry.Length - 1), endPos);
                int l = (e - s) + 1;
                _editor.StartStyling(s);
                _editor.SetStyling(l, entry.Style);
            }
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
