using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public abstract class BaseToken : IBaseToken
    {
        public TextRange Range { get; set; }
        public string Value { get; set; }
        public bool IsComplete { get; set; }
        public int Index => Range.Index;
        public int End => Range.End;
        public TextPosition Position => Range.Position;
        public int Length
        {
            get { return Range.Length; }
            set { Range = new TextRange(Range.Position, value); }
        }

        public abstract bool IsEOF { get; }
        public abstract bool IsEndOfLine { get; }
        public abstract bool IsValid { get; }
        public abstract bool IsMarker { get; }

        public BaseToken(TextRange range, bool isComplete)
        {
            Range = Range;
            IsComplete = isComplete;
            Value = null;
        }
        public BaseToken() : this(TextRange.Invalid, false)
        {
        }

        protected void Set(TextRange range, bool isComplete)
        {
            Range = range;
            IsComplete = isComplete;
        }

        public override string ToString()
        {
            return $"{Range} => {Value}";
        }
    }
}
