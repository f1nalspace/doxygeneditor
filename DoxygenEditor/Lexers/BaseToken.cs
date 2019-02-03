using System;

namespace DoxygenEditor.Lexers
{
    public abstract class BaseToken
    {
        public int Index { get; private set; }
        public int Length { get; private set; }
        public bool IsComplete { get; }

        public int End
        {
            get
            {
                int result = Index + Math.Max(0, Length - 1);
                return (result);
            }
        }

        public BaseToken(int index, int length, bool isComplete)
        {
            Index = index;
            Length = length;
            IsComplete = isComplete;
        }

        public bool InterectsWith(BaseToken other)
        {
            bool result = (Index <= other.End) && (End >= other.Index);
            return (result);
        }
    }
}
