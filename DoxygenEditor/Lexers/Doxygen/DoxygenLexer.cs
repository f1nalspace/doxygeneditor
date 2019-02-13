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

        public DoxygenLexer(SourceBuffer source) : base(source)
        {

        }

        private DoxygenToken LexCharToken()
        {
            Buffer.Start();
            Buffer.NextChar();
            DoxygenToken result = new DoxygenToken(DoxygenTokenType.Text, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        private void LexCodeTokens()
        {
            if (Buffer.PeekChar() == '{')
            {
                Buffer.Start();
                Buffer.AdvanceChar();
                while (!Buffer.IsEOF)
                {
                    if (Buffer.PeekChar() == '}')
                    {
                        Buffer.AdvanceChar();
                        break;
                    }
                    Buffer.AdvanceChar();
                }
                DoxygenToken token = new DoxygenToken(DoxygenTokenType.CodeType, Buffer.LexemeStart, Buffer.LexemeWidth, true);
                PushToken(token);
            }
            Buffer.Start();
            while (!Buffer.IsEOF)
            {
                if (IsCommandBegin(Buffer.PeekChar()))
                {
                    if (Buffer.Compare(1, "endcode") == 0)
                    {
                        DoxygenToken codeToken = new DoxygenToken(DoxygenTokenType.CodeBlock, Buffer.LexemeStart, Buffer.LexemeWidth, true);
                        string codeText = Buffer.GetText(codeToken.Index, codeToken.Length);
                        PushToken(codeToken);
                        Buffer.Start();
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
            Debug.Assert(Buffer.PeekChar() == '@' || Buffer.PeekChar() == '\\');
            Debug.Assert(IsCommandIdentStart(Buffer.PeekChar(1)));

            // Command
            Buffer.Start();
            Buffer.NextChar();
            StringBuilder commandString = new StringBuilder();

            DoxygenTokenType type = DoxygenTokenType.Command;
            if (Buffer.PeekChar() == '{' || Buffer.PeekChar() == '}')
            {
                // Special case for { } command
                type = DoxygenTokenType.GroupStart;
                if (Buffer.PeekChar() == '}')
                    type = DoxygenTokenType.GroupEnd;
                commandString.Append(Buffer.PeekChar());
                Buffer.NextChar();
            }
            else
            {
                // Normal case
                while (!Buffer.IsEOF)
                {
                    if (IsCommandIdent(Buffer.PeekChar()))
                    {
                        commandString.Append(Buffer.PeekChar());
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
                    if (SyntaxUtils.IsIdentStart(Buffer.PeekChar()))
                    {
                        Buffer.Start();
                        while (!Buffer.IsEOF)
                        {
                            if (!char.IsWhiteSpace(Buffer.PeekChar()))
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
                    if (Buffer.PeekChar() != '\n')
                    {
                        Buffer.Start();
                        bool hadQuote = false;
                        if (Buffer.PeekChar() == '\"' && rule.Caption == CaptionMode.Text)
                        {
                            hadQuote = true;
                            Buffer.AdvanceChar();
                        }
                        while (!Buffer.IsEOF)
                        {
                            if (Buffer.PeekChar() == '\n')
                                break;
                            if (rule.Caption == CaptionMode.Text)
                            {
                                if (!hadQuote)
                                    break;
                                else
                                {
                                    if (Buffer.PeekChar() == '\"')
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

        private void StartText()
        {
            Buffer.Start();
        }
        private void PushText()
        {
            if (Buffer.LexemeWidth > 0)
            {
                string text = Buffer.GetText(Buffer.LexemeStart, Buffer.LexemeWidth);
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

        private void Done(ref BlockFlags flags)
        {
            PushText();
            if (flags.HasFlag(BlockFlags.InsideBlock))
            {
                // Block was not closed, so we close it now
                flags = BlockFlags.None;
                PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Math.Max(0, Buffer.End - 1), 1, false));
            }
        }

        protected override bool LexNext()
        {
            BlockFlags flags = BlockFlags.None;
            do
            {
                SkipWhitespaces();
                switch (Buffer.PeekChar())
                {
                    case '/':
                        {
                            char n = Buffer.PeekChar(1);
                            if (n == '*')
                            {
                                // Multi line
                                char n2 = Buffer.PeekChar(2);
                                if (n2 == '!' || n2 == '*')
                                {
                                    Debug.Assert(!flags.HasFlag(BlockFlags.InsideBlock));
                                    Buffer.Start();
                                    Buffer.AdvanceChar(3);
                                    flags = BlockFlags.InsideBlock;
                                    if (n2 == '*') flags |= BlockFlags.JavaDoc;
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStartMulti, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText();
                                    continue;
                                }
                            }
                            else if (n == '/')
                            {
                                // Single line
                                char n2 = Buffer.PeekChar(2);
                                if (n2 == '!' || n2 == '/')
                                {
                                    Debug.Assert(!flags.HasFlag(BlockFlags.InsideBlock));
                                    Buffer.Start();
                                    Buffer.AdvanceChar(3);
                                    flags = BlockFlags.InsideBlock | BlockFlags.SingleLine;
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStartSingle, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText();
                                    continue;
                                }
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '\n':
                        {
                            if (flags.HasFlag(BlockFlags.InsideBlock) && flags.HasFlag(BlockFlags.SingleLine))
                            {
                                PushText();
                                flags = BlockFlags.None;
                                return (PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Buffer.Position, 0, true)));
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '*':
                        {
                            char n = Buffer.PeekChar(1);
                            if (flags.HasFlag(BlockFlags.InsideBlock))
                            {
                                if (n == '/')
                                {
                                    PushText();
                                    Buffer.Start();
                                    Buffer.AdvanceChar(2);
                                    flags = BlockFlags.None;
                                    return PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                }
                                else if (flags.HasFlag(BlockFlags.JavaDoc))
                                {
                                    // Push single star token (java doc style)
                                    PushText();
                                    Buffer.Start();
                                    Buffer.AdvanceChar();
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                                    StartText();
                                    continue;
                                }
                            }
                            Buffer.NextChar();
                        }
                        break;

                    case '@':
                    case '\\':
                        {
                            char n = Buffer.PeekChar(1);
                            if (flags.HasFlag(BlockFlags.InsideBlock) && IsCommandIdentStart(n))
                            {
                                PushText();
                                LexCommandTokens();
                                StartText();
                            }
                            else
                                Buffer.NextChar();
                        }
                        break;

                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                Done(ref flags);
                                PushToken(new DoxygenToken(DoxygenTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
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
            Done(ref flags);
            PushToken(new DoxygenToken(DoxygenTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
            return (false);
        }
    }
}
