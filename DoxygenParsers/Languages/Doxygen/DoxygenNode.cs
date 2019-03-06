using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenNode : BaseNode<DoxygenEntity>
    {
        public override bool ShowChildren => DoxygenParser.ShowChildrensSet.Contains(((DoxygenEntity)Entity).Kind);

        public DoxygenNode(IBaseNode parent, DoxygenEntity entity) : base(parent, entity)
        {
        }
    }
}
