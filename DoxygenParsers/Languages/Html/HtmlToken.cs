using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Html
{
    public class HtmlToken : BaseToken
    {
        public HtmlTokenKind Kind { get; private set; }
        public override bool IsEOF => Kind == HtmlTokenKind.EOF;
        public override bool IsValid => Kind != HtmlTokenKind.Invalid;
        public override bool IsEndOfLine => false;
        public override bool IsMarker => false;
        public HtmlToken() : base()
        {
            Kind = HtmlTokenKind.Invalid;
        }
        private HtmlToken(HtmlTokenKind kind, TextRange range, bool isComplete) : base(range, isComplete)
        {
            Kind = kind;
        }
        public void Set(HtmlTokenKind kind, TextRange range, bool isComplete)
        {
            base.Set(range, isComplete);
            Kind = kind;
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
