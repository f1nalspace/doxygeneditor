using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppTypeValue
    {
        public TextRange StartRange { get; }
        public TextRange EndRange { get; }
        public string Value { get; }

        public CppTypeValue(TextRange startRange, TextRange endRange, string value)
        {
            StartRange = startRange;
            EndRange = endRange;
            Value = value;
        }

        public override string ToString() => Value;
    }

    public class CppFunctionArgument
    {
        public CppNode Parent { get; }
        public TextRange StartRange { get; }
        public TextRange EndRange { get; }
        public string Value { get; }
        public string Name { get; }
        public CppTypeValue Type { get; }

        public CppFunctionArgument(CppNode parent, TextRange startRange, TextRange endRange, string value, string name, CppTypeValue type)
        {
            Parent = parent;
            StartRange = startRange;
            EndRange = endRange;
            Name = name;
            Value = value;
            Type = type;
        }

        public override string ToString() => Value;
    }

    public class CppFunctionDefinition
    {
        public CppNode Node { get; }
        public TextRange StartRange { get; }
        public TextRange EndRange { get; }
        public ImmutableArray<CppFunctionArgument> Arguments { get; }
        public CppTypeValue ReturnType { get; }
        public string Ident { get; }

        public CppFunctionDefinition(CppNode node, TextRange startRange, TextRange endRange, IEnumerable<CppFunctionArgument> arguments, CppTypeValue returnType)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            StartRange = startRange;
            EndRange = endRange;
            Arguments = arguments?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(arguments));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            Ident = node.Id;
        }

        public override string ToString()
        {
            return $"{ReturnType}{(ReturnType.Value.EndsWith("*") ? "" : " ")}{Ident}({string.Join(", ", Arguments)})";
        }
    }

    public class CppMacroDefinition
    {
        public CppNode Node { get; }
        public TextRange StartRange { get; }
        public TextRange EndRange { get; }
        public ImmutableArray<CppFunctionArgument> Arguments { get; }
        public string Ident { get; }
        public bool NoBraces { get; }

        public CppMacroDefinition(CppNode node, TextRange startRange, TextRange endRange, IEnumerable<CppFunctionArgument> arguments, bool noBraces)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            StartRange = startRange;
            EndRange = endRange;
            Arguments = arguments?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(arguments));
            Ident = node.Id;
            NoBraces = noBraces;
        }

        public override string ToString()
        {
            if (NoBraces)
                return $"#define {Ident}";
            else
                    return $"#define {Ident}({string.Join(", ", Arguments)})";
        }
    }
}
