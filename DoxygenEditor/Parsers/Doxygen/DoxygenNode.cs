using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    class DoxygenNode : BaseNode
    {
        public override bool ShowChildren => DoxygenTree.ShowChildrensSet.Contains(((DoxygenEntity)Entity).Type);

        public DoxygenNode(BaseNode parent, BaseEntity entity) : base(parent, entity)
        {
        }
    }
}
