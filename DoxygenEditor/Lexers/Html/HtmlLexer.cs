using System;
using System.Diagnostics;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Lexers.Html
{
    class HtmlLexer : BaseLexer<HtmlToken>
    {
        public HtmlLexer(string source, int sbase, int length) : base(source, sbase, length)
        {
        }

        private void LexTag()
        {
            Debug.Assert(Buffer.Peek() == '<');
            Buffer.StartLexeme();

            HtmlToken startTagToken = new HtmlToken(HtmlTokenType.MetaTagStart, Buffer.LexemeStart, 0, false);
            PushToken(startTagToken);

            bool allowAttributes = true;
            Buffer.AdvanceChar();
            if (Buffer.Peek() == '/')
            {
                startTagToken.ChangeType(HtmlTokenType.MetaTagClose);
                Buffer.AdvanceChar();
                allowAttributes = false;
            }
            PushToken(new HtmlToken(HtmlTokenType.TagChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));

            if (SyntaxUtils.IsIdentStart(Buffer.Peek()))
            {
                Buffer.StartLexeme();
                while (!Buffer.IsEOF)
                {
                    if (SyntaxUtils.IsIdent(Buffer.Peek()))
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
                    char c = Buffer.Peek();
                    if (!SyntaxUtils.IsIdentStart(c))
                        break;
                    else
                    {
                        Buffer.StartLexeme();
                        while (!Buffer.IsEOF)
                        {
                            if (SyntaxUtils.IsIdent(Buffer.Peek()))
                                Buffer.AdvanceChar();
                            else
                                break;
                        }
                        PushToken(new HtmlToken(HtmlTokenType.AttrName, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                        SkipWhitespaces(); // Allow whitespaces before =
                        if (Buffer.Peek() == '=')
                        {
                            Buffer.StartLexeme();
                            Buffer.AdvanceChar();
                            PushToken(new HtmlToken(HtmlTokenType.AttrChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
                            SkipWhitespaces(); // Allow whitespaces after =
                            if (Buffer.Peek() == '"' || Buffer.Peek() == '\'')
                            {
                                char quote = Buffer.Peek();
                                Buffer.StartLexeme();
                                Buffer.AdvanceChar();
                                while (!Buffer.IsEOF)
                                {
                                    char attrC = Buffer.Peek();
                                    if (attrC != quote && attrC != '\n')
                                        Buffer.AdvanceChar();
                                    else
                                        break;
                                }
                                if (Buffer.Peek() == quote)
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
            if (Buffer.Peek() == '/')
            {
                startTagToken.ChangeType(HtmlTokenType.MetaTagStartAndClose);
                Buffer.AdvanceChar();
                SkipWhitespaces(); // Allow whitespaces after /
            }

            SkipUntil('>');

            if (Buffer.Peek() == '>')
            {
                Buffer.StartLexeme();
                Buffer.AdvanceChar();
                PushToken(new HtmlToken(HtmlTokenType.TagChars, Buffer.LexemeStart, Buffer.LexemeWidth, true));
            }

            int tagLength = Buffer.StreamPosition - startTagToken.Index;
            startTagToken.ChangeLength(tagLength);
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces();
                switch (Buffer.Peek())
                {
                    case TextStream.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                PushToken(new HtmlToken(HtmlTokenType.EOF, Buffer.StreamOnePastEnd, 0, false));
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
            PushToken(new HtmlToken(HtmlTokenType.EOF, Buffer.StreamOnePastEnd, 0, false));
            return (false);
        }
    }
}
