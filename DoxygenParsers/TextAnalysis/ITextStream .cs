using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public interface ITextStream : IDisposable
    {
        bool IsEOF { get; }
        int StreamBase { get; }
        int StreamEnd { get; }
        int StreamLength { get; }
        int StreamPosition { get; }

        TextPosition LexemeStart { get; }
        int LexemeWidth { get; }
        TextRange LexemeRange { get; }
        TextPosition TextPosition { get; }

        char Peek();
        char Peek(int delta);

        string GetSourceText(int index, int length);
        string GetSourceText(TextRange range);
        ReadOnlySpan<char> GetSourceSpan(int index, int length);

        bool MatchText(int index, string match);
        bool MatchSpan(int index, ReadOnlySpan<char> match);
        bool MatchCharacters(int index, int length, Func<char, bool> predicate);

        void AdvanceColumn();
        void AdvanceColumns(int numChars);
        void AdvanceColumnsWhile(Func<char, bool> func, int maxCols = -1);
        void AdvanceTab();
        void AdvanceLine(int charsPerLine);
        void AdvanceLineAuto();
        void AdvanceManual(char first, char second);
        int AdvanceAuto(int numChars = 1);

        void SkipWhitespaces();
        void SkipSpaces(RepeatKind repeat);
        void SkipLineBreaks(RepeatKind repeat);
        void SkipUntil(char c);

        void StartLexeme();
    }
}
