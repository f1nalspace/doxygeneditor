using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public abstract class BaseToken
    {
        public bool IsComplete { get; private set; }
        public TextRange Range { get; set; }

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
        public string Value
        {
            get { return Range.Value; }
            set {
                TextRange newRange = new TextRange(Range.Position, Range.Length) { Value = value };
                Range = newRange;
            }
        }

        public abstract bool IsEOF { get; }
        public abstract bool IsEndOfLine { get; }
        public abstract bool IsValid { get; }
        public abstract bool IsMarker { get; }

        public BaseToken()
        {
            Range = new TextRange(new TextPosition(0), 0);
            IsComplete = false;
        }
        public BaseToken(TextRange range, bool isComplete)
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
            return $"{GetType().Name}, {base.ToString()}";
        }        
    }
}
