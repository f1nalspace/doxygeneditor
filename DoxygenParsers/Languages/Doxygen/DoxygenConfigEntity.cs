using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenConfigEntity : BaseEntity
    {
        public DoxygenConfigEntityKind Kind { get; }
        public override string Value { get; set; }
        public override string Id { get; set; }

        private readonly List<string> _settings = new List<string>();
        public IEnumerable<string> Settings => _settings;
        public void AddSettingsValue(string value)
        {
            _settings.Add(value);
        }

        public override string DisplayName
        {
            get
            {
                return Id;
            }
        }

        public DoxygenConfigEntity(DoxygenConfigEntityKind kind, TextRange range) : base(range)
        {
            Kind = kind;
        }
        public DoxygenConfigEntity(DoxygenConfigEntityKind kind, IBaseToken token) : this(kind, token.Range)
        {
        }

        public override int CompareTo(object obj)
        {
            DoxygenConfigEntity other = obj as DoxygenConfigEntity;
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
