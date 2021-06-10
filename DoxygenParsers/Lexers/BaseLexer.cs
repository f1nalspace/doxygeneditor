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

        public enum LexIntern
        {
            Normal,
            Intern,
        }

        public virtual bool IsFullParser => false;

        public abstract class State
        {
            public abstract void StartLex(TextStream stream);
        }
        protected abstract State CreateState();

        public BaseLexer(string source, string filePath, TextPosition pos, int length)
        {
            Buffer = new BasicTextStream(source, filePath, pos, length);
        }

        protected void AddError(TextPosition pos, string message, string what, string symbol = null)
        {
            string category = GetType().Name;
            _lexErrors.Add(new TextError(pos, category, message, what, symbol) { Tag = this });
        }

        protected bool PushToken(T token, LexIntern intern = LexIntern.Normal)
        {
            string value = Buffer.GetSourceText(token.Index, token.Length);
            if ((intern == LexIntern.Intern) && !string.IsNullOrWhiteSpace(value))
                token.Value = string.Intern(value);
            else
                token.Value = value;
            T lastToken = _tokens.LastOrDefault();
            if (lastToken != null && !lastToken.Equals(token))
                Debug.Assert(token.Index >= lastToken.End);
            _tokens.Add(token);
            return (true);
        }
        protected void RemoveToken(T token)
        {
            _tokens.Remove(token);
        }

        protected abstract bool LexNext(State state);

        protected virtual void LexFull() { }

        public IEnumerable<T> Tokenize()
        {
            _tokens.Clear();
            if (IsFullParser)
                LexFull();
            else
            {
                State state = CreateState();
                do
                {
                    int p = Buffer.StreamPosition;
                    state.StartLex(Buffer);
                    bool r = LexNext(state);
                    if (!r)
                        break;
                    else
                        Debug.Assert(Buffer.StreamPosition > p);
                } while (!Buffer.IsEOF);
            }
            Debug.Assert(Buffer.IsEOF);
            return (_tokens);
        }

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
            Buffer.Dispose();
            _tokens.Clear();
        }
        protected virtual void DisposeUnmanaged()
        {
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
                DisposeManaged();
            DisposeUnmanaged();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~BaseLexer()
        {
            Dispose(false);
        }
        #endregion
    }
}
