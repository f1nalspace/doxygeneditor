using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public interface IBaseToken
    {
        int Index { get; }
        int End { get; }
        TextPosition Position { get; }

        bool IsEOF { get; }
        bool IsEndOfLine { get; }
        bool IsValid { get; }
        bool IsMarker { get; }

        TextRange Range { get; set; }
        string Value { get; set; }
        bool IsComplete { get; set; }
        int Length { get; set; }

#if false
        public bool IsComplete { get; private set; }
        public TextRange Range { get; set; }
        public string Value { get; set; }

        public int Index => Range.Index;
        public int End => Range.End;
        public TextPosition Position => Range.Position;
        public int Length
        {
            get { return Range.Length; }
            set {
                TextRange newRange = new TextRange(Range.Position, value);
                Range = newRange;
            }
        }
        public abstract bool IsEOF { get; }
        public abstract bool IsEndOfLine { get; }
        public abstract bool IsValid { get; }
        public abstract bool IsMarker { get; }

        public IBaseToken()
        {
            Range = new TextRange(new TextPosition(0), 0);
            IsComplete = false;
        }
        public IBaseToken(TextRange range, bool isComplete)
        {
            Range = range;
            IsComplete = isComplete;
        }
        public void Set(TextRange range, bool isComplete)
        {
            Range = range;
            IsComplete = isComplete;
        }
        public override string ToString()
        {
            return $"{GetType().Name}, {base.ToString()} => {Value}";
        }
#endif
    }
}
