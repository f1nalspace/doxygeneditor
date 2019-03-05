using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenNode : BaseNode
    {
        public override bool ShowChildren => DoxygenParser.ShowChildrensSet.Contains(((DoxygenEntity)Entity).Type);

        public DoxygenNode(BaseNode parent, BaseEntity entity) : base(parent, entity)
        {
        }
    }
}
