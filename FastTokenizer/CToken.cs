using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.FastTokenizer
{
    struct CToken
    {
        public CppTokenKind Kind { get; }
        public TextPosition Start { get; set; }
        public TextPosition End { get; set; }
        public int Length => End.Index - Start.Index;
        public string Value { get; }

        public CToken(CppTokenKind kind, TextPosition start, TextPosition end, string value = null)
        {
            Kind = kind;
            Start = new TextPosition(start);
            End = new TextPosition(end);
            Value = value;
        }

        public override string ToString()
        {
            return $"{Kind}@{Start.Index} (line: {Start.Line}, column: {Start.Column}, length: {Length})";
        }
    }
}
