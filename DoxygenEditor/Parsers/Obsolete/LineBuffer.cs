namespace TSP.DoxygenEditor.Parsers.Obsolete
{
    public class LineBuffer : TextBuffer
    {
        public SequenceInfo Info { get; }
        public LineBuffer(SequenceInfo info, string source) : base(source)
        {
            Info = info;
        }
    }
}
