using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public class BasicTextStream : TextStream
    {
        private readonly string _source;

        public BasicTextStream(string source, int index, int length, TextPosition pos) : base(index, length, pos)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            _source = source;
        }

        public override string GetSourceText(int index, int length)
        {
            if ((index < StreamBase) || ((index + length) > StreamOnePastEnd))
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {StreamBase} to {StreamOnePastEnd - 1}");
            string result = _source.Substring(index, length);
            return (result);
        }

        public override ReadOnlySpan<char> GetSourceSpan(int index, int length)
        {
            if ((index < StreamBase) || ((index + length) > StreamOnePastEnd))
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {StreamBase} to {StreamOnePastEnd - 1}");
            return _source.AsSpan(index, length);
        }

        public override bool MatchText(int index, string match)
        {
            if ((StreamPosition + index + match.Length) < StreamLength)
                return string.CompareOrdinal(_source, StreamPosition + index, match, 0, match.Length) == 0;
            return false;
        }

        public override bool MatchSpan(int index, ReadOnlySpan<char> match)
        {
            if ((StreamPosition + index + match.Length) < StreamLength)
            {
                var span = _source.AsSpan(index, match.Length);
                bool result = match.SequenceEqual(span);
                return result;
            }
            return false;
        }

        public override bool MatchCharacters(int index, int length, Func<char, bool> predicate)
        {
            if (index < StreamBase)
                return (false);
            if (index + length > StreamOnePastEnd)
                return (false);
            if (length == 0)
                return (false);
            for (int i = index, e = index + length; i < e; ++i)
            {
                if (!predicate(_source[i]))
                    return (false);
            }
            return (true);
        }

        public override char Peek()
        {
            if (StreamPosition >= StreamBase && StreamPosition < StreamOnePastEnd)
            {
                char result = _source[StreamPosition];
                return (result);
            }
            return char.MaxValue;
        }

        public override char Peek(int delta)
        {
            int p = StreamPosition + delta;
            // @TODO(final): StreamEnd - actual stream end?
            if (p >= StreamBase && p < StreamOnePastEnd)
            {
                char result = _source[p];
                return (result);
            }
            return char.MaxValue;
        }
    }
}
