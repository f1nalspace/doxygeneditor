using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class TextRange
    {
        public int Index { get; }
        public int Length { get; set; }

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

        public override string ToString()
        {
            return $"{Index}, {Length}";
        }
    }
}
