using System.Collections.Generic;

namespace DoxygenEditor.Lexers
{
    abstract class BaseLexer<T> where T : BaseToken
    {
        internal readonly SlidingTextBuffer Buffer;

        public BaseLexer(SourceBuffer source)
        {
            Buffer = new SlidingTextBuffer(source);
        }

        protected abstract T LexNext();

        public IEnumerable<T> Tokenize()
        {
            List<T> result = new List<T>();
            while (!Buffer.IsEOF)
            {
                T token = LexNext();
                if (token.IsEOF)
                    break;
                if (token.IsValid)
                    result.Add(token);
            }
            return (result);
        }

        protected void SkipWhitespaces(bool ignoreLinebreak)
        {
            while (!Buffer.IsEOF && char.IsWhiteSpace(Buffer.PeekChar()))
            {
                if (ignoreLinebreak)
                {
                    if (Buffer.PeekChar() == '\n')
                        break;
                }
                Buffer.AdvanceChar();
            }
        }
    }
}
