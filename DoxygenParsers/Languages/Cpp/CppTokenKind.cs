using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public enum CppTokenKind
    {
        // Unknown token
        Unknown = -1,

        // End of stream/file
        Eof = 0,
        // ([ \v\f]+)|([\t]+)
        Spacings,
        // ([\r][\n])|([\n][\r])|([\n\r])
        EndOfLine,

        // \/\/^[\r\n]*
        SingleLineComment,
        // \/\/[/!]^[\r\n]*
        SingleLineCommentDoc,
        // \/\*.*\*\/
        MultiLineComment,
        // \/\*[*!].*\*\/
        MultiLineCommentDoc,

        // #(.*)
        Preprocessor,

        // [a-zA-Z_][a-zA-Z0-9_]* (Normal identifier)
        IdentLiteral,
        // [a-zA-Z_][a-zA-Z0-9_]* (Reserved keyword)
        ReservedKeyword,
        // [a-zA-Z_][a-zA-Z0-9_]* (Type)
        TypeKeyword,

        // L?\"^[\"]*\"
        // @TODO(final): Support for C++/11 unicode escape
        StringLiteral,
        // [L]?\'([\w^\\]|[\\a\\b\\f\\n\\r\\t\\v]|\\[0-7]{1,3}|\\X[0-9a-fA-F]{1,2})\'
        // @TODO(final): Support for C++/11 unicode escape
        CharLiteral,
        // [0-9]+
        IntegerLiteral,
        // 0[xX][a-fA-F0-9]+
        HexLiteral,
        // 0[0-7]*
        OctalLiteral,
        // 0b[0-1]+
        BinaryLiteral,
        // (\.[0-9]+)|([0-9]+\.[0-9]+)
        IntegerFloatLiteral,
        // (\.[0-9a-fA-F]+)|([0-9a-fA-F]+\.[0-9a-fA-F]+)
        HexadecimalFloatLiteral,

        [TokenKind(Text = ">>=")]
        RightShiftAssign,
        [TokenKind(Text = "<<=")]
        LeftShiftAssign,
        [TokenKind(Text = "+=")]
        AddAssign,
        [TokenKind(Text = "-=")]
        SubAssign,
        [TokenKind(Text = "*=")]
        MulAssign,
        [TokenKind(Text = "/=")]
        DivAssign,
        [TokenKind(Text = "%=")]
        ModAssign,
        [TokenKind(Text = "&=")]
        AndAssign,
        [TokenKind(Text = "|=")]
        OrAssign,
        [TokenKind(Text = "^=")]
        XorAssign,
        [TokenKind(Text = ">>")]
        RightShiftOp,
        [TokenKind(Text = "<<")]
        LeftShiftOp,
        [TokenKind(Text = "++")]
        IncOp,
        [TokenKind(Text = "--")]
        DecOp,
        [TokenKind(Text = "->")]
        PtrOp,
        [TokenKind(Text = "&&")]
        LogicalAndOp,
        [TokenKind(Text = "||")]
        LogicalOrOp,
        [TokenKind(Text = "<=")]
        LessOrEqualOp,
        [TokenKind(Text = ">=")]
        GreaterOrEqualOp,
        [TokenKind(Text = "==")]
        LogicalEqualsOp,
        [TokenKind(Text = "!=")]
        LogicalNotEqualsOp,

        [TokenKind(Text = "=")]
        EqOp,
        [TokenKind(Text = "&")]
        AndOp,
        [TokenKind(Text = "|")]
        OrOp,
        [TokenKind(Text = "^")]
        XorOp,
        [TokenKind(Text = "+")]
        AddOp,
        [TokenKind(Text = "-")]
        SubOp,
        [TokenKind(Text = "*")]
        MulOp,
        [TokenKind(Text = "/")]
        DivOp,
        [TokenKind(Text = "%")]
        ModOp,
        [TokenKind(Text = "<")]
        LessThanOp,
        [TokenKind(Text = ">")]
        GreaterThanOp,

        [TokenKind(Text = "(")]
        LeftParen,
        [TokenKind(Text = ")")]
        RightParen,
        [TokenKind(Text = "{")]
        LeftBrace,
        [TokenKind(Text = "}")]
        RightBrace,
        [TokenKind(Text = "[")]
        LeftBracket,
        [TokenKind(Text = "]")]
        RightBracket,

        [TokenKind(Text = "...")]
        Ellipsis,
        [TokenKind(Text = "!")]
        ExclationMark,
        [TokenKind(Text = "?")]
        QuestionMark,
        [TokenKind(Text = ".")]
        Dot,
        [TokenKind(Text = "#")]
        Raute,
        [TokenKind(Text = "\\")]
        Backslash,
        [TokenKind(Text = "~")]
        Tilde,
        [TokenKind(Text = ";")]
        Semicolon,
        [TokenKind(Text = ",")]
        Comma,
        [TokenKind(Text = ":")]
        Colon,
    }
}
