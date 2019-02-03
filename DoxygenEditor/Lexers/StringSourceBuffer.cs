namespace DoxygenEditor.Lexers
{
    class StringSourceBuffer : SourceBuffer
    {
        private readonly string _sourceText;
        private readonly int _offset;
        private readonly int _length;

        public override char this[int position] => _sourceText[position];
        public override int Offset { get { return _offset; } }
        public override int Length => _length;

        public StringSourceBuffer(string source, int length, int offset)
        {
            _sourceText = source;
            _length = length;
            _offset = offset;
        }
        public StringSourceBuffer(string source)
        {
            _sourceText = source;
            _length = source.Length;
            _offset = 0;
        }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            _sourceText.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public override int Compare(int thisIndex, string otherString, int otherIndex, int length)
        {
            int result = string.Compare(_sourceText, thisIndex, otherString, otherIndex, length);
            return (result);
        }
    }
}
