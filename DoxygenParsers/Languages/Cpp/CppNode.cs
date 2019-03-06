using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppNode : BaseNode<CppEntity>
    {
        public CppNode(IBaseNode parent, CppEntity entity) : base(parent, entity)
        {
        }
    }
}
