﻿using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public class BasicTextStream : TextStream
    {
        private string _source;

        public BasicTextStream(string source, int length, TextPosition pos) : base(length, pos)
        {
            _source = source;
        }

        public override string GetSourceText(int index, int length)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            string result = _source.Substring(index, length);
            return (result);
        }

        public override ReadOnlySpan<char> GetSourceSpan(int index, int length)
        {
            if (index < 0 || index + length > StreamLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The index '{index}' with length '{length}' is out-of-range {0} to {StreamLength}");
            return _source.AsSpan(index, length);
        }

        public override int CompareText(int delta, string match, bool ignoreCase = false)
        {
            int result = string.Compare(_source, StreamPosition + delta, match, 0, match.Length, ignoreCase);
            return (result);
        }

        public override bool MatchCharacters(int index, int length, Func<char, bool> predicate)
        {
            if (index < StreamBase)
                return (false);
            if (index + (length - 1) >= StreamOnePastEnd)
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
