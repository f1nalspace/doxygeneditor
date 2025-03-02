using System;
using System.Diagnostics.CodeAnalysis;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public struct TextRange : IEquatable<TextRange>
    {
        public TextPosition Position { get; }
        public int Length { get; }

        public int Index => Position.Index;
        public int End => Index + Math.Max(0, Length - 1);

        public static TextRange Invalid => new TextRange(new TextPosition(-1), 0);

        public TextRange(TextPosition pos, int length)
        {
            Position = pos;
            Length = length;
        }

        public TextRange(TextRange other) : this(other.Position, other.Length)
        {
        }

        public bool InterectsWith(TextRange other)
        {
            bool result = (Index <= other.End) && (End >= other.Index);
            return (result);
        }

        public bool Equals(TextRange other) => (other.Index == Index) && (other.Length == Length);
        public override bool Equals([NotNullWhen(true)] object obj) => obj is TextRange range && Equals(range);
        public override int GetHashCode() => HashCode.Combine(Index, Length);
        public override string ToString() => $"{Position}, {Length}";
    }
}
