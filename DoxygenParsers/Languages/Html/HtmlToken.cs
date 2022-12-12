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

        private HtmlToken(HtmlTokenKind kind, TextRange range, bool isComplete) : base(range, isComplete)
        {
            Kind = kind;
        }
        public HtmlToken() : this(HtmlTokenKind.Invalid, TextRange.Invalid, false)
        {
        }
        public void Set(HtmlTokenKind kind, TextRange range, bool isComplete)
        {
            Set(LanguageKind.Html, range, isComplete);
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
