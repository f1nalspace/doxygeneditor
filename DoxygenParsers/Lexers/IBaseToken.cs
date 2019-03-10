using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Lexers
{
    public interface IBaseToken
    {
        int Index { get; }
        int End { get; }
        TextPosition Position { get; }

        bool IsEOF { get; }
        bool IsEndOfLine { get; }
        bool IsValid { get; }
        bool IsMarker { get; }

        TextRange Range { get; set; }
        string Value { get; set; }
        bool IsComplete { get; set; }
        int Length { get; set; }
    }
}
