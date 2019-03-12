using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Lexers;
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

        class RootNode : BaseNode<TEntity>
        {
            public RootNode() : base(null, null)
            {
            }
        }

        public object Tag { get; }

        protected IEntityBaseNode<TEntity> Top { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        public BaseParser(object tag)
        {
            Tag = tag;
            Root = new RootNode();
        }
        protected void AddError(TextPosition pos, string message, string type, string symbol = null)
        {
            string category = GetType().Name;
            _parseErrors.Add(new TextError(pos, category, message, type, symbol) { Tag = this });
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
            var n = inNode;
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

        public abstract bool ParseToken(LinkedListStream<IBaseToken> stream);

        public void ParseTokens(IEnumerable<IBaseToken> tokens)
        {
            LinkedListStream<IBaseToken> tokenStream = new LinkedListStream<IBaseToken>(tokens);
            while (!tokenStream.IsEOF)
            {
                IBaseToken old = tokenStream.CurrentValue;
                if (!typeof(TToken).Equals(old.GetType()))
                {
                    tokenStream.Next();
                    continue;
                }
                if (!ParseToken(tokenStream))
                    tokenStream.Next();
                Debug.Assert(old != tokenStream.CurrentValue);
            }
        }

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

        public virtual void Dispose()
        {
        }
    }
}
