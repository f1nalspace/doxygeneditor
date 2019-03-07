using System;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseParser<TEntity> : IDisposable where TEntity : BaseEntity
    {
        private readonly Stack<IEntityBaseNode<TEntity>> _stack = new Stack<IEntityBaseNode<TEntity>>();

        private readonly List<TextError> _parseErrors = new List<TextError>();
        public IEnumerable<TextError> ParseErrors => _parseErrors;
        public IBaseNode Root { get; }

        class RootNode : BaseNode<TEntity>
        {
            public RootNode() : base(null, null)
            {
            }
        }

        public object Tag { get; }

        public BaseParser(object tag)
        {
            Tag = tag;
            Root = new RootNode();
        }
        protected void AddParseError(TextPosition pos, string message)
        {
            string category = GetType().Name;
            _parseErrors.Add(new TextError(pos, category, message));
        }

        protected enum SearchMode
        {
            Current,
            Next,
            Forward,
            Prev,
            Backward,
        }

        protected class SearchResult<TToken>
        {
            public LinkedListNode<BaseToken> Node { get; }
            public TToken Token { get; }
            public SearchResult(LinkedListNode<BaseToken> node, TToken token)
            {
                Node = node;
                Token = token;
            }
        }

        protected SearchResult<TToken> Search<TToken>(LinkedListNode<BaseToken> inNode, SearchMode mode, Func<TToken, bool> matchFunc) where TToken : BaseToken
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
                    case SearchMode.Backward:
                        n = n.Previous;
                        break;
                    case SearchMode.Next:
                    case SearchMode.Forward:
                        n = n.Next;
                        break;
                }
                if (n != null)
                {
                    TToken token = n.Value as TToken;
                    if (token != null)
                    {
                        if (matchFunc(token))
                            return new SearchResult<TToken>(n, token);
                    }
                }
            } while (n != null && canTravel);
            return (null);
        }

        public abstract bool ParseToken(LinkedListStream<BaseToken> stream);

        protected IEntityBaseNode<TEntity> Top { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        protected void Add(IBaseNode node)
        {
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
