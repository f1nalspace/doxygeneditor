using System.Collections.Generic;
using System.Diagnostics;

namespace DoxygenEditor.Lexers
{
    abstract class BaseLexer<T> where T : BaseToken
    {
        internal readonly SlidingTextBuffer Buffer;
        private readonly List<T> _tokens = new List<T>();
        private IEnumerable<T> Tokens => _tokens;

        public BaseLexer(SourceBuffer source)
        {
            Buffer = new SlidingTextBuffer(source);
        }

        protected bool PushToken(T token)
        {
            _tokens.Add(token);
            return (true);
        }

        protected abstract bool LexNext();

        public IEnumerable<T> Tokenize()
        {
            _tokens.Clear();
            while (!Buffer.IsEOF)
            {
                int p = Buffer.Position;
                bool r = LexNext();
                if (!r)
                    break;
                else
                    Debug.Assert(Buffer.Position > p);
            }
            return (_tokens);
        }

        protected int SkipWhitespaces(bool ignoreLinebreak)
        {
            int result = 0;
            while (!Buffer.IsEOF && char.IsWhiteSpace(Buffer.PeekChar()))
            {
                if (ignoreLinebreak)
                {
                    if (Buffer.PeekChar() == '\n')
                        break;
                }
                Buffer.AdvanceChar();
                ++result;
            }
            return (result);
        }
    }
}
