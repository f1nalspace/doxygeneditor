using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenToken : IBaseToken
    {
        public DoxygenTokenKind Kind { get; private set; }

        public bool IsEOF => Kind == DoxygenTokenKind.EOF;
        public bool IsValid => Kind != DoxygenTokenKind.Invalid;
        public bool IsEndOfLine => Kind == DoxygenTokenKind.EndOfLine;
        public bool IsArgument => (Kind == DoxygenTokenKind.ArgumentCaption || Kind == DoxygenTokenKind.ArgumentIdent || Kind == DoxygenTokenKind.ArgumentText);
        public bool IsMarker =>
            (Kind == DoxygenTokenKind.TextStart ||
             Kind == DoxygenTokenKind.TextEnd ||
             Kind == DoxygenTokenKind.ArgumentCaption ||
             Kind == DoxygenTokenKind.ArgumentFile ||
             Kind == DoxygenTokenKind.ArgumentIdent ||
             Kind == DoxygenTokenKind.ArgumentText
            );

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

        private DoxygenToken(DoxygenTokenKind kind, TextRange range, bool isComplete)
        {
            Kind = kind;
            Range = range;
            IsComplete = isComplete;
        }
        public DoxygenToken() : this(DoxygenTokenKind.Invalid, TextRange.Invalid, false)
        {
        }
        public void Set(DoxygenTokenKind kind, TextRange range, bool isComplete)
        {
            Kind = kind;
            Range = range;
            IsComplete = isComplete;
        }
        public override string ToString()
        {
            return $"{Kind}, {base.ToString()}";
        }
    }
}
