using System.Diagnostics;

namespace DoxygenEditor.Lexers
{
    class StringSourceBuffer : SourceBuffer
    {
        private readonly string _sourceText;
        private readonly int _basis;
        private readonly int _length;

        public override char this[int position] => _sourceText[position];
        public override int Basis { get { return _basis; } }
        public override int Length => _length;

        public StringSourceBuffer(string source, int basis, int length)
        {
            _sourceText = source;
            _length = length;
            _basis = basis;
        }
        public StringSourceBuffer(string source)
        {
            _sourceText = source;
            _length = source.Length;
            _basis = 0;
        }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            Debug.Assert(sourceIndex >= _basis && sourceIndex < _sourceText.Length);
            _sourceText.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public override int Compare(int thisIndex, string otherString, int otherIndex, int length)
        {
            Debug.Assert(thisIndex >= _basis && thisIndex < _sourceText.Length);
            int result = string.Compare(_sourceText, thisIndex, otherString, otherIndex, length);
            return (result);
        }
    }
}
