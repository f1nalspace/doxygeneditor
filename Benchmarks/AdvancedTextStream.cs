using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TSP.DoxygenEditor.TextAnalysis;

namespace Benchmarks
{
    class AdvancedTextStream : TextStream
    {
        private readonly ReadOnlyMemory<char> _source;

        public AdvancedTextStream(ReadOnlyMemory<char> source, TextPosition pos) : base(0, source.Length, pos)
        {
            _source = source;
        }

        public override string GetSourceText(int index, int length)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            ReadOnlySpan<char> span = _source.Span.Slice(index, length);
            return span.ToString();
        }

        public override ReadOnlySpan<char> GetSourceSpan(int index, int length)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            return _source.Span.Slice(index, length);
        }

        public override int CompareText(int delta, string match, bool ignoreCase = false)
        {
            if ((StreamPosition + delta + match.Length) < StreamLength)
            {
                ReadOnlySpan<char> matchSpan = match.AsSpan();
                ReadOnlySpan<char> sourceSpan = _source.Span.Slice(StreamPosition + delta, match.Length);
                int result = MemoryExtensions.SequenceCompareTo(sourceSpan, matchSpan);
                return result;
            }
            return -1;
        }

        public override bool MatchCharacters(int index, int length, Func<char, bool> predicate)
        {
            if ((StreamPosition + index + length) < StreamLength)
            {
                ReadOnlySpan<char> span = _source.Span.Slice(index, length);
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

        public override char Peek()
        {
            if (StreamPosition < StreamLength)
            {
                ReadOnlySpan<char> span = _source.Span.Slice(StreamPosition);
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
                ReadOnlySpan<char> span = _source.Span.Slice(StreamPosition);
                return span[delta];
                //ReadOnlySpan<char> span = _source.Span.Slice(StreamPosition);
                //ref char p = ref MemoryMarshal.GetReference(span);
                //char result = Unsafe.Add(ref p, delta);
                //return result;
            }
            return char.MaxValue;
        }
    }
}
