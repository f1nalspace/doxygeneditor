using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public abstract class BaseLexer<T> : IDisposable where T : IBaseToken
    {
        internal readonly TextStream Buffer;
        private readonly List<T> _tokens = new List<T>();
        private readonly List<TextError> _lexErrors = new List<TextError>();
        protected IEnumerable<T> Tokens => _tokens;
        public bool HasTokens => _tokens.Count > 0;
        public IEnumerable<TextError> LexErrors => _lexErrors;

        public BaseLexer(string source, TextPosition pos, int length)
        {
            Buffer = new BasicTextStream(source, pos, length);
        }

        protected void PushError(TextPosition pos, string message, string type, string symbol = null)
        {
            string category = GetType().Name;
            _lexErrors.Add(new TextError(pos, category, message, type, symbol));
        }

        protected bool PushToken(T token)
        {
            token.Value = Buffer.GetSourceText(token.Index, token.Length);
            var lastToken = _tokens.LastOrDefault();
            if (lastToken != null)
                Debug.Assert(token.Index >= lastToken.End);
            _tokens.Add(token);
            return (true);
        }

        protected abstract bool LexNext();

        public IEnumerable<T> Tokenize()
        {
            _tokens.Clear();
            do
            {
                int p = Buffer.StreamPosition;
                bool r = LexNext();
                if (!r)
                    break;
                else
                    Debug.Assert(Buffer.StreamPosition > p);
            } while (!Buffer.IsEOF);
            return (_tokens);
        }

        public enum SkipType
        {
            Single,
            All
        }

        protected void SkipAllWhitespaces()
        {
            do
            {
                char c0 = Buffer.Peek();
                char c1 = Buffer.Peek(1);
                if (c0 == TextStream.InvalidCharacter)
                    break;
                else if (c0 == '\t')
                    Buffer.AdvanceTab();
                else if (SyntaxUtils.IsLineBreak(c0))
                {
                    int nb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    Buffer.AdvanceLine(nb);
                }
                else if (char.IsWhiteSpace(c0))
                    Buffer.AdvanceColumn();
                else
                    break;
            } while (!Buffer.IsEOF);
        }
        protected void SkipSpacings(SkipType type)
        {
            do
            {
                char c = Buffer.Peek();
                if (c == TextStream.InvalidCharacter)
                    break;
                else if (c == '\t')
                    Buffer.AdvanceTab();
                else if (SyntaxUtils.IsSpacing(c))
                    Buffer.AdvanceColumn();
                else
                    break;
            } while (!Buffer.IsEOF && type == SkipType.All);
        }
        protected void SkipLineBreaks(SkipType type)
        {
            do
            {
                char c0 = Buffer.Peek();
                char c1 = Buffer.Peek(1);
                if (c0 == TextStream.InvalidCharacter)
                    break;
                else if (SyntaxUtils.IsLineBreak(c0))
                {
                    int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    Buffer.AdvanceLine(lb);
                }
                else break;
            } while (!Buffer.IsEOF && type == SkipType.All);
        }

        protected void SkipUntil(char c)
        {
            while (!Buffer.IsEOF)
            {
                if (Buffer.Peek() == c)
                    break;
                Buffer.AdvanceAuto();
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
            _tokens.Clear();
        }
    }
}
