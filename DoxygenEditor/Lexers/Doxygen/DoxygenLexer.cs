using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Lexers.Doxygen
{
    class DoxygenLexer : BaseLexer<DoxygenToken>
    {
        public static HashSet<char> MultiLineDocChars = new HashSet<char>() { '!', '*' };
        public static HashSet<char> SingleLineDocChars = new HashSet<char>() { '!', '/' };

        enum NameMode
        {
            None,
            Identifier,
            UntilWhitespace
        }

        enum CaptionMode
        {
            None,
            UntilEndOfLine,
            Text,
        }

        class LexCommandRule
        {
            public NameMode Name { get; }
            public CaptionMode Caption { get; }
            public LexCommandRule(NameMode name, CaptionMode caption)
            {
                Name = name;
                Caption = caption;
            }
        }

        private static Dictionary<string, LexCommandRule> _lexCommandRules = new Dictionary<string, LexCommandRule>()
        {
            {"page", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.UntilEndOfLine) },
            {"defgroup", new LexCommandRule(NameMode.Identifier, CaptionMode.UntilEndOfLine) },
            {"section", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.UntilEndOfLine) },
            {"subsection", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.UntilEndOfLine) },
            {"subsubsection", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.UntilEndOfLine) },

            {"subpage", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.Text) },
            {"ref", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.Text) },
            {"param", new LexCommandRule(NameMode.Identifier, CaptionMode.UntilEndOfLine) },
            {"cond", new LexCommandRule(NameMode.UntilWhitespace, CaptionMode.None) },
            {"brief", new LexCommandRule(NameMode.None, CaptionMode.UntilEndOfLine) },
        };

        private bool IsCommandBegin(char c)
        {
            bool result = c == '@' || c == '\\';
            return (result);
        }
        private bool IsCommandIdentStart(char c)
        {
            bool result = SyntaxUtils.IsIdentStart(c) || c == '{' || c == '}';
            return (result);
        }
        private bool IsCommandIdent(char c)
        {
            bool result = SyntaxUtils.IsIdent(c);
            return (result);
        }

        public DoxygenLexer(string source, int sbase, int length) : base(source, sbase, length)
        {

        }

        private DoxygenToken LexCharToken()
        {
            Buffer.StartLexeme();
            Buffer.NextChar();
            DoxygenToken result = new DoxygenToken(DoxygenTokenType.Text, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        private void LexCodeTokens()
        {
            if (Buffer.Peek() == '{')
            {
                Buffer.StartLexeme();
                Buffer.AdvanceChar();
                while (!Buffer.IsEOF)
                {
                    if (Buffer.Peek() == '}')
                    {
                        Buffer.AdvanceChar();
                        break;
                    }
                    Buffer.AdvanceChar();
                }
                DoxygenToken token = new DoxygenToken(DoxygenTokenType.CodeType, Buffer.LexemeStart, Buffer.LexemeWidth, true);
                PushToken(token);
            }
            int codeStart = Buffer.StreamPosition;
            while (!Buffer.IsEOF)
            {
                if (IsCommandBegin(Buffer.Peek()))
                {
                    if (Buffer.CompareText(1, "endcode") == 0)
                    {
                        int codeLength = Buffer.StreamPosition - codeStart;
                        DoxygenToken codeToken = new DoxygenToken(DoxygenTokenType.CodeBlock, codeStart, codeLength, true);
                        string codeText = Buffer.GetStreamText(codeToken.Index, codeToken.Length);
                        PushToken(codeToken);
                        Buffer.StartLexeme();
                        Buffer.AdvanceChar(1 + "endcode".Length);
                        PushToken(new DoxygenToken(DoxygenTokenType.Command, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                        break;
                    }
                }
                Buffer.AdvanceChar();
            }
        }

        private void LexCommandTokens()
        {
            Debug.Assert(Buffer.Peek() == '@' || Buffer.Peek() == '\\');
            Debug.Assert(IsCommandIdentStart(Buffer.Peek(1)));

            // Command
            Buffer.StartLexeme();
            Buffer.NextChar();
            StringBuilder commandString = new StringBuilder();

            DoxygenTokenType type = DoxygenTokenType.Command;
            if (Buffer.Peek() == '{' || Buffer.Peek() == '}')
            {
                // Special case for { } command
                type = DoxygenTokenType.GroupStart;
                if (Buffer.Peek() == '}')
                    type = DoxygenTokenType.GroupEnd;
                commandString.Append(Buffer.Peek());
                Buffer.NextChar();
            }
            else
            {
                // Normal case
                while (!Buffer.IsEOF)
                {
                    if (IsCommandIdent(Buffer.Peek()))
                    {
                        commandString.Append(Buffer.Peek());
                        Buffer.AdvanceChar();
                    }
                    else
                        break;
                }
            }

            int commandStart = Buffer.LexemeStart;
            int commandLen = Buffer.LexemeWidth;
            string command = commandString.ToString();
            DoxygenToken commandToken = new DoxygenToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            PushToken(commandToken);

            if ("code".Equals(command))
            {
                LexCodeTokens();
                return;
            }

            LexCommandRule rule = _lexCommandRules.ContainsKey(command) ? _lexCommandRules[command] : null;
            if (rule != null)
            {
                if (rule.Name != NameMode.None)
                {
                    SkipWhitespaces(true);
                    if (SyntaxUtils.IsIdentStart(Buffer.Peek()))
                    {
                        Buffer.StartLexeme();
                        while (!Buffer.IsEOF)
                        {
                            if (!char.IsWhiteSpace(Buffer.Peek()))
                                Buffer.AdvanceChar();
                            else
                                break;
                        }
                        int identStart = Buffer.LexemeStart;
                        int identLen = Buffer.LexemeWidth;
                        DoxygenToken identToken = new DoxygenToken(DoxygenTokenType.Name, identStart, identLen, true);
                        PushToken(identToken);
                    }
                }

                if (rule.Caption != CaptionMode.None)
                {
                    SkipWhitespaces(true);
                    if (Buffer.Peek() != '\n')
                    {
                        Buffer.StartLexeme();
                        bool hadQuote = false;
                        if (Buffer.Peek() == '\"' && rule.Caption == CaptionMode.Text)
                        {
                            hadQuote = true;
                            Buffer.AdvanceChar();
                        }
                        while (!Buffer.IsEOF)
                        {
                            if (Buffer.Peek() == '\n')
                                break;
                            if (rule.Caption == CaptionMode.Text)
                            {
                                if (!hadQuote)
                                    break;
                                else
                                {
                                    if (Buffer.Peek() == '\"')
                                    {
                                        Buffer.AdvanceChar();
                                        break;
                                    }
                                }
                            }
                            Buffer.AdvanceChar();
                        }
                        if (Buffer.LexemeWidth > 0)
                            PushToken(new DoxygenToken(DoxygenTokenType.Caption, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                    }
                }
            }
        }

        private void StartText(State state)
        {
            state.TextStart = Buffer.StreamPosition;
        }
        private void PushText(State state)
        {
            Debug.Assert(state.TextStart != -1);
            int length = Math.Max(Buffer.StreamPosition - state.TextStart, 0);
            if (length > 0)
            {
                string text = Buffer.GetStreamText(state.TextStart, length);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    DoxygenToken token = new DoxygenToken(DoxygenTokenType.Text, Buffer.LexemeStart, Buffer.LexemeWidth, true);
                    PushToken(token);
                }
            }
        }

        [Flags]
        enum BlockFlags
        {
            None = 0,
            InsideBlock = 1 << 0,
            SingleLine = 1 << 1,
            JavaDoc = 1 << 2,
        }

        class State
        {
            public BlockFlags Flags { get; set; }
            public int TextStart { get; set; }

            public State()
            {
                Flags = BlockFlags.None;
                TextStart = -1;
            }
        }

        private void Done(State state)
        {
            PushText(state);
            if (state.Flags.HasFlag(BlockFlags.InsideBlock))
            {
                // Block was not closed, so we close it now
                state.Flags = BlockFlags.None;
                PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Math.Max(0, Buffer.StreamEnd - 1), 1, false));
            }
        }

        protected override bool LexNext()
        {
            State state = new State();
            do
            {
                SkipWhitespaces();
                switch (Buffer.Peek())
                {
                    case '/':
                        {
                            char n = Buffer.Peek(1);
                            if (n == '*')
                            {
                                // Multi line
                                char n2 = Buffer.Peek(2);
                                if (MultiLineDocChars.Contains(n2))
                                {
                                    Debug.Assert(!state.Flags.HasFlag(BlockFlags.InsideBlock));
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceChar(3);
                                    state.Flags = BlockFlags.InsideBlock;
                                    if (n2 == '*') state.Flags |= BlockFlags.JavaDoc;
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStartMulti, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText(state);
                                    continue;
                                }
                            }
                            else if (n == '/')
                            {
                                // Single line
                                char n2 = Buffer.Peek(2);
                                if (SingleLineDocChars.Contains(n2))
                                {
                                    Debug.Assert(!state.Flags.HasFlag(BlockFlags.InsideBlock));
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceChar(3);
                                    state.Flags = BlockFlags.InsideBlock | BlockFlags.SingleLine;
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStartSingle, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText(state);
                                    continue;
                                }
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '\n':
                        {
                            if (state.Flags.HasFlag(BlockFlags.InsideBlock) && state.Flags.HasFlag(BlockFlags.SingleLine))
                            {
                                PushText(state);
                                state.Flags = BlockFlags.None;
                                return (PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Buffer.StreamPosition, 0, true)));
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '*':
                        {
                            char n = Buffer.Peek(1);
                            if (state.Flags.HasFlag(BlockFlags.InsideBlock))
                            {
                                if (n == '/')
                                {
                                    PushText(state);
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceChar(2);
                                    state.Flags = BlockFlags.None;
                                    return PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                }
                                else if (state.Flags.HasFlag(BlockFlags.JavaDoc))
                                {
                                    // Push single star token (java doc style)
                                    PushText(state);
                                    Buffer.StartLexeme();
                                    Buffer.AdvanceChar();
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText(state);
                                    continue;
                                }
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '@':
                    case '\\':
                        {
                            char n = Buffer.Peek(1);
                            if (state.Flags.HasFlag(BlockFlags.InsideBlock) && IsCommandIdentStart(n))
                            {
                                PushText(state);
                                LexCommandTokens();
                                StartText(state);
                            }
                            else
                                Buffer.NextChar();
                        }
                        break;

                    case TextStream.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                Done(state);
                                PushToken(new DoxygenToken(DoxygenTokenType.EOF, Math.Max(0, Buffer.StreamEnd - 1), 0, false));
                                return (false);
                            }
                            else
                                Buffer.NextChar();
                        }
                        break;

                    default:
                        {
                            Buffer.NextChar();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            Done(state);
            PushToken(new DoxygenToken(DoxygenTokenType.EOF, Math.Max(0, Buffer.StreamEnd - 1), 0, false));
            return (false);
        }
    }
}
