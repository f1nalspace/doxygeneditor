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

        public static readonly HashSet<string> PreProcessorKeywords = new HashSet<string>()
        {
            "define",
            "defined",
            "undef",
            "ifdef",
            "ifndef",
            "include",

            "error",
            "import",
            "pragma",

            "if",
            "elif",
            "else",
            "endif",
            "using",

            "line",
        };

        class CppLexerState : State
        {
            public bool IsInsidePreprocessor { get; private set; }
            public void StartPreprocessor() { IsInsidePreprocessor = true; }
            public void EndPreprocessor() { IsInsidePreprocessor = false; }

            public override void StartLex(TextStream stream)
            {
            }
        }

        protected override State CreateState()
        {
            return new CppLexerState();
        }

        public CppLexer(string source, TextPosition pos, int length) : base(source, pos, length)
        {
        }

        struct LexResult
        {
            public CppTokenKind Kind { get; set; }
            public bool IsComplete { get; set; }
            public LexResult(CppTokenKind kind, bool isComplete)
            {
                Kind = kind;
                IsComplete = isComplete;
            }
        }

        private LexResult LexSingleLineComment(bool init)
        {
            CppTokenKind kind = CppTokenKind.SingleLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '/');
                Buffer.AdvanceColumns(2);
                if (DoxygenSyntax.SingleLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceColumn();
                    kind = CppTokenKind.SingleLineCommentDoc;
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
            return new LexResult(kind, isComplete);
        }

        private LexResult LexMultiLineComment(bool init)
        {
            CppTokenKind kind = CppTokenKind.MultiLineComment;
            if (init)
            {
                Debug.Assert(Buffer.Peek(0) == '/');
                Debug.Assert(Buffer.Peek(1) == '*');
                Buffer.AdvanceColumns(2);
                if (DoxygenSyntax.MultiLineDocChars.Contains(Buffer.Peek()))
                {
                    Buffer.AdvanceColumn();
                    kind = CppTokenKind.MultiLineCommentDoc;
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
            return new LexResult(kind, isComplete);
        }

        private LexResult LexIdent(bool isPreprocessor)
        {
            Debug.Assert(SyntaxUtils.IsIdentStart(Buffer.Peek()));
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
            CppTokenKind kind = CppTokenKind.IdentLiteral;
            TextPosition identStart = Buffer.LexemeStart;
            int identLength = Buffer.LexemeWidth;
            string identString = identBuffer.ToString();

            if (isPreprocessor && PreProcessorKeywords.Contains(identString))
                kind = CppTokenKind.PreprocessorKeyword;
            else if (ReservedKeywords.Contains(identString))
                kind = CppTokenKind.ReservedKeyword;
            else if (TypeKeywords.Contains(identString) || GlobalClassKeywords.Contains(identString))
                kind = CppTokenKind.TypeKeyword;
            else
                kind = CppTokenKind.IdentLiteral;
            return new LexResult(kind, true);
        }

        private LexResult LexString(string typeName)
        {
            Debug.Assert(Buffer.Peek(0) == '"' || Buffer.Peek(0) == '\'');
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
                                    PushError(Buffer.TextPosition, $"Unsupported hex escape character '{Buffer.Peek()}'!", typeName);
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
                                PushError(Buffer.TextPosition, $"Not supported escape character '{Buffer.Peek()}'!", typeName);
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
                PushError(Buffer.LexemeStart, $"Unterminated {typeName} literal!", typeName);
            else
            {
                if (minCount > 0 && count < minCount)
                    PushError(Buffer.LexemeStart, $"Not enough characters for {typeName} literal, expect {minCount} but got {count}!", typeName);
                else if (maxCount > -1 && (count > maxCount))
                    PushError(Buffer.LexemeStart, $"Too many characters for {typeName} literal, expect {maxCount} but got {count}!", typeName);
            }
            return new LexResult(kind, isComplete);
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

        private LexResult LexNumber()
        {
            Debug.Assert(SyntaxUtils.IsNumeric(Buffer.Peek()) || Buffer.Peek() == '.');
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
                            PushError(Buffer.TextPosition, $"Expect integer literal, but got '{Buffer.Peek()}'", kind.ToString());
                        break;

                    case CppTokenKind.OctalLiteral:
                        if (SyntaxUtils.IsOctal(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsOctal);
                        else
                            PushError(Buffer.TextPosition, $"Expect octal literal, but got '{Buffer.Peek()}'", kind.ToString());
                        break;

                    case CppTokenKind.HexLiteral:
                        if (SyntaxUtils.IsHex(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsHex);
                        else
                            PushError(Buffer.TextPosition, $"Expect hex literal, but got '{Buffer.Peek()}'", kind.ToString());
                        break;

                    case CppTokenKind.BinaryLiteral:
                        if (SyntaxUtils.IsBinary(Buffer.Peek()))
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsBinary);
                        else
                            PushError(Buffer.TextPosition, $"Expect binary literal, but got '{Buffer.Peek()}'", kind.ToString());
                        break;

                    default:
                        PushError(Buffer.TextPosition, $"Unsupported token kind '{kind}' for integer literal on {Buffer}", kind.ToString());
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
                            PushError(Buffer.TextPosition, $"Too many single quote escape in integer literal, expect any integer literal but got '{Buffer.Peek()}'", kind.ToString());
                            return new LexResult(kind, false);
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
                    PushError(Buffer.TextPosition, $"Expect any integer literal after starting dot, but got '{Buffer.Peek()}'", kind.ToString());
                    return new LexResult(kind, false);
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
            return new LexResult(kind, true);
        }

        private bool LexPreprocessor(CppLexerState state)
        {
            Debug.Assert(Buffer.Peek() == '#');

            state.StartPreprocessor();

            // Preprocessor start
            Buffer.StartLexeme();
            Buffer.AdvanceColumn();
            PushToken(CppTokenPool.Make(CppTokenKind.PreprocessorStart, Buffer.LexemeRange, true));

            do
            {
                SkipSpacings(SkipType.All);
                Buffer.StartLexeme();
                char first = Buffer.Peek();
                char second = Buffer.Peek(1);
                char third = Buffer.Peek(2);
                if (first == '\\')
                {
                    if (SyntaxUtils.IsLineBreak(second))
                    {
                        Buffer.AdvanceColumn();
                        int lb = SyntaxUtils.GetLineBreakChars(second, third);
                        Buffer.AdvanceLine(lb);
                        continue;
                    }
                    else
                    {
                        PushError(Buffer.TextPosition, $"Unterminated preprocessor next-line, expect linebreak after '\' but got '{second}'", "Preprocessor");
                        return (false);
                    }
                }
                else if (first == '#')
                {
                    Buffer.AdvanceColumn();
                    PushToken(CppTokenPool.Make(CppTokenKind.PreprocessorOperator, Buffer.LexemeRange, true));
                }
                else if (SyntaxUtils.IsLineBreak(first))
                {
                    int lb = SyntaxUtils.GetLineBreakChars(first, second);
                    Buffer.AdvanceLine(lb);
                    PushToken(CppTokenPool.Make(CppTokenKind.EndOfLine, Buffer.LexemeRange, true));
                    break;
                }
                else if (SyntaxUtils.IsIdentStart(first))
                {
                    LexResult identResult = LexIdent(true);
                    CppToken identToken = CppTokenPool.Make(identResult.Kind, Buffer.LexemeRange, identResult.IsComplete);
                    PushToken(identToken);
                    SkipSpacings(SkipType.All);
                    Buffer.StartLexeme();
                    if (identToken.Kind == CppTokenKind.PreprocessorKeyword)
                    {
                        switch (identToken.Value)
                        {
                            case "define":
                                {
                                    if (!SyntaxUtils.IsIdentStart(Buffer.Peek()))
                                    {
                                        PushError(Buffer.TextPosition, $"Expect identifier for define, but got '{Buffer.Peek()}'", "Preprocessor");
                                        return (false);
                                    }
                                    LexResult defineValueResult = LexIdent(false);
                                    CppToken defineValueToken = CppTokenPool.Make(CppTokenKind.PreprocessorDefineSource, Buffer.LexemeRange, defineValueResult.IsComplete);
                                    PushToken(defineValueToken);
                                }
                                break;

                            case "defined":
                                {
                                    if (Buffer.Peek() == '(')
                                    {
                                        Buffer.AdvanceColumn();
                                        if (!SyntaxUtils.IsIdentStart(Buffer.Peek()))
                                        {
                                            PushError(Buffer.TextPosition, $"Expect identifier for defined, but got '{Buffer.Peek()}'", "Preprocessor");
                                            return (false);
                                        }
                                        LexResult definedValueResult = LexIdent(false);
                                        CppToken definedValueToken = CppTokenPool.Make(CppTokenKind.PreprocessorDefineTarget, Buffer.LexemeRange, definedValueResult.IsComplete);
                                        PushToken(definedValueToken);
                                        SkipSpacings(SkipType.All);
                                        if (Buffer.Peek() != ')')
                                        {
                                            PushError(Buffer.TextPosition, $"Unterminated defined token, expect ')' but got '{Buffer.Peek()}'", "Preprocessor");
                                            return (false);
                                        }
                                    }
                                }
                                break;

                            case "include":
                                {
                                    char n = Buffer.Peek();
                                    if (n == '<' || n == '"')
                                    {
                                        bool isComplete = false;
                                        Buffer.AdvanceColumn();
                                        char quote = (n == '<') ? '>' : n;
                                        while (!Buffer.IsEOF)
                                        {
                                            if (Buffer.Peek() == quote)
                                            {
                                                isComplete = true;
                                                Buffer.AdvanceColumn();
                                                break;
                                            }
                                            Buffer.AdvanceColumn();
                                        }
                                        CppToken includeToken = CppTokenPool.Make(CppTokenKind.PreprocessorInclude, Buffer.LexemeRange, isComplete);
                                        PushToken(includeToken);
                                    }
                                    else
                                    {
                                        return (false);
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (!LexNext(state))
                        break;
                }
            } while (!Buffer.IsEOF);

            state.EndPreprocessor();

            PushToken(CppTokenPool.Make(CppTokenKind.PreprocessorEnd, new TextRange(Buffer.TextPosition, 0), true));

            return (true);
        }

        protected override bool LexNext(State hiddenState)
        {
            CppLexerState state = (CppLexerState)hiddenState;
            bool allowWhitespaces = !state.IsInsidePreprocessor;
            if (allowWhitespaces)
                SkipAllWhitespaces();
            if (Buffer.IsEOF)
                return (false);
            int line = Buffer.TextPosition.Line;
            char first = Buffer.Peek();
            char second = Buffer.Peek(1);
            char third = Buffer.Peek(2);
            Buffer.StartLexeme();
            LexResult lexRes = new LexResult(CppTokenKind.Unknown, true);
            switch (first)
            {
                case '&':
                    {
                        if (second == '&')
                        {
                            lexRes.Kind = CppTokenKind.LogicalAndOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.AndAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.AndOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '|':
                    {
                        if (second == '|')
                        {
                            lexRes.Kind = CppTokenKind.LogicalOrOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.OrAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.OrOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '=':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.LogicalEqualsOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.EqOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '!':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.LogicalNotEqualsOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.ExclationMark;
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
                                lexRes.Kind = CppTokenKind.LeftShiftAssign;
                                Buffer.AdvanceColumns(3);
                            }
                            else
                            {
                                lexRes.Kind = CppTokenKind.LeftShiftOp;
                                Buffer.AdvanceColumns(2);
                            }
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.LessOrEqualOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.LessThanOp;
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
                                lexRes.Kind = CppTokenKind.RightShiftAssign;
                                Buffer.AdvanceColumns(3);
                            }
                            else
                            {
                                lexRes.Kind = CppTokenKind.RightShiftOp;
                                Buffer.AdvanceColumns(2);
                            }
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.GreaterOrEqualOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.GreaterThanOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '+':
                    {
                        if (second == '+')
                        {
                            lexRes.Kind = CppTokenKind.IncOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.AddAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.AddOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '-':
                    {
                        if (second == '-')
                        {
                            lexRes.Kind = CppTokenKind.DecOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.SubAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '>')
                        {
                            lexRes.Kind = CppTokenKind.PtrOp;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.SubOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '/':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.DivAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else if (second == '/')
                            lexRes = LexSingleLineComment(true);
                        else if (second == '*')
                            lexRes = LexMultiLineComment(true);
                        else
                        {
                            Buffer.AdvanceColumn();
                            lexRes.Kind = CppTokenKind.DivOp;
                        }
                    }
                    break;

                case '*':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.MulAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.MulOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '%':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.ModAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.ModOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '.':
                    {
                        if (second == '.' && third == '.')
                        {
                            lexRes.Kind = CppTokenKind.Ellipsis;
                            Buffer.AdvanceColumns(3);
                        }
                        else if (SyntaxUtils.IsNumeric(second))
                            lexRes = LexNumber();
                        else
                        {
                            lexRes.Kind = CppTokenKind.Dot;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '^':
                    {
                        if (second == '=')
                        {
                            lexRes.Kind = CppTokenKind.XorAssign;
                            Buffer.AdvanceColumns(2);
                        }
                        else
                        {
                            lexRes.Kind = CppTokenKind.XorOp;
                            Buffer.AdvanceColumn();
                        }
                    }
                    break;

                case '#':
                    return LexPreprocessor(state);

                case '"':
                    lexRes = LexString("string");
                    break;

                case '\'':
                    lexRes = LexString("char");
                    break;

                case '~':
                    lexRes.Kind = CppTokenKind.Tilde;
                    Buffer.AdvanceColumn();
                    break;
                case '\\':
                    lexRes.Kind = CppTokenKind.Backslash;
                    Buffer.AdvanceColumn();
                    break;
                case ',':
                    lexRes.Kind = CppTokenKind.Comma;
                    Buffer.AdvanceColumn();
                    break;
                case ';':
                    lexRes.Kind = CppTokenKind.Semicolon;
                    Buffer.AdvanceColumn();
                    break;
                case ':':
                    lexRes.Kind = CppTokenKind.Colon;
                    Buffer.AdvanceColumn();
                    break;
                case '?':
                    lexRes.Kind = CppTokenKind.QuestionMark;
                    Buffer.AdvanceColumn();
                    break;
                case '{':
                    lexRes.Kind = CppTokenKind.LeftBrace;
                    Buffer.AdvanceColumn();
                    break;
                case '}':
                    lexRes.Kind = CppTokenKind.RightBrace;
                    Buffer.AdvanceColumn();
                    break;
                case '[':
                    lexRes.Kind = CppTokenKind.LeftBracket;
                    Buffer.AdvanceColumn();
                    break;
                case ']':
                    lexRes.Kind = CppTokenKind.RightBracket;
                    Buffer.AdvanceColumn();
                    break;
                case '(':
                    lexRes.Kind = CppTokenKind.LeftParen;
                    Buffer.AdvanceColumn();
                    break;
                case ')':
                    lexRes.Kind = CppTokenKind.RightParen;
                    Buffer.AdvanceColumn();
                    break;

                default:
                    {
                        if (SyntaxUtils.IsLineBreak(first) && allowWhitespaces)
                        {
                            lexRes.Kind = CppTokenKind.EndOfLine;
                            int nb = SyntaxUtils.GetLineBreakChars(first, second);
                            Buffer.AdvanceLine(nb);
                        }
                        else if (first == '\t' && allowWhitespaces)
                        {
                            lexRes.Kind = CppTokenKind.Spacings;
                            while (!Buffer.IsEOF)
                            {
                                if (Buffer.Peek() != '\t')
                                    break;
                                Buffer.AdvanceTab();
                            }
                        }
                        else if (SyntaxUtils.IsSpacing(first) && allowWhitespaces)
                        {
                            lexRes.Kind = CppTokenKind.Spacings;
                            Buffer.AdvanceColumnsWhile(SyntaxUtils.IsSpacing);
                        }
                        else if (SyntaxUtils.IsIdentStart(first))
                        {
                            Debug.Assert(!state.IsInsidePreprocessor);
                            lexRes = LexIdent(false);
                        }
                        else if (SyntaxUtils.IsNumeric(first))
                            lexRes = LexNumber();
                        else
                        {
                            PushError(Buffer.TextPosition, $"Unexpected character '{first}'", "Character");
                            return (false);
                        }
                    }
                    break;
            }
            return PushToken(CppTokenPool.Make(lexRes.Kind, Buffer.LexemeRange, lexRes.IsComplete));
        }
    }
}
