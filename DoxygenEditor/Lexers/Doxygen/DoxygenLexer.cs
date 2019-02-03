using System.Diagnostics;

namespace DoxygenEditor.Lexers.Doxygen
{
    class DoxygenLexer : BaseLexer<DoxygenToken>
    {
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

        private DoxygenToken LexIdentToken(DoxygenTokenType type)
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.PeekChar()));
            Buffer.Start();
            while (!Buffer.IsEOF)
            {
                if (SyntaxUtils.IsIdent(Buffer.PeekChar()))
                    Buffer.AdvanceChar();
                else
                    break;
            }
            DoxygenToken result = new DoxygenToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        private DoxygenToken LexCommandToken()
        {
            Debug.Assert(Buffer.PeekChar() == '@' || Buffer.PeekChar() == '\\');
            Buffer.Start();
            while (!Buffer.IsEOF)
            {
                if (SyntaxUtils.IsIdent(Buffer.PeekChar()))
                    Buffer.AdvanceChar();
                else
                    break;
            }
            int commandStart = Buffer.LexemeStart;
            int commandLen = Buffer.LexemeWidth;
            string command = Buffer.GetText(commandStart, commandLen);
            DoxygenToken result = new DoxygenToken(DoxygenTokenType.Command, commandStart, commandLen, true);
            return (result);
        }

        protected override DoxygenToken LexNext()
        {
            do
            {
                SkipWhitespaces(false);
                switch (Buffer.PeekChar())
                {
                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            return new DoxygenToken(DoxygenTokenType.Invalid, 0, 0, false);
                        }

                    case '@':
                    case '\\':
                        {
                            DoxygenToken token;
                            char n = Buffer.PeekChar(1);
                            if (SyntaxUtils.IsIdentStart(n))
                                token = LexCommandToken();
                            else
                                token = LexCharToken();
                            return (token);
                        }

                    default:
                        {
                            DoxygenToken token = LexCharToken();
                            return (token);
                        }
                }
            } while (!Buffer.IsEOF);
            return new DoxygenToken(DoxygenTokenType.EOF, 0, 0, false);
        }
    }
}
