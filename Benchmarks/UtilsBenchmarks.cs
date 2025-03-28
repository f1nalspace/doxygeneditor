using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using TSP.DoxygenEditor.Languages.Utils;

namespace Benchmarks
{
    [MinColumn, MaxColumn, MedianColumn]
    public class UtilsBenchmarks
    {
        private static readonly char[] _allChars = Enumerable.Range(char.MinValue, char.MaxValue).Select(i => (char)i).ToArray();
        private char[] _randomChars = Array.Empty<char>();

        [GlobalSetup]
        public void GlobalSetup()
        {
            Random rng = new Random(42);
            _randomChars = new char[_allChars.Length];
            _allChars.CopyTo(_randomChars, 0);
            for (int i = _allChars.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                char temp = _randomChars[i];
                _randomChars[i] = _randomChars[j];
                _randomChars[j] = temp;
            }
        }

        [Benchmark]
        public bool SyntaxUtilsForLoop()
        {
            bool result = false;
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                result |= SyntaxUtils.IsAlpha(c);
                result |= SyntaxUtils.IsNumeric(c);
                result |= SyntaxUtils.IsHex(c);
                result |= SyntaxUtils.IsOctal(c);
                result |= SyntaxUtils.IsBinary(c);
                result |= SyntaxUtils.IsSpacing(c);
                result |= SyntaxUtils.IsLineBreak(c);
                result |= SyntaxUtils.IsExponentPrefix(c);
                result |= SyntaxUtils.IsIntegerSuffix(c);
                result |= SyntaxUtils.IsFloatSuffix(c);
                result |= SyntaxUtils.IsIdentStart(c);
                result |= SyntaxUtils.IsIdentPart(c);
                result |= SyntaxUtils.IsFilename(c);
            }
            return result;
        }

        [Benchmark]
        public bool SyntaxUtilsRandom()
        {
            bool result = false;
            foreach (char c in _randomChars)
            {
                result |= SyntaxUtils.IsAlpha(c);
                result |= SyntaxUtils.IsNumeric(c);
                result |= SyntaxUtils.IsHex(c);
                result |= SyntaxUtils.IsOctal(c);
                result |= SyntaxUtils.IsBinary(c);
                result |= SyntaxUtils.IsSpacing(c);
                result |= SyntaxUtils.IsLineBreak(c);
                result |= SyntaxUtils.IsExponentPrefix(c);
                result |= SyntaxUtils.IsIntegerSuffix(c);
                result |= SyntaxUtils.IsFloatSuffix(c);
                result |= SyntaxUtils.IsIdentStart(c);
                result |= SyntaxUtils.IsIdentPart(c);
                result |= SyntaxUtils.IsFilename(c);
            }
            return result;
        }
    }
}
