using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppToken : BaseToken
    {
        public CppTokenKind Kind { get; internal set; }
        public override bool IsEOF => Kind == CppTokenKind.Eof;
        public override bool IsValid => Kind != CppTokenKind.Unknown;
        public override bool IsEndOfLine => false;
        public override bool IsMarker => false;
        public CppToken() : base()
        {
            Kind = CppTokenKind.Unknown;
        }
        private CppToken(CppTokenKind kind, TextRange range, bool isComplete) : base()
        {
            Kind = kind;
        }
        public void Set(CppTokenKind kind, TextRange range, bool isComplete)
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
