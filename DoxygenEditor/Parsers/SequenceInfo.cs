using System;

namespace DoxygenEditor.Parsers
{
    public struct SequenceInfo
    {
        public int Line;
        public int Start;
        public int Length;

        public int End
        {
            get
            {
                if (Length > 0)
                    return Start + (Length - 1);
                return Start;
            }
        }

        public bool IntersectsWith(SequenceInfo other)
        {
            if (End < other.Start)
                return (false);
            if (Start > other.End)
                return (false);
            return (true);
        }
    }
}
