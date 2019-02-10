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
        private static HashSet<string> _identCommands = new HashSet<string>()
        {
            "page",
            "section",
            "subsection",
            "subsubsection",
            "defgroup", 

            "cond",
            "subpage",
            "ref",
            "param",
        };

        enum CaptionMode
        {
            UntilEndOfLine,
            Literal,
        }

        class CaptionCommandSetting
        {
            public CaptionMode Mode { get; }
            public CaptionCommandSetting(CaptionMode mode = CaptionMode.UntilEndOfLine)
            {
                Mode = mode;
            }
        }

        private static Dictionary<string, CaptionCommandSetting> _captionCommands = new Dictionary<string, CaptionCommandSetting>()
        {
            {"page", new CaptionCommandSetting() },
            {"defgroup", new CaptionCommandSetting(CaptionMode.Literal) },
            {"section", new CaptionCommandSetting() },
            {"subsection", new CaptionCommandSetting() },
            {"subsubsection", new CaptionCommandSetting() },

            {"subpage", new CaptionCommandSetting(CaptionMode.Literal) },
            {"ref", new CaptionCommandSetting(CaptionMode.Literal) },
            {"param", new CaptionCommandSetting(CaptionMode.Literal) },
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

            if (_identCommands.Contains(command))
            {
                SkipWhitespaces();
                if (SyntaxUtils.IsIdentStart(Buffer.PeekChar()))
                {
                    Buffer.Start();
                    while (!Buffer.IsEOF)
                    {
                        if (SyntaxUtils.IsIdent(Buffer.PeekChar()))
                            Buffer.AdvanceChar();
                        else
                            break;
                    }
                    int identStart = Buffer.LexemeStart;
                    int identLen = Buffer.LexemeWidth;
                    DoxygenToken identToken = new DoxygenToken(DoxygenTokenType.Ident, identStart, identLen, true);
                    PushToken(identToken);
                }

                if (_captionCommands.ContainsKey(command))
                {
                    CaptionCommandSetting setting = _captionCommands[command];
                    SkipWhitespaces(true);
                    if (Buffer.PeekChar() != '\n')
                    {
                        Buffer.Start();
                        bool hadQuote = false;
                        if (Buffer.PeekChar() == '\"' && setting.Mode == CaptionMode.Literal)
                        {
                            hadQuote = true;
                            Buffer.AdvanceChar();
                        }
                        while (!Buffer.IsEOF)
                        {
                            if (Buffer.PeekChar() == '\n')
                                break;
                            if (setting.Mode == CaptionMode.Literal)
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
                DoxygenToken token = new DoxygenToken(DoxygenTokenType.Text, Buffer.LexemeStart, Buffer.LexemeWidth, true);
                PushToken(token);
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
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStart, Buffer.LexemeStart, Buffer.LexemeWidth, true));
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
                                    PushToken(new DoxygenToken(DoxygenTokenType.BlockStart, Buffer.LexemeStart, Buffer.LexemeWidth, true));
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
                        } break;

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
                                    return (PushToken(new DoxygenToken(DoxygenTokenType.BlockEnd, Buffer.LexemeStart, Buffer.LexemeWidth, true)));
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
                            return PushToken(new DoxygenToken(Buffer.IsEOF ? DoxygenTokenType.EOF : DoxygenTokenType.Invalid, 0, 0, false));
                        }

                    default:
                        {
                            Buffer.NextChar();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            return (false);
        }
    }
}
