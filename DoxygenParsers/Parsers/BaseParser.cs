using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseParser<TEntity, TToken> : IDisposable where TEntity : BaseEntity where TToken : BaseToken
    {
        private readonly Stack<IEntityBaseNode<TEntity>> _stack = new Stack<IEntityBaseNode<TEntity>>();

        private readonly List<TextError> _parseErrors = new List<TextError>();
        public IEnumerable<TextError> ParseErrors => _parseErrors;
        public IBaseNode Root { get; }
        public int TotalNodeCount { get; private set; }

        public SymbolTable LocalSymbolTable { get; }

        public LanguageKind Language { get; }

        class RootNode : BaseNode<TEntity>
        {
            public RootNode() : base(null, null)
            {
            }
        }

        protected IEntityBaseNode<TEntity> Top { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        public BaseParser(ISymbolTableId id, LanguageKind language)
        {
            Root = new RootNode();
            LocalSymbolTable = new SymbolTable(id);
            Language = language;
        }
        protected void AddError(TextPosition pos, string message, string type, string symbol = null)
        {
            string category = GetType().Name;
            _parseErrors.Add(new TextError(Language, pos, category, message, type, symbol));
        }

        protected enum SearchMode
        {
            Current,
            Next,
            Forward,
            Prev,
            Backward,
        }

        protected class SearchResult<T>
        {
            public LinkedListNode<IBaseToken> Node { get; }
            public T Token { get; }
            public SearchResult(LinkedListNode<IBaseToken> node, T token)
            {
                Node = node;
                Token = token;
            }
        }

        protected SearchResult<TToken> Search(LinkedListNode<IBaseToken> inNode, SearchMode mode, Func<TToken, bool> matchFunc)
        {
            bool canTravel = (mode == SearchMode.Forward || mode == SearchMode.Backward);
            LinkedListNode<IBaseToken> n = inNode;
            do
            {
                if (n == null)
                    break;
                switch (mode)
                {
                    case SearchMode.Current:
                        break;
                    case SearchMode.Prev:
                        n = n.Previous;
                        break;
                    case SearchMode.Next:
                        n = n.Next;
                        break;
                }
                if (n != null)
                {
                    Type type = n.Value.GetType();
                    if (typeof(TToken).Equals(type))
                    {
                        TToken token = (TToken)n.Value;
                        if (matchFunc(token))
                            return new SearchResult<TToken>(n, token);
                    }
                    switch (mode)
                    {
                        case SearchMode.Backward:
                            n = n.Previous;
                            break;
                        case SearchMode.Forward:
                            n = n.Next;
                            break;
                    }
                }
            } while (n != null && canTravel);
            return (null);
        }

        public enum ParseTokenResult
        {
            ReadNext,
            AlreadyAdvanced,
        }

        protected abstract ParseTokenResult ParseToken(string source, LinkedListStream<IBaseToken> stream);

        public void ParseTokens(string source, IEnumerable<IBaseToken> tokens)
        {
            IEnumerable<IBaseToken> filteredTokens = FilterTokens(tokens);
            LinkedListStream<IBaseToken> tokenStream = new LinkedListStream<IBaseToken>(filteredTokens);
            while (!tokenStream.IsEOF)
            {
                IBaseToken old = tokenStream.CurrentValue;
                if (!typeof(TToken).Equals(old.GetType()))
                {
                    tokenStream.Next();
                    continue;
                }
                ParseTokenResult tokResult = ParseToken(source, tokenStream);
                if (tokResult == ParseTokenResult.ReadNext)
                    tokenStream.Next();
                else
                    Debug.Assert(old != tokenStream.CurrentValue);
            }
            Finished(filteredTokens);
        }

        public virtual IEnumerable<IBaseToken> FilterTokens(IEnumerable<IBaseToken> tokens) { return tokens; }
        public virtual void Finished(IEnumerable<IBaseToken> tokens) { }

        protected void Add(IBaseNode node)
        {
            ++TotalNodeCount;
            if (_stack.Count == 0)
                Root.AddChild(node);
            else
                Top.AddChild(node);
        }

        protected void Push(IEntityBaseNode<TEntity> node)
        {
            Add(node);
            _stack.Push(node);
        }

        protected IBaseNode Pop()
        {
            IBaseNode result = _stack.Pop();
            return (result);
        }

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
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
        ~BaseParser()
        {
            Dispose(false);
        }
        #endregion
    }
}
