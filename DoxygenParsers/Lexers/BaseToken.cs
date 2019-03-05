using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public abstract class BaseToken : TextRange
    {
        public bool IsComplete { get; private set; }
        public abstract bool IsEOF { get; }
        public abstract bool IsEndOfLine { get; }
        public abstract bool IsValid { get; }
        public abstract bool IsMarker { get; }
        public BaseToken() : base()
        {
        }
        public BaseToken(TextRange range, bool isComplete) : base(range)
        {
        }
        public void Set(TextRange range, bool isComplete)
        {
            base.Set(range);
            IsComplete = isComplete;
        }
        public override string ToString()
        {
            return $"{GetType().Name}, {base.ToString()}";
        }        
    }
}
