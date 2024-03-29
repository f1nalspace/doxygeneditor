﻿using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppEntity : BaseEntity
    {
        public CppEntityKind Kind { get; }
        public override string DisplayName => Value;
        public override string Value { get; set; }
        public override string Id { get; set; }

        public IBaseNode DocumentationNode { get; set; }

        public bool IsDefinition => (Kind != CppEntityKind.MacroMatch && Kind != CppEntityKind.MacroUsage);

        public CppEntity(CppEntityKind kind, IBaseToken token, string ident) : base(token != null ? token.Range : new TextRange())
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
