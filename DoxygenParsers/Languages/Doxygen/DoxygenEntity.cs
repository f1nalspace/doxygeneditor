using System.Text;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenEntity : BaseEntity
    {
        public DoxygenEntityType Type { get; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public override string Id { get; }
        public override string DisplayName
        {
            get
            {
                if ((Type == DoxygenEntityType.Page) && string.IsNullOrWhiteSpace(Id))
                    return ("Main");
                else if (!string.IsNullOrWhiteSpace(Value))
                    return Value;
                else if (!string.IsNullOrWhiteSpace(Caption))
                    return Caption;
                else
                return Id;
            }
        }
        public DoxygenEntity Group { get; set; }
        public DoxygenEntity(DoxygenEntityType type, BaseToken token, string id) : base(token)
        {
            Type = type;
            Id = id;
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append($"{Type}");
            if (!string.IsNullOrWhiteSpace(Id))
            {
                if (s.Length > 0) s.Append(",");
                s.Append(Id);
                if (!string.IsNullOrWhiteSpace(Caption))
                {
                    s.Append(", '");
                    s.Append(Caption);
                    s.Append("'");
                }
            }
            if (Group != null)
            {
                if (s.Length > 0)
                    s.Append(", ");
                s.Append($"[Group: {Group.Id}]");
            }
            return (s.ToString());
        }

        public override int CompareTo(object obj)
        {
            DoxygenEntity other = obj as DoxygenEntity;
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
