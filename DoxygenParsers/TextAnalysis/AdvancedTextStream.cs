using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Types;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public class AdvancedTextStream : TextStream
    {
        private readonly char[] _source;

        public AdvancedTextStream(string source, int index, int length, TextPosition pos) : base(index, length, pos)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            _source = source.ToArray();
        }

        public override string GetSourceText(int index, int length, InternMode intern = InternMode.Normal)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            ReadOnlySpan<char> span = _source.AsSpan(index, length);
            if (intern == InternMode.Intern && length > 0)
                return string.Intern(span.ToString());
            return span.ToString();
        }

        public override ReadOnlySpan<char> GetSourceSpan(int index, int length)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            return _source.AsSpan(index, length);
        }

        public override bool MatchRelative(int index, string match)
        {
            if ((StreamPosition + index + match.Length) < StreamLength)
            {
                ReadOnlySpan<char> matchSpan = match.AsSpan();
                ReadOnlySpan<char> sourceSpan = _source.AsSpan(StreamPosition + index, match.Length);
                bool result = MemoryExtensions.SequenceEqual(sourceSpan, matchSpan);
                return result;
            }
            return false;
        }

        public override bool MatchRelative(int index, ReadOnlySpan<char> match)
        {
            if ((StreamPosition + index + match.Length) < StreamLength)
            {
                ReadOnlySpan<char> sourceSpan = _source.AsSpan(StreamPosition + index, match.Length);
                bool result = match.SequenceEqual(sourceSpan);
                return result;
            }
            return false;
        }

        public override bool MatchAbsolute(int index, int length, Func<char, bool> predicate)
        {
            if ((StreamPosition + index + length) < StreamLength)
            {
                ReadOnlySpan<char> span = _source.AsSpan(index, length);
                ref char p = ref MemoryMarshal.GetReference(span);
                for (int i = 0; i < length; ++i)
                {
                    char x = Unsafe.Add(ref p, i);
                    if (!predicate(x))
                        return false;
                }
                return true;
            }
            return false;
        }

        public override void AdvanceColumnsWhile(Func<char, bool> func, int maxCols = -1)
        {
            int colCount = 0;
            ReadOnlySpan<char> span = _source.AsSpan(StreamPosition);
            if (span.IsEmpty)
                return;
            ref char p = ref MemoryMarshal.GetReference(span);
            for (int i = 0; i < span.Length; ++i)
            {
                char c = Unsafe.Add(ref p, i);
                if ((!func(c)) || (maxCols > -1 && (colCount >= maxCols)))
                    break;
                ++colCount;
            }
            if (colCount > 0)
                AdvanceColumns(colCount);
        }

        public override void AdvanceLineAuto()
        {
            ReadOnlySpan<char> span = _source.AsSpan(StreamPosition);
            if (span.IsEmpty)
                return;
            ref char p = ref MemoryMarshal.GetReference(span);
            char c0, c1;
            if (span.Length >= 2)
            {
                c0 = Unsafe.Add(ref p, 0);
                c1 = Unsafe.Add(ref p, 1);
            }
            else
            {
                c0 = Unsafe.Add(ref p, 0);
                c1 = char.MaxValue;
            }
            int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
            AdvanceLine(lb);
        }

        public override int AdvanceAuto(int numChars = 1)
        {
            Debug.Assert(numChars >= 1);

            ReadOnlySpan<char> span = _source.AsSpan(StreamPosition);
            if (span.Length < numChars)
                return 0;

            ref char ptr = ref MemoryMarshal.GetReference(span);

            TextPosition p = TextPosition;

            int result = 0;
            for (int i = 0; i < numChars; ++i)
            {
                char c0 = Unsafe.Add(ref ptr, i);
                char c1 = (i + 1 < span.Length) ? Unsafe.Add(ref ptr, i + 1) : char.MaxValue;
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

        public override char Peek()
        {
            if (StreamPosition < StreamLength)
            {
                ReadOnlySpan<char> span = _source.AsSpan(StreamPosition, 1);
                return span[0];
                //ReadOnlySpan<char> span = _source.Span.Slice(StreamPosition);
                //ref char p = ref MemoryMarshal.GetReference(span);
                //char result = Unsafe.Add(ref p, 0);
                //return result;
            }
            return char.MaxValue;
        }

        public override char Peek(int delta)
        {
            if (StreamPosition + delta < StreamLength)
            {
                ReadOnlySpan<char> span = _source.AsSpan(StreamPosition, 1);
                return span[0];
            }
            return char.MaxValue;
        }
    }
}
