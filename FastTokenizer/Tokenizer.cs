using System;
using System.Diagnostics;

namespace TSP.FastTokenizer
{
    static class CustomTokenizer
    {
        public static bool IsIdentStart(char c)
        {
            bool result = char.IsLetter(c) || c == '_';
            return (result);
        }
        public static bool IsIdentPart(char c)
        {
            bool result = char.IsLetterOrDigit(c) || c == '_';
            return (result);
        }
        public static bool IsInteger(char c)
        {
            bool result = c >= '0' && c <= '9';
            return (result);
        }
        public static bool IsHex(char c)
        {
            bool result = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            return (result);
        }
        public static bool IsOctal(char c)
        {
            bool result = (c >= '0' && c <= '7');
            return (result);
        }
        public static bool IsSpacing(char c)
        {
            bool result = c == ' ' || c == '\t' || c == '\v' || c == '\f';
            return (result);
        }
        public static bool IsEndOfLine(char c)
        {
            bool result = c == '\n' || c == '\r';
            return (result);
        }

        private static Token MakeToken(TokenKind kind, TextCursor prevState, TextCursor newState)
        {
            int length = newState.Position.Index - prevState.Position.Index;
            string value = prevState.Stream.GetStreamText(prevState.Position.Index, length);
            Token result = new Token(kind, prevState.Position, newState.Position, value);
            return (result);
        }

        private static void AdvanceExponent(TextCursor cursor, char test)
        {
            char c = cursor.Peek();
            if (char.ToLower(test) == char.ToLower(c))
            {
                cursor.AdvanceColumn();
                c = cursor.Peek();
                if (c == '+' || c == '-')
                    cursor.AdvanceColumn();
                cursor.AdvanceColumnsWhile(IsInteger);
            }
        }

        private static TokenKind ReadNumberLiteral(TextCursor cursor, char first)
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

            TokenKind result;
            char second = cursor.Peek();
            bool dotSeen = false;
            if (first == '0')
            {
                if (second == 'x' || second == 'X')
                {
                    // Hex
                    cursor.AdvanceColumn(); // Skip [xX]
                    result = TokenKind.HexLiteral;
                }
                else if (second == '.')
                {
                    // 0.123456f decimal
                    result = TokenKind.DecimalFloatLiteral;
                }
                else
                {
                    // Octal
                    result = TokenKind.OctalLiteral;
                }
            }
            else if (first == '.')
            {
                // .123488E+4 decimal
                Debug.Assert(IsInteger(second));
                result = TokenKind.DecimalFloatLiteral;
                cursor.AdvanceColumn();
                dotSeen = true;
            }
            else
            {
                Debug.Assert(IsInteger(first));
                result = TokenKind.IntegerLiteral;
            }

            // First number part
            switch (result)
            {
                case TokenKind.IntegerLiteral:
                    cursor.AdvanceColumnsWhile(IsInteger);
                    break;

                case TokenKind.HexLiteral:
                    cursor.AdvanceColumnsWhile(IsHex);
                    break;

                case TokenKind.OctalLiteral:
                    cursor.AdvanceColumnsWhile(IsOctal);
                    break;

                case TokenKind.DecimalFloatLiteral:
                    {
                        // .12345f number without a number start
                    }
                    break;

                default:
                    throw new Exception($"Unsupport number literal token '{result}' as number literal on {cursor}");
            }

            if (cursor.Peek() == '.')
            {
                cursor.AdvanceColumn();
                if (!dotSeen)
                {
                    result = result == TokenKind.HexLiteral ? TokenKind.DecimalHexLiteral : TokenKind.DecimalFloatLiteral;
                    dotSeen = true;
                }
                else
                    throw new Exception($"Invalid decimal literal for token '{result}' as number literal on {cursor}");
            }

            if (result == TokenKind.IntegerLiteral)
            {
                char n = cursor.Peek();
                if (n == 'e' || n == 'E')
                    result = TokenKind.DecimalFloatLiteral;
            }

            if (result == TokenKind.IntegerLiteral || result == TokenKind.OctalLiteral || result == TokenKind.HexLiteral)
            {
                // Integer type [uUlL]{1,3}
                cursor.AdvanceColumnsWhile((c) => c == 'u' || c == 'U' || c == 'l' || c == 'L', 3);
            }
            else
            {
                // Expontial ([eE][+-]?[0-9]+)|([pP][+-]?[0-9]+)
                if (result == TokenKind.DecimalHexLiteral)
                {
                    cursor.AdvanceColumnsWhile(IsHex);
                    AdvanceExponent(cursor, 'p');
                }
                else
                {
                    cursor.AdvanceColumnsWhile(IsInteger);
                    AdvanceExponent(cursor, 'e');
                }

                // Double type [fFlL]{0,1}
                char n = cursor.Peek();
                if (n == 'f' || n == 'F' || n == 'l' || n == 'L')
                    cursor.AdvanceColumn();
            }

