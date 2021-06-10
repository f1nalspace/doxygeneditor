namespace TSP.DoxygenEditor.TextAnalysis
{
    public interface ITextStream
    {
        string FilePath { get; }
        int StreamBase { get; }
        int StreamLength { get; }
        int StreamPosition { get; }
        char Peek();
        char Peek(int delta);
        string GetSourceText(int index, int length);
    }
}
