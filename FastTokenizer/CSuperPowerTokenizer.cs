using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Lexers;

namespace TSP.FastTokenizer
{
    class CSuperPowerTokenizer : Tokenizer<CppTokenKind>
    {
        private readonly List<string> _tokenValues = new List<string>();
        private readonly Dictionary<string, CppTokenKind> _valueToTokenMap = new Dictionary<string, CppTokenKind>();

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

        private TextParser<TextSpan> CreateParserFromStrings(IEnumerable<string> list)
        {
            TextParser<TextSpan> result = null;
            foreach (var item in list)
            {
                if (result == null)
                    result = Span.EqualTo(item);
                else
                    result = result.Or(Span.EqualTo(item));
            }
            return (result);
        }

        public CSuperPowerTokenizer() : base()
        {
            var type = typeof(CppTokenKind);
            var tokenKinds = Enum.GetValues(typeof(CppTokenKind));
            foreach (CppTokenKind tokenKind in tokenKinds)
            {
                var memInfo = type.GetMember(tokenKind.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(TokenKindAttribute), false);
                if (attributes.Length > 0)
                {
                    var text = ((TokenKindAttribute)attributes[0]).Text;
                    _tokenValues.Add(text);
                    _valueToTokenMap.Add(text, tokenKind);
                }
            }
        }

        protected override IEnumerable<Result<CppTokenKind>> Tokenize(TextSpan span)
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
                                CppTokenKind kind = CppTokenKind.MultiLineComment;
                                var content = Comment.CStyle(next.Location);
                                if (content.HasValue)
                                {
                                    TextSpan s = content.Location.Skip(2);
                                    var r = s.ConsumeChar();
                                    if (r.HasValue && DoxygenSyntax.MultiLineDocChars.Contains(r.Value))
                                        kind = CppTokenKind.MultiLineCommentDoc;

                                }
                                yield return Result.Value(kind, content.Location, content.Remainder);
                                next = content.Remainder.ConsumeChar();
                            }
                            else if (tmp.Value == '/')
                            {
                                CppTokenKind kind = CppTokenKind.SingleLineComment;
                                var content = Comment.CPlusPlusStyle(next.Location);
                                if (content.HasValue)
                                {
                                    TextSpan s = content.Location.Skip(2);
                                    var r = s.ConsumeChar();
                                    if (r.HasValue && DoxygenSyntax.SingleLineDocChars.Contains(r.Value))
                                        kind = CppTokenKind.SingleLineCommentDoc;

                                }
                                yield return Result.Value(kind, content.Location, content.Remainder);
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
                            yield return Result.Value(CppTokenKind.StringLiteral, content.Location, content.Remainder);
                            next = content.Remainder.ConsumeChar();
                        }
                        break;

                    case '\'':
                        {
                            var content = CCharParser(next.Location);
                            yield return Result.Value(CppTokenKind.CharLiteral, content.Location, content.Remainder);
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
                                var result = Result.Value(CppTokenKind.HexLiteral, next.Location, content.Remainder);
                                yield return result;
                                next = content.Remainder.ConsumeChar();
                                break;
                            }

                            // Octal
                            content = COctalNumberParser(next.Location);
                            if (content.HasValue)
                            {
                                var result = Result.Value(CppTokenKind.OctalLiteral, next.Location, content.Remainder);
                                yield return result;
                                next = content.Remainder.ConsumeChar();
                                break;
                            }

                            // Decimal-Float
                            content = CDecimalFloatNumberParser(next.Location);
                            if (content.HasValue)
                            {
                                var result = Result.Value(CppTokenKind.IntegerFloatLiteral, next.Location, content.Remainder);
                                yield return result;
                                next = content.Remainder.ConsumeChar();
                                break;
                            }

                            // Hex-Float
                            content = CDecimalHexNumberParser(next.Location);
                            if (content.HasValue)
                            {
                                var result = Result.Value(CppTokenKind.HexadecimalFloatLiteral, next.Location, content.Remainder);
                                yield return result;
                                next = content.Remainder.ConsumeChar();
                                break;
                            }

                            // Integer
                            content = CIntegerNumberParser(next.Location);
                            if (content.HasValue)
                            {
                                var result = Result.Value(CppTokenKind.IntegerLiteral, next.Location, content.Remainder);
                                yield return result;
                                next = content.Remainder.ConsumeChar();
                                break;
                            }

                            throw new ParseException($"Unknown number format for '{next.Location}'");
                        }

                    case char n when ((n >= 'a' && n <= 'z') || (n >= 'A' && n <= 'Z') || (n == '_')):
                        {
                            // Ident
                            CppTokenKind kind = CppTokenKind.IdentLiteral;
                            var content = IdentParser(next.Location);
                            if (content.HasValue)
                            {
                                string value = content.Value.ToStringValue();
                                if (CppLexer.ReservedKeywords.Contains(value))
                                    kind = CppTokenKind.ReservedKeyword;
                                else if (CppLexer.GlobalClassKeywords.Contains(value) || CppLexer.TypeKeywords.Contains(value))
                                    kind = CppTokenKind.TypeKeyword;
                            }
                            yield return Result.Value(kind, content.Location, content.Remainder);
                            next = content.Remainder.ConsumeChar();
                        }
                        break;

                    default:
                        {
                            CppTokenKind foundTokenKind = CppTokenKind.Unknown;
                            string foundTokenValue = null;
                            string source = next.Location.Source;
                            int index = next.Location.Position.Absolute;
                            foreach (var tokenValue in _tokenValues)
                            {
                                if (string.Compare(tokenValue, 0, source, index, tokenValue.Length) == 0)
                                {
                                    foundTokenKind = _valueToTokenMap[tokenValue];
                                    foundTokenValue = tokenValue;
                                    break;
                                }
                            }

                            if (foundTokenKind != CppTokenKind.Unknown)
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
                                yield return Result.Value(CppTokenKind.Unknown, tmp.Location, tmp.Remainder);
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
}
