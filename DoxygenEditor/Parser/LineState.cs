using System;

namespace DoxygenEditor.Parser
{
    public class LineState
    {
        public SequenceInfo Info { get; }
        public string Value { get; }
        public int Offset { get; private set; }
        public int Length { get; set; }
        public char CurrentChar
        {
            get
            {
                return Value[Offset];
            }
        }
        public bool IsEndOfLine
        {
            get
            {
                bool result = !(Offset < Length);
                return (result);
            }
        }
        public string RemainingValue
        {
            get
            {
                int len = Length - Offset;
                string result = Value.Substring(Offset, len);
                return (result);
            }
        }
        public LineState(SequenceInfo info, string value)
        {
            Info = info;
            Value = value;
            Offset = 0;
            Length = value.Length;
        }
        public void IncOffset(int addon = 1)
        {
            if (Offset < Length - (addon - 1))
                Offset += addon;
            else
                throw new InvalidOperationException("End of line reached!");
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
