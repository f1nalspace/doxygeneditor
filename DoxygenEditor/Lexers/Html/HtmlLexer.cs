using System;
using System.Diagnostics;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Lexers.Html
{
    class HtmlLexer : BaseLexer<HtmlToken>
    {
        public HtmlLexer(SourceBuffer source) : base(source)
        {
        }

        private void LexTag()
        {
            Debug.Assert(Buffer.PeekChar() == '<');
            Buffer.Start();

            HtmlToken startTagToken = new HtmlToken(HtmlTokenType.MetaTagStart, Buffer.LexemeStart, 0, false);
            PushToken(startTagToken);

            bool allowAttributes = true;
            Buffer.AdvanceChar();
            if (Buffer.PeekChar() == '/')
            {
                startTagToken.ChangeType(HtmlTokenType.MetaTagClose);
                Buffer.AdvanceChar();
                allowAttributes = false;
            }
            PushToken(new HtmlToken(HtmlTokenType.TagChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));

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
                PushToken(new HtmlToken(HtmlTokenType.TagName, Buffer.LexemeStart, Buffer.LexemeWidth, true));
            }

            if (allowAttributes)
            {
                while (!Buffer.IsEOF)
                {
                    SkipWhitespaces();
                    char c = Buffer.PeekChar();
                    if (!SyntaxUtils.IsIdentStart(c))
                        break;
                    else
                    {
                        Buffer.Start();
                        while (!Buffer.IsEOF)
                        {
                            if (SyntaxUtils.IsIdent(Buffer.PeekChar()))
                                Buffer.AdvanceChar();
                            else
                                break;
                        }
                        PushToken(new HtmlToken(HtmlTokenType.AttrName, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                        SkipWhitespaces(); // Allow whitespaces before =
                        if (Buffer.PeekChar() == '=')
                        {
                            Buffer.Start();
                            Buffer.AdvanceChar();
                            PushToken(new HtmlToken(HtmlTokenType.AttrChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                            SkipWhitespaces(); // Allow whitespaces after =
                            if (Buffer.PeekChar() == '"' || Buffer.PeekChar() == '\'')
                            {
                                char quote = Buffer.PeekChar();
                                Buffer.Start();
                                Buffer.AdvanceChar();
                                while (!Buffer.IsEOF)
                                {
                                    char attrC = Buffer.PeekChar();
                                    if (attrC != quote && attrC != '\n')
                                        Buffer.AdvanceChar();
                                    else
                                        break;
                                }
                                if (Buffer.PeekChar() == quote)
                                    Buffer.AdvanceChar();
                                PushToken(new HtmlToken(HtmlTokenType.AttrValue, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                            }
                        }
                        else
                            break;
                    }
                }
            }

            SkipWhitespaces(); // Allow whitespaces before /
            if (Buffer.PeekChar() == '/')
            {
                startTagToken.ChangeType(HtmlTokenType.MetaTagStartAndClose);
                Buffer.AdvanceChar();
                SkipWhitespaces(); // Allow whitespaces after /
            }

            SkipUntil('>');

            if (Buffer.PeekChar() == '>')
            {
                Buffer.Start();
                Buffer.AdvanceChar();
                PushToken(new HtmlToken(HtmlTokenType.TagChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
            }

            int tagLength = Buffer.Position - startTagToken.Index;
            startTagToken.ChangeLength(tagLength);
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces();
                switch (Buffer.PeekChar())
                {
                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                PushToken(new HtmlToken(HtmlTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
                                return (false);
                            }
                            else
                                Buffer.NextChar();
                        }
                        break;

                    case '<':
                        {
                            LexTag();
                            return (true);
                        }

                    default:
                        {
                            Buffer.NextChar();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            PushToken(new HtmlToken(HtmlTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
            return (false);
        }
    }
}
