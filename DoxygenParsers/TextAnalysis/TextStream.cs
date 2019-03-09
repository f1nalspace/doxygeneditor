using System;
using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Utils;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public abstract class TextStream : IDisposable, ITextStream
    {
        private TextPosition _lexemeStart;

        public const char InvalidCharacter = char.MaxValue;
        public int StreamBase { get; }
        public int StreamLength { get; }
        public int StreamOnePastEnd { get; }
        public int StreamPosition => TextPosition.Index;

        public bool IsEOF => TextPosition.Index >= StreamOnePastEnd;
        public TextPosition LexemeStart => _lexemeStart;
        public int LexemeWidth => _lexemeStart.Index > -1 ? Math.Max(TextPosition.Index - _lexemeStart.Index, 0) : 0;
        public TextRange LexemeRange => new TextRange(LexemeStart, LexemeWidth);
        public TextPosition TextPosition { get; }
        public int ColumnsPerTab { get; } = 4;
        public string Remaining
        {
            get
            {
                int l = StreamOnePastEnd - StreamPosition;
                return GetSourceText(StreamPosition, l);
            }
        }

        public TextStream(TextPosition pos, int length)
        {
            StreamBase = pos.Index;
            StreamLength = length;
            StreamOnePastEnd = StreamBase + StreamLength;
            TextPosition = new TextPosition(pos);
            _lexemeStart = new TextPosition(-1);
        }

        public abstract string GetSourceText(int index, int length);
        public abstract char Peek();
        public abstract char Peek(int delta);
        public abstract int CompareText(int delta, string match, bool ignoreCase = false);
        public abstract bool MatchCharacters(int index, int length, Func<char, bool> predicate);
        public abstract void Dispose();

        public void AdvanceColumns(int numChars)
        {
            for (int i = 0; i < numChars; ++i)
            {
                char c = Peek(i);
                Debug.Assert(c != '\t' && c != '\n' && c != '\r');
            }
            TextPosition.Column += numChars;
            TextPosition.Index += numChars;
        }

        public void AdvanceColumn()
        {
            AdvanceColumns(1);
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

        public void AdvanceTab()
        {
            TextPosition.Column += ColumnsPerTab;
            TextPosition.Index++;
        }

        public void AdvanceLine(int charsPerLine)
        {
            TextPosition.Index += charsPerLine;
            TextPosition.Line++;
            TextPosition.Column = 0;
        }

        public void AdvanceManual(char first, char second)
        {
            if (first == '\t')
                AdvanceTab();
            else if (SyntaxUtils.IsLineBreak(first))
            {
                int nb = SyntaxUtils.GetLineBreakChars(first, second);
                AdvanceLine(nb);
            }
            else
                AdvanceColumn();
        }

        public int AdvanceAuto(int numChars = 1)
        {
            // @NOTE(final): This is super slow, so only use it when needed
            Debug.Assert(numChars >= 1);
            int result = 0;
            while (result < numChars)
            {
                char c0 = Peek();
                char c1 = Peek(1);
                if (SyntaxUtils.IsLineBreak(c0))
                {
                    int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    TextPosition.Line++;
                    TextPosition.Column = 1;
                    TextPosition.Index += lb;
                    result += lb;
                }
                if (c0 == '\t')
                {
                    TextPosition.Column += ColumnsPerTab;
                    TextPosition.Index++;
                    result++;
                }
                else
                {
                    TextPosition.Column++;
                    TextPosition.Index++;
                    result++;
                }
            }
            return (result);
        }

        public void Seek(TextPosition pos)
        {
            TextPosition.Index = pos.Index;
            TextPosition.Column = pos.Column;
            TextPosition.Line = pos.Line;
        }

        public void StartLexeme()
        {
            _lexemeStart = new TextPosition(TextPosition);
        }

    }
}
