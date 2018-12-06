using System;

namespace DoxygenEditor.Parser
{
    public struct SequenceInfo
    {
        public int Line;
        public int Start;
        public int Length;

        public SequenceInfo Offset(int start, int length)
        {
            SequenceInfo r = new SequenceInfo();
            r.Start = Start + start;
            r.Length = length;
            r.Line = Line;
            return (r);
        }
    }
}
