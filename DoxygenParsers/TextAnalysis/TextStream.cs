using System;
using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Types;
using static TSP.DoxygenEditor.TextAnalysis.ITextStream;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public abstract class TextStream : IDisposable, ITextStream
    {
        private TextPosition _lexemeStart;

        public const char InvalidCharacter = char.MaxValue;
        public int StreamBase { get; }
        public int StreamLength { get; }
        public int StreamOnePastEnd { get; }
        public int StreamEnd { get; }
        public int StreamPosition => TextPosition.Index;

        public bool IsEOF => TextPosition.Index >= StreamOnePastEnd;
        public TextPosition LexemeStart => _lexemeStart;
        public int LexemeWidth => _lexemeStart.Index > -1 ? Math.Max(TextPosition.Index - _lexemeStart.Index, 0) : 0;
        public TextRange LexemeRange => new TextRange(LexemeStart, LexemeWidth);
        public TextPosition TextPosition { get; set; }
        public int ColumnsPerTab { get; } = 4;

#if DEBUG
        public string Remaining
        {
            get
            {
                int l = StreamOnePastEnd - StreamPosition;
                return GetSourceText(StreamPosition, l);
            }
        }
#endif

        public TextStream(int index, int length, TextPosition pos)
        {
            StreamBase = index;
            StreamLength = length;
            StreamOnePastEnd = StreamBase + StreamLength;
            StreamEnd = StreamBase + Math.Max(0, StreamLength - 1);
            TextPosition = pos;
            _lexemeStart = new TextPosition(-1);
        }

        public abstract string GetSourceText(int index, int length, InternMode intern = InternMode.Normal);
        public virtual string GetSourceText(TextRange range, InternMode intern = InternMode.Normal) => GetSourceText(range.Index, range.Length, intern);

        public abstract ReadOnlySpan<char> GetSourceSpan(int index, int length);
        public virtual ReadOnlySpan<char> GetSourceSpan(TextRange range) => GetSourceSpan(range.Index, range.Length);

        public abstract char Peek();
        public abstract char Peek(int delta);

        public abstract bool MatchRelative(int index, string match);
        public abstract bool MatchRelative(int index, ReadOnlySpan<char> match);
        public abstract bool MatchAbsolute(int index, int length, Func<char, bool> predicate);

        public void AdvanceColumns(int numChars)
        {
            TextPosition p = TextPosition;
#if DEBUG
            for (int i = 0; i < numChars; ++i)
            {
                char c = Peek(i);
                Debug.Assert(c != '\t' && !SyntaxUtils.IsLineBreak(c));
            }
#endif
            p.Column += numChars;
            p.Index += numChars;
            TextPosition = p;
        }

        public void AdvanceColumn() => AdvanceColumns(1);

        public virtual void AdvanceColumnsWhile(Func<char, bool> func, int maxCols = -1)
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

        public virtual void AdvanceLineAuto()
        {
            char c0 = Peek();
            char c1 = Peek(1);
            int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
            AdvanceLine(lb);
        }

        public void AdvanceManual(char first, char second)
        {
            if (first == '\t')
                AdvanceTab();
            else if (SyntaxUtils.IsLineBreak(first))
            {
                int lb = SyntaxUtils.GetLineBreakChars(first, second);
                AdvanceLine(lb);
            }
            else
                AdvanceColumn();
        }

        public virtual int AdvanceAuto(int numChars = 1)
        {
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
                    p.Column = 0;
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

        public void SkipWhitespaces()
        {
            do
            {
                char c = Peek();
                if (c == InvalidCharacter)
                    break;
                else if (c == '\t')
                    AdvanceTab();
                else if (SyntaxUtils.IsLineBreak(c))
                    AdvanceLineAuto();
                else if (char.IsWhiteSpace(c))
                    AdvanceColumn();
                else
                    break;
            } while (!IsEOF);
        }

        public void SkipSpaces(RepeatKind repeat)
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
            } while (!IsEOF && repeat == RepeatKind.All);
        }

        public void SkipLineBreaks(RepeatKind repeat)
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
            } while (!IsEOF && repeat == RepeatKind.All);
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

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
        }
        protected virtual void DisposeUnmanaged()
        {
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
                DisposeManaged();
            DisposeUnmanaged();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TextStream()
        {
            Dispose(false);
        }
        #endregion
    }
}
