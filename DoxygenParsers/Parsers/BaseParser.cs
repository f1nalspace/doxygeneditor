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
