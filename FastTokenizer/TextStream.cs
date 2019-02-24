using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    abstract class TextStream : IDisposable, ITextStream
    {
        public const char InvalidCharacter = char.MaxValue;
        protected string Source { get; }
        public int StreamBase { get; }
        public int StreamLength { get; }
        public int StreamEnd { get; }

        public abstract int StreamPosition { get; }
        public abstract bool IsEOF { get; }
        public abstract int LexemeStart { get; }
        public abstract int LexemeWidth { get; }

        public TextStream(string source, int sbase, int length)
        {
            Source = source;
            StreamBase = sbase;
            StreamLength = length;
            StreamEnd = StreamBase + StreamLength;
        }

        public string GetStreamText(int index, int length)
        {
            string result = Source.Substring(index, length);
            return (result);
        }

        public abstract void StartLexeme();
        public abstract void AdvanceChar(int numChars = 1);
        public abstract char NextChar();
        public abstract char Peek();
        public abstract char Peek(int delta);
        public abstract int CompareText(int delta, string match, bool ignoreCase = false);
        public abstract void SetPosition(int pos);

        public abstract void Dispose();
    }
}
