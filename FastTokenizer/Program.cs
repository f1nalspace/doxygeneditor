using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace FastTokenizer
{
    class Program
    {
        enum CTokenKind
        {
            Unknown = 0,
            Spacings,
            EndOfLine,
            SingleLineComment,
            MultiLineComment,
#if false
            Preprocessor,
#endif
            Ident,
            StringLiteral,
            CharLiteral,
            IntegerLiteral,
            HexLiteral,
            OctalLiteral,
            Binary,
            DecimalHexLiteral,
            DecimalFloatLiteral,

            [Token(Example = "...")]
            Ellipsis,
            [Token(Example = ">>=")]
            RightAssign,
            [Token(Example = "<<=")]
            LeftAssign,
            [Token(Example = "+=")]
            AddAssign,
            [Token(Example = "-=")]
            SubAssign,
            [Token(Example = "*=")]
            MulAssign,
            [Token(Example = "/=")]
            DivAssign,
            [Token(Example = "%=")]
            ModAssign,
            [Token(Example = "&=")]
            AndAssign,
            [Token(Example = "^=")]
            XorAssign,
            [Token(Example = "|=")]
            OrAssign,
            [Token(Example = ">>")]
            RightOp,
            [Token(Example = "<<")]
            LeftOp,
            [Token(Example = "++")]
            IncOp,
            [Token(Example = "--")]
            DecOp,
            [Token(Example = "->")]
            PtrOp,
            [Token(Example = "&&")]
            AndOp,
            [Token(Example = "||")]
            OrOp,
            [Token(Example = "<=")]
            LeOp,
            [Token(Example = ">=")]
            GeOp,
            [Token(Example = "==")]
            EqOp,
            [Token(Example = "!=")]
            NeOp,

            [Token(Example = ";")]
            Semicolon,
            [Token(Example = "{")]
            LeftBrace,
            [Token(Example = "}")]
            RightBrace,
            [Token(Example = ",")]
            Comma,
            [Token(Example = ":")]
            Colon,
            [Token(Example = "=")]
            EqualsSign,
            [Token(Example = "(")]
            LeftParen,
            [Token(Example = ")")]
            RightParen,
            [Token(Example = "[")]
            LeftBracket,
            [Token(Example = "]")]
            RightBracket,
            [Token(Example = ".")]
            Dot,
            [Token(Example = "&")]
            Ampersand,
            [Token(Example = "!")]
            ExlamationMark,
            [Token(Example = "~")]
            Tilde,
            [Token(Example = "-")]
            Minus,
            [Token(Example = "+")]
            Plus,
            [Token(Example = "*")]
            Asterisk,
            [Token(Example = "/")]
            Slash,
            [Token(Example = "%")]
            Percent,
            [Token(Example = "<")]
            Lesser,
            [Token(Example = ">")]
            Greater,
            [Token(Example = "^")]
            CircumFlex,
            [Token(Example = "|")]
            Pipe,
            [Token(Example = "?")]
            QuestionMark,

            [Token(Example = "#")]
            Raute,
            [Token(Example = "\\")]
            Backslash,
        }

        static TextParser<char> AlphaChar = Character.Matching(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'), "Alpha");
        static TextParser<char> NumericChar = Character.Matching(c => (c >= '0' && c <= '9'), "Numeric");
        static TextParser<char> AlphaNumericChar = AlphaChar.Or(NumericChar);
        static TextParser<char> HexChar = Character.Matching(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'), "Hex");
        static TextParser<char> OctalChar = Character.Matching(c => c >= '0' && c <= '7', "Octal");
        static TextParser<char> BinaryChar = Character.Matching(c => c == '0' && c == '1', "Binary");
        static TextParser<char> NaturalChar = Character.Matching(c => c >= '0' && c <= '9', "Natural");
        static TextParser<char> SpacingChar = Character.In(' ', '\t', '\v', '\f');
        static TextParser<char> LineBreakChar = Character.In('\r', '\n');

        static TextParser<TextSpan> SpacingParser = Span.MatchedBy(SpacingChar.AtLeastOnce());
        static TextParser<TextSpan> LineBreakParser = Span.MatchedBy(LineBreakChar.AtLeastOnce());
        static TextParser<TextSpan> WhitespaceParser = Span.MatchedBy(SpacingParser.Or(LineBreakParser));

        static TextParser<TextSpan> IdentParser = Span.MatchedBy(AlphaChar.Or(Character.EqualTo('_')).IgnoreThen(AlphaNumericChar.Or(Character.EqualTo('_')).Many()));

        static TextParser<TextSpan> PreprocessorNextLineDelimiter = Span.MatchedBy(Character.EqualTo('\\'));

        static TextParser<string> EParser =
            from first in Character.In('e', 'E', 'p', 'P')
            from sign in Character.In('+', '-').Optional()
            from rest in NaturalChar.AtLeastOnce()
            select first + (sign.HasValue ? "" + sign.Value : "") + new string(rest);

        static TextParser<TextSpan> ISParser = Span.MatchedBy(Character.In('u', 'U', 'l', 'L').Many());

        static TextParser<TextSpan> CHexNumberParser =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Character.EqualToIgnoreCase('x')))
            .Then(_ => Span.MatchedBy(HexChar.AtLeastOnce()))
            .Then(_ => Span.MatchedBy(ISParser.Optional()));

        static TextParser<TextSpan> COctalNumberParser =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Span.MatchedBy(OctalChar.Many())))
            .Then(_ => Span.MatchedBy(ISParser.Optional()));

        static TextParser<TextSpan> CBinaryNumberParser =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Character.EqualToIgnoreCase('b')))
            .Then(_ => Span.MatchedBy(BinaryChar.AtLeastOnce()))
            .Then(_ => Span.MatchedBy(ISParser.Optional()));

        static TextParser<TextSpan> CIntegerNumberParser =
            Span.MatchedBy(NaturalChar.Where(f => f != '0'))
            .Then(_ => Span.MatchedBy(NaturalChar.Many()))
            .Then(_ => Span.MatchedBy(ISParser.Optional()));

        static TextParser<char> FSParser = Character.In('f', 'F', 'l', 'L');

        // {D}+{E}{FS}?
        static TextParser<TextSpan> CDecimalFloatNumberParser_Simple =
            Span.MatchedBy(NaturalChar.AtLeastOnce())
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // {D}*\\.{D}+{E}?{FS}?
        static TextParser<TextSpan> CDecimalFloatNumberParser_Complex1 =
            Span.MatchedBy(NaturalChar.Many())
            .Then(_ => Span.MatchedBy(Character.EqualTo('.')))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(NaturalChar.AtLeastOnce()))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // {D}+\\.{D}*{E}?{FS}?
        static TextParser<TextSpan> CDecimalFloatNumberParser_Complex2 =
            Span.MatchedBy(NaturalChar.AtLeastOnce())
            .Then(_ => Span.MatchedBy(Character.EqualTo('.')))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(NaturalChar.Many()))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // All 3 decimal float
        static TextParser<TextSpan> CDecimalFloatNumberParser =
            CDecimalFloatNumberParser_Simple
            .Or(CDecimalFloatNumberParser_Complex1)
            .Or(CDecimalFloatNumberParser_Complex2);

        // 0[xX]{H}+{P}{FS}?
        static TextParser<TextSpan> CDecimalHexNumberParser_Simple =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Character.EqualToIgnoreCase('x')))
            .Then(_ => Span.MatchedBy(HexChar.AtLeastOnce()))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // 0[xX]{H}*\\.{H}+{P}?{FS}?
        static TextParser<TextSpan> CDecimalHexNumberParser_Complex1 =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Character.EqualToIgnoreCase('x')))
            .Then(_ => Span.MatchedBy(HexChar.Many()))
            .Then(_ => Span.MatchedBy(Character.EqualTo('.')))
            .Then(_ => Span.MatchedBy(HexChar.AtLeastOnce()))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // 0[xX]{H}+\\.{H}*{P}?{FS}?
        static TextParser<TextSpan> CDecimalHexNumberParser_Complex2 =
            Span.MatchedBy(Character.EqualTo('0'))
            .Then(_ => Span.MatchedBy(Character.EqualToIgnoreCase('x')))
            .Then(_ => Span.MatchedBy(HexChar.AtLeastOnce()))
            .Then(_ => Span.MatchedBy(Character.EqualTo('.')))
            .Then(_ => Span.MatchedBy(HexChar.Many()))
            .Then(_ => EParser.OptionalOrDefault(""))
            .Then(_ => Span.MatchedBy(FSParser.Optional()));

        // All 3 decimal hex parser
        static TextParser<TextSpan> CDecimalHexNumberParser =
            CDecimalHexNumberParser_Simple
            .Or(CDecimalHexNumberParser_Complex1)
            .Or(CDecimalHexNumberParser_Complex2);

        static TextParser<TextSpan> PreprocessorParser
        {
            get
            {
                var beginPreprocessor = Span.EqualTo("#");
                return i =>
                {
                    // #
                    var content = beginPreprocessor(i);
                    if (!content.HasValue)
                        return content;
                    var remainder = content.Remainder;
                    while (!remainder.IsAtEnd)
                    {
                        content = Comment.CPlusPlusStyle.Or(Comment.CStyle)(remainder);
                        if (content.HasValue)
                            return Result.Value(i.Until(remainder), i, remainder);
                        bool skipOverLineBreak = false;
                        content = PreprocessorNextLineDelimiter(remainder);
                        if (content.HasValue)
                        {
                            remainder = content.Remainder;
                            skipOverLineBreak = true;
                        }
                        content = LineBreakParser(remainder);
                        if (content.HasValue && !skipOverLineBreak)
                            return Result.Value(i.Until(remainder), i, remainder);
                        remainder = remainder.ConsumeChar().Remainder;
                    }
                    return Result.Value(i.Until(remainder), i, remainder);
                };
            }
        }

        static TextParser<TextSpan> CStringParser
        {
            get
            {
                return i =>
                {
                    var next = Character.EqualTo('"')(i);
                    if (!next.HasValue)
                        return Result.Empty<TextSpan>(next.Location);
                    next = next.Remainder.ConsumeChar();
                    while (next.HasValue)
                    {
                        if (next.Value == '\\')
                            next = next.Remainder.ConsumeChar();
                        else if (next.Value == '"')
                            break;
                        else
                            next = next.Remainder.ConsumeChar();
                    }
                    return Result.Value(i.Until(next.Location), i, next.Remainder);
                };
            }
        }

        static TextParser<TextSpan> CCharParser
        {
            get
            {
                return i =>
                {
                    Result<char> next = Character.EqualTo('\'')(i);
                    if (!next.HasValue)
                        return Result.Empty<TextSpan>(next.Location);
                    next = next.Remainder.ConsumeChar();
                    if (next.Value == '\\')
                    {
                        next = next.Remainder.ConsumeChar();
                        char escapeChar = next.Value;
                        switch (escapeChar)
                        {
                            case '\'':
                            case '"':
                            case '?':
                            case '\\':
                            case 'a':
                            case 'b':
                            case 'f':
                            case 'n':
                            case 'r':
                            case 't':
                            case 'v':
                                next = next.Remainder.ConsumeChar();
                                break;

                            case 'u':
                            case 'U':
                                {
                                    // Code point
                                    throw new ParseException("Codepoint character literal escape not supported!");
                                }

                            case 'X':
                                {
                                    // Hex
                                    throw new ParseException("Hex character literal escape not supported!");
                                }

                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                                {
                                    // Octal
                                    while (next.HasValue)
                                    {
                                        if (!(next.Value >= '0' && next.Value <= '7'))
                                            break;
                                        next = next.Remainder.ConsumeChar();
                                    }
                                }
                                break;

                            default:
                                throw new ParseException($"Unsupported char literal escape character '{escapeChar}' on '{next}'");
                        }
                    }
                    else if (next.Value != '\'')
                        next = next.Remainder.ConsumeChar();
                    if (next.Value != '\'')
                        throw new ParseException($"Unterminated char literal on '{next}'");
                    next = next.Remainder.ConsumeChar();
                    var r = Result.Value(i.Until(next.Location), i, next.Location);
                    return r;
                };
            }
        }

        class CTokenizer : Tokenizer<CTokenKind>
        {
            private readonly List<string> tokenValues = new List<string>();
            private readonly Dictionary<string, CTokenKind> valueToTokenMap = new Dictionary<string, CTokenKind>();

            public CTokenizer() : base()
            {
                var type = typeof(CTokenKind);
                var tokenKinds = Enum.GetValues(typeof(CTokenKind));
                foreach (CTokenKind tokenKind in tokenKinds)
                {
                    var memInfo = type.GetMember(tokenKind.ToString());
                    var attributes = memInfo[0].GetCustomAttributes(typeof(TokenAttribute), false);
                    if (attributes.Length > 0)
                    {
                        var exampleValue = ((TokenAttribute)attributes[0]).Example;
                        tokenValues.Add(exampleValue);
                        valueToTokenMap.Add(exampleValue, tokenKind);
                    }
                }
            }

            protected override IEnumerable<Result<CTokenKind>> Tokenize(TextSpan span)
            {
                var next = SkipWhiteSpace(span);
                if (!next.HasValue)
                    yield break;
                do
                {
                    char ch = next.Value;
                    var prevLocation = next.Location;
                    switch (ch)
                    {
                        case '/':
                            {
                                // Consume /
                                var tmp = next.Remainder.ConsumeChar();
                                if (tmp.Value == '*')
                                {
                                    var content = Comment.CStyle(next.Location);
                                    yield return Result.Value(CTokenKind.MultiLineComment, content.Location, content.Remainder);
                                    next = content.Remainder.ConsumeChar();
                                }
                                else if (tmp.Value == '/')
                                {
                                    var content = Comment.CPlusPlusStyle(next.Location);
                                    yield return Result.Value(CTokenKind.SingleLineComment, content.Location, content.Remainder);
                                    next = content.Remainder.ConsumeChar();
                                }
                                else goto default;
                            }
                            break;

#if false
                        case '#':
                            {
                                var content = PreprocessorParser(next.Location);
                                yield return Result.Value(CTokenKind.Preprocessor, content.Location, content.Remainder);
                                next = content.Remainder.ConsumeChar();
                            }
                            break;
#endif

                        case '"':
                            {
                                var content = CStringParser(next.Location);
                                yield return Result.Value(CTokenKind.StringLiteral, content.Location, content.Remainder);
                                next = content.Remainder.ConsumeChar();
                            }
                            break;

                        case '\'':
                            {
                                var content = CCharParser(next.Location);
                                yield return Result.Value(CTokenKind.CharLiteral, content.Location, content.Remainder);
                                next = content.Remainder.ConsumeChar();
                            }
                            break;

                        case '.':
                            {
                                var tmp = next.Remainder.ConsumeChar();
                                if (tmp.Value >= '0' && tmp.Value <= '9')
                                    goto case '0';
                                goto default;
                            }

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            {

                                // Hex
                                var content = CHexNumberParser(next.Location);
                                if (content.HasValue)
                                {
                                    var result = Result.Value(CTokenKind.HexLiteral, next.Location, content.Remainder);
                                    yield return result;
                                    next = content.Remainder.ConsumeChar();
                                    break;
                                }

                                // Octal
                                content = COctalNumberParser(next.Location);
                                if (content.HasValue)
                                {
                                    var result = Result.Value(CTokenKind.OctalLiteral, next.Location, content.Remainder);
                                    yield return result;
                                    next = content.Remainder.ConsumeChar();
                                    break;
                                }

                                // Integer
                                content = CIntegerNumberParser(next.Location);
                                if (content.HasValue)
                                {
                                    var result = Result.Value(CTokenKind.IntegerLiteral, next.Location, content.Remainder);
                                    yield return result;
                                    next = content.Remainder.ConsumeChar();
                                    break;
                                }

                                // Decimal-Float
                                content = CDecimalFloatNumberParser(next.Location);
                                if (content.HasValue)
                                {
                                    var result = Result.Value(CTokenKind.DecimalFloatLiteral, next.Location, content.Remainder);
                                    yield return result;
                                    next = content.Remainder.ConsumeChar();
                                    break;
                                }

                                // Hex-Float
                                content = CDecimalHexNumberParser(next.Location);
                                if (content.HasValue)
                                {
                                    var result = Result.Value(CTokenKind.DecimalHexLiteral, next.Location, content.Remainder);
                                    yield return result;
                                    next = content.Remainder.ConsumeChar();
                                    break;
                                }

                                throw new ParseException($"Unknown number format for '{next.Location}'");
                            }

                        case char n when ((n >= 'a' && n <= 'z') || (n >= 'A' && n <= 'Z') || (n == '_')):
                            {
                                // Ident
                                var content = IdentParser(next.Location);
                                yield return Result.Value(CTokenKind.Ident, content.Location, content.Remainder);
                                next = content.Remainder.ConsumeChar();
                            }
                            break;

                        default:
                            {
                                CTokenKind foundTokenKind = CTokenKind.Unknown;
                                string foundTokenValue = null;
                                string source = next.Location.Source;
                                int index = next.Location.Position.Absolute;
                                foreach (var tokenValue in tokenValues)
                                {
                                    if (string.Compare(tokenValue, 0, source, index, tokenValue.Length) == 0)
                                    {
                                        foundTokenKind = valueToTokenMap[tokenValue];
                                        foundTokenValue = tokenValue;
                                        break;
                                    }
                                }

                                if (foundTokenKind != CTokenKind.Unknown)
                                {
                                    var content = Span.EqualTo(foundTokenValue)(next.Location);
                                    Debug.Assert(content.HasValue && content.Value.Length > 0);
                                    yield return Result.Value(foundTokenKind, content.Location, content.Remainder);
                                    next = content.Remainder.ConsumeChar();
                                }
                                else
                                {
                                    var tmp = next.Location.ConsumeChar();
                                    Debug.Assert(tmp.HasValue);
                                    yield return Result.Value(CTokenKind.Unknown, tmp.Location, tmp.Remainder);
                                }
                            }
                            break;
                    }

                    if (next.Location.Equals(prevLocation))
                    {
                        // ERROR: Consume next character while cursor not changed
                        next = next.Remainder.ConsumeChar();
                    }

                    next = SkipWhiteSpace(next.Location);
                } while (next.HasValue);
            }
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Missing arguments!");
                return (-1);
            }
            string tokenizerType = args[0];
            string filePath = args[1];
            string source = File.ReadAllText(filePath);

            var p = CIntegerNumberParser(new TextSpan("600"));



            Stopwatch timer = new Stopwatch();
            switch (tokenizerType.ToLower())
            {
                case "superpower":
                    {
                        Console.WriteLine($"Superpower tokenizer start...");
                        timer.Restart();
                        var tokens = new CTokenizer().Tokenize(source);
                        foreach (var token in tokens)
                        {
                            Console.WriteLine(token);
                        }
                        timer.Stop();
                        Console.WriteLine($"Superpower tokenizer got {tokens.Count()} tokens, took {timer.Elapsed.TotalMilliseconds}");
                    }
                    break;

                default:
                    {
                        Console.WriteLine($"Custom tokenizer start...");
                        timer.Restart();
                        var stream = new BasicTextStream(source, 0, source.Length);
                        var cursor = new StreamCursor(stream);
                        var tokens = new List<TSP.DoxygenEditor.TextAnalysis.Token>();
                        while (!cursor.IsEOF)
                        {
                            var token = CustomTokenizer.GetToken(cursor);
                            if (token.Kind == TokenKind.EOF)
                                break;
                            tokens.Add(token);
                        }
                        foreach (var token in tokens)
                        {
                            Console.WriteLine(token);
                        }
                        timer.Stop();
                        Console.WriteLine($"Custom tokenizer got {tokens.Count()} tokens, took {timer.Elapsed.TotalMilliseconds}");
                    }
                    break;
            }

            return (0);
        }
    }
}
