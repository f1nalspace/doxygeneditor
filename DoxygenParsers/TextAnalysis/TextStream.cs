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
        public TextPosition TextPosition { get; set; }
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
            TextPosition = pos;
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
            TextPosition p = TextPosition;
            for (int i = 0; i < numChars; ++i)
            {
                char c = Peek(i);
                Debug.Assert(c != '\t' && c != '\n' && c != '\r');
            }
            p.Column += numChars;
            p.Index += numChars;
            TextPosition = p;
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
            TextPosition p = TextPosition;
            p.Column += ColumnsPerTab;
            p.Index++;
            TextPosition = p;
        }

        public void AdvanceLine(int charsPerLine)
        {
            TextPosition p = TextPosition;
            p.Index += charsPerLine;
            p.Line++;
            p.Column = 0;
            TextPosition = p;
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
            TextPosition p = TextPosition;
            int result = 0;
            while (result < numChars)
            {
                char c0 = Peek();
                char c1 = Peek(1);
                if (SyntaxUtils.IsLineBreak(c0))
                {
                    int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    p.Line++;
                    p.Column = 1;
                    p.Index += lb;
                    result += lb;
                }
                if (c0 == '\t')
                {
                    p.Column += ColumnsPerTab;
                    p.Index++;
                    result++;
                }
                else
                {
                    p.Column++;
                    p.Index++;
                    result++;
                }
            }
            TextPosition = p;
            return (result);
        }

        public enum SkipType
        {
            Single,
            All
        }

        public void SkipAllWhitespaces()
        {
            do
            {
                char c0 = Peek();
                char c1 = Peek(1);
                if (c0 == InvalidCharacter)
                    break;
                else if (c0 == '\t')
                    AdvanceTab();
                else if (SyntaxUtils.IsLineBreak(c0))
                {
                    int nb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    AdvanceLine(nb);
                }
                else if (char.IsWhiteSpace(c0))
                    AdvanceColumn();
                else
                    break;
            } while (!IsEOF);
        }

        public void SkipSpacings(SkipType type)
        {
            do
            {
                char c = Peek();
                if (c == InvalidCharacter)
                    break;
                else if (c == '\t')
                    AdvanceTab();
                else if (SyntaxUtils.IsSpacing(c))
                    AdvanceColumn();
                else
                    break;
            } while (!IsEOF && type == SkipType.All);
        }

        public void SkipLineBreaks(SkipType type)
        {
            do
            {
                char c0 = Peek();
                char c1 = Peek(1);
                if (c0 == InvalidCharacter)
                    break;
                else if (SyntaxUtils.IsLineBreak(c0))
                {
                    int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
                    AdvanceLine(lb);
                }
                else break;
            } while (!IsEOF && type == SkipType.All);
        }

        public void SkipUntil(char c)
        {
            while (!IsEOF)
            {
                if (Peek() == c)
                    break;
                AdvanceAuto();
            }
        }

        public void Seek(TextPosition pos)
        {
            TextPosition = pos;
        }

        public void StartLexeme()
        {
            _lexemeStart = TextPosition;
        }

    }
}
