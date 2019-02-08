using System;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    abstract class BaseToken : TextRange
    {
        public bool IsComplete { get; }
        public abstract bool IsEOF { get; }
        public abstract bool IsValid { get; }
        public BaseToken(int index, int length, bool isComplete) : base(index, length)
        {
            IsComplete = isComplete;
        }
        public override string ToString()
        {
            return $"{GetType()}, {base.ToString()}";
        }
    }
}
