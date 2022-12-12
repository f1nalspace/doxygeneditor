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
        public override bool IsMarker =>
            (Kind == CppTokenKind.PreprocessorStart) ||
            (Kind == CppTokenKind.PreprocessorEnd);

        private CppToken(CppTokenKind kind, TextRange range, bool isComplete) : base(range, isComplete)
        {
            Kind = kind;
        }
        public CppToken() : this(CppTokenKind.Unknown, TextRange.Invalid, false)
        {
        }
        public void Set(LanguageKind lang, CppTokenKind kind, TextRange range, bool isComplete)
        {
            Set(lang, range, isComplete);
            Kind = kind;
        }
        public override string ToString()
        {
            return $"{Kind}, {base.ToString()}";
        }
    }
}
