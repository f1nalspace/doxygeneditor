namespace TSP.DoxygenEditor.TextAnalysis
{
    public class AdvancedTextStreamFactory : TextStreamFactory
    {
        protected override ITextStream CreateStream(string source, int index, int length, TextPosition pos)
            => new AdvancedTextStream(source, index, length, pos);

        public override string ToString() => "Advanced";
    }
}
