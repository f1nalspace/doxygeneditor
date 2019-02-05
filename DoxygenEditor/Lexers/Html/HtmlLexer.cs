namespace DoxygenEditor.Lexers.Html
{
    class HtmlLexer : BaseLexer<HtmlToken>
    {
        public HtmlLexer(SourceBuffer source) : base(source)
        {
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces(false);
                switch (Buffer.PeekChar())
                {
                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            return (PushToken(new HtmlToken(HtmlTokenType.Invalid, 0, 0, false)));
                        }

                    case '<':
                        {
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
            return (false);
        }
    }
}
