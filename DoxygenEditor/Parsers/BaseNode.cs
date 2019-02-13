using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Parsers
{
    abstract class BaseNode
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

        public override string ToString()
        {
            return $"{Level} -> {Entity} ({Entity.StartRange} - {Entity.EndRange})";
        }
    }
}
