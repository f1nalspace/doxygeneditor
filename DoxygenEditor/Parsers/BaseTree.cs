using System;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lists;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    abstract class BaseTree : BaseNode, IDisposable
    {
        public delegate string GetTextRangeEventHandler(int index, int length);
        public event GetTextRangeEventHandler GetTextRange;

        public delegate int GetLinePosEventHandler(int position);
        public event GetLinePosEventHandler GetLinePos;

        private readonly Stack<BaseNode> _stack = new Stack<BaseNode>();
        private readonly List<BaseNode> _allNodes = new List<BaseNode>();

        public BaseTree() : base(null, null)
        {
        }
        protected int GetLine(int index)
        {
            if (GetLinePos == null)
                return (-1);
            int result = GetLinePos.Invoke(index);
            return (result);
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

        public BaseNode FindNodeByRange(TextRange range)
        {
            BaseNode result = _allNodes.FirstOrDefault(n => n.Entity.EndRange.Equals(range));
            return (result);
        }

        public abstract bool ParseToken(LinkedListStream<BaseToken> stream);

        protected BaseNode Root { get { return _stack.Count > 0 ? _stack.Peek() : null; } }

        protected void Add(BaseNode node)
        {
            if (_stack.Count == 0)
                AddChild(node);
            else
                Root.AddChild(node);
            _allNodes.Add(node);
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
