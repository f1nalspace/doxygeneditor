using System;
using System.Diagnostics;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.FastTokenizer
{
    static class CTokenizer
    {
        private static CToken MakeToken(CppTokenKind kind, TextStream stream, TextPosition startPos, TextPosition endPos)
        {
            int length = endPos.Index - startPos.Index;
            string value = stream.GetSourceText(startPos.Index, length);
            if (kind == CppTokenKind.IdentLiteral)
            {
                if (CppLexer.ReservedKeywords.Contains(value))
                    kind = CppTokenKind.ReservedKeyword;
                else if (CppLexer.TypeKeywords.Contains(value) || CppLexer.GlobalClassKeywords.Contains(value))
                    kind = CppTokenKind.TypeKeyword;
            }
            CToken result = new CToken(kind, startPos, endPos, value);
            return (result);
        }

        private static void AdvanceExponent(TextStream stream, char test)
        {
            char c = stream.Peek();
            if (char.ToLower(test) == char.ToLower(c))
            {
                stream.AdvanceColumn();
                c = stream.Peek();
                if (c == '+' || c == '-')
                    stream.AdvanceColumn();
                stream.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
            }
        }

        private static CppTokenKind ReadNumberLiteral(TextStream stream)
        {
            // D = [0-9]
            // H = [a-fA-F0-9]
            // IS = ((u|U)|(u|U)?(l|L|ll|LL)|(l|L|ll|LL)(u|U))
            // FS = (f|F|l|L)
            // E = ([Ee][+-]?{D}+)
            // P = ([Pp][+-]?{D}+)
            // Hex integer = 0[xX]{H}+{IS}?
            // Octal integer = 0[0-7]*{IS}?
            // Integer = [1-9]{D}*{IS}?
            // Decimal = {D}+{E}{FS}?
            // Decimal = {D}*"."{D}+{E}?{FS}?
            // Decimal = {D}+"."{D}*{E}?{FS}?
            // Decimal = 0[xX]{H}+{P}{FS}?
            // Decimal = 0[xX]{H}*"."{H}+{P}?{FS}?
            // Decimal = 0[xX]{H}+"."{H}*{P}?{FS}?

            // Examples: 42
            // Examples: 427.893
            // Examples: 0x10.1p0
            // Examples: 0x1.2p3
            // Examples: .1E4f
            // Examples: 58.
            // Examples: 123.456e-67
            // Examples: 4e2

            CppTokenKind result;
            char first = stream.Peek(0);
            char second = stream.Peek(1);
            bool dotSeen = false;
            if (first == '0')
            {
                if (second == 'x' || second == 'X')
                {
                    // Hex
                    stream.AdvanceColumns(2); // Skip 0[xX]
                    result = CppTokenKind.HexLiteral;
                }
                else if (second == '.')
                {
                    // 0.123456f decimal
                    result = CppTokenKind.IntegerFloatLiteral;
                }
                else
                {
                    // Octal
                    result = CppTokenKind.OctalLiteral;
                }
            }
            else if (first == '.')
            {
                // .123488E+4 decimal
                Debug.Assert(SyntaxUtils.IsNumeric(second));
                result = CppTokenKind.IntegerFloatLiteral;
                stream.AdvanceColumn();
                dotSeen = true;
            }
            else
            {
                Debug.Assert(SyntaxUtils.IsNumeric(first));
                result = CppTokenKind.IntegerLiteral;
            }

            // First number part
            switch (result)
            {
                case CppTokenKind.IntegerLiteral:
                    stream.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
                    break;

                case CppTokenKind.HexLiteral:
                    stream.AdvanceColumnsWhile(SyntaxUtils.IsHex);
                    break;

                case CppTokenKind.OctalLiteral:
                    stream.AdvanceColumnsWhile(SyntaxUtils.IsOctal);
                    break;

                case CppTokenKind.IntegerFloatLiteral:
                    {
                        // .12345f number without a number start
                    }
                    break;

                default:
                    throw new Exception($"Unsupport number literal token '{result}' as number literal on {stream}");
            }

            if (stream.Peek() == '.')
            {
                stream.AdvanceColumn();
                if (!dotSeen)
                {
                    result = result == CppTokenKind.HexLiteral ? CppTokenKind.HexadecimalFloatLiteral : CppTokenKind.IntegerFloatLiteral;
                    dotSeen = true;
                }
                else
                    throw new Exception($"Invalid decimal literal for token '{result}' as number literal on {stream}");
            }

            if (result == CppTokenKind.IntegerLiteral)
            {
                char n = stream.Peek();
                if (n == 'e' || n == 'E')
                    result = CppTokenKind.IntegerFloatLiteral;
            }

            if (result == CppTokenKind.IntegerLiteral || result == CppTokenKind.OctalLiteral || result == CppTokenKind.HexLiteral)
            {
                // Integer type [uUlL]{1,3}
                stream.AdvanceColumnsWhile((c) => c == 'u' || c == 'U' || c == 'l' || c == 'L', 3);
            }
            else
            {
                // Expontial ([eE][+-]?[0-9]+)|([pP][+-]?[0-9]+)
                if (result == CppTokenKind.HexadecimalFloatLiteral)
                {
                    stream.AdvanceColumnsWhile(SyntaxUtils.IsHex);
                    AdvanceExponent(stream, 'p');
                }
                else
                {
                    stream.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
                    AdvanceExponent(stream, 'e');
                }

                // Double type [fFlL]{0,1}
                char n = stream.Peek();
                if (n == 'f' || n == 'F' || n == 'l' || n == 'L')
                    stream.AdvanceColumn();
            }

            return (result);
        }

        private static CppTokenKind ReadStringLiteral(TextStream stream)
        {
            // @TODO(final): Support for C++ Rbla"()"bla strings
            while (!stream.IsEOF)
            {
                char n = stream.Peek();
                if (n == '\\')
                {
                    // Escape
                    stream.AdvanceColumn();
                    if (!stream.IsEOF)
                        stream.AdvanceColumn();
                    continue;
                }
                else if (n == '"')
                {
                    stream.AdvanceColumn();
                    break;
                }
                stream.AdvanceColumn();
            }
            return CppTokenKind.StringLiteral;
        }

        private static CppTokenKind ReadCharacterLiteral(TextStream stream)
        {
            char n = stream.Peek();
            if (n == '\\')
            {
                stream.AdvanceColumn(); // Skip over backslash
                char escapeChar = stream.Peek();
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
                        stream.AdvanceColumn();
                        break;

                    case 'u':
                    case 'U':
                        {
                            int maxCount = escapeChar == 'u' ? 4 : 8;
                            stream.AdvanceColumn();
                            if (SyntaxUtils.IsNumeric(stream.Peek()))
                                stream.AdvanceColumnsWhile(SyntaxUtils.IsNumeric, maxCount);
                        }
                        break;

                    case 'X':
                        {

                            // Hex escape
                            stream.AdvanceColumn();
                            if (SyntaxUtils.IsHex(stream.Peek()))
                                stream.AdvanceColumnsWhile(SyntaxUtils.IsHex, 2);
                        }
                        break;

                    default:
                        {
                            // Octal escape
                            if (SyntaxUtils.IsOctal(escapeChar))
                                stream.AdvanceColumnsWhile(SyntaxUtils.IsOctal, 3);
                        }
                        break;
                }
            }
            else stream.AdvanceColumn();
            if (stream.Peek() == '\'')
                stream.AdvanceColumn();
            return CppTokenKind.CharLiteral;
        }

        public static CToken PeekTokenRaw(TextStream stream)
        {
            TextPosition startPos = stream.TextPosition;
            if (stream.IsEOF)
                return new CToken(CppTokenKind.Eof, startPos, stream.TextPosition);
            char first = stream.Peek(0);
            char second = stream.Peek(1);
            char third = stream.Peek(2);
            CppTokenKind kind;
            switch (first)
            {
                case '&':
                    {
                        if (second == '&')
                        {
                            kind = CppTokenKind.LogicalAndOp;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.AndAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.AndOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '|':
                    {
                        if (second == '|')
                        {
                            kind = CppTokenKind.LogicalOrOp;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.OrAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.OrOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '=':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.LogicalEqualsOp;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.EqOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '!':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.LogicalNotEqualsOp;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.ExclationMark;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '<':
                    {
                        if (second == '<')
                        {
                            if (third == '=')
                            {
                                kind = CppTokenKind.LeftShiftAssign;
                                stream.AdvanceColumns(3);
                            }
                            else
                            {
                                kind = CppTokenKind.LeftShiftOp;
                                stream.AdvanceColumns(2);
                            }
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.LessOrEqualOp;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.LessThanOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '>':
                    {
                        if (second == '>')
                        {
                            if (third == '=')
                            {
                                kind = CppTokenKind.RightShiftAssign;
                                stream.AdvanceColumns(3);
                            }
                            else
                            {
                                kind = CppTokenKind.RightShiftOp;
                                stream.AdvanceColumns(2);
                            }
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.GreaterOrEqualOp;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.GreaterThanOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '+':
                    {
                        if (second == '+')
                        {
                            kind = CppTokenKind.IncOp;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.AddAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.AddOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '-':
                    {
                        if (second == '-')
                        {
                            kind = CppTokenKind.DecOp;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            kind = CppTokenKind.SubAssign;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '>')
                        {
                            kind = CppTokenKind.PtrOp;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.SubOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '/':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.DivAssign;
                            stream.AdvanceColumns(2);
                        }
                        else if (second == '/')
                        {
                            kind = CppTokenKind.SingleLineComment;
                            stream.AdvanceColumns(2);
                            char specialChar = stream.Peek();
                            if (DoxygenSyntax.SingleLineDocChars.Contains(specialChar))
                            {
                                kind = CppTokenKind.SingleLineCommentDoc;
                                stream.AdvanceColumn();
                            }
                            while (!stream.IsEOF)
                            {
                                if (SyntaxUtils.IsLineBreak(stream.Peek()))
                                    break;
                                stream.AdvanceColumn();
                            }
                        }
                        else if (second == '*')
                        {
                            kind = CppTokenKind.MultiLineComment;
                            stream.AdvanceColumns(2);
                            char specialChar = stream.Peek();
                            if (DoxygenSyntax.MultiLineDocChars.Contains(specialChar))
                            {
                                kind = CppTokenKind.MultiLineCommentDoc;
                                stream.AdvanceColumn();
                            }
                            while (!stream.IsEOF)
                            {
                                char n0 = stream.Peek();
                                char n1 = stream.Peek(1);
                                if (n0 == '*' && n1 == '/')
                                {
                                    stream.AdvanceColumns(2);
                                    break;
                                }
                                else
                                    stream.AdvanceManual(n0, n1);
                            }
                        }
                        else
                        {
                            stream.AdvanceColumn();
                            kind = CppTokenKind.DivOp;
                        }
                    }
                    break;

                case '*':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.MulAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.MulOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '%':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.ModAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.ModOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '.':
                    {
                        if (second == '.' && third == '.')
                        {
                            kind = CppTokenKind.Ellipsis;
                            stream.AdvanceColumns(3);
                        }
                        else if (SyntaxUtils.IsNumeric(second))
                        {
                            stream.AdvanceColumn();
                            kind = ReadNumberLiteral(stream);
                        }
                        else
                        {
                            kind = CppTokenKind.Dot;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '^':
                    {
                        if (second == '=')
                        {
                            kind = CppTokenKind.XorAssign;
                            stream.AdvanceColumns(2);
                        }
                        else
                        {
                            kind = CppTokenKind.XorOp;
                            stream.AdvanceColumn();
                        }
                    }
                    break;

                case '"':
                case '\'':
                    {
                        stream.AdvanceColumn();
                        if (first == '"')
                            kind = ReadStringLiteral(stream);
                        else
                            kind = ReadCharacterLiteral(stream);
                    }
                    break;

                case '~':
                    kind = CppTokenKind.Tilde;
                    stream.AdvanceColumn();
                    break;
                case '\\':
                    kind = CppTokenKind.Backslash;
                    stream.AdvanceColumn();
                    break;
                case '#':
                    kind = CppTokenKind.PreprocessorStart;
                    stream.AdvanceColumn();
                    break;
                case ',':
                    kind = CppTokenKind.Comma;
                    stream.AdvanceColumn();
                    break;
                case ';':
                    kind = CppTokenKind.Semicolon;
                    stream.AdvanceColumn();
                    break;
                case ':':
                    kind = CppTokenKind.Colon;
                    stream.AdvanceColumn();
                    break;
                case '?':
                    kind = CppTokenKind.QuestionMark;
                    stream.AdvanceColumn();
                    break;
                case '{':
                    kind = CppTokenKind.LeftBrace;
                    stream.AdvanceColumn();
                    break;
                case '}':
                    kind = CppTokenKind.RightBrace;
                    stream.AdvanceColumn();
                    break;
                case '[':
                    kind = CppTokenKind.LeftBracket;
                    stream.AdvanceColumn();
                    break;
                case ']':
                    kind = CppTokenKind.RightBracket;
                    stream.AdvanceColumn();
                    break;
                case '(':
                    kind = CppTokenKind.LeftParen;
                    stream.AdvanceColumn();
                    break;
                case ')':
                    kind = CppTokenKind.RightParen;
                    stream.AdvanceColumn();
                    break;

                default:
                    {
                        if (SyntaxUtils.IsLineBreak(first))
                        {
                            kind = CppTokenKind.EndOfLine;
                            int nb = SyntaxUtils.GetLineBreakChars(first, second);
                            stream.AdvanceLine(nb);
                        }
                        else if (first == '\t')
                        {
                            kind = CppTokenKind.Spacings;
                            while (!stream.IsEOF)
                            {
                                if (stream.Peek() != '\t')
                                    break;
                                stream.AdvanceTab();
                            }
                        }
                        else if (SyntaxUtils.IsSpacing(first))
                        {
                            kind = CppTokenKind.Spacings;
                            stream.AdvanceColumnsWhile(SyntaxUtils.IsSpacing);
                        }
                        else if (SyntaxUtils.IsIdentStart(first))
                        {
                            kind = CppTokenKind.IdentLiteral;
                            stream.AdvanceColumnsWhile(SyntaxUtils.IsIdentPart);
                        }
                        else if (SyntaxUtils.IsNumeric(first))
                            kind = ReadNumberLiteral(stream);
                        else
                            kind = CppTokenKind.Unknown;
                    }
                    break;
            }
            CToken result = MakeToken(kind, stream, startPos, stream.TextPosition);
            return (result);
        }
        public static CToken GetTokenRaw(TextStream cursor)
        {
            CToken result = PeekTokenRaw(cursor);
            if (result.Kind != CppTokenKind.Eof)
            {
                Debug.Assert(result.Length > 0);
                cursor.Seek(result.End);
            }
            return (result);
        }
        public static CToken PeekToken(TextStream cursor)
        {
            TextPosition startPos = cursor.TextPosition;
            CToken token = PeekTokenRaw(cursor);
            do
            {
                if (token.Kind == CppTokenKind.Eof)
                    break;
                if (!(token.Kind == CppTokenKind.Spacings || token.Kind == CppTokenKind.EndOfLine))
                    break;
                Debug.Assert(token.Length > 0);
                cursor.Seek(token.End);
                token = PeekTokenRaw(cursor);
            } while (!cursor.IsEOF);
            cursor.Seek(startPos);
            return (token);
        }
        public static CToken GetToken(TextStream cursor)
        {
            CToken result = PeekToken(cursor);
            if (result.Kind != CppTokenKind.Eof)
            {
                Debug.Assert(result.Length > 0);
                cursor.Seek(result.End);
            }
            return (result);
        }
    }
}
