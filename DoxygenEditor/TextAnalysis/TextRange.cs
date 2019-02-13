using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class TextRange : IEquatable<TextRange>
    {
        public int Index { get; }
        public int Length { get; protected set; }

#if DEBUG
        public string DebugValue { get; set; }
#endif

        public int End
        {
            get
            {
                int result = Index + Math.Max(0, Length - 1);
                return (result);
            }
        }

        public TextRange(int index, int length)
        {
            Index = index;
            Length = length;
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
#if DEBUG
            return $"{Index}, {Length} => {DebugValue}";
#else
            return $"{Index}, {Length}";
#endif
        }
    }
}
