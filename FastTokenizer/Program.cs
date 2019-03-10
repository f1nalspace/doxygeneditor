/***
C# Test sandbox for comparing and validating several Cpp lexer implementations.
From all my tests, a naive switch-by-char tokenizer is okay in performance - but not really fast.
Parse generators, like SuperPower are super slow :-(
The project CParserTest is a Cpp lexer written in C++ - which is pretty damn fast.
***/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.FastTokenizer
{
    class Program
    {
        enum TokenizerType
        {
            CustomCpp,
            Superpower,
            DoxygenCpp
        }

        static TimeSpan GetAvgTime(List<TimeSpan> list)
        {
            int count = list.Count;
            long ticks = 0;
            foreach (var item in list)
                ticks += item.Ticks;
            return new TimeSpan(ticks / count);
        }
        static TimeSpan GetMinTime(List<TimeSpan> list)
        {
            int count = list.Count;
            TimeSpan result = list.First();
            foreach (var item in list)
            {
                if (item < result)
                    result = item;
            }
            return (result);
        }
        static TimeSpan GetMaxTime(List<TimeSpan> list)
        {
            int count = list.Count;
            TimeSpan result = list.First();
            foreach (var item in list)
            {
                if (item > result)
                    result = item;
            }
            return (result);
        }

        static void CompareTokens(TokenizerType typeA, List<CToken> tokensA, TokenizerType typeB, List<CToken> tokensB)
        {
            int maxCount = Math.Max(tokensA.Count, tokensB.Count);
            for (int i = 0; i < maxCount; ++i)
            {
                CToken? tokenA = (i < tokensA.Count) ? new CToken?(tokensA[i]) : null;
                CToken? tokenB = (i < tokensB.Count) ? new CToken?(tokensB[i]) : null;
                if (tokenA.HasValue && !tokenB.HasValue)
                {
                    Console.WriteLine($"Missing token B at index {i} [{typeB}] -> Found A [{typeA}] = {tokenA.Value.Start}");
                    break;
                }
                else if (tokenB.HasValue && !tokenA.HasValue)
                {
                    Console.WriteLine($"Missing token A at index {i} [{typeA}] -> Found B [{typeB}] = {tokenB.Value.Start}");
                    break;
                }
                else if (!tokenA.HasValue && !tokenB.HasValue)
                {
                    Console.WriteLine($"Missing any token at index {i}");
                    break;
                }
                else
                {
                    if (tokenA.Value.Kind != tokenB.Value.Kind)
                        Console.WriteLine($"Different token kinds ({typeA}:{tokenA.Value.Kind} vs {typeB}:{tokenB.Value.Kind}) on index {i}");
                    if (tokenA.Value.Start.Index != tokenB.Value.Start.Index)
                        Console.WriteLine($"Different token starts ({typeA}:{tokenA.Value.Start} vs {typeB}:{tokenB.Value.Start}) on index {i}");
                    if (tokenA.Value.Length != tokenB.Value.Length)
                        Console.WriteLine($"Different token lengths ({typeA}:{tokenA.Value.Length} vs {typeB}:{tokenB.Value.Length}) on index {i}");
                }
            }
        }

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Missing filepath argument!");
                return (-1);
            }

            string filePath = args[0];
            string source = File.ReadAllText(filePath);

            TokenizerType[] tokenizerTypes = new[] { TokenizerType.Superpower, TokenizerType.DoxygenCpp, TokenizerType.CustomCpp };

            const int numberOfIterationsPerType = 1;
            Dictionary<TokenizerType, List<TimeSpan>> durations = new Dictionary<TokenizerType, List<TimeSpan>>();
            Dictionary<TokenizerType, List<CToken>> tokenMap = new Dictionary<TokenizerType, List<CToken>>();

            foreach (TokenizerType tokenizerType in tokenizerTypes)
            {
                List<TimeSpan> spans = new List<TimeSpan>();
                durations.Add(tokenizerType, spans);
                List<CToken> outTokens = new List<CToken>();
                tokenMap.Add(tokenizerType, outTokens);
                for (int iteration = 1; iteration <= numberOfIterationsPerType; ++iteration)
                {
                    outTokens.Clear();
                    Console.WriteLine($"{tokenizerType} tokenizer[{iteration}/{numberOfIterationsPerType}] start...");
                    Stopwatch timer = Stopwatch.StartNew();
                    switch (tokenizerType)
                    {
                        case TokenizerType.Superpower:
                            {
                                var tokenizer = new CSuperPowerTokenizer();
                                var tokens = tokenizer.Tokenize(source);
                                foreach (var token in tokens)
                                {
                                    if (token.Kind == CppTokenKind.Eof)
                                        break;
                                    var start = new TextPosition(token.Position.Absolute, token.Position.Line - 1, token.Position.Column - 1);
                                    var end = new TextPosition(token.Position.Absolute + token.Span.Length, token.Span.Position.Line - 1, token.Span.Position.Column - 1);
                                    var value = source.Substring(start.Index, end.Index - start.Index);
                                    outTokens.Add(new CToken(token.Kind, start, end, value));
                                }
                            }
                            break;

                        case TokenizerType.CustomCpp:
                            {
                                using (var stream = new BasicTextStream(source, new TextPosition(0), source.Length))
                                {
                                    CToken token;
                                    do
                                    {
                                        token = CTokenizer.GetToken(stream);
                                        if (token.Kind == CppTokenKind.Eof)
                                            break;
                                        outTokens.Add(token);
                                    } while (token.Kind != CppTokenKind.Eof);
                                }
                            }
                            break;

                        case TokenizerType.DoxygenCpp:
                            {
                                using (var lexer = new CppLexer(source, new TextPosition(0), source.Length))
                                {
                                    var tokens = lexer.Tokenize();
                                    foreach (var token in tokens)
                                    {
                                        if (token.Kind == CppTokenKind.Eof)
                                            break;
                                        var start = token.Position;
                                        var end = new TextPosition(token.Position.Index + token.Length, token.Position.Line, token.Position.Column);
                                        var value = source.Substring(start.Index, end.Index - start.Index);
                                        outTokens.Add(new CToken(token.Kind, start, end, value));
                                    }
                                }
                            }
                            break;

                        default:
                            throw new Exception($"Unsupported tokenizer type -> {tokenizerType}");
                    }
                    timer.Stop();
                    spans.Add(timer.Elapsed);
                    Console.WriteLine($"{tokenizerType} tokenizer[{iteration}/{numberOfIterationsPerType}] done, got {outTokens.Count()} tokens, took {timer.Elapsed.TotalMilliseconds} ms");
                }
            }

            foreach (TokenizerType tokenizerType in tokenizerTypes)
            {
                List<TimeSpan> timeSpans = durations[tokenizerType];
                TimeSpan minTime = GetMinTime(timeSpans);
                TimeSpan maxTime = GetMaxTime(timeSpans);
                TimeSpan avgTime = GetAvgTime(timeSpans);
                Console.WriteLine($"{tokenizerType} tokenizer, min: {minTime}, max: {maxTime}, avg: {avgTime}, iterations: {numberOfIterationsPerType}");
            }

#if false
            // Compare tokens against each other
            foreach (TokenizerType tokenizerTypeA in tokenizerTypes)
            {
                List<CToken> tokensA = tokenMap[tokenizerTypeA];
                foreach (TokenizerType tokenizerTypeB in tokenizerTypes)
                {
                    List<CToken> tokensB = tokenMap[tokenizerTypeB];
                    if (tokenizerTypeA != tokenizerTypeB)
                    {
                        CompareTokens(tokenizerTypeA, tokensA, tokenizerTypeB, tokensB);
                    }
                }
            }
#endif

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach (TokenizerType tokenizerType in tokenizerTypes)
            {
                string filename = $"tokenizer_{tokenizerType}.txt";
                string singleFilePath = Path.Combine(desktopPath, filename);
                List<CToken> tokens = tokenMap[tokenizerType];
                using (StreamWriter writer = new StreamWriter(singleFilePath, false, Encoding.ASCII))
                {
                    foreach (var token in tokens)
                    {
                        writer.Write(token);
                        writer.Write("\n");
                    }
                }
            }


            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            return (0);
        }
    }
}
