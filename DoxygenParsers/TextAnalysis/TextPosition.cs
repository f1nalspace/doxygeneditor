using System;
using System.Diagnostics.CodeAnalysis;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public struct TextPosition : IEquatable<TextPosition>
    {
        public int Index { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string LineInfo { get { return $"Line: {Line + 1}, Col: {Column + 1}"; } }

        public TextPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }
        public TextPosition(int index) : this(index, 0, 0)
        {
        }
        public TextPosition(TextPosition other) : this(other.Index, other.Line, other.Column)
        {
        }

        public static TextPosition Invalid => new TextPosition(-1);

        public string ToDisplayString()
        {
            return $"@{Index} -> (Line: {Line + 1}, Col: {Column + 1})";
        }

        public override string ToString()
        {
            return $"@{Index} -> ({LineInfo})";
        }

        public bool Equals(TextPosition other) => Index == other.Index && Line == other.Line && Column == other.Column;
        public override int GetHashCode() => HashCode.Combine(Index, Line, Column);
        public override bool Equals([NotNullWhen(true)] object obj) => obj is TextPosition textPos && Equals(textPos);
        public static bool operator ==(TextPosition left, TextPosition right) => left.Equals(right);
        public static bool operator !=(TextPosition left, TextPosition right) => !(left == right);
    }
}
