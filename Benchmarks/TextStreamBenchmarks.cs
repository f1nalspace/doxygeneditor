using BenchmarkDotNet.Attributes;
using TSP.DoxygenEditor.TextAnalysis;
using System;
using System.Collections.Generic;

namespace Benchmarks
{
    [MinColumn, MaxColumn, MedianColumn]
    public class TextStreamBenchmarks
    {
        public string HeaderSource { get; set; }

        private static readonly ITextStreamFactory BasicFactory = new TextStreamFactory();
        private static readonly ITextStreamFactory AdvancedFactory = new AdvancedTextStreamFactory();

        public IEnumerable<ITextStreamFactory> Factories => _factories;
        private readonly List<ITextStreamFactory> _factories = new List<ITextStreamFactory>() { BasicFactory, AdvancedFactory };

        [ParamsSource(nameof(Factories))]
        public ITextStreamFactory Factory { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            HeaderSource = global::Benchmarks.Properties.Resources.final_platform_layer_h;
        }

        [Benchmark]
        public int FullRead()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            while (!stream.IsEOF)
                result += stream.AdvanceAuto();
            return result;
        }

        [Benchmark]
        public int LinearPeek()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            while (!stream.IsEOF)
            {
                char c = stream.Peek();
                result += c.GetHashCode();
                stream.AdvanceAuto();
            }
            return result;
        }

        [Benchmark]
        public int RandomPeek()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int remaining = stream.StreamLength - stream.StreamPosition;
                int p = rnd.Next(0, remaining);
                char c = stream.Peek(p);
                result += c.GetHashCode();
                stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int LinearGetSourceSpan()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            while (!stream.IsEOF)
            {
                int pos = stream.StreamPosition;
                int remaining = stream.StreamLength - pos;
                int len = Math.Min(remaining, 30);
                ReadOnlySpan<char> span = stream.GetSourceSpan(pos, len);
                result += span.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int LinearGetSourceText()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            while (!stream.IsEOF)
            {
                int pos = stream.StreamPosition;
                int remaining = stream.StreamLength - pos;
                int len = Math.Min(remaining, 30);
                string text = stream.GetSourceText(pos, len);
                result += text.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int RandomGetSourceSpan()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int initialPos = stream.StreamPosition;
                int initialRemaining = stream.StreamLength - initialPos;
                int end = initialPos + initialRemaining;
                int pos = rnd.Next(initialPos, end);
                int remaining = stream.StreamLength - pos;
                int maxLen = rnd.Next(1, 30);
                int len = Math.Min(remaining, maxLen);
                ReadOnlySpan<char> span = stream.GetSourceSpan(pos, len);
                result += span.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int RandomGetSourceText()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int initialPos = stream.StreamPosition;
                int initialRemaining = stream.StreamLength - initialPos;
                int end = initialPos + initialRemaining;
                int pos = rnd.Next(initialPos, end);
                int remaining = stream.StreamLength - pos;
                int maxLen = rnd.Next(1, 30);
                int len = Math.Min(remaining, maxLen);
                string text = stream.GetSourceText(pos, len);
                result += text.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int MatchText()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            string comparend = "void";
            while (!stream.IsEOF)
            {
                bool r = stream.MatchText(0, comparend);
                if (r)
                {
                    result += 1;
                    stream.AdvanceAuto(comparend.Length);
                }
                else
                    stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int MatchSpan()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            string comparend = "void";
            ReadOnlySpan<char> span = comparend.AsSpan();
            while (!stream.IsEOF)
            {
                bool r = stream.MatchSpan(0, span);
                if (r)
                {
                    result += 1;
                    stream.AdvanceAuto(comparend.Length);
                }
                else
                    stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int MatchCharacters()
        {
            int result = 0;
            ITextStream stream = Factory.Create(HeaderSource, 0, HeaderSource.Length, new TextPosition());
            while (!stream.IsEOF)
            {
                if (stream.MatchCharacters(0, 4, char.IsWhiteSpace))
                {
                    result += 4 * stream.StreamPosition;
                    stream.AdvanceAuto(4);
                }
                else
                    stream.AdvanceAuto(1);
            }
            return result;
        }

#if false
        [Benchmark]
        public int AdvancedFullRead()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            while (!stream.IsEOF)
                result += stream.AdvanceAuto();
            return result;
        }

        [Benchmark]
        public int AdvancedLinearPeek()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            while (!stream.IsEOF)
            {
                char c = stream.Peek();
                result += c.GetHashCode();
                stream.AdvanceAuto();
            }
            return result;
        }

        [Benchmark]
        public int AdvancedRandomPeek()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int remaining = stream.StreamLength - stream.StreamPosition;
                int p = rnd.Next(0, remaining);
                char c = stream.Peek(p);
                result += c.GetHashCode();
                stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedLinearGetSourceSpan()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            while (!stream.IsEOF)
            {
                int pos = stream.StreamPosition;
                int remaining = stream.StreamLength - pos;
                int len = Math.Min(remaining, 30);
                ReadOnlySpan<char> span = stream.GetSourceSpan(pos, len);
                result += span.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedLinearGetSourceText()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            while (!stream.IsEOF)
            {
                int pos = stream.StreamPosition;
                int remaining = stream.StreamLength - pos;
                int len = Math.Min(remaining, 30);
                string text = stream.GetSourceText(pos, len);
                result += text.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedRandomGetSourceSpan()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int initialPos = stream.StreamPosition;
                int initialRemaining = stream.StreamLength - initialPos;
                int end = initialPos + initialRemaining;
                int pos = rnd.Next(initialPos, end);
                int remaining = stream.StreamLength - pos;
                int maxLen = rnd.Next(1, 30);
                int len = Math.Min(remaining, maxLen);
                ReadOnlySpan<char> span = stream.GetSourceSpan(pos, len);
                result += span.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedRandomGetSourceText()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            Random rnd = new Random(42);
            while (!stream.IsEOF)
            {
                int initialPos = stream.StreamPosition;
                int initialRemaining = stream.StreamLength - initialPos;
                int end = initialPos + initialRemaining;
                int pos = rnd.Next(initialPos, end);
                int remaining = stream.StreamLength - pos;
                int maxLen = rnd.Next(1, 30);
                int len = Math.Min(remaining, maxLen);
                string text = stream.GetSourceText(pos, len);
                result += text.Length;
                stream.AdvanceAuto(len);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedMatchText()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            string comparend = "void";
            while (!stream.IsEOF)
            {
                bool r = stream.MatchText(0, comparend);
                if (r)
                {
                    result += 1;
                    stream.AdvanceAuto(comparend.Length);
                }
                else
                    stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedMatchSpan()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            string comparend = "void";
            ReadOnlySpan<char> span = comparend.AsSpan();
            while (!stream.IsEOF)
            {
                bool r = stream.MatchSpan(0, span);
                if (r)
                {
                    result += 1;
                    stream.AdvanceAuto(comparend.Length);
                }
                else
                    stream.AdvanceAuto(1);
            }
            return result;
        }

        [Benchmark]
        public int AdvancedMatchCharacters()
        {
            int result = 0;
            AdvancedTextStream stream = new AdvancedTextStream(HeaderSource, new TextPosition());
            while (!stream.IsEOF)
            {
                if (stream.MatchCharacters(0, 4, char.IsWhiteSpace))
                {
                    result += 4 * stream.StreamPosition;
                    stream.AdvanceAuto(4);
                } else
                    stream.AdvanceAuto(1);
            }
            return result;
        }
#endif
    }
}
