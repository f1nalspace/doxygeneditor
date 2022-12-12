namespace TSP.DoxygenEditor.TextAnalysis
{
    public interface ITextStreamFactory
    {
        ITextStream Create(string source, int index, int length, TextPosition pos);
    }
}
