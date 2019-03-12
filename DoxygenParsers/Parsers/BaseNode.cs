using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseNode<TEntity> : IEntityBaseNode<TEntity> where TEntity : BaseEntity
    {
        public IBaseNode Parent { get; }
        public int Level { get; }
        private readonly List<IBaseNode> _children = new List<IBaseNode>();
        public IEnumerable<IBaseNode> Children => _children;
        public IEnumerable<BaseNode<TEntity>> TypedChildren => _children.Select(c => (BaseNode<TEntity>)c);
        public TEntity Entity { get; }
        public TextRange StartRange => Entity.StartRange;
        public TextRange EndRange => Entity.EndRange;
        public string Id => Entity.Id;
        public string Value => Entity.Value;
        public virtual bool ShowChildren => false;
        public string FullId
        {
            get
            {
                List<string> ids = new List<string>();
                IBaseNode prevNode = this;
                do
                {
                    if (!string.IsNullOrWhiteSpace(prevNode.Id))
                        ids.Add(prevNode.Id);
                    prevNode = prevNode.Parent;
                } while (prevNode != null);
                ids.Reverse();
                string result = string.Join(".", ids);
                return (result);
            }
        }

        public BaseNode(IBaseNode parent, TEntity entity)
        {
            Parent = parent;
            Level = parent != null ? parent.Level + 1 : 0;
            Entity = entity;
        }

        public void AddChild(IBaseNode child)
        {
            _children.Add(child);
        }

        public IBaseNode FindNodeByRange(TextRange range)
        {
            IBaseNode found = _children.FirstOrDefault(n => n.EndRange.Equals(range));
            if (found != null)
                return (found);
            foreach (IBaseNode child in _children)
            {
                IBaseNode foundInChild = child.FindNodeByRange(range);
                if (foundInChild != null)
                    return (foundInChild);
            }
            return (null);
        }

        public override string ToString()
        {
            return $"{Level} -> {Entity} ({Entity.StartRange} - {Entity.EndRange}, {Entity.Length})";
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return (-1);
            var t = obj.GetType();
            if (!typeof(IEntityBaseNode<TEntity>).IsAssignableFrom(t))
                return (-1);
            IEntityBaseNode<TEntity> a = (IEntityBaseNode<TEntity>)obj;
            return a.Entity.CompareTo(Entity);
        }
    }
}
