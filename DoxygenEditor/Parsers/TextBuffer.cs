using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoxygenEditor.Parsers
{
    public class TextBuffer
    {
        public string Source { get; }
        public int Length { get; }
        public int Offset { get; set; }
        public int Start { get; }
        public int Index
        {
            get
            {
                int result = Start + Offset;
                return (result);
            }
        }

        public int End
        {
            get
            {
                int result = Start + Math.Max(0, Length - 1);
                return (result);
            }
        }

        public bool IsEOF
        {
            get
            {
                bool result = Offset >= Length;
                return (result);
            }
        }

        public int RemainingLength
        {
            get
            {
                int result = Length - Offset;
                return (result);
            }
        }

        public char CurrentChar
        {
            get
            {
                if (Offset < Length)
                {
                    char result = Source[Start + Offset];
                    return (result);
                }
                return '\0';
            }
        }

        public string Remaining
        {
            get
            {
                if (Offset < Length)
                {
                    int len = Length - Offset;
                    string result = Source.Substring(Start + Offset, len);
                    return (result);
                }
                return (null);
            }
        }

        public char NextChar()
        {
            if (Offset < Length)
            {
                char result = Source[Start + Offset++];
                return (result);
            }
            return '\0';
        }

        public char PeekChar(int count = 1)
        {
            if ((Offset + count) < Length)
            {
                char result = Source[Start + Offset + count];
                return (result);
            }
            return '\0';
        }

        public void Skip(int amount = 1)
        {
            if ((Offset + (amount - 1)) < Length)
                Offset += amount;
            else
                throw new InvalidOperationException("End of line reached!");
        }

        public void SkipWhitespaces()
        {
            while (!IsEOF && (char.IsWhiteSpace(CurrentChar)))
            {
                Skip();
            }
        }

        public void SkipCharacters(HashSet<char> untilChars)
        {
            while (!IsEOF && (untilChars.Contains(CurrentChar)))
            {
                Skip();
            }
        }

        public TextBuffer(string source, int offset = 0)
        {
            Source = source;
            Offset = 0;
            Length = source.Length;
            Start = Offset;
        }

        public TextBuffer(string source, int offset, int length)
        {
            Source = source;
            Offset = 0;
            Length = length;
            Start = offset;
        }
    }
}
