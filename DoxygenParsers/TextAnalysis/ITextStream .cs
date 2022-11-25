using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public interface ITextStream
    {
        int StreamBase { get; }
        int StreamLength { get; }
        int StreamPosition { get; }
        char Peek();
        char Peek(int delta);
        string GetSourceText(int index, int length);
        ReadOnlySpan<char> GetSourceSpan(int index, int length);
    }
}
