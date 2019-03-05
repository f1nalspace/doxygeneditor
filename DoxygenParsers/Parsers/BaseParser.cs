using System;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseParser : IDisposable
    {
        private readonly Stack<BaseNode> _stack = new Stack<BaseNode>();

        private readonly List<TextError> _parseErrors = new List<TextError>();
        public IEnumerable<TextError> ParseErrors => _parseErrors;
        public BaseNode Root { get; }

        public BaseParser()
        {
            Root = new BaseNode(null, null);
        }
        protected void AddParseError(TextPosition pos, string message)
        {
            string category = GetType().Name;
            _parseErrors.Add(new TextError(pos, category, message));
        }

        public abstract bool ParseToken(LinkedListStream<BaseToken> stream);

        protected BaseNode Top { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        protected void Add(BaseNode node)
        {
            if (_stack.Count == 0)
                Root.AddChild(node);
            else
                Top.AddChild(node);
        }

        protected void Push(BaseNode node)
        {
            Add(node);
            _stack.Push(node);
        }

        protected BaseNode Pop()
        {
            BaseNode result = _stack.Pop();
            return (result);
        }

        public virtual void Dispose()
        {
        }
    }
}
