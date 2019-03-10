using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public class TextRange : IEquatable<TextRange>
    {
        public TextPosition Position { get; private set; }
        public int Index => Position.Index;
        public int Length { get; protected set; }
        public string Value { get; set; }

        public int End
        {
            get
            {
                int result = Index + Math.Max(0, Length - 1);
                return (result);
            }
        }
        public TextRange()
        {
            Position = new TextPosition(-1);
            Length = 0;
        }
        public TextRange(TextPosition pos, int length)
        {
            Position = pos;
            Length = length;
        }
        public TextRange(TextRange other) : this(other.Position, other.Length)
        {
        }
        public void Set(TextRange range)
        {
            Position = range.Position;
            Length = range.Length;
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
