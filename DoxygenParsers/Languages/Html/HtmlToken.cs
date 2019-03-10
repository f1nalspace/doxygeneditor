using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Html
{
    public class HtmlToken : IBaseToken
    {
        public HtmlTokenKind Kind { get; private set; }
        public bool IsEOF => Kind == HtmlTokenKind.EOF;
        public bool IsValid => Kind != HtmlTokenKind.Invalid;
        public bool IsEndOfLine => false;
        public bool IsMarker => false;

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

        private HtmlToken(HtmlTokenKind kind, TextRange range, bool isComplete)
        {
            Kind = kind;
            Range = range;
            IsComplete = isComplete;
        }
        public HtmlToken() : this(HtmlTokenKind.Invalid, TextRange.Invalid, false)
        {
        }
        public void Set(HtmlTokenKind kind, TextRange range, bool isComplete)
        {
            Kind = kind;
            Range = range;
            IsComplete = isComplete;
        }
        public void ChangeKind(HtmlTokenKind kind)
        {
            Kind = kind;
        }
        public void ChangeLength(int length)
        {
            Length = length;
        }
        public override string ToString()
        {
            return $"{Kind}, {base.ToString()}";
        }
    }
}
