using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lists;
using TSP.DoxygenEditor.TextAnalysis;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Lexers.Cpp
{
    class CppLexer : BaseLexer<CppToken>
    {
        // @TODO(final): Make primary-keywords configurable
        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>{
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
        private static readonly HashSet<string> TypeKeywords = new HashSet<string>{
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
        private static readonly HashSet<string> GlobalClassKeywords = new HashSet<string>
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

        private static Dictionary<char, CppTokenType> _charTypeMapping = new Dictionary<char, CppTokenType>()
        {
            { '(', CppTokenType.LeftParen },
            { ')', CppTokenType.RightParen },
            { '[', CppTokenType.LeftBracket },
            { ']', CppTokenType.RightBracket },
            { '{', CppTokenType.LeftCurlyBrace },
            { '}', CppTokenType.RightCurlyBrace },
            { ';', CppTokenType.Semicolon },
            { ',', CppTokenType.Comma },
            { '?', CppTokenType.QuestionMark },
            { ':', CppTokenType.Colon },
            { '~', CppTokenType.OpNot },
            { '%', CppTokenType.OpMod },
            { '!', CppTokenType.OpNeg },
            { '<', CppTokenType.OpLess },
            { '>', CppTokenType.OpGreater },
            { '=', CppTokenType.OpEquals },
            { '*', CppTokenType.OpMul },
            //{ '/', CppTokenType.OpDiv },
            { '+', CppTokenType.OpPlus },
            { '-', CppTokenType.OpMinus },
            { '|', CppTokenType.OpOr },
            { '&', CppTokenType.OpAnd },
            //{ '.', CppTokenType.Dot },
        };

        public CppLexer(string source, int sbase, int length) : base(source, sbase, length)
        {

        }

        private CppToken LexSingleLineComment(bool init)
        {
            int commentStart = Buffer.StreamPosition;
            CppTokenType type = CppTokenType.SingleLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '/');
                Buffer.AdvanceChar(2);
                if (DoxygenLexer.SingleLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceChar();
                    type = CppTokenType.SingleLineCommentDoc;
                }
            }
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek();
                if (c == '\n')
                {
                    Buffer.AdvanceChar();
                    break;
                }
                Buffer.AdvanceChar();
            }
            int commentLength = Buffer.StreamPosition - commentStart;
            CppToken token = new CppToken(type, commentStart, commentLength, true);
            return (token);
        }

        private CppToken LexMultiLineComment(bool init)
        {
            int commentStart = Buffer.StreamPosition;
            CppTokenType type = CppTokenType.MultiLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '*');
                Buffer.AdvanceChar(2);
                if (DoxygenLexer.MultiLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceChar();
                    type = CppTokenType.MultiLineCommentDoc;
                }
            }
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek(0);
                if (c == '*')
                {
                    char n = Buffer.Peek(1);
                    if (n == '/')
                    {
                        Buffer.AdvanceChar(2);
                        isComplete = true;
                        break;
                    }
                }
                Buffer.AdvanceChar();
            }
            int commentLength = Buffer.StreamPosition - commentStart;
            CppToken token = new CppToken(type, commentStart, commentLength, isComplete);
            return (token);
        }

        private CppToken AddChar(CppTokenType type)
        {
            Buffer.StartLexeme();
            Buffer.AdvanceChar();
            return new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
        }

        private CppToken LexPreprocessor()
        {
            Debug.Assert(Buffer.Peek(0) == '#');
            CppTokenType type = CppTokenType.Preprocessor;
            int preprocessorStart = Buffer.StreamPosition;
            Buffer.AdvanceChar();
            bool nextLine = false;
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek();
                if (c == '\n')
                {
                    if (nextLine)
                    {
                        Buffer.AdvanceChar();
                        nextLine = false;
                        continue;
                    }
                    else
                    {
                        isComplete = true;
                        break;
                    }
                }
                else if (c == '\\')
                {
                    Buffer.AdvanceChar();
                    if (!nextLine)
                    {
                        SkipWhitespaces(true);
                        nextLine = true;
                        continue;
                    }
                }
                Buffer.AdvanceChar();
            }
            int preprocessorLength = Buffer.StreamPosition - preprocessorStart;
            CppToken result = new CppToken(type, preprocessorStart, preprocessorLength, isComplete);
            return (result);
        }

        private CppToken LexIdent()
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.Peek()));
            Buffer.StartLexeme();
            StringBuilder identBuffer = new StringBuilder();
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek();
                if (SyntaxUtils.IsIdent(c))
                {
                    identBuffer.Append(c);
                    Buffer.AdvanceChar();
                }
                else
                    break;
            }

            CppTokenType type = CppTokenType.Identifier;
            int identStart = Buffer.LexemeStart;
            int identLength = Buffer.LexemeWidth;
            string identString = identBuffer.ToString();

            if (ReservedKeywords.Contains(identString))
                type = CppTokenType.ReservedKeyword;
            else if (TypeKeywords.Contains(identString) || GlobalClassKeywords.Contains(identString))
                type = CppTokenType.TypeKeyword;
            else
                type = CppTokenType.Identifier;
            CppToken result = new CppToken(type, identStart, identLength, true);
            return (result);
        }

        private CppToken LexString()
        {
            Debug.Assert(Buffer.Peek(0) == '"' || Buffer.Peek(0) == '\'');
            Buffer.StartLexeme();
            char quoteChar = Buffer.Peek();
            Buffer.AdvanceChar();
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.Peek();
                if (c == quoteChar)
                {
                    isComplete = true;
                    Buffer.AdvanceChar();
                    break;
                }
                else if (c == '\\')
                {
                    char n = Buffer.Peek(1);
                    switch (n)
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
                                Buffer.AdvanceChar(2);
                            }
                            continue;

                        case 'x':
                        case 'X':
                        case 'u':
                        case 'U':
                            {
                                Buffer.AdvanceChar(2);
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
                                            Buffer.AdvanceChar();
                                        }
                                    }
                                }
                                else throw new Exception($"Unsupported hex string character '{Buffer.Peek()}'!");
                            }
                            continue;

                        default:
                            if (SyntaxUtils.IsOctal(n))
                            {
                                Buffer.AdvanceChar();
                                while (!Buffer.IsEOF)
                                {
                                    if (!SyntaxUtils.IsOctal(Buffer.Peek()))
                                        break;
                                    else
                                        Buffer.AdvanceChar();
                                }
                                continue;
                            }
                            else throw new Exception($"Not supported string escape character '{Buffer.Peek()}'!");
                    }
                }
                Buffer.AdvanceChar();
            }
            CppToken result = new CppToken(CppTokenType.String, Buffer.LexemeStart, Buffer.LexemeWidth, isComplete);
            return (result);
        }

        private CppToken LexNumber()
        {
            // @TODO(final): Support \ escape characters
            Debug.Assert(SyntaxUtils.IsNumeric(Buffer.Peek()) || Buffer.Peek() == '.');
            Buffer.StartLexeme();
            bool dotSeen = false;
            bool allowDecimal = true;
            bool allowLongSuffix = false;
            CppTokenType type;
            if (Buffer.Peek() == '0')
            {
                // Either hex or octal
                char n = Buffer.Peek(1);
                if (n == 'x' || n == 'X')
                {
                    // Skip 0x or 0X
                    Buffer.AdvanceChar(2);
                    type = CppTokenType.Hex;
                }
                else if (n == 'b' || n == 'B')
                {
                    // Skip 0b or 0B
                    Buffer.AdvanceChar(2);
                    type = CppTokenType.Binary;
                }
                else if (SyntaxUtils.IsOctal(n))
                {
                    // Skip zero octal
                    Buffer.AdvanceChar();
                    type = CppTokenType.Octal;
                    allowDecimal = false;
                }
                else
                {
                    type = CppTokenType.Integer;
                    allowLongSuffix = true;
                }
            }
            else if (Buffer.Peek() == '.')
            {
                // .0-9+ decimal number
                Buffer.AdvanceChar();
                type = CppTokenType.Double;
                dotSeen = true;
            }
            else
            {
                // Normal number
                type = CppTokenType.Integer;
            }
            while (!Buffer.IsEOF)
            {
                if (Buffer.Peek() == '.')
                {
                    if (!dotSeen && allowDecimal)
                    {
                        dotSeen = true;
                        type = CppTokenType.Double;
                        Buffer.AdvanceChar();
                        continue;
                    }
                    else
                        break;
                }
                if (type == CppTokenType.Integer || type == CppTokenType.Double)
                {
                    if (!SyntaxUtils.IsNumeric(Buffer.Peek()))
                        break;
                }
                else if (type == CppTokenType.Hex)
                {
                    if (!SyntaxUtils.IsHex(Buffer.Peek()))
                        break;
                }
                else if (type == CppTokenType.Octal)
                {
                    if (!SyntaxUtils.IsOctal(Buffer.Peek()))
                        break;
                }
                else if (type == CppTokenType.Binary)
                {
                    if (!SyntaxUtils.IsBinary(Buffer.Peek()))
                        break;
                }
                else
                    throw new Exception($"Unsupported token type '{type}' for a number!");
                Buffer.AdvanceChar();
            }

            bool allowExpontial = (type == CppTokenType.Double);
            bool allowFloatSuffix = (type == CppTokenType.Double);
            if (allowLongSuffix)
            {
                bool notUnsignedSeen = false;
                while (!Buffer.IsEOF)
                {
                    char n = Buffer.Peek();
                    if ((n == 'u' || n == 'U') && !notUnsignedSeen)
                    {
                        notUnsignedSeen = true;
                        Buffer.AdvanceChar();
                    }
                    else if (n == 'l' || n == 'L')
                    {
                        Buffer.AdvanceChar();
                    }
                    else break;
                }
            }

            if (allowExpontial)
            {
                char[] n = new char[3] { Buffer.Peek(0), Buffer.Peek(1), Buffer.Peek(2) };
                if (n[0] == 'e' || n[0] == 'E' && n[1] == '+' || n[1] == '-' && SyntaxUtils.IsNumeric(n[2]))
                {
                    Buffer.AdvanceChar(2);
                    while (!Buffer.IsEOF)
                    {
                        if (SyntaxUtils.IsNumeric(Buffer.Peek()))
                            Buffer.AdvanceChar();
                        else
                            break;
                    }
                }
            }

            if (allowFloatSuffix)
            {
                Debug.Assert(type == CppTokenType.Double);
                if (Buffer.Peek() == 'f')
                {
                    type = CppTokenType.Float;
                    Buffer.AdvanceChar();
                }
            }

            Debug.Assert(!SyntaxUtils.IsNumeric(Buffer.Peek()));

            CppToken result = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces();
                switch (Buffer.Peek())
                {
                    case TextStream.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                PushToken(new CppToken(CppTokenType.EOF, Buffer.StreamOnePastEnd, 0, false));
                                return (false);
                            }
                            else
                                Buffer.NextChar();
                        }
                        break;

