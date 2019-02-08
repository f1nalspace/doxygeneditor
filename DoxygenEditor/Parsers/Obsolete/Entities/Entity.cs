using System;
using System.Collections.Generic;
using System.Linq;

namespace TSP.DoxygenEditor.Parsers.Obsolete.Entities
{
    public abstract class Entity : IComparable
    {
        public Entity Parent { get; protected set; }
        private readonly List<Entity> _children = new List<Entity>();
        public IEnumerable<Entity> Children { get { return _children; } }
        public SequenceInfo LineInfo { get; }
        public abstract string Id { get; }
        public abstract string DisplayName { get; }

        public string FullId
        {
            get
            {
                List<string> ids = new List<string>();
                Entity p = this;
                while (p != null)
                {
                    if (!string.IsNullOrEmpty(p.Id))
                        ids.Add(p.Id);
                    p = p.Parent;
                }
                ids.Reverse();
                string result = string.Join(".", ids);
                return (result);
            }
        }

        public Entity(SequenceInfo lineInfo)
        {
            LineInfo = lineInfo;
            Parent = null;
        }
        public void AddChild(Entity child)
        {
            child.Parent = this;
            _children.Add(child);
        }
        public void Clear()
        {
            foreach (var child in _children)
                child.Clear();
            _children.Clear();
        }

        public T FindChildByType<T>() where T : Entity
        {
            Type t = typeof(T);
            Entity match = _children.FirstOrDefault(f => t.Equals(f.GetType()));
            T result = (T)match;
            return (result);
        }

        public T FindChildByExpression<T>(Func<T, bool> func) where T : Entity
        {
            Type t = typeof(T);
            Entity match = _children.Where(f => t.Equals(f.GetType())).FirstOrDefault(f => func((T)f));
            T result = (T)match;
            return (result);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return (-1);
            if (!GetType().Equals(obj.GetType()))
                return (-1);
            Entity e = (Entity)obj;
            int result = string.Compare(e.FullId, FullId);
            return (result);
        }
    }
}
