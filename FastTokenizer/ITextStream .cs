namespace TSP.FastTokenizer
{
    interface ITextStream
    {
        int StreamBase { get; }
        int StreamLength { get; }
        int StreamPosition { get; }
        char Peek();
        char Peek(int delta);
        string GetStreamText(int index, int length);
    }
}
