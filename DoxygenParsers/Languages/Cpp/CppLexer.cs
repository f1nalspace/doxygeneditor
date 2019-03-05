//#define LEX_PREPROCESSOR_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppLexer : BaseLexer<CppToken>
    {
        // @TODO(final): Make primary-keywords configurable
        public static readonly HashSet<string> ReservedKeywords = new HashSet<string>{
            // C99
            "auto",
            "break",
            "case",
            "const",
            "continue",
            "default",
            "do",
            "else",
            "enum",
            "extern",
            "for",
            "goto",
            "if",
            "inline",
            "register",
            "restrict",
            "return",
            "signed",
            "sizeof",
            "static",
            "struct",
            "switch",
            "typedef",
            "union",
            "unsigned",
            "void",
            "volatile",
            "while",
            "_Alignas",
            "_Alignof",
            "__asm__",
            "__volatile__",

            // C++
            "abstract",
            "alignas",
            "alignof",
            "asm",
            "catch",
            "class",
            "constexpr",
            "const_cast",
            "decltype",
            "delete",
            "dynamic_cast",
            "explicit",
            "export",
            "false",
            "friend",
            "mutable",
            "namespace",
            "new",
            "noexcept",
            "nullptr",
            "operator",
            "override",
            "private",
            "protected",
            "public",
            "reinterpret_cast",
            "static_assert",
            "static_cast",
            "template",
            "this",
            "thread_local",
            "throw",
            "try",
            "typeid",
            "typename",
            "virtual",
        };

        // @TODO(final): Make secondary-keywords configurable
        public static readonly HashSet<string> TypeKeywords = new HashSet<string>{
            // C99
            "char",
            "double",
            "float",
            "int",
            "long",
            "short",
            "_Bool",
            "_Complex",
            "_Imaginary",

            // C++
            "bool",
            "complex",
            "imaginary",
        };

        // @TODO(final): Make global-class-keywords configurable
        public static readonly HashSet<string> GlobalClassKeywords = new HashSet<string>
        {
            "NULL",
            "int8_t",
            "int16_t",
            "int32_t",
            "int64_t",
            "intptr_t",
            "offset_t",
            "size_t",
            "ssize_t",
            "time_t",
            "uint8_t",
            "uint16_t",
            "uint32_t",
            "uint64_t",
            "uintptr_t",
            "wchar_t",
        };

        public CppLexer(string source, TextPosition pos, int length) : base(source, pos, length)
        {
        }

        private CppToken LexSingleLineComment(bool init)
        {
            TextPosition commentStart = new TextPosition(Buffer.TextPosition);
            CppTokenKind type = CppTokenKind.SingleLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '/');
                Buffer.AdvanceColumns(2);
                if (DoxygenSyntax.SingleLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceColumn();
                    type = CppTokenKind.SingleLineCommentDoc;
                }
            }
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c0 = Buffer.Peek();
                char c1 = Buffer.Peek(1);
                if (c0 == TextStream.InvalidCharacter)
                    break;
                else if (SyntaxUtils.IsLineBreak(c0))
                {
                    isComplete = true;
                    break;
                }
                else if (c0 == '\t')
                    Buffer.AdvanceTab();
                else
                    Buffer.AdvanceColumn();
            }
            int commentLength = Buffer.StreamPosition - commentStart.Index;
            TextRange commandRange = new TextRange(commentStart, commentLength);
            CppToken token = CppTokenPool.Make(type, commandRange, isComplete);
            return (token);
        }

        private CppToken LexMultiLineComment(bool init)
        {
            TextPosition commentStart = new TextPosition(Buffer.TextPosition);
            CppTokenKind type = CppTokenKind.MultiLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '*');
                Buffer.AdvanceColumns(2);
                if (DoxygenSyntax.MultiLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceColumn();
                    type = CppTokenKind.MultiLineCommentDoc;
                }
            }
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c0 = Buffer.Peek();
                if (c0 == '*')
                {
                    char c1 = Buffer.Peek(1);
                    if (c1 == '/')
                    {
                        Buffer.AdvanceColumns(2);
                        isComplete = true;
                        break;
                    }
                    else Buffer.AdvanceColumn();
                }
                else if (char.IsWhiteSpace(c0))
                    SkipAllWhitespaces();
                else
                    Buffer.AdvanceColumn();
            }
            int commentLength = Buffer.StreamPosition - commentStart.Index;
            TextRange commentRange = new TextRange(commentStart, commentLength);
            CppToken token = CppTokenPool.Make(type, commentRange, isComplete);
            return (token);
        }

        private CppToken AddChar(CppTokenKind type)
        {
            Buffer.StartLexeme();
            Buffer.AdvanceColumn();
            return CppTokenPool.Make(type, Buffer.LexemeRange, true);
        }