            return (result);
        }

        private static TokenKind ReadStringLiteral(TextCursor cursor)
        {
            // @TODO(final): Support for C++ Rbla"()"bla strings
            while (cursor.IsParsing)
            {
                char n = cursor.Peek();
                if (n == '\\')
                {
                    // Escape
                    cursor.AdvanceColumn();
                    if (!cursor.IsEOF)
                        cursor.AdvanceColumn();
                    continue;
                }
                else if (n == '"')
                {
                    cursor.AdvanceColumn();
                    break;
                }
                cursor.AdvanceColumn();
            }
            return TokenKind.StringLiteral;
        }

        private static TokenKind ReadCharacterLiteral(TextCursor cursor)
        {
            char n = cursor.Peek();
            if (n == '\\')
            {
                cursor.AdvanceColumn(); // Skip over backslash
                char escapeChar = cursor.Peek();
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
                        cursor.AdvanceColumn();
                        break;

                    case 'u':
                    case 'U':
                        {
                            int maxCount = escapeChar == 'u' ? 4 : 8;
                            cursor.AdvanceColumn();
                            if (IsInteger(cursor.Peek()))
                                cursor.AdvanceColumnsWhile(IsInteger, maxCount);
                        }
                        break;

                    case 'X':
                        {

                            // Hex escape
                            cursor.AdvanceColumn();
                            if (IsHex(cursor.Peek()))
                                cursor.AdvanceColumnsWhile(IsHex, 2);
                        }
                        break;

                    default:
                        {
                            // Octal escape
                            if (IsOctal(escapeChar))
                                cursor.AdvanceColumnsWhile(IsOctal, 3);
                        }
                        break;
                }
            }
            else cursor.AdvanceColumn();
            if (cursor.Peek() == '\'')
                cursor.AdvanceColumn();
            return TokenKind.CharLiteral;
        }

        public static Token PeekTokenRaw(TextCursor cursor)
        {
            TextCursor newCursor = new TextCursor(cursor);
            if (cursor.IsEOF || cursor.HasError)
                return new Token(TokenKind.EOF, cursor.Position, newCursor.Position);
            char first = newCursor.Next();
            char second = newCursor.Peek();
            char third = newCursor.Peek(1);
            TokenKind kind;
            switch (first)
            {
                case '&':
                    {
                        if (second == '&')
                        {
                            kind = TokenKind.AndOp;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.AndAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Ampersand;
                    }
                    break;

                case '|':
                    {
                        if (second == '|')
                        {
                            kind = TokenKind.OrOp;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.OrAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Pipe;
                    }
                    break;

                case '=':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.EqOp;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.EqualsSign;
                    }
                    break;

                case '!':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.NeOp;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.ExlamationMark;
                    }
                    break;

                case '<':
                    {
                        if (second == '<')
                        {
                            if (third == '=')
                            {
                                kind = TokenKind.LeftAssign;
                                newCursor.AdvanceColumn(2);
                            }
                            else
                            {
                                kind = TokenKind.LeftOp;
                                newCursor.AdvanceColumn();
                            }
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.LeOp;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Lesser;
                    }
                    break;

                case '>':
                    {
                        if (second == '>')
                        {
                            if (third == '=')
                            {
                                kind = TokenKind.RightAssign;
                                newCursor.AdvanceColumn(2);
                            }
                            else
                            {
                                kind = TokenKind.RightOp;
                                newCursor.AdvanceColumn();
                            }
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.GeOp;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Greater;
                    }
                    break;

                case '+':
                    {
                        if (second == '+')
                        {
                            kind = TokenKind.IncOp;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.AddAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Plus;
                    }
                    break;

                case '-':
                    {
                        if (second == '-')
                        {
                            kind = TokenKind.DecOp;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '=')
                        {
                            kind = TokenKind.SubAssign;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '>')
                        {
                            kind = TokenKind.PtrOp;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Minus;
                    }
                    break;

                case '/':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.DivAssign;
                            newCursor.AdvanceColumn();
                        }
                        else if (second == '/')
                        {
                            kind = TokenKind.SingleLineComment;
                            newCursor.AdvanceColumn();
                            while (newCursor.IsParsing)
                            {
                                if (IsEndOfLine(newCursor.Peek()))
                                    break;
                                newCursor.AdvanceColumn();
                            }
                        }
                        else if (second == '*')
                        {
                            kind = TokenKind.MultiLineComment;
                            newCursor.AdvanceColumn();
                            while (newCursor.IsParsing)
                            {
                                char n0 = newCursor.Peek();
                                char n1 = newCursor.Peek(1);
                                if (n0 == '*' && n1 == '/')
                                {
                                    newCursor.AdvanceColumn(2);
                                    break;
                                }
                                else if (IsEndOfLine(n0))
                                {
                                    if ((n0 == '\r' && n1 == '\n') || (n0 == '\n' && n1 == '\r'))
                                        newCursor.AdvanceColumn(2);
                                    else
                                        newCursor.AdvanceColumn();
                                    newCursor.AdvanceLine();
                                }
                                else
                                    newCursor.AdvanceColumn();
                            }
                        }
                        else
                            kind = TokenKind.Slash;
                    }
                    break;

                case '*':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.MulAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Asterisk;
                    }
                    break;

                case '%':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.ModAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.Percent;
                    }
                    break;

                case '.':
                    {
                        if (second == '.' && third == '.')
                        {
                            kind = TokenKind.Ellipsis;
                            newCursor.AdvanceColumn(2);
                        }
                        else if (IsInteger(second))
                            kind = ReadNumberLiteral(newCursor, first);
                        else
                            kind = TokenKind.Dot;
                    }
                    break;

                case '^':
                    {
                        if (second == '=')
                        {
                            kind = TokenKind.XorAssign;
                            newCursor.AdvanceColumn();
                        }
                        else
                            kind = TokenKind.CircumFlex;
                    }
                    break;

                case '"':
                case '\'':
                    {
                        if (first == '"')
                            kind = ReadStringLiteral(newCursor);
                        else
                            kind = ReadCharacterLiteral(newCursor);
                    }
                    break;

                case '~':
                    kind = TokenKind.Tilde;
                    break;
                case '\\':
                    kind = TokenKind.Backslash;
                    break;
                case '#':
                    kind = TokenKind.Raute;
                    break;
                case ',':
                    kind = TokenKind.Comma;
                    break;
                case ';':
                    kind = TokenKind.Semicolon;
                    break;
                case ':':
                    kind = TokenKind.Colon;
                    break;
                case '?':
                    kind = TokenKind.QuestionMark;
                    break;
                case '{':
                    kind = TokenKind.LeftBrace;
                    break;
                case '}':
                    kind = TokenKind.RightBrace;
                    break;
                case '[':
                    kind = TokenKind.LeftBracket;
                    break;
                case ']':
                    kind = TokenKind.RightBracket;
                    break;
                case '(':
                    kind = TokenKind.LeftParen;
                    break;
                case ')':
                    kind = TokenKind.RightParen;
                    break;

                default:
                    {
                        if (IsSpacing(first))
                        {
                            kind = TokenKind.Spacings;
                            newCursor.AdvanceColumnsWhile(IsSpacing);
                        }
                        else if (IsEndOfLine(first))
                        {
                            kind = TokenKind.EndOfLine;
                            if ((first == '\r' && second == '\n') || (first == '\n' && second == '\r'))
                                newCursor.AdvanceColumn();
                            newCursor.AdvanceLine();
                        }
                        else if (IsIdentStart(first))
                        {
                            kind = TokenKind.Ident;
                            newCursor.AdvanceColumnsWhile(IsIdentPart);
                        }
                        else if (IsInteger(first))
                            kind = ReadNumberLiteral(newCursor, first);
                        else
                            kind = TokenKind.Unknown;
                    }
                    break;
            }
            Token result = MakeToken(kind, cursor, newCursor);
            return (result);
        }
        public static Token GetTokenRaw(TextCursor cursor)
        {
            Token result = PeekTokenRaw(cursor);
            if (result.Kind != TokenKind.EOF)
            {
                Debug.Assert(result.Length > 0);
                cursor.Set(result.End);
            }
            return (result);
        }
        public static Token PeekToken(TextCursor cursor)
        {
            TextCursor newCursor = new TextCursor(cursor);
            Token token = PeekTokenRaw(newCursor);
            while (newCursor.IsParsing)
            {
                if (token.Kind == TokenKind.EOF)
                    break;
                if (!(token.Kind == TokenKind.Spacings || token.Kind == TokenKind.EndOfLine))
                    break;
                Debug.Assert(token.Length > 0);
                newCursor.Set(token.End);
                token = PeekTokenRaw(newCursor);
            }
            return (token);
        }
        public static Token GetToken(TextCursor cursor)
        {
            Token result = PeekToken(cursor);
            if (result.Kind != TokenKind.EOF)
            {
                Debug.Assert(result.Length > 0);
                cursor.Set(result.End);
            }
            return (result);
        }
    }
}
