using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenToken : BaseToken
    {
        public DoxygenTokenKind Kind { get; private set; }
        public override bool IsEOF => Kind == DoxygenTokenKind.EOF;
        public override bool IsValid => Kind != DoxygenTokenKind.Invalid;
        public override bool IsEndOfLine => Kind == DoxygenTokenKind.EndOfLine;
        public bool IsArgument => (Kind == DoxygenTokenKind.ArgumentCaption || Kind == DoxygenTokenKind.ArgumentIdent || Kind == DoxygenTokenKind.ArgumentText);
        public override bool IsMarker =>
            (Kind == DoxygenTokenKind.TextStart ||
             Kind == DoxygenTokenKind.TextEnd ||
             Kind == DoxygenTokenKind.ArgumentCaption ||
             Kind == DoxygenTokenKind.ArgumentFile ||
             Kind == DoxygenTokenKind.ArgumentIdent ||
             Kind == DoxygenTokenKind.ArgumentText
            );
        public DoxygenToken() : base()
        {
            Kind = DoxygenTokenKind.Invalid;
        }
        private DoxygenToken(DoxygenTokenKind kind, TextRange range, bool isComplete) : base(range, isComplete)
        {
            Kind = kind;
        }
        public void Set(DoxygenTokenKind kind, TextRange range, bool isComplete)
        {
            base.Set(range, isComplete);
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind}, {base.ToString()}";
        }
    }
}
