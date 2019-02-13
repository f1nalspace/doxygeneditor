using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    abstract class BaseLexer<T> : IDisposable where T : BaseToken
    {
        internal readonly SlidingTextBuffer Buffer;
        private readonly List<T> _tokens = new List<T>();
        private IEnumerable<T> Tokens => _tokens;
        public bool HasTokens => _tokens.Count > 0;

        public BaseLexer(SourceBuffer source)
        {
            Buffer = new SlidingTextBuffer(source);
        }

        protected bool PushToken(T token)
        {
            _tokens.Add(token);
            return (true);
        }

        protected void ReplaceTokens(T startToken, T endToken, IEnumerable<T> newTokens)
        {
            int startIndex = _tokens.IndexOf(startToken);
            int endIndex = _tokens.IndexOf(endToken);

            Debug.Assert(startIndex <= endIndex);
            if (startIndex == endIndex)
                _tokens.RemoveAt(startIndex);
            else
            {
                int count = endIndex - startIndex;
                _tokens.RemoveRange(startIndex, count);
            }
            _tokens.InsertRange(startIndex, newTokens);
        }

        protected abstract bool LexNext();

#if DEBUG
        private void RefreshDebugValues()
        {
            foreach (T token in _tokens)
            {
                token.DebugValue = Buffer.Source.GetText(token.Index, token.Length);
            }
        }
#endif

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
            while (!Buffer.IsEOF && char.IsWhiteSpace(Buffer.PeekChar()))
            {
                if (stopOnLinebreak)
                {
                    if ((Buffer.PeekChar() == '\n') || (Buffer.PeekChar() == '\r' && Buffer.PeekChar(1) == '\n'))
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
                if (Buffer.PeekChar() == c)
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