#if LEX_PREPROCESSOR_ENABLED
        private CppToken LexPreprocessor()
        {
            Debug.Assert(Buffer.Peek(0) == '#');
            CppTokenKind type = CppTokenKind.Preprocessor;
            TextPosition preprocessorStart = new TextPosition(Buffer.TextPosition);
            Buffer.AdvanceColumn();
            bool nextLine = false;
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c0 = Buffer.Peek();
                char c1 = Buffer.Peek(1);
                if (SyntaxUtils.IsLineBreak(c0))
                {
                    if (nextLine)
                    {
                        int lb = SyntaxUtils.GetLineBreakChars(c0, c1);
                        Buffer.AdvanceLine(lb);
                        nextLine = false;
                        continue;
                    }
                    else
                    {
                        isComplete = true;
                        break;
                    }
                }
                else if (c0 == '\\')
                {
                    Buffer.AdvanceColumn();
                    if (!nextLine)
                    {
                        SkipSpacings(SkipType.All);
                        nextLine = true;
                        continue;
                    }
                }
                else
                    Buffer.AdvanceManual(c0, c1);
            }
            int preprocessorLength = Buffer.StreamPosition - preprocessorStart.Index;
            TextRange preprocessorRange = new TextRange(preprocessorStart, preprocessorLength);
            CppToken result = CppTokenPool.Make(type, preprocessorRange, isComplete);
            return (result);
        }
