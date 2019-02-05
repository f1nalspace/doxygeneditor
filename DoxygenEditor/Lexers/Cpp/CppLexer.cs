using DoxygenEditor.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoxygenEditor.Lexers.Cpp
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

        public CppLexer(SourceBuffer source) : base(source)
        {

        }

        private CppTokenType GetTokenType()
        {
            foreach (CppTokenType type in Enum.GetValues(typeof(CppTokenType)))
            {
                if (type.HasText())
                {
                    string typeText = type.ToText();
                    if (Buffer.RemainingLength >= typeText.Length)
                    {
                        if (Buffer.Source.Compare(Buffer.Position, typeText, 0, typeText.Length) == 0)
                        {
                            return (type);
                        }
                    }
                }
            }
            return (CppTokenType.Invalid);
        }

        private CppToken LexSingleLineComment(bool init)
        {
            Buffer.Start();
            CppTokenType type = CppTokenType.SingleLineComment;
            if (init)
            {
                Debug.Assert(Buffer.PeekChar(0) == '/');
                Debug.Assert(Buffer.PeekChar(1) == '/');
                Buffer.AdvanceChar(2);
                if (Buffer.PeekChar() == '!')
                {
                    Buffer.AdvanceChar();
                    type = CppTokenType.SingleLineCommentDoc;
                }
            }
            while (!Buffer.IsEOF)
            {
                char c = Buffer.PeekChar();
                if (c == '\n')
                {
                    Buffer.AdvanceChar();
                    break;
                }
                Buffer.AdvanceChar();
            }
            CppToken token = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (token);
        }

        private CppToken LexMultiLineComment(bool init)
        {
            Buffer.Start();
            CppTokenType type = CppTokenType.MultiLineComment;
            if (init)
            {
                Debug.Assert(Buffer.PeekChar(0) == '/');
                Debug.Assert(Buffer.PeekChar(1) == '*');
                Buffer.AdvanceChar(2);
                if ((Buffer.PeekChar() == '*' || Buffer.PeekChar() == '!') && Buffer.PeekChar(1) != '/')
                {
                    Buffer.AdvanceChar();
                    type = CppTokenType.MultiLineCommentDoc;
                }
            }
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.PeekChar(0);
                if (c == '*')
                {
                    char n = Buffer.PeekChar(1);
                    if (n == '/')
                    {
                        Buffer.AdvanceChar(2);
                        isComplete = true;
                        break;
                    }
                }
                Buffer.AdvanceChar();
            }
            CppToken token = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, isComplete);
            return (token);
        }

        private CppToken LexCharToken()
        {
            if (Buffer.IsEOF)
                return new CppToken(CppTokenType.EOF, Buffer.Position, 0, true);
            CppTokenType tokenType = GetTokenType();
            if (tokenType.HasText())
            {
                string typeText = tokenType.ToText();
                Buffer.AdvanceChar(typeText.Length);
            }
            else
            {
                Buffer.AdvanceChar();
            }
            return new CppToken(tokenType, Buffer.LexemeStart, Buffer.LexemeWidth, true);
        }

        private CppToken LexPreprocessorToken()
        {
            Debug.Assert(Buffer.PeekChar(0) == '#');
            CppTokenType type = CppTokenType.Preprocessor;
            Buffer.Start();
            Buffer.AdvanceChar();
            bool nextLine = false;
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.PeekChar();
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
            CppToken result = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, isComplete);
            return (result);
        }

        private CppToken LexIdentToken()
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.PeekChar()));
            Buffer.Start();
            while (!Buffer.IsEOF)
            {
                if (SyntaxUtils.IsIdent(Buffer.PeekChar()))
                    Buffer.AdvanceChar();
                else
                    break;
            }
            int identStart = Buffer.LexemeStart;
            int identLength = Buffer.LexemeWidth;
            string identString = Buffer.GetText(identStart, identLength);

            CppTokenType type;
            if (ReservedKeywords.Contains(identString))
                type = CppTokenType.ReservedKeyword;
            else if (TypeKeywords.Contains(identString) || GlobalClassKeywords.Contains(identString))
                type = CppTokenType.TypeKeyword;
            else
                type = CppTokenType.Identifier;

            CppToken result = new CppToken(type, identStart, identLength, true);
            return (result);
        }

        private CppToken LexStringToken()
        {
            Debug.Assert(Buffer.PeekChar(0) == '"' || Buffer.PeekChar(0) == '\'');
            Buffer.Start();
            char quoteChar = Buffer.PeekChar();
            Buffer.AdvanceChar();
            bool isComplete = false;
            while (!Buffer.IsEOF)
            {
                char c = Buffer.PeekChar();
                if (c == quoteChar)
                {
                    isComplete = true;
                    Buffer.AdvanceChar();
                    break;
                }
                else if (c == '\\')
                {
                    char n = Buffer.PeekChar(1);
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
                                if (SyntaxUtils.IsHex(Buffer.PeekChar()))
                                {
                                    int len = 0;
                                    while (!Buffer.IsEOF)
                                    {
                                        if (!SyntaxUtils.IsHex(Buffer.PeekChar()))
                                            break;
                                        else
                                        {
                                            ++len;
                                            Buffer.AdvanceChar();
                                        }
                                    }
                                }
                                else throw new Exception($"Unsupported hex string character '{Buffer.PeekChar()}'!");
                            }
                            continue;

                        default:
                            if (SyntaxUtils.IsOctal(n))
                            {
                                Buffer.AdvanceChar();
                                while (!Buffer.IsEOF)
                                {
                                    if (!SyntaxUtils.IsOctal(Buffer.PeekChar()))
                                        break;
                                    else
                                        Buffer.AdvanceChar();
                                }
                                continue;
                            }
                            else throw new Exception($"Not supported string escape character '{Buffer.PeekChar()}'!");
                    }
                }
                Buffer.AdvanceChar();
            }
            CppToken result = new CppToken(CppTokenType.String, Buffer.LexemeStart, Buffer.LexemeWidth, isComplete);
            return (result);
        }

        private CppToken LexNumberToken()
        {
            Debug.Assert(SyntaxUtils.IsNumeric(Buffer.PeekChar()) || Buffer.PeekChar() == '.');
            Buffer.Start();
            bool dotSeen = false;
            bool allowDecimal = true;
            CppTokenType type;
            if (Buffer.PeekChar() == '0')
            {
                // Either hex or octal
                char n = Buffer.PeekChar(1);
                if (n == 'x' || n == 'X')
                {
                    // Skip 0x or 0X
                    Buffer.AdvanceChar(2);
                    type = CppTokenType.Hex;
                }
                else if (SyntaxUtils.IsOctal(n))
                {
                    // Skip zero octal
                    Buffer.AdvanceChar();
                    type = CppTokenType.Octal;
                    allowDecimal = false;
                }
                else
                    type = CppTokenType.Integer;
            }
            else if (Buffer.PeekChar() == '.')
            {
                // 0. decimal number
                Buffer.AdvanceChar();
                type = CppTokenType.Decimal;
                dotSeen = true;
            }
            else
            {
                // Normal number
                type = CppTokenType.Integer;
            }
            while (!Buffer.IsEOF)
            {
                if (Buffer.PeekChar() == '.')
                {
                    if (!dotSeen && allowDecimal)
                    {
                        dotSeen = true;
                        Buffer.AdvanceChar();
                        continue;
                    }
                    else
                        break;
                }
                if (type == CppTokenType.Integer)
                {
                    if (!SyntaxUtils.IsNumeric(Buffer.PeekChar()))
                        break;
                }
                else if (type == CppTokenType.Hex)
                {
                    if (!SyntaxUtils.IsHex(Buffer.PeekChar()))
                        break;
                }
                else if (type == CppTokenType.Octal)
                {
                    if (!SyntaxUtils.IsOctal(Buffer.PeekChar()))
                        break;
                }
                else
                    throw new Exception($"Unsupported token type '{type}' for a number!");
                Buffer.AdvanceChar();
            }
            CppToken result = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces(false);
                switch (Buffer.PeekChar())
                {
                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            return(PushToken(new CppToken(CppTokenType.Invalid, 0, 0, false)));
                        }

                    case '/':
                        {
                            char next = Buffer.PeekChar(1);
                            CppToken token;
                            if (next == '*')
                                token = LexMultiLineComment(true);
                            else if (next == '/')
                                token = LexSingleLineComment(true);
                            else
                                token = LexCharToken();
                            return (PushToken(token));
                        }

                    case '#':
                        {
                            CppToken token = LexPreprocessorToken();
                            return (PushToken(token));
                        }

                    case '"':
                    case '\'':
                        {
                            CppToken token = LexStringToken();
                            return (PushToken(token));
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
                            CppToken token = LexIdentToken();
                            return (PushToken(token));
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
                            if (SyntaxUtils.IsNumeric(Buffer.PeekChar()))
                                token = LexNumberToken();
                            else
                            {
                                Debug.Assert(Buffer.PeekChar() == '.');
                                char n = Buffer.PeekChar(1);
                                if (SyntaxUtils.IsNumeric(n))
                                    token = LexNumberToken();
                                else
                                    token = LexCharToken();
                            }
                            return (PushToken(token));
                        }

                    default:
                        {
                            if (SyntaxUtils.IsIdentStart(Buffer.PeekChar()))
                                goto case 'a';

                            if (SyntaxUtils.IsNumeric(Buffer.PeekChar()) || Buffer.PeekChar() == '.')
                                goto case '0';

                            Buffer.AdvanceChar();
                        }
                        break;
                }
            } while (!Buffer.IsEOF);
            return(false);
        }


    }
}
