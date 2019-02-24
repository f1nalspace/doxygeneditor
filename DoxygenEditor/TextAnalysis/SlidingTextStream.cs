using System;
using System.Diagnostics;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class SlidingTextStream : TextStream
    {
        private const int DefaultBufferLength = 2048;

        public override int StreamPosition => _base + _bufferIndex;

        private int _base; // The base position in the source text
        private char[] _buffer; // The char buffer
        private int _bufferIndex; // Index in the buffer
        private int _bufferCount; // Number of valid chars in the buffer
        private int _lexemeStart; // Start of lexeme relative to the buffer index
        public override int LexemeStart => _base + _lexemeStart;
        public override int LexemeWidth => _bufferIndex - _lexemeStart;

        public override bool IsEOF
        {
            get
            {
                bool result = (_bufferIndex >= _bufferCount) && (StreamPosition >= StreamOnePastEnd);
                return (result);
            }
        }

#if DEBUG
        public string BufferText
        {
            get
            {
                if (_bufferIndex < _bufferCount)
                {
                    int amount = Math.Max(0, _bufferCount - _bufferIndex);
                    string result = new string(_buffer, _bufferIndex, amount);
                    return (result);
                }
                return string.Empty;
            }
        }
#endif

        public SlidingTextStream(string source, int sbase, int length) : base(source, sbase, length)
        {
            _base = sbase;
            _bufferIndex = 0;
            _bufferCount = 0;
            _buffer = new char[DefaultBufferLength];
        }

        public override void StartLexeme()
        {
            _lexemeStart = _bufferIndex;
        }

        public override void SetPosition(int position)
        {
            int bufferPos = position - _base;
            if (bufferPos >= 0 && bufferPos <= _bufferCount)
            {
                _bufferIndex = bufferPos;
            }
            else
            {
                int amountToRead = Math.Min(StreamLength, position + _bufferCount) - position;
                amountToRead = Math.Max(amountToRead, 0);
                if (amountToRead > 0)
                {
                    Source.CopyTo(position, _buffer, 0, amountToRead);
                }
                _lexemeStart = 0;
                _bufferIndex = 0;
                _base = position;
                _bufferCount = amountToRead;
            }
        }

        private bool GetMoreChars()
        {
            if (_bufferIndex >= _bufferCount)
            {
                if (StreamPosition >= StreamOnePastEnd)
                {
                    return (false);
                }

                // If lexeme scanning is sufficiently into the char buffer, then refocus the buffer onto the lexeme
                if (_lexemeStart > (_bufferCount / 4))
                {
                    Array.Copy(_buffer, _lexemeStart, _buffer, 0, _bufferCount - _lexemeStart);
                    _bufferCount -= _lexemeStart;
                    _bufferIndex -= _lexemeStart;
                    _base += _lexemeStart;
                    _lexemeStart = 0;
                }

                if (_bufferCount >= _buffer.Length)
                {
                    // Grow char array, since we need more contiguous space
                    char[] oldBuffer = _buffer;
                    char[] newBuffer = new char[_buffer.Length * 2];
                    Array.Copy(oldBuffer, 0, newBuffer, 0, _bufferCount);
                    _buffer = newBuffer;
                }

                int amountToRead = Math.Min(StreamOnePastEnd - (_base + _bufferCount), _buffer.Length - _bufferCount);
                Source.CopyTo(_base + _bufferCount, _buffer, _bufferCount, amountToRead);
                _bufferCount += amountToRead;
                return (amountToRead > 0);
            }

            return (true);
        }

        public override void AdvanceChar(int n = 1)
        {
            _bufferIndex += n;
        }

        public override char Peek()
        {
            if (_bufferIndex >= _bufferCount && !this.GetMoreChars())
                return (InvalidCharacter);
            return _buffer[_bufferIndex];
        }

        public override char Peek(int delta)
        {
            int oldPos = StreamPosition;
            AdvanceChar(delta);
            char result;
            if (_bufferIndex >= _bufferCount && !this.GetMoreChars())
                result = InvalidCharacter;
            else
                result = _buffer[_bufferIndex];
            SetPosition(oldPos);
            return (result);
        }

        public override int CompareText(int delta, string match, bool ignoreCase = false)
        {
            int oldPos = StreamPosition;
            int matchLen = match.Length;
            AdvanceChar(delta + match.Length);
            int result = 0;
            if (_bufferIndex >= _bufferCount && !this.GetMoreChars())
                result = -1;
            else
            {
                string part = new string(_buffer, _bufferIndex - matchLen, matchLen);
                result = string.Compare(match, part, ignoreCase);
            }
            SetPosition(oldPos);
            return (result);
        }

        public override char NextChar()
        {
            char result = this.Peek();
            if (result != InvalidCharacter)
                this.AdvanceChar();
            return (result);
        }

        public string GetBufferText(int streamPos, int length)
        {
            int bufferPosition = streamPos - _base;
            Debug.Assert(bufferPosition >= 0 && bufferPosition < _bufferCount);
            switch (length)
            {
                case 0:
                    return string.Empty;

                case 1:
                    {
                        if (_buffer[bufferPosition] == ' ')
                            return " ";
                        else if (_buffer[bufferPosition] == '\n')
                            return "\n";
                        else if (_buffer[bufferPosition] == '\r')
                            return "\r";
                    }
                    break;

                case 2:
                    {
                        char firstChar = _buffer[bufferPosition];
                        if (firstChar == '\r' && _buffer[_bufferIndex + 1] == '\n')
                            return "\r\n";
                        else if (firstChar == '/' && _buffer[_bufferIndex + 1] == '/')
                            return "//";
                        else if (firstChar == '/' && _buffer[_bufferIndex + 1] == '*')
                            return "/*";
                    }
                    break;
            }
            return new string(_buffer, bufferPosition, length);
        }

        public override void Dispose()
        {
            _buffer = null;
        }
    }
}
