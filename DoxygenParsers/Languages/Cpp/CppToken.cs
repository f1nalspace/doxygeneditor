using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppToken : IBaseToken
    {
        public CppTokenKind Kind { get; internal set; }

        public bool IsEOF => Kind == CppTokenKind.Eof;
        public bool IsValid => Kind != CppTokenKind.Unknown;
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

        public CppToken()
        {
            Kind = CppTokenKind.Unknown;
            Range = new TextRange(new TextPosition(-1), 0);
            IsComplete = false;
        }
        private CppToken(CppTokenKind kind, TextRange range, bool isComplete)
        {
            Kind = kind;
            Range = range;
            IsComplete = isComplete;
        }
        public void Set(CppTokenKind kind, TextRange range, bool isComplete)
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
