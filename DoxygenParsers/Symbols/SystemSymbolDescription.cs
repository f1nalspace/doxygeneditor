using TSP.DoxygenEditor.Languages;

namespace TSP.DoxygenEditor.Symbols
{
    public readonly struct SystemSymbolDescription
    {
        public LanguageKind Language { get; }
        public SystemSymbolKind Kind { get; }
        public string Name { get; }
        public string Value { get; }

        public SystemSymbolDescription(LanguageKind lang, SystemSymbolKind kind, string name, string value = null)
        {
            Language = lang;
            Kind = kind;
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            if (Value != null)
                return $"{Kind} => {Name} => {Value} [{Language}]";
            else
                return $"{Kind} => {Name} [{Language}]";
        }
    }
}
