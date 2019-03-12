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

        public abstract class State
        {
            public abstract void StartLex(TextStream stream);
        }
        protected abstract State CreateState();

        public BaseLexer(string source, TextPosition pos, int length)
        {
            Buffer = new BasicTextStream(source, pos, length);
        }

        protected void AddError(TextPosition pos, string message, string type, string symbol = null)
        {
            string category = GetType().Name;
            _lexErrors.Add(new TextError(pos, category, message, type, symbol) { Tag = this });
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
        protected void RemoveToken(T token)
        {
            _tokens.Remove(token);
        }

        protected abstract bool LexNext(State state);

        public IEnumerable<T> Tokenize()
        {
            _tokens.Clear();
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
            return (_tokens);
        }      

        public void Dispose()
        {
            Buffer.Dispose();
            _tokens.Clear();
        }
    }
}