#if false
                    case '\r':
                    case '\n':
                        {
                            char cur = Buffer.PeekChar(0);
                            char next = Buffer.PeekChar(1);
                            if (cur == '\r')
                            {
                                if (next == '\n')
                                {
                                    PushToken(new CppToken(CppTokenType.LineBreak, Buffer.Position, 2, true));
                                    Buffer.AdvanceChar(2);
                                }
                                else
                                {
                                    Buffer.AdvanceChar();
                                }
                            }
                            else
                            {
                                Debug.Assert(cur == '\n');
                                PushToken(new CppToken(CppTokenType.LineBreak, Buffer.Position, 1, true));
                                Buffer.AdvanceChar();
                            }
                            return (true);
                        }
#endif

                    case '/':
                        {
                            char next = Buffer.Peek(1);
                            CppToken token;
                            if (next == '*')
                                token = LexMultiLineComment(true);
                            else if (next == '/')
                                token = LexSingleLineComment(true);
                            else
                                token = AddChar(CppTokenType.OpDiv);
                            return PushToken(token);
                        }

                    case '#':
                        {
                            CppToken token = LexPreprocessor();
                            return PushToken(token);
                        }

                    case '"':
                    case '\'':
                        {
                            // @TODO(final): Support for R() string
                            CppToken token = LexString();
                            return PushToken(token);
                        }

                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                        {
                            CppToken token = LexIdent();
                            return PushToken(token);
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
                    case '.':
                        {
                            CppToken token;
                            if (SyntaxUtils.IsNumeric(Buffer.Peek()))
                                token = LexNumber();
                            else
                            {
                                Debug.Assert(Buffer.Peek() == '.');
                                char n = Buffer.Peek(1);
                                if (SyntaxUtils.IsNumeric(n))
                                    token = LexNumber();
                                else
                                    token = AddChar(CppTokenType.Dot);
                            }
                            return PushToken(token);
                        }

                    default:
                        {
                            if (SyntaxUtils.IsIdentStart(Buffer.Peek()))
                                goto case 'a';

                            if (SyntaxUtils.IsNumeric(Buffer.Peek()) || Buffer.Peek() == '.')
                                goto case '0';

                            char c = Buffer.Peek();
                            if (_charTypeMapping.ContainsKey(c))
                                PushToken(new CppToken(_charTypeMapping[c], Buffer.StreamPosition, 1, true));

                            Buffer.AdvanceChar();
                        }
                        break;
                }
            } while (!Buffer.IsEOF);
            PushToken(new CppToken(CppTokenType.EOF, Buffer.StreamOnePastEnd, 0, false));
            return (false);
        }
    }
}
