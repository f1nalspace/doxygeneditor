using System.Collections.Generic;

namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    class DoxygenNode : BaseNode
    {
        public override bool ShowChildren => DoxygenTree.AllowedChildren.Contains(((DoxygenEntity)Entity).Type);

        public DoxygenNode(BaseNode parent, int level, BaseEntity entity) : base(parent, level, entity)
        {
        }
    }
}
