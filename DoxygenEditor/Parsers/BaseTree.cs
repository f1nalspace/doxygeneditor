using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lists;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    abstract class BaseTree : BaseNode, IDisposable
    {
        public delegate string GetTextRangeEventHandler(int index, int length);
        public event GetTextRangeEventHandler GetTextRange;

        private readonly Stack<BaseNode> _stack = new Stack<BaseNode>();

        public BaseTree() : base(null, null)
        {
        }

        protected string GetText(int index, int length)
        {
            string text = GetTextRange?.Invoke(index, length);
            return (text);
        }
        protected string GetText(TextRange range)
        {
            string text = GetText(range.Index, range.Length);
            return (text);
        }

        public abstract bool ParseToken(LinkedListStream<BaseToken> stream);

        protected BaseNode Root { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        protected void Add(BaseNode node)
        {
            if (_stack.Count == 0)
                AddChild(node);
            else
                Root.AddChild(node);
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
