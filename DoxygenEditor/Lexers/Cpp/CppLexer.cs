﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public CppLexer(SourceBuffer source) : base(source)
        {

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

        private CppToken AddChar(CppTokenType type)
        {
            Buffer.Start();
            Buffer.AdvanceChar();
            return new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
        }

        private CppToken LexPreprocessor()
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

        private CppToken LexIdent()
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.PeekChar()));
            Buffer.Start();
            StringBuilder identBuffer = new StringBuilder();
            while (!Buffer.IsEOF)
            {
                char c = Buffer.PeekChar();
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

        private CppToken LexNumber()
        {
            // @TODO(final): Support \ escape characters
            Debug.Assert(SyntaxUtils.IsNumeric(Buffer.PeekChar()) || Buffer.PeekChar() == '.');
            Buffer.Start();
            bool dotSeen = false;
            bool allowDecimal = true;
            bool allowLongSuffix = false;
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
            else if (Buffer.PeekChar() == '.')
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
                if (Buffer.PeekChar() == '.')
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
                else if (type == CppTokenType.Binary)
                {
                    if (!SyntaxUtils.IsBinary(Buffer.PeekChar()))
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
                    char n = Buffer.PeekChar();
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
                char[] n = new char[3] { Buffer.PeekChar(0), Buffer.PeekChar(1), Buffer.PeekChar(2) };
                if (n[0] == 'e' || n[0] == 'E' && n[1] == '+' || n[1] == '-' && SyntaxUtils.IsNumeric(n[2]))
                {
                    Buffer.AdvanceChar(2);
                    while (!Buffer.IsEOF)
                    {
                        if (SyntaxUtils.IsNumeric(Buffer.PeekChar()))
                            Buffer.AdvanceChar();
                        else
                            break;
                    }
                }
            }

            if (allowFloatSuffix)
            {
                Debug.Assert(type == CppTokenType.Double);
                if (Buffer.PeekChar() == 'f')
                {
                    type = CppTokenType.Float;
                    Buffer.AdvanceChar();
                }
            }

            Debug.Assert(!SyntaxUtils.IsNumeric(Buffer.PeekChar()));

            CppToken result = new CppToken(type, Buffer.LexemeStart, Buffer.LexemeWidth, true);
            return (result);
        }

        protected override bool LexNext()
        {
            do
            {
                SkipWhitespaces();
                switch (Buffer.PeekChar())
                {
                    case SlidingTextBuffer.InvalidCharacter:
                        {
                            if (Buffer.IsEOF)
                            {
                                PushToken(new CppToken(CppTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
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
                            char next = Buffer.PeekChar(1);
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
                            if (SyntaxUtils.IsNumeric(Buffer.PeekChar()))
                                token = LexNumber();
                            else
                            {
                                Debug.Assert(Buffer.PeekChar() == '.');
                                char n = Buffer.PeekChar(1);
                                if (SyntaxUtils.IsNumeric(n))
                                    token = LexNumber();
                                else
                                    token = AddChar(CppTokenType.Dot);
                            }
                            return PushToken(token);
                        }

                    default:
                        {
                            if (SyntaxUtils.IsIdentStart(Buffer.PeekChar()))
                                goto case 'a';

                            if (SyntaxUtils.IsNumeric(Buffer.PeekChar()) || Buffer.PeekChar() == '.')
                                goto case '0';

                            char c = Buffer.PeekChar();
                            if (_charTypeMapping.ContainsKey(c))
                                PushToken(new CppToken(_charTypeMapping[c], Buffer.Position, 1, true));

                            Buffer.AdvanceChar();
                        }
                        break;
                }
            } while (!Buffer.IsEOF);
            PushToken(new CppToken(CppTokenType.EOF, Math.Max(0, Buffer.End - 1), 0, false));
            return (false);
        }
    }
}
