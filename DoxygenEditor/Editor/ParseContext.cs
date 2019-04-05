using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Html;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Editor
{
    class ParseContext : IParseControl, IParseInfo, IDisposable
    {
        private BackgroundWorker _parseWorker;
        private readonly List<IBaseToken> _tokens = new List<IBaseToken>();
        private readonly List<TextError> _errors = new List<TextError>();
        private readonly List<PerformanceItemModel> _performanceItems = new List<PerformanceItemModel>();
        public IEnumerable<TextError> Errors => _errors;
        public IEnumerable<PerformanceItemModel> PerformanceItems => _performanceItems;
        public IBaseNode DoxyTree { get; private set; }
        public IBaseNode CppTree { get; private set; }
        public SymbolTable SymbolTable { get; private set; }
        public delegate void ParseEventHandler(object sender);
        public event ParseEventHandler ParseCompleted;
        public event ParseEventHandler ParseStarting;
        private readonly IEditorId _id;
        private readonly IStylerData _stylerRefresh;
        public bool IsParsing => _parseWorker.IsBusy;

        public ParseContext(IEditorId id, IStylerData dataStyler)
        {
            _id = id;
            _stylerRefresh = dataStyler;

            SymbolTable = new SymbolTable(id);
            _parseWorker = new BackgroundWorker();
            _parseWorker.WorkerSupportsCancellation = true;
            _parseWorker.DoWork += (s, e) =>
            {
                // @TODO(final): Support for incremental parsing, so only changes are applied
                string text = (string)e.Argument;
                Tokenize(text);
                Parse(text, _stylerRefresh);
            };
            _parseWorker.RunWorkerCompleted += (s, e) =>
            {
                GlobalSymbolCache.AddOrReplaceTable(SymbolTable);
                ParseCompleted?.Invoke(this);
            };
        }

        public void Dispose()
        {
            GiveTokensBackToPool();
        }

        public void StartParsing(string text)
        {
            ParseStarting?.Invoke(this);
            _parseWorker.RunWorkerAsync(text);
        }
        public void StopParsing()
        {
            _parseWorker.CancelAsync();
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
                        startState.StartPosition = new TextPosition(last.End + 1, last.Position.Line, last.Position.Column + 1);
                    }
                    else
                        startState.StartPosition = new TextPosition(doxyToken.End + 1, doxyToken.Position.Line, doxyToken.Position.Column + 1);

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
                                    DoxygenToken codeRangedToken = DoxygenTokenPool.Make(DoxygenTokenKind.Code, new TextRange(commandContentStart, commandContentLength), true);
                                    result.AddToken(codeRangedToken);
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
            SymbolTable.Clear();

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

            int countCppTokens = _tokens.Count(t => typeof(CppToken).Equals(t.GetType()));
            int countHtmlTokens = _tokens.Count(t => typeof(HtmlToken).Equals(t.GetType()));
            int countDoxyTokens = _tokens.Count(t => typeof(DoxygenToken).Equals(t.GetType()));
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{text.Length} chars", $"{countCppTokens} tokens", "C++ lexer", totalStats.CppDuration));
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{text.Length} chars", $"{countDoxyTokens} tokens", "Doxygen lexer", totalStats.DoxyDuration));
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{text.Length} chars", $"{countHtmlTokens} tokens", "Html lexer", totalStats.HtmlDuration));
        }

        private void Parse(string text, IStylerData stylerData)
        {
            // Clear stream from all invalid tokens
            _tokens.RemoveAll(d => d.IsEOF || (!d.IsMarker && d.Length == 0));

            // @NOTE(final): Right know, the tokens are not in incremental range
            // Several reasons for this:
            // - No tokens gets replaced by another range
            // - Start/End marker tokens
            // - Zero-length tokens
#if false
            // Validate token stream
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
            using (DoxygenParser doxyParser = new DoxygenParser(_id, text))
            {
                doxyParser.ParseTokens(_tokens);
                _errors.InsertRange(0, doxyParser.ParseErrors);
                DoxyTree = doxyParser.Root;
                doxyNodeCount = doxyParser.TotalNodeCount;
                SymbolTable.AddTable(doxyParser.SymbolTable);
            }
            timer.Stop();
            Debug.WriteLine($"Doxygen parse done, took {timer.Elapsed.ToMilliseconds()} ms");
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{_tokens.Count} tokens", $"{doxyNodeCount} nodes", "Doxygen parser", timer.Elapsed));

            // C++ parsing
            timer.Restart();
            int cppNodeCount = 0;
            using (CppParser cppParser = new CppParser(_id))
            {
                cppParser.GetDocumentationNode += (token) =>
                {
                    IBaseNode result = DoxyTree.FindNodeByRange(token.Range);
                    return (result);
                };
                cppParser.ParseTokens(_tokens);
                _errors.InsertRange(0, cppParser.ParseErrors);
                CppTree = cppParser.Root;
                cppNodeCount = cppParser.TotalNodeCount;
                SymbolTable.AddTable(cppParser.SymbolTable);
            }
            timer.Stop();
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{_tokens.Count} tokens", $"{cppNodeCount} nodes", "C++ parser", timer.Elapsed));

            timer.Restart();
            stylerData.RefreshData(_tokens);
            timer.Stop();
            _performanceItems.Add(new PerformanceItemModel(_id, _id.TabIndex, $"{_tokens.Count} tokens", $"{stylerData.Count} styles", "Styler", timer.Elapsed));
        }
    }
}
