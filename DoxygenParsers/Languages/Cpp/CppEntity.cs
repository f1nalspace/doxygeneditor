using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppEntity : BaseEntity
    {
        public CppEntityKind Kind { get; }
        public override string Value { get; set; }
        public override string Id { get; set; }
        public override string DisplayName => Value;

        public IBaseNode DocumentationNode { get; set; }

        public CppEntity(CppEntityKind kind, BaseToken token, string ident) : base(token.Range)
        {
            Kind = kind;
            Id = ident;
        }

        public override int CompareTo(object obj)
        {
            CppEntity other = obj as CppEntity;
            if (other != null)
            {
                if (other.Kind != Kind)
                    return (1);
                int r = string.Compare(other.Id, Id);
                if (r != 0)
                    return (r);
                return (0);
            }
            return (-1);
        }
    }
}
