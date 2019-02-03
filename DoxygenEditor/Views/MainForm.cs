using DoxygenEditor.Extensions;
using DoxygenEditor.Lexers;
using DoxygenEditor.Lexers.Cpp;
using DoxygenEditor.Models;
using DoxygenEditor.Natives;
using DoxygenEditor.Parsers;
using DoxygenEditor.Parsers.Entities;
using DoxygenEditor.SearchReplace;
using DoxygenEditor.Services;
using DoxygenEditor.Solid;
using DoxygenEditor.SymbolSearch;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DoxygenEditor.Views
{
    public partial class MainForm : Form
    {
        private readonly IConfigurationService _configService;
        private readonly ConfigurationModel _config;
        private readonly string _appName;

        private static ErrorMessageModel _errorFileOpenMessage = new ErrorMessageModel($"Cannot open file '%FILEPATH%'!", $"File to open file '%FILENAME%'");
        private static ErrorMessageModel _errorFileSaveMessage = new ErrorMessageModel($"Cannot save file '%FILEPATH%'!", $"File to save file '%FILENAME%'");

        public MainForm()
        {
            InitializeComponent();

            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            _appName = verInfo.ProductName;

            _configService = IOCContainer.Get<IConfigurationService>();
            _config = new ConfigurationModel();
            _config.Load(_configService);

            _searchControl = new SearchReplace.SearchReplaceControl();
            Controls.Add(_searchControl);

            TextSelectedTimer = new System.Windows.Forms.Timer() { Enabled = true, Interval = 500 };
            TextSelectedTimer.Tick += (s, e) =>
            {
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
                    UpdateMenuSelection(editorState);
                }
                else UpdateMenuSelection(null);
            };
            NativeMethods.AddClipboardFormatListener(Handle);
        }

        private void ShowError(Exception exception, string text, string caption, string filePath)
        {
            string filename = !string.IsNullOrWhiteSpace(filePath) ? Path.GetFileName(filePath) : null;
            StringBuilder message = new StringBuilder();
            message.AppendLine($"[{_appName}] {text}!");
            if (exception != null)
            {
                message.AppendLine();
                message.Append(exception.ToHumanReadable(filePath));
            }
            MessageBox.Show(this, message.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void ShowError(Exception exception, ErrorMessageModel messageTemplate, string filePath)
        {
            Tuple<string, string> r = messageTemplate.ToFileError(filePath);
            ShowError(exception, r.Item1, r.Item2, filePath);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                EditorState editorState = null;
                if (tcFiles.TabPages.Count > 0)
                {
                    Debug.Assert(tcFiles.SelectedTab != null);
                    editorState = (EditorState)tcFiles.SelectedTab.Tag;
                }
                UpdateMenuEditChange(editorState);
            }
            base.WndProc(ref m);
        }


        #region Editor
        private System.Windows.Forms.Timer TextSelectedTimer { get; }
        private SearchReplaceControl _searchControl;

        private static int _parseCounter = 0;

        class EditorState
        {
            private readonly IWin32Window _window;
            private Panel _containerPanel;
            private SearchReplaceControl _searchControl;
            private readonly Scintilla _editor;
            private int _maxLineNumberCharLength;
            private readonly DoxygenParser _doxyParser;
            private System.Windows.Forms.Timer _textChangedTimer;
            private BackgroundWorker _parseWorker;
            private ParseTree _tree;
            private readonly List<Lexers.Cpp.CppToken> _cppTokens = new List<Lexers.Cpp.CppToken>();
            public ParseTree Tree { get { return _tree; } }

            public object Tag { get; set; }
            public string FilePath { get; set; }
            public string Name { get; set; }
            public bool IsChanged { get; set; }
            public Encoding FileEncoding { get; set; }
            public Panel Container { get { return _containerPanel; } }

            public delegate void ParseEventHandler(object sender, ParseTree tree);
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
                _doxyParser = new DoxygenParser();
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

                    Stopwatch timer = Stopwatch.StartNew();
                    ParseTree tree = _doxyParser.Parse(text);
                    timer.Stop();
                    Debug.WriteLine($"Doxygen parse took {timer.ElapsedMilliseconds} ms");
                    e.Result = tree;

                    timer.Start();
                    CppLexer lexer = new CppLexer(new StringSourceBuffer(text));
                    var newTokens = lexer.Tokenize();
                    timer.Stop();
                    Debug.WriteLine($"Cpp lexing took {timer.ElapsedMilliseconds} ms");
                    _cppTokens.AddRange(newTokens);
                };
                _parseWorker.RunWorkerCompleted += (s, e) =>
                {
                    if (_tree != null) _tree.Dispose();
                    _tree = (ParseTree)e.Result;
                    _editor.Colorize(0, Math.Max(0, _editor.TextLength - 1));
                    ParseComplete?.Invoke(this, _tree);
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

            public void SetShowWhitespaces(bool value)
            {
                if (value)
                    _editor.ViewWhitespace = WhitespaceMode.VisibleAlways;
                else
                    _editor.ViewWhitespace = WhitespaceMode.Invisible;
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
                editor.UseTabs = true;

                Font editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);
                editor.StyleResetDefault();
                editor.Styles[Style.Default].Font = editorFont.Name;
                editor.Styles[Style.Default].Size = (int)editorFont.SizeInPoints;
                editor.StyleClearAll();
                editor.Lexer = Lexer.Container;

                int styleIndex = 1;

                int cppMultiLineCommentStyle = styleIndex++;
                int cppMultiLineCommentDocStyle = styleIndex++;
                int cppSingleLineCommentStyle = styleIndex++;
                int cppSingleLineCommentDocStyle = styleIndex++;
                int cppPreprocessorStyle = styleIndex++;
                int cppReservedKeywordStyle = styleIndex++;
                int cppTypeKeywordStyle = styleIndex++;
                int cppStringStyle = styleIndex++;
                int cppNumberStyle = styleIndex++;

                editor.Styles[cppMultiLineCommentStyle].ForeColor = Color.Green;
                editor.Styles[cppMultiLineCommentDocStyle].ForeColor = Color.Purple;
                editor.Styles[cppSingleLineCommentStyle].ForeColor = Color.DarkGreen;
                editor.Styles[cppSingleLineCommentDocStyle].ForeColor = Color.DarkTurquoise;
                editor.Styles[cppPreprocessorStyle].ForeColor = Color.Red;
                editor.Styles[cppReservedKeywordStyle].ForeColor = Color.Blue;
                editor.Styles[cppTypeKeywordStyle].ForeColor = Color.Blue;
                editor.Styles[cppStringStyle].ForeColor = Color.Green;
                editor.Styles[cppNumberStyle].ForeColor = Color.Red;

                Dictionary<CppTokenType, int> cppTokenTypeToStyleDict = new Dictionary<CppTokenType, int>() {
                    { CppTokenType.MultiLineComment, cppMultiLineCommentStyle },
                    { CppTokenType.MultiLineCommentDoc, cppMultiLineCommentDocStyle },
                    { CppTokenType.SingleLineComment, cppSingleLineCommentStyle },
                    { CppTokenType.SingleLineCommentDoc, cppSingleLineCommentDocStyle },
                    { CppTokenType.Preprocessor, cppPreprocessorStyle },
                    { CppTokenType.ReservedKeyword, cppReservedKeywordStyle },
                    { CppTokenType.TypeKeyword, cppTypeKeywordStyle },
                    { CppTokenType.String, cppStringStyle },
                    { CppTokenType.Integer, cppNumberStyle },
                    { CppTokenType.Decimal, cppNumberStyle },
                    { CppTokenType.Hex, cppNumberStyle },
                    { CppTokenType.Octal, cppNumberStyle },
                };

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

                    _cppTokens.Clear();

                    IsChanged = true;
                    TabUpdating?.Invoke(this, new EventArgs());

                    _textChangedTimer.Stop();
                    _textChangedTimer.Start();
                };

                editor.StyleNeeded += (s, e) =>
                {
                    Scintilla thisEditor = (Scintilla)s;
                    EditorState editorState = (EditorState)thisEditor.Parent.Tag;
                    int startLine = thisEditor.LineFromPosition(thisEditor.GetEndStyled());
                    int endLine = thisEditor.LineFromPosition(e.Position);
                    int startPos = Math.Min(thisEditor.Lines[startLine].Position, thisEditor.TextLength - 1);
                    int endPos = Math.Min(thisEditor.Lines[endLine].Position + Math.Max(0, thisEditor.Lines[endLine].Length - 1), thisEditor.TextLength - 1);
                    int length = (endPos - startPos) + 1;
                    if (!_parseWorker.IsBusy && _cppTokens.Count > 0)
                    {
                        Stopwatch timer = Stopwatch.StartNew();

                        thisEditor.StartStyling(startPos);
                        thisEditor.SetStyling(length, 0);

                        var rangeToken = new Lexers.Cpp.CppToken(CppTokenType.Invalid, startPos, length, false);
                        var intersectingTokens = _cppTokens.Where(r => r.InterectsWith(rangeToken));
                        foreach (var token in intersectingTokens)
                        {
                            if (cppTokenTypeToStyleDict.ContainsKey(token.Type))
                            {
                                int style = cppTokenTypeToStyleDict[token.Type];
                                thisEditor.StartStyling(token.Index);
                                thisEditor.SetStyling(token.Length, style);
                            }
                        }
                        timer.Stop();
                        Debug.WriteLine($"Styling ({startPos} to {endPos}) took: {timer.Elapsed.TotalMilliseconds} ms");
                    }

#if false
                    // Use Cpp as default lexer
                    CppLexer.Lex(thisEditor, startPos, endPos);

                    // Use doxygen as second lexer
                    DoxygenLexer.Lex(thisEditor, startPos, endPos);

                    // Switch back to container
                    thisEditor.Lexer = Lexer.Container;
#endif


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
        #endregion

        #region Tabs
        private readonly Regex _rexIndexFromName = new Regex("(?<index>[0-9]+)$", RegexOptions.Compiled);
        private string GetNextTabName(string prefix)
        {
            int highIndex = 0;
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
                if (tabState.FilePath == null)
                {
                    string name = tabState.Name;
                    Match m = _rexIndexFromName.Match(name);
                    if (m.Success)
                    {
                        int testIndex = int.Parse(m.Groups["index"].Value);
                        if (testIndex > highIndex)
                            highIndex = testIndex;
                    }
                }
            }
            string result = $"{prefix}{highIndex + 1}";
            return (result);
        }

        private IEnumerable<EditorState> GetChangedEditorStates()
        {
            List<EditorState> result = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
                if (tabState.IsChanged)
                    result.Add(tabState);
            }
            return (result);
        }

        private IEnumerable<EditorState> GetAllEditorStates()
        {
            List<EditorState> result = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState tabState = (EditorState)tab.Tag;
                result.Add(tabState);
            }
            return (result);
        }

        private void UpdateTabState(EditorState editorState)
        {
            IEnumerable<EditorState> changedEditorStates = GetChangedEditorStates();
            bool anyChanges = changedEditorStates.Count() > 0;

            miFileSave.Enabled = editorState != null && editorState.IsChanged;
            miFileSaveAll.Enabled = anyChanges;
            miFileClose.Enabled = tcFiles.SelectedTab != null;
            miFileCloseAll.Enabled = tcFiles.TabCount > 0;

            if (editorState != null)
            {
                string title = editorState.Name;
                if (editorState.IsChanged) title += "*";
                TabPage tab = (TabPage)editorState.Tag;
                tab.Text = title;
            }

            UpdateMenuEditChange(editorState);
            UpdateMenuSelection(editorState);
        }

        private EditorState AddFileTab(string name)
        {
            TabPage newTab = new TabPage() { Text = name };
            EditorState newState = new EditorState(this) { Name = name, Tag = newTab };
            newState.TabUpdating += (s, e) => UpdateTabState((EditorState)s);
            newState.ParseComplete += (object s, ParseTree tree) =>
            {
                Debug.Assert(tree == newState.Tree);
                RebuildSymbolTree(newState, tree);
                if (Interlocked.Decrement(ref _parseCounter) == 0)
                {
                    IEnumerable<EditorState> states = GetAllEditorStates();
                    RefreshIssues(states);
                }
            };
            newTab.Tag = newState;
            newTab.Controls.Add(newState.Container);
            tcFiles.TabPages.Add(newTab);
            AddToSymbolTree(newState, newState.Name);
            return (newState);
        }

        private void RemoveFileTab(EditorState editorState)
        {
            editorState.Stop();
            TabPage tab = (TabPage)editorState.Tag;
            tcFiles.TabPages.Remove(tab);
            RemoveFromSymbolTree(editorState);
        }

        private void OpenFileTab(string filePath)
        {
            EditorState newState = AddFileTab(Path.GetFileName(filePath));
            TabPage tab = (TabPage)newState.Tag;
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
            Tuple<bool, Exception> openRes = IOOpenFile(newState, filePath);
            if (!openRes.Item1)
            {
                ShowError(openRes.Item2, _errorFileOpenMessage, filePath);
                RemoveFileTab(newState);
            }
            else
            {
                // Remove first tab when it was a "New" and is still unchanged
                if (tcFiles.TabPages.Count == 2)
                {
                    TabPage firstTab = tcFiles.TabPages[0];
                    EditorState existingState = (EditorState)firstTab.Tag;
                    if (existingState.FilePath == null && !existingState.IsChanged)
                        tcFiles.TabPages.Remove(firstTab);
                }
                newState.SetFocus();
            }
        }

        private bool CloseTabs(IEnumerable<EditorState> editorStates)
        {
            foreach (EditorState editorState in editorStates)
            {
                if (editorState.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(editorState, false);
                    if (!saveRes.Item1)
                        return (false);
                }
                RemoveFileTab(editorState);
            }
            return (true);
        }
        #endregion

        #region IO
        private Tuple<bool, Exception> IOOpenFile(EditorState editorState, string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string contents = reader.ReadToEnd();
                    editorState.FileEncoding = reader.CurrentEncoding;
                    editorState.SetText(contents);
                }
            }
            catch (IOException e)
            {
                return new Tuple<bool, Exception>(false, e);
            }
            editorState.Name = Path.GetFileName(filePath);
            editorState.FilePath = filePath;
            editorState.IsChanged = false;
            UpdateTabState(editorState);
            return new Tuple<bool, Exception>(true, null);
        }

        private Tuple<bool, Exception> IOSaveFile(EditorState editorState)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(editorState.FilePath, false, editorState.FileEncoding))
                {
                    writer.Write(editorState.GetText());
                    writer.Flush();
                }
                editorState.IsChanged = false;
                UpdateTabState(editorState);
                return new Tuple<bool, Exception>(true, null);
            }
            catch (IOException e)
            {
                return new Tuple<bool, Exception>(false, e);
            }
        }
        #endregion

        private void tcFiles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tcFiles.TabCount; ++i)
                {
                    Rectangle r = tcFiles.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        cmsTabActions.Show(tcFiles, e.Location);
                        break;
                    }
                }
            }
        }

        private void tcFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcFiles.SelectedIndex == -1)
                UpdateTabState(null);
            else
            {
                TabPage selectedTab = tcFiles.TabPages[tcFiles.SelectedIndex];
                EditorState editorState = (EditorState)selectedTab.Tag;
                UpdateTabState(editorState);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IEnumerable<EditorState> changes = GetChangedEditorStates();
            if (changes.Count() > 0)
                e.Cancel = !CloseTabs(changes);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                for (int i = 1; i < args.Length; ++i)
                    OpenFileTab(args[i]);
            }
            else
                MenuActionFileNew(this, new EventArgs());
        }

        #region Symbols
        private void tvTree_DoubleClick(object sender, EventArgs e)
        {
            if (tvTree.SelectedNode != null && tcFiles.SelectedIndex > -1)
            {
                TabPage selectedTab = tcFiles.TabPages[tcFiles.SelectedIndex];
                EditorState editorState = (EditorState)selectedTab.Tag;
                TreeNode treeNode = tvTree.SelectedNode;
                if (treeNode.Level > 0)
                {
                    Entity entity = (Entity)tvTree.SelectedNode.Tag;
                    editorState.GoToPosition(entity.LineInfo.Start);
                }
            }
        }

        private TreeNode FindRootSymbolNode(object tag)
        {
            TreeNode result = null;
            foreach (TreeNode node in tvTree.Nodes)
            {
                if (node.Tag.Equals(tag))
                {
                    result = node;
                    break;
                }
            }
            return (result);
        }

        private void RemoveFromSymbolTree(object tag)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode != null);

            tvTree.BeginUpdate();
            foundNode.Remove();
            tvTree.EndUpdate();
        }

        private void RenamedInSymbolTree(object tag, string name)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode != null);

            tvTree.BeginUpdate();
            foundNode.Text = name;
            tvTree.EndUpdate();
        }

        private void AddToSymbolTree(object tag, string name)
        {
            TreeNode foundNode = FindRootSymbolNode(tag);
            Debug.Assert(foundNode == null);

            tvTree.BeginUpdate();
            TreeNode newNode = new TreeNode() { Text = name };
            newNode.Tag = tag;
            tvTree.Nodes.Add(newNode);
            tvTree.EndUpdate();
        }

        private static HashSet<Type> AllowedTreeEntities = new HashSet<Type>()
        {
            typeof(PageEntity),
            typeof(SectionEntity),
            typeof(SubSectionEntity),
        };

        private List<TreeNode> BuildSymbolTree(Entity rootEntity, TreeNode rootNode, Entity selectedEntity)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (Entity entity in rootEntity.Children)
            {
                Type entityType = entity.GetType();
                if (!AllowedTreeEntities.Contains(entityType))
                    continue;
                TreeNode node = new TreeNode(entity.DisplayName);
                node.Tag = entity;
                rootNode.Nodes.Add(node);
                if (selectedEntity != null)
                {
                    if (selectedEntity.CompareTo(entity) == 0)
                        result.Add(node);
                }
                if (node.Level < 2)
                    result.AddRange(BuildSymbolTree(entity, node, selectedEntity));
            }
            return (result);
        }

        private void RebuildSymbolTree(object tag, ParseTree tree)
        {
            Entity lastEntity = null;
            if (tvTree.SelectedNode != null)
                lastEntity = tvTree.SelectedNode.Tag as Entity;

            TreeNode newSelectedNode = null;

            // Find root node by tag
            TreeNode rootNode = FindRootSymbolNode(tag);
            Debug.Assert(rootNode != null);

            tvTree.BeginUpdate();
            rootNode.Nodes.Clear();

            if (tree != null)
            {
                List<TreeNode> selNodes = BuildSymbolTree(tree.RootEntity, rootNode, lastEntity);
                if (selNodes.Count == 1)
                    newSelectedNode = selNodes.First();
            }
            tvTree.EndUpdate();

            tvTree.SelectedNode = newSelectedNode;
            if (newSelectedNode != null)
                newSelectedNode.Expand();
        }
        #endregion

        #region Menu
        private Tuple<bool, Exception> SaveFileAs(EditorState editorState, string filePath)
        {
            editorState.FilePath = filePath;
            editorState.Name = Path.GetFileName(filePath);
            RenamedInSymbolTree(editorState, editorState.Name);
            Tuple<bool, Exception> result = IOSaveFile(editorState);
            return (result);
        }

        private Tuple<bool, Exception> SaveWithConfirmation(EditorState editorState, bool skipConfirmation)
        {
            Debug.Assert(editorState.IsChanged);
            string caption = $"File '{editorState.Name}' was changed";
            string text = $"The file '{editorState.Name}' contains changes, do you want to save it first before continue?";
            DialogResult r = skipConfirmation ? DialogResult.OK : MessageBox.Show(this, text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
                return new Tuple<bool, Exception>(false, null);
            else if (r == DialogResult.No)
                return new Tuple<bool, Exception>(true, null);
            else
            {
                if (string.IsNullOrEmpty(editorState.FilePath))
                {
                    if (dlgSaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dlgSaveFile.FileName;
                        Tuple<bool, Exception> result = SaveFileAs(editorState, filePath);
                        return (result);
                    }
                    else return new Tuple<bool, Exception>(false, null);
                }
                else
                {
                    Tuple<bool, Exception> result = IOSaveFile(editorState);
                    if (!result.Item1)
                        ShowError(result.Item2, _errorFileSaveMessage, editorState.FilePath);
                    return (result);
                }
            }
        }

        private void MenuActionFileNew(object sender, EventArgs e)
        {
            string name = GetNextTabName("File");
            EditorState newState = AddFileTab(name);
            TabPage tab = (TabPage)newState.Tag;
            tcFiles.SelectedIndex = tcFiles.TabPages.IndexOf(tab);
            newState.SetFocus();
            UpdateTabState(newState);
        }
        private void MenuActionFileOpen(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)

                foreach (string filePath in dlgOpenFile.FileNames)
                    OpenFileTab(filePath);
        }
        private void MenuActionFileSave(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            SaveWithConfirmation(editorState, true);
        }
        private void MenuActionFileSaveAs(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            if (dlgSaveFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlgSaveFile.FileName;
                EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
                Tuple<bool, Exception> r = SaveFileAs(editorState, filePath);
                if (!r.Item1)
                    ShowError(r.Item2, _errorFileSaveMessage, filePath);
            }
        }
        private void MenuActionFileSaveAll(object sender, EventArgs e)
        {
            foreach (TabPage tab in tcFiles.TabPages)
            {
                EditorState editorState = (EditorState)tab.Tag;
                if (editorState.IsChanged)
                {
                    Tuple<bool, Exception> saveRes = SaveWithConfirmation(editorState, true);
                    if (!saveRes.Item1)
                        return;
                }
            }
        }
        private void MenuActionFileClose(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            List<EditorState> tabsToClose = new List<EditorState>();
            tabsToClose.Add(editorState);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAll(object sender, EventArgs e)
        {
            List<EditorState> tabsToClose = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
                tabsToClose.Add((EditorState)tab.Tag);
            CloseTabs(tabsToClose);
        }
        private void MenuActionFileCloseAllButThis(object sender, EventArgs e)
        {
            List<EditorState> tabsToClose = new List<EditorState>();
            foreach (TabPage tab in tcFiles.TabPages)
            {
                if (tab != tcFiles.SelectedTab)
                    tabsToClose.Add((EditorState)tab.Tag);
            }
            CloseTabs(tabsToClose);
        }

        private void MenuActionEditSearchAndReplaceQuickSearch(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.ShowSearch();
        }
        private void MenuActionEditSearchAndReplaceQuickReplace(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.ShowReplace();
        }
        private void MenuActionEditUndo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Undo();
        }
        private void MenuActionEditRedo(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Redo();
        }
        private void MenuActionEditCut(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Cut();
        }
        private void MenuActionEditCopy(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Copy();
        }
        private void MenuActionEditPaste(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.Paste();
        }
        private void MenuActionEditSelectAll(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            editorState.SelectAll();
        }

        private void MenuActionEditGoToSymbol(object sender, EventArgs e)
        {
            Debug.Assert(tcFiles.SelectedTab != null);
            EditorState editorState = (EditorState)tcFiles.SelectedTab.Tag;
            if (editorState.Tree != null)
            {
                IEnumerable<Entity> allEntities = editorState.Tree.GetAllEntities();
                List<SymbolItemModel> symbols = new List<SymbolItemModel>();
                HashSet<Type> types = new HashSet<Type>();
                foreach (Entity entity in allEntities)
                {
                    Type t = entity.GetType();
                    if (typeof(PageEntity).Equals(t) ||
                        typeof(SectionEntity).Equals(t) ||
                        typeof(SubSectionEntity).Equals(t) ||
                        typeof(DeclarationEntity).Equals(t))
                    {
                        symbols.Add(new SymbolItemModel()
                        {
                            Caption = entity.DisplayName,
                            Id = entity.Id,
                            TypeString = entity.GetType().Name,
                            Position = entity.LineInfo.Start,
                        });
                        types.Add(entity.GetType());
                    }
                }
                SymbolSearchForm form = new SymbolSearchForm(symbols, types);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    SymbolItemModel selectedItem = form.SelectedItem;
                    editorState.GoToPosition(selectedItem.Position);
                }
            }
        }

        private void UpdateMenuSelection(EditorState editorState)
        {
            miEditCut.Enabled = editorState != null && editorState.CanCut();
            miEditCopy.Enabled = editorState != null && editorState.CanCopy();
        }

        private void UpdateMenuEditChange(EditorState editorState)
        {
            miEditUndo.Enabled = editorState != null && editorState.CanUndo();
            miEditRedo.Enabled = editorState != null && editorState.CanRedo();
            miEditPaste.Enabled = editorState != null && editorState.CanPaste();
        }
        #endregion

        #region Issues
        enum IssueType
        {
            Error,
            Warning,
            Info,
        }
        struct IssueTag
        {
            public EditorState State { get; }
            public Entity Entity { get; }
            public IssueTag(EditorState state, Entity entity)
            {
                State = state;
                Entity = entity;
            }
        }
        private void AddIssue(IssueTag tag, IssueType type, string message, string symbolName, string symbolType, string group, string file)
        {
            ListViewItem newItem = new ListViewItem(message);
            newItem.Tag = tag;
            newItem.ImageIndex = (int)type;
            newItem.SubItems.Add(symbolName);
            newItem.SubItems.Add(symbolType);
            newItem.SubItems.Add(group);
            newItem.SubItems.Add(file);
            lvIssues.Items.Add(newItem);
        }
        private readonly Regex _rexRefWithIdent = new Regex("^(@ref\\s+[a-zA-Z_][a-zA-Z0-9_]+)$", RegexOptions.Compiled);
        private void AddIssuesFromEntity(IEnumerable<EditorState> states, EditorState state, Entity entity, string fileName, string groupName)
        {
            if (typeof(CommentEntity).Equals(entity.GetType()))
            {
                CommentEntity comment = (CommentEntity)entity;
                DeclarationEntity decl = entity.FindChildByType<DeclarationEntity>();
                if (decl != null)
                {
                    ParamEntity seeParam = comment.FindChildByExpression<ParamEntity>(f => "see".Equals(f.ParamName) && _rexRefWithIdent.IsMatch(f.ParamValue));
                    if (seeParam == null)
                        AddIssue(new IssueTag(state, decl), IssueType.Warning, "Missing documentation", decl.DisplayName, decl.DeclarationType.ToString(), groupName, fileName);
                }
            }
        }
        private void RefreshIssues(IEnumerable<EditorState> states)
        {
            lvIssues.BeginUpdate();
            lvIssues.Items.Clear();
            foreach (EditorState state in states)
            {
                foreach (Entity childEntity in state.Tree.RootEntity.Children)
                {
                    if (typeof(GroupEntity).Equals(childEntity.GetType()))
                    {
                        GroupEntity group = (GroupEntity)childEntity;
                        foreach (var groupChild in childEntity.Children)
                        {
                            AddIssuesFromEntity(states, state, groupChild, state.Name, group.GroupCaption);
                        }
                    }
                    else
                    {
                        AddIssuesFromEntity(states, state, childEntity, state.Name, "Root");
                    }
                }
            }
            lvIssues.EndUpdate();
        }

        private void lvIssues_DoubleClick(object sender, EventArgs e)
        {
            if (lvIssues.SelectedItems.Count > 0)
            {
                ListViewItem item = lvIssues.SelectedItems[0];
                if (item.Tag != null)
                {
                    IssueTag tag = (IssueTag)item.Tag;
                    Entity entity = tag.Entity;
                    EditorState state = tag.State;
                    TabPage tab = (TabPage)state.Tag;
                    tcFiles.SelectedTab = tab;
                    state.GoToPosition(entity.LineInfo.Start);
                }
            }
        }
        #endregion
    }
}