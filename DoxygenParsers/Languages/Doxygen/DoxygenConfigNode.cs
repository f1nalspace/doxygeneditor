using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenConfigNode : BaseNode<DoxygenConfigEntity>
    {
        public override bool ShowChildren => false;

        public DoxygenConfigNode(IBaseNode parent, DoxygenConfigEntity entity) : base(parent, entity)
        {
        }
    }
}
