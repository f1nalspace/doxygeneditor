using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenBlockNode : BaseNode<DoxygenBlockEntity>
    {
        public override bool ShowChildren => DoxygenBlockParser.ShowChildrensSet.Contains(((DoxygenBlockEntity)Entity).Kind);

        public DoxygenBlockNode(IBaseNode parent, DoxygenBlockEntity entity) : base(parent, entity)
        {
        }
    }
}
