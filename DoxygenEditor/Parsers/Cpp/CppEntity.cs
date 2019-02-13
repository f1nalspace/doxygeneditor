using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers.Cpp
{
    class CppEntity : BaseEntity
    {
        public string Ident { get; }
        public string Value { get; set; }
        public CppEntityType Type { get; }

        public override string Id => Ident;
        public override string DisplayName => Value;

        public BaseNode DocumentationNode { get; set; }

        public CppEntity(CppEntityType type, BaseToken range, string ident) : base(range)
        {
            Type = type;
            Ident = ident;
        }

        public override int CompareTo(object obj)
        {
            CppEntity other = obj as CppEntity;
            if (other != null)
            {
                if (other.Type != Type)
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
