using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class BasicTextStream : TextStream
    {
        private int _position;
        private int _lexemeStart;

        public BasicTextStream(string source, int sbase, int length) : base(source, sbase, length)
        {
            _position = sbase;
            _lexemeStart = -1;
        }

        public override int StreamPosition => _position;

        public override bool IsEOF => _position >= StreamEnd;

        public override int LexemeStart => _lexemeStart;

        public override int LexemeWidth => _lexemeStart > -1 ? Math.Max(_position - _lexemeStart, 0) : 0;

        public override void AdvanceChar(int count = 1)
        {
            _position += count;
        }

        public override int CompareText(int delta, string match, bool ignoreCase = false)
        {
            int result = string.Compare(Source, _position + delta, match, 0, match.Length, ignoreCase);
            return (result);
        }

        public override void Dispose()
        {
        }

        public override char Peek()
        {
            if (_position >= StreamBase && _position < StreamEnd)
            {
                char result = Source[_position];
                return (result);
            }
            return char.MaxValue;
        }

        public override char Peek(int delta)
        {
            int p = _position + delta;
            if (p >= StreamBase && p < StreamEnd)
            {
                char result = Source[p];
                return (result);
            }
            return char.MaxValue;
        }

        public override char NextChar()
        {
            char result = Peek();
            if (result != TextStream.InvalidCharacter)
                AdvanceChar();
            return (result);
        }

        public override void SetPosition(int pos)
        {
            _position = pos;
        }

        public override void StartLexeme()
        {
            _lexemeStart = _position;
        }
    }
}
