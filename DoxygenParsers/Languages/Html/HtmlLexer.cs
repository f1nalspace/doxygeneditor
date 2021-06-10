using System;
using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Html
{
    public class HtmlLexer : BaseLexer<HtmlToken>
    {
        class HtmlLexerState : State
        {
            public override void StartLex(TextStream stream)
            {
            }
        }

        protected override State CreateState()
        {
            return new HtmlLexerState();
        }

        public HtmlLexer(string source, string filename, TextPosition pos, int length) : base(source, filename, pos, length)
        {
        }

        private void LexTag()
        {
            Debug.Assert(Buffer.Peek() == '<');
            Buffer.StartLexeme();

            HtmlToken startTagToken = HtmlTokenPool.Make(HtmlTokenKind.MetaTagStart, Buffer.LexemeRange, false);
            PushToken(startTagToken);

            bool allowAttributes = true;
            Buffer.AdvanceColumn();
            if (Buffer.Peek() == '/')
            {
                startTagToken.ChangeKind(HtmlTokenKind.MetaTagClose);
                Buffer.AdvanceColumn();
                allowAttributes = false;
            }
            PushToken(HtmlTokenPool.Make(HtmlTokenKind.TagChars, Buffer.LexemeRange, true));

            if (SyntaxUtils.IsIdentStart(Buffer.Peek()))
            {
                Buffer.StartLexeme();
                while (!Buffer.IsEOF)
                {
                    if (SyntaxUtils.IsIdentPart(Buffer.Peek()))
                        Buffer.AdvanceColumn();
                    else
                        break;
                }
                PushToken(HtmlTokenPool.Make(HtmlTokenKind.TagName, Buffer.LexemeRange, true));
            }

            if (allowAttributes)
            {
                while (!Buffer.IsEOF)
                {
                    Buffer.SkipWhitespaces();
                    char c = Buffer.Peek();
                    if (!SyntaxUtils.IsIdentStart(c))
                        break;
                    else
                    {
                        Buffer.StartLexeme();
                        while (!Buffer.IsEOF)
                        {
                            if (SyntaxUtils.IsIdentPart(Buffer.Peek()))
                                Buffer.AdvanceColumn();
                            else
                                break;
                        }
                        PushToken(HtmlTokenPool.Make(HtmlTokenKind.AttrName, Buffer.LexemeRange, true));
                        Buffer.SkipWhitespaces(); // Allow whitespaces before =
                        if (Buffer.Peek() == '=')
                        {
                            Buffer.StartLexeme();
                            Buffer.AdvanceColumn();
                            PushToken(HtmlTokenPool.Make(HtmlTokenKind.AttrChars, Buffer.LexemeRange, true));
                            Buffer.SkipWhitespaces(); // Allow whitespaces after =
                            if (Buffer.Peek() == '"' || Buffer.Peek() == '\'')
                            {
                                char quote = Buffer.Peek();
                                Buffer.StartLexeme();
                                Buffer.AdvanceColumn();
                                while (!Buffer.IsEOF)
                                {
                                    char attrC = Buffer.Peek();
                                    if (attrC != quote && attrC != '\n')
                                        Buffer.AdvanceColumn();
                                    else
                                        break;
                                }
                                if (Buffer.Peek() == quote)
                                    Buffer.AdvanceColumn();
                                PushToken(HtmlTokenPool.Make(HtmlTokenKind.AttrValue, Buffer.LexemeRange, true));
                            }
                        }
                        else
                            break;
                    }
                }
            }

            Buffer.SkipWhitespaces(); // Allow whitespaces before /
            if (Buffer.Peek() == '/')
            {
                startTagToken.ChangeKind(HtmlTokenKind.MetaTagStartAndClose);
                Buffer.AdvanceColumn();
                Buffer.SkipWhitespaces(); // Allow whitespaces after /
            }

            Buffer.SkipUntil('>');

            if (Buffer.Peek() == '>')
            {
                Buffer.StartLexeme();
                Buffer.AdvanceColumn();
                PushToken(HtmlTokenPool.Make(HtmlTokenKind.TagChars, Buffer.LexemeRange, true));
            }

            int tagLength = Buffer.StreamPosition - startTagToken.Index;
            startTagToken.ChangeLength(tagLength);
        }

        protected override bool LexNext(State state)
        {
            do
            {
                Buffer.SkipWhitespaces();
                switch (Buffer.Peek())
                {
                    case TextStream.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                PushToken(HtmlTokenPool.Make(HtmlTokenKind.EOF, new TextRange(Buffer.TextPosition, 0), false));
                                return (false);
                            }
                            else
                                Buffer.AdvanceColumn();
                        }
                        break;

                    case '<':
                        {
                            LexTag();
                            return (true);
                        }

                    default:
                        {
                            Buffer.AdvanceColumn();
                            break;
                        }
                }
            } while (!Buffer.IsEOF);
            PushToken(HtmlTokenPool.Make(HtmlTokenKind.EOF, new TextRange(Buffer.TextPosition, 0), false));
            return (false);
        }
    }
}
