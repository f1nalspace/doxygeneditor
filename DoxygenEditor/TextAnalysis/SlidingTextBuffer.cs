using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class SlidingTextBuffer : IDisposable
    {
        public const char InvalidCharacter = char.MaxValue;

        private const int DefaultBufferLength = 2048;

        private readonly SourceBuffer _source; // The source text
        private readonly int _sourceEnd; // The end position in the source text
        private int _base; // The base position in the source text
        private int _offset; // Offset into the buffer
        private char[] _buffer; // The char buffer
        private int _bufferCount; // Number of valid chars in the buffer
        private int _lexemeStart; // Start of lexeme relative to the buffer offset

        public SourceBuffer Source => _source;
        public int End => _sourceEnd;
        public int Position => _base + _offset;
        public int Offset => _offset;
        public char[] Buffer => _buffer;
        public int BufferCount => _bufferCount;
        public int LexemeStart => _base + _lexemeStart;
        public int LexemeWidth => _offset - _lexemeStart;
        public int RemainingLength => (_sourceEnd - Position);

        public bool IsEOF
        {
            get
            {
                bool result = (_offset >= _bufferCount) && (Position >= _sourceEnd);
                return (result);
            }
        }

        public string BufferText
        {
            get
            {
                if (_offset < _bufferCount)
                {
                    int amount = Math.Max(0, _bufferCount - _offset);
                    string result = new string(_buffer, _offset, amount);
                    return (result);
                }
                return string.Empty;
            }
        }

        public SlidingTextBuffer(SourceBuffer source)
        {
            _source = source;
            _sourceEnd = source.Basis + source.Length;
            _base = source.Basis;
            _offset = 0;
            _bufferCount = 0;
            _buffer = new char[DefaultBufferLength];
        }

        public void Start()
        {
            _lexemeStart = _offset;
        }

        public void Reset(int position)
        {
            int relative = position - _base;
            if (relative >= 0 && relative <= _bufferCount)
            {
                _offset = relative;
            }
            else
            {
                int amountToRead = Math.Min(_source.Length, position + _bufferCount) - position;
                amountToRead = Math.Max(amountToRead, 0);
                if (amountToRead > 0)
                {
                    _source.CopyTo(position, _buffer, 0, amountToRead);
                }
                _lexemeStart = 0;
                _offset = 0;
                _base = position;
                _bufferCount = amountToRead;
            }
        }

        private bool GetMoreChars()
        {
            if (_offset >= _bufferCount)
            {
                if (Position >= _sourceEnd)
                {
                    return (false);
                }

                // If lexeme scanning is sufficiently into the char buffer, then refocus the buffer onto the lexeme
                if (_lexemeStart > (_bufferCount / 4))
                {
                    Array.Copy(_buffer, _lexemeStart, _buffer, 0, _bufferCount - _lexemeStart);
                    _bufferCount -= _lexemeStart;
                    _offset -= _lexemeStart;
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

                int amountToRead = Math.Min(_sourceEnd - (_base + _bufferCount), _buffer.Length - _bufferCount);
                _source.CopyTo(_base + _bufferCount, _buffer, _bufferCount, amountToRead);
                _bufferCount += amountToRead;
                return (amountToRead > 0);
            }

            return (true);
        }

        public void AdvanceChar(int n = 1)
        {
            _offset += n;
        }

        public char PeekChar()
        {
            if (_offset >= _bufferCount && !this.GetMoreChars())
                return (InvalidCharacter);
            return _buffer[_offset];
        }

        public char PeekChar(int delta)
        {
            int position = this.Position;
            AdvanceChar(delta);
            char result;
            if (_offset >= _bufferCount && !this.GetMoreChars())
                result = InvalidCharacter;
            else
                result = _buffer[_offset];
            this.Reset(position);
            return (result);
        }

        public int Compare(int delta, string match)
        {
            int result = 0;
            int position = this.Position;
            int matchLen = match.Length;
            AdvanceChar(delta + match.Length);
            if (_offset >= _bufferCount && !this.GetMoreChars())
                return (-1);
            else
            {
                string part = new string(_buffer, _offset - matchLen, match.Length);
                result = string.Compare(match, part);
            }
            this.Reset(position);
            return (result);
        }

        public char NextChar()
        {
            char result = this.PeekChar();
            if (result != InvalidCharacter)
                this.AdvanceChar();
            return (result);
        }

        public string GetText(int position, int length)
        {
            int offset = position - _base;
            switch (length)
            {
                case 0:
                    return string.Empty;

                case 1:
                    {
                        if (_buffer[offset] == ' ')
                            return " ";
                        else if (_buffer[offset] == '\n')
                            return "\n";
                    }
                    break;

                case 2:
                    {
                        char firstChar = _buffer[offset];
                        if (firstChar == '\r' && _buffer[_offset + 1] == '\n')
                            return "\r\n";
                        else if (firstChar == '/' && _buffer[_offset + 1] == '/')
                            return "//";
                        else if (firstChar == '/' && _buffer[_offset + 1] == '*')
                            return "/*";
                    }
                    break;
            }
            return new string(_buffer, offset, length);
        }

        public void Dispose()
        {
            _buffer = null;
        }
    }
}
