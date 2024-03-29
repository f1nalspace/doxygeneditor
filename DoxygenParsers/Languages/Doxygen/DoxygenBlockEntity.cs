﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenBlockEntity : BaseEntity
    {
        public DoxygenBlockEntityKind Kind { get; }
        public override string Value { get; set; }
        public override string Id { get; set; }

        public class Parameter
        {
            public DoxygenToken Token { get; }
            public string Name { get; }
            public string Value { get; }
            public Parameter(DoxygenToken token, string name, string value)
            {
                Token = token;
                Name = name;
                Value = value;
            }
            public override string ToString()
            {
                return $"{Name} => {Value}";
            }
        }
        private readonly List<Parameter> _parameters = new List<Parameter>();
        public IEnumerable<Parameter> Parameters => _parameters;
        public void AddParameter(DoxygenToken token, string name, string value)
        {
            _parameters.Add(new Parameter(token, name, value));
        }
        public Parameter FindParameterByName(params string[] names)
        {
            Parameter param = _parameters.FirstOrDefault(p => names.Contains(p.Name));
            return (param);
        }
        public string GetParameterValue(params string[] names)
        {
            Parameter param = FindParameterByName(names);
            if (param != null)
                return (param.Value);
            return (null);
        }

        public override string DisplayName
        {
            get
            {
                if (Kind == DoxygenBlockEntityKind.Page && "mainpage".Equals(Id))
                    return ("Main");
                else {
                    string nameValue = GetParameterValue("name", "id");
                    string textValue = GetParameterValue("text", "title", "caption");
                    if (!string.IsNullOrWhiteSpace(textValue))
                        return textValue;
                    else if (!string.IsNullOrWhiteSpace(nameValue))
                        return nameValue;
                    else
                        return Id;

                }
            }
        }

        public DoxygenBlockEntity Group { get; set; }

        public DoxygenBlockEntity(DoxygenBlockEntityKind kind, TextRange range) : base(range)
        {
            Kind = kind;
        }
        public DoxygenBlockEntity(DoxygenBlockEntityKind kind, IBaseToken token) : this(kind, token.Range)
        {
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append($"{Kind}");
            if (!string.IsNullOrWhiteSpace(Id))
            {
                if (s.Length > 0) s.Append(", ");
                s.Append(Id);
                if (!string.IsNullOrWhiteSpace(Value))
                {
                    s.Append(", '");
                    s.Append(Value);
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
            DoxygenBlockEntity other = obj as DoxygenBlockEntity;
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