#endif

        private CppToken LexIdent()
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.Peek()));
            Buffer.StartLexeme();
            StringBuilder identBuffer = new StringBuilder();
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek();
                if (SyntaxUtils.IsIdentPart(c))
                {
                    identBuffer.Append(c);
                    Buffer.AdvanceColumn();
                }
                else
                    break;
            }

            CppTokenKind type = CppTokenKind.IdentLiteral;
            TextPosition identStart = Buffer.LexemeStart;
            int identLength = Buffer.LexemeWidth;
            string identString = identBuffer.ToString();

            if (ReservedKeywords.Contains(identString))
                type = CppTokenKind.ReservedKeyword;
            else if (TypeKeywords.Contains(identString) || GlobalClassKeywords.Contains(identString))
                type = CppTokenKind.TypeKeyword;
            else
                type = CppTokenKind.IdentLiteral;
            TextRange identRange = new TextRange(identStart, identLength);
            CppToken result = CppTokenPool.Make(type, identRange, true);
            return (result);
        }

        private CppToken LexString(string name)
        {
            Debug.Assert(Buffer.Peek(0) == '"' || Buffer.Peek(0) == '\'');
            Buffer.StartLexeme();
            char quoteChar = Buffer.Peek();
            Buffer.AdvanceColumn();
            bool isComplete = false;
            CppTokenKind kind = quoteChar == '\'' ? CppTokenKind.CharLiteral : CppTokenKind.StringLiteral;
            int maxCount = (kind == CppTokenKind.CharLiteral) ? 1 : -1;
            int minCount = (kind == CppTokenKind.CharLiteral) ? 1 : 0;
            int count = 0;
            while (!Buffer.IsEOF)
            {
                char first = Buffer.Peek();
                char second = Buffer.Peek(1);
                if (first == quoteChar)
                {
                    isComplete = true;
                    break;
                }
                else if (first == '\\')
                {
                    switch (second)
                    {
                        case '\'':
                        case '"':
                        case '?':
                        case '\\':
                        case 'a':
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'e':
                        case 'r':
                        case 't':
                        case 'v':
                            {
                                Buffer.AdvanceColumns(2);
                                ++count;
                                continue;
                            }

                        case 'x':
                        case 'X':
                        case 'u':
                        case 'U':
                            {
                                Buffer.AdvanceColumns(2);
                                if (SyntaxUtils.IsHex(Buffer.Peek()))
                                {
                                    int len = 0;
                                    while (!Buffer.IsEOF)
                                    {
                                        if (!SyntaxUtils.IsHex(Buffer.Peek()))
                                            break;
                                        else
                                        {
                                            ++len;
                                            Buffer.AdvanceColumn();
                                        }
                                    }
                                }
                                else
                                {
                                    PushError(Buffer.TextPosition, $"Unsupported hex escape character '{Buffer.Peek()}'!");
                                    break;
                                }
                                ++count;
                                continue;
                            }

                        default:
                            if (SyntaxUtils.IsOctal(second))
                            {
                                Buffer.AdvanceColumn();
                                while (!Buffer.IsEOF)
                                {
                                    if (!SyntaxUtils.IsOctal(Buffer.Peek()))
                                        break;
                                    else
                                        Buffer.AdvanceColumn();
                                }
                                ++count;
                                continue;
                            }
                            else
                            {
                                PushError(Buffer.TextPosition, $"Not supported escape character '{Buffer.Peek()}'!");
                                break;
                            }
                    }
                }
                else if (SyntaxUtils.IsLineBreak(first))
                    break;
                else if (char.IsWhiteSpace(first))
                    Buffer.AdvanceManual(first, second);
                else
                    Buffer.AdvanceColumn();
                ++count;
            }

            // Skip over quote char
            if (isComplete)
            {
                Debug.Assert(Buffer.Peek() == quoteChar);
                Buffer.AdvanceColumn();
            }

            if (!isComplete)
                PushError(Buffer.LexemeStart, $"Unterminated {name} literal!");
            else
            {
                if (minCount > 0 && count < minCount)
                    PushError(Buffer.LexemeStart, $"Not enough characters for {name} literal, expect {minCount} but got {count}!");
                else if (maxCount > -1 && (count > maxCount))
                    PushError(Buffer.LexemeStart, $"Too many characters for {name} literal, expect {maxCount} but got {count}!");
            }

            CppToken result = CppTokenPool.Make(kind, Buffer.LexemeRange, isComplete);
            return (result);
        }

        private void AdvanceExponent(char test)
        {
            char c = Buffer.Peek();
            if (char.ToLower(test) == char.ToLower(c))
            {
                Buffer.AdvanceColumn();
                c = Buffer.Peek();
                if (c == '+' || c == '-')
                    Buffer.AdvanceColumn();
                Buffer.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
            }
        }

        private CppToken LexNumber()
        {
            Debug.Assert(SyntaxUtils.IsNumeric(Buffer.Peek()) || Buffer.Peek() == '.');

            Buffer.StartLexeme();

            CppTokenKind kind;
            char first = Buffer.Peek(0);
            char second = Buffer.Peek(1);
            bool dotSeen = false;
            if (first == '0')
            {
                if (second == 'x' || second == 'X')
                {
                    // Hex
                    kind = CppTokenKind.HexLiteral;
                    Buffer.AdvanceColumns(2); // Skip 0[xX]
                }
                else if (second == 'b' || second == 'B')
                {
                    // Binary
                    kind = CppTokenKind.BinaryLiteral;
                    Buffer.AdvanceColumns(2); // Skip 0[bB]
                }
                else
                {
                    // Octal
                    kind = CppTokenKind.OctalLiteral;
                }
            }
            else if (first == '.')
            {
                Debug.Assert(SyntaxUtils.IsNumeric(second));
                kind = CppTokenKind.IntegerFloatLiteral;
                Buffer.AdvanceColumn();
                dotSeen = true;
            }
            else
            {
                Debug.Assert(SyntaxUtils.IsNumeric(first));
                kind = CppTokenKind.IntegerLiteral;
            }

            // @NOTE(final): We never set the DecimalHexLiteral kind initially,
            // as every hex decimal always starts as a normal hex literal!
            Debug.Assert(kind != CppTokenKind.HexadecimalFloatLiteral);

            // First number part
            int firstLiteralPos = Buffer.TextPosition.Index;
            bool readNextLiteral = false;
            do
            {
                readNextLiteral = false;
                int s = Buffer.TextPosition.Index;
                switch (kind)
                {
                    case CppTokenKind.IntegerLiteral:
                    case CppTokenKind.IntegerFloatLiteral:
                        if (SyntaxUtils.IsNumeric(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
                        else
                            PushError(Buffer.TextPosition, $"Expect integer literal, but got '{Buffer.Peek()}'");
                        break;

                    case CppTokenKind.OctalLiteral:
                        if (SyntaxUtils.IsOctal(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsOctal);
                        else
                            PushError(Buffer.TextPosition, $"Expect octal literal, but got '{Buffer.Peek()}'");
                        break;

                    case CppTokenKind.HexLiteral:
                        if (SyntaxUtils.IsHex(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsHex);
                        else
                            PushError(Buffer.TextPosition, $"Expect hex literal, but got '{Buffer.Peek()}'");
                        break;

                    case CppTokenKind.BinaryLiteral:
                        if (SyntaxUtils.IsBinary(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsBinary);
                        else
                            PushError(Buffer.TextPosition, $"Expect binary literal, but got '{Buffer.Peek()}'");
                        break;

                    default:
                        PushError(Buffer.TextPosition, $"Unsupported token kind '{kind}' for integer literal on {Buffer}");
                        break;
                }
                bool hadIntegerLiteral = Buffer.TextPosition.Index > s;
                if (kind != CppTokenKind.IntegerFloatLiteral && kind != CppTokenKind.HexadecimalFloatLiteral)
                {
                    // @NOTE(final): Single quotes (') are allowed as separators for any non-decimal literal
                    char check0 = Buffer.Peek();
                    if (check0 == '\'')
                    {
                        if (!hadIntegerLiteral)
                        {
                            PushError(Buffer.TextPosition, $"Too many single quote escape in integer literal, expect any integer literal but got '{Buffer.Peek()}'");
                            return (null);
                        }
                        Buffer.AdvanceColumn();
                        readNextLiteral = true;
                    }
                }
            } while (!Buffer.IsEOF && readNextLiteral);

            // Validate any literal after starting dot
            if (dotSeen)
            {
                if (firstLiteralPos == Buffer.TextPosition.Index)
                {
                    PushError(Buffer.TextPosition, $"Expect any integer literal after starting dot, but got '{Buffer.Peek()}'");
                }
            }

            // Dot separator
            if ((!dotSeen) &&
               ((kind == CppTokenKind.IntegerLiteral) ||
                (kind == CppTokenKind.HexLiteral) ||
            (kind == CppTokenKind.OctalLiteral)
            ))
            {
                char check0 = Buffer.Peek();
                if (check0 == '.')
                {
                    dotSeen = true;
                    Buffer.AdvanceColumn();
                    if (kind == CppTokenKind.IntegerLiteral || kind == CppTokenKind.OctalLiteral)
                    {
                        kind = CppTokenKind.IntegerFloatLiteral;
                    }
                    else
                    {
                        Debug.Assert(kind == CppTokenKind.HexLiteral);
                        kind = CppTokenKind.HexadecimalFloatLiteral;
                    }
                }
                else if (SyntaxUtils.IsExponentPrefix(check0))
                {
                    if (kind == CppTokenKind.IntegerLiteral || kind == CppTokenKind.OctalLiteral)
                    {
                        kind = CppTokenKind.IntegerFloatLiteral;
                    }
                    else
                    {
                        Debug.Assert(kind == CppTokenKind.HexLiteral);
                        kind = CppTokenKind.HexadecimalFloatLiteral;
                    }
                }
            }

            // Decimal after dot separator
            if ((kind != CppTokenKind.IntegerFloatLiteral) &&
                (kind != CppTokenKind.HexadecimalFloatLiteral))
            {
                // Integer suffix
                if (SyntaxUtils.IsIntegerSuffix(Buffer.Peek()))
                {
                    Buffer.AdvanceColumnsWhile(SyntaxUtils.IsIntegerSuffix, 3);
                }
            }
            else
            {
                if (kind == CppTokenKind.IntegerFloatLiteral)
                {
                    // Float decimal
                    if (SyntaxUtils.IsNumeric(Buffer.Peek()))
                        Buffer.AdvanceColumnsWhile(SyntaxUtils.IsNumeric);
                    if (Buffer.Peek() == 'e' || Buffer.Peek() == 'E')
                        AdvanceExponent('e');
                }
                else
                {
                    // Hex decimal
                    Debug.Assert(kind == CppTokenKind.HexadecimalFloatLiteral);
                    if (SyntaxUtils.IsHex(Buffer.Peek()))
                        Buffer.AdvanceColumnsWhile(SyntaxUtils.IsHex);
                    if (Buffer.Peek() == 'p' || Buffer.Peek() == 'P')
                        AdvanceExponent('e');
                }

                // Float suffix
                if (SyntaxUtils.IsFloatSuffix(Buffer.Peek()))
                    Buffer.AdvanceColumn();
            }

            CppToken result = CppTokenPool.Make(kind, Buffer.LexemeRange, true);
            return (result);
        }

        protected override bool LexNext()
        {
            SkipAllWhitespaces();
            int line = Buffer.TextPosition.Line;
            while (!Buffer.IsEOF)
            {
                TextPosition startPos = new TextPosition(Buffer.TextPosition);
                char first = Buffer.Peek();
                char second = Buffer.Peek(1);
                char third = Buffer.Peek(2);
                CppTokenKind kind;
                switch (first)
                {
                    case '&':
                        {
                            if (second == '&')
                            {
                                kind = CppTokenKind.LogicalAndOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.AndAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.AndOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '|':
                        {
                            if (second == '|')
                            {
                                kind = CppTokenKind.LogicalOrOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.OrAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.OrOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '=':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.LogicalEqualsOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.EqOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '!':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.LogicalNotEqualsOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.ExclationMark;
                                Buffer.AdvanceColumn();
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
                                    Buffer.AdvanceColumns(3);
                                }
                                else
                                {
                                    kind = CppTokenKind.LeftShiftOp;
                                    Buffer.AdvanceColumns(2);
                                }
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.LessOrEqualOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.LessThanOp;
                                Buffer.AdvanceColumn();
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
                                    Buffer.AdvanceColumns(3);
                                }
                                else
                                {
                                    kind = CppTokenKind.RightShiftOp;
                                    Buffer.AdvanceColumns(2);
                                }
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.GreaterOrEqualOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.GreaterThanOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '+':
                        {
                            if (second == '+')
                            {
                                kind = CppTokenKind.IncOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.AddAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.AddOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '-':
                        {
                            if (second == '-')
                            {
                                kind = CppTokenKind.DecOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '=')
                            {
                                kind = CppTokenKind.SubAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '>')
                            {
                                kind = CppTokenKind.PtrOp;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.SubOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '/':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.DivAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else if (second == '/')
                            {
                                CppToken token = LexSingleLineComment(true);
                                return PushToken(token);
                            }
                            else if (second == '*')
                            {
                                CppToken token = LexMultiLineComment(true);
                                return PushToken(token);
                            }
                            else
                            {
                                Buffer.AdvanceColumn();
                                kind = CppTokenKind.DivOp;
                            }
                        }
                        break;

                    case '*':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.MulAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.MulOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '%':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.ModAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.ModOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '.':
                        {
                            if (second == '.' && third == '.')
                            {
                                kind = CppTokenKind.Ellipsis;
                                Buffer.AdvanceColumns(3);
                            }
                            else if (SyntaxUtils.IsNumeric(second))
                            {
                                CppToken token = LexNumber();
                                return PushToken(token);
                            }
                            else
                            {
                                kind = CppTokenKind.Dot;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '^':
                        {
                            if (second == '=')
                            {
                                kind = CppTokenKind.XorAssign;
                                Buffer.AdvanceColumns(2);
                            }
                            else
                            {
                                kind = CppTokenKind.XorOp;
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;

                    case '"':
                        {
                            CppToken token = LexString("string");
                            return PushToken(token);
                        }

                    case '\'':
                        {
                            CppToken token = LexString("char");
                            return PushToken(token);
                        }

                    case '~':
                        kind = CppTokenKind.Tilde;
                        Buffer.AdvanceColumn();
                        break;
                    case '\\':
                        kind = CppTokenKind.Backslash;
                        Buffer.AdvanceColumn();
                        break;
                    case '#':
#if LEX_PREPROCESSOR_ENABLED
                        {
                            CppToken token = LexPreprocessor();
                            return PushToken(token);
                        }
#else
                        kind = CppTokenKind.Raute;
                        Buffer.AdvanceColumn();
                        break;
#endif
                    case ',':
                        kind = CppTokenKind.Comma;
                        Buffer.AdvanceColumn();
                        break;
                    case ';':
                        kind = CppTokenKind.Semicolon;
                        Buffer.AdvanceColumn();
                        break;
                    case ':':
                        kind = CppTokenKind.Colon;
                        Buffer.AdvanceColumn();
                        break;
                    case '?':
                        kind = CppTokenKind.QuestionMark;
                        Buffer.AdvanceColumn();
                        break;
                    case '{':
                        kind = CppTokenKind.LeftBrace;
                        Buffer.AdvanceColumn();
                        break;
                    case '}':
                        kind = CppTokenKind.RightBrace;
                        Buffer.AdvanceColumn();
                        break;
                    case '[':
                        kind = CppTokenKind.LeftBracket;
                        Buffer.AdvanceColumn();
                        break;
                    case ']':
                        kind = CppTokenKind.RightBracket;
                        Buffer.AdvanceColumn();
                        break;
                    case '(':
                        kind = CppTokenKind.LeftParen;
                        Buffer.AdvanceColumn();
                        break;
                    case ')':
                        kind = CppTokenKind.RightParen;
                        Buffer.AdvanceColumn();
                        break;

                    default:
                        {
                            if (SyntaxUtils.IsLineBreak(first))
                            {
                                kind = CppTokenKind.EndOfLine;
                                int nb = SyntaxUtils.GetLineBreakChars(first, second);
                                Buffer.AdvanceLine(nb);
                            }
                            else if (first == '\t')
                            {
                                kind = CppTokenKind.Spacings;
                                while (!Buffer.IsEOF)
                                {
                                    if (Buffer.Peek() != '\t')
                                        break;
                                    Buffer.AdvanceTab();
                                }
                            }
                            else if (SyntaxUtils.IsSpacing(first))
                            {
                                kind = CppTokenKind.Spacings;
                                Buffer.AdvanceColumnsWhile(SyntaxUtils.IsSpacing);
                            }
                            else if (SyntaxUtils.IsIdentStart(first))
                            {
                                CppToken token = LexIdent();
                                return PushToken(token);
                            }
                            else if (SyntaxUtils.IsNumeric(first))
                            {
                                CppToken token = LexNumber();
                                return PushToken(token);
                            }
                            else
                            {
                                kind = CppTokenKind.Unknown;
                                PushError(Buffer.TextPosition, $"Unsupported character '{first}'");
                                Buffer.AdvanceColumn();
                            }
                        }
                        break;
                }

                {
                    int tokenLen = Buffer.TextPosition.Index - startPos.Index;
                    return PushToken(CppTokenPool.Make(kind, new TextRange(startPos, tokenLen), true));
                }
            }
            return (false);
        }
    }
}
