namespace TSP.DoxygenEditor.TextAnalysis
{
    abstract class SourceBuffer
    {
        public virtual int Basis { get { return 0; } }
        public abstract char this[int position] { get; }
        public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);
        public abstract int Length { get; }
        public abstract int Compare(int thisIndex, string otherString, int otherIndex, int length);
        public abstract string GetText(int index, int length);
    }
}
