using System;
using System.Diagnostics;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class TextCursor
    {
        public ITextStream Stream { get; }
        public TextPosition Position { get; private set; }
        public bool IsEOF => (Position.Index < Stream.StreamBase) || Position.Index >= (Stream.StreamBase + Math.Max(Stream.StreamLength, 0));
        public bool HasError => !string.IsNullOrWhiteSpace(Error);
        public string Error { get; private set; }
        public bool IsParsing => !IsEOF && !HasError;
        public TextCursor(ITextStream stream, int startLine = 1, int startColumn = 1)
        {
            Stream = stream;
            Error = null;
            Position = new TextPosition(stream.StreamBase, startLine, startColumn);
        }
        public TextCursor(TextCursor other)
        {
            Stream = other.Stream;
            Error = other.Error;
            Position = new TextPosition(other.Position);
        }
        public virtual void AdvanceColumn(int charCount = 1)
        {
            Debug.Assert(!HasError);
            Position.AdvanceColumn(charCount);
        }
        public virtual void AdvanceLine()
        {
            Debug.Assert(!HasError);
            Position.AdvanceLine();
        }
        public virtual void Set(TextPosition position)
        {
            Debug.Assert(!HasError);
            Position = new TextPosition(position);
        }

        public void AdvanceColumnsWhile(Func<char, bool> func, int maxCols = -1)
        {
            int colCount = 0;
            while (!IsEOF)
            {
                if ((!func(Peek())) || (maxCols > -1 && (colCount >= maxCols)))
                    break;
                AdvanceColumn();
                ++colCount;
            }
        }
        public char Peek()
        {
            int delta = Position.Index - Stream.StreamPosition;
            char result = Stream.Peek(delta);
            return (result);
        }
        public char Peek(int delta)
        {
            int streamDelta = Position.Index - Stream.StreamPosition;
            char result = Stream.Peek(streamDelta + delta);
            return (result);
        }
        public char Next()
        {
            char result = Peek();
            AdvanceColumn();
            return (result);
        }
        public void SetError(string message)
        {
            Error = message;
        }

        public override string ToString()
        {
            if (IsEOF)
                return $"EOF({Position})";
            else
                return $"Position";
        }
    }
}
