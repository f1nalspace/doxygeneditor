using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public class BaseNode
    {
        public BaseNode Parent { get; }
        public int Level { get; }
        private readonly List<BaseNode> _children = new List<BaseNode>();
        public IEnumerable<BaseNode> Children
        {
            get { return _children; }
        }
        public BaseEntity Entity { get; }
        public virtual bool ShowChildren { get { return (false); } }

        public string FullId
        {
            get
            {
                List<string> ids = new List<string>();
                BaseNode p = this;
                while (p != null)
                {
                    if (!string.IsNullOrWhiteSpace(p.Entity.Id))
                        ids.Add(p.Entity.Id);
                    p = p.Parent;
                }
                ids.Reverse();
                string result = string.Join(".", ids);
                return (result);
            }
        }

        public BaseNode(BaseNode parent, BaseEntity entity)
        {
            Parent = parent;
            Level = parent != null ? parent.Level + 1 : 0;
            Entity = entity;
        }

        public void AddChild(BaseNode child)
        {
            _children.Add(child);
        }

        public BaseNode FindNodeByRange(TextRange range)
        {
            BaseNode found = _children.FirstOrDefault(n => n.Entity.EndRange.Equals(range));
            if (found != null)
                return (found);
            foreach (BaseNode child in _children)
            {
                BaseNode foundInChild = child.FindNodeByRange(range);
                if (foundInChild != null)
                    return (foundInChild);
            }
            return (null);
        }

        public override string ToString()
        {
            return $"{Level} -> {Entity} ({Entity.StartRange} - {Entity.EndRange}, {Entity.Length})";
        }
    }
}
