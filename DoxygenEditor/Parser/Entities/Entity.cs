using System.Collections.Generic;

namespace DoxygenEditor.Parser.Entities
{
    public abstract class Entity
    {
        public Entity Parent { get; protected set; }
        private readonly List<Entity> _children = new List<Entity>();
        public IEnumerable<Entity> Children { get { return _children; } }
        public SequenceInfo LineInfo { get; }
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
    }
}
