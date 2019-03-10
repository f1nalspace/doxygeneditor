using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public struct TextRange : IEquatable<TextRange>
    {
        public TextPosition Position { get; }
        public int Length { get; }
        public string Value { get; set; }

        public int Index => Position.Index;
        public int End => Index + Math.Max(0, Length - 1);
        public TextRange(TextPosition pos, int length)
        {
            Position = pos;
            Length = length;
            Value = null;
        }
        public TextRange(TextRange other) : this(other.Position, other.Length)
        {
        }
        public bool InterectsWith(TextRange other)
        {
            bool result = (Index <= other.End) && (End >= other.Index);
            return (result);
        }

        public bool Equals(TextRange other)
        {
            bool result = (other.Index == Index) && (other.Length == Length);
            return (result);
        }

        public override int GetHashCode()
        {
            int result = Index.GetHashCode() ^ Length.GetHashCode();
            return (result);
        }

        public override string ToString()
        {
            return $"{Position}, {Length} => {Value}";
        }
    }
}
