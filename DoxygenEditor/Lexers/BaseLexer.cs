using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    abstract class BaseLexer<T> : IDisposable where T : BaseToken
    {
        internal readonly TextStream Buffer;
        private readonly List<T> _tokens = new List<T>();
        private IEnumerable<T> Tokens => _tokens;
        public bool HasTokens => _tokens.Count > 0;

        public BaseLexer(string source, int sbase, int length)
        {
            Buffer = new SlidingTextStream(source, sbase, length);
        }

        protected bool PushToken(T token)
        {
            _tokens.Add(token);
            return (true);
        }

        protected abstract bool LexNext();

#if DEBUG
        private void RefreshDebugValues()
        {
            foreach (T token in _tokens)
            {
                token.DebugValue = Buffer.GetStreamText(token.Index, token.Length);
            }
        }
#endif

        public IEnumerable<T> Tokenize()
        {
            _tokens.Clear();
            while (!Buffer.IsEOF)
            {
                int p = Buffer.StreamPosition;
                bool r = LexNext();
                if (!r)
                    break;
                else
                    Debug.Assert(Buffer.StreamPosition > p);
            }

            if (_tokens.Count == 1)
            {
                T token = _tokens[0];
                if (token.IsEOF)
                    _tokens.Clear();
            }

#if DEBUG
            RefreshDebugValues();
#endif

            return (_tokens);
        }

        protected int SkipWhitespaces(bool stopOnLinebreak = false)
        {
            int result = 0;
            while (!Buffer.IsEOF && char.IsWhiteSpace(Buffer.Peek()))
            {
                if (stopOnLinebreak)
                {
                    char c0 = Buffer.Peek();
                    char c1 = Buffer.Peek(1);
                    if ((c0 == '\r' && c1 == '\n') || (c0 == '\n') || (c0 == '\r'))
                        break;
                }
                Buffer.AdvanceChar();
                ++result;
            }
            return (result);
        }

        protected int SkipUntil(char c)
        {
            int result = 0;
            while (!Buffer.IsEOF)
            {
                if (Buffer.Peek() == c)
                    break;
                Buffer.AdvanceChar();
                ++result;
            }
            return (result);
        }

        public void Dispose()
        {
            Buffer.Dispose();
            _tokens.Clear();
        }
    }
}
