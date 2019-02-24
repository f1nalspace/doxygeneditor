namespace TSP.FastTokenizer
{
    enum TokenKind
    {
        // Unknown token
        Unknown = -1,

        // End of stream/file
        EOF = 0,

        // [ \v\t\f]+
        Spacings,
        // (\r\n)|(\n\r)|(\n)|(\r)
        EndOfLine,
        // \/\/^[\r\n]*
        SingleLineComment,
        // \/\*.*\*\/
        MultiLineComment,
        // [a-zA-Z_][a-zA-Z0-9_]*
        Ident,
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
        // (\.[0-9]+)|([0-9]+\.[0-9]+)
        DecimalFloatLiteral,
        // (\.[0-9a-fA-F]+)|([0-9a-fA-F]+\.[0-9a-fA-F]+)
        DecimalHexLiteral,

        // &
        Ampersand,
        // &&
        AndOp,
        // &=
        AndAssign,

        // |
        Pipe,
        // ||
        OrOp,
        // |=
        OrAssign,

        // =
        EqualsSign,
        // ==
        EqOp,

        // !
        ExlamationMark,
        // !=
        NeOp,

        // %
        Percent,
        // %=
        ModAssign,

        // <
        Lesser,
        // <<
        LeftOp,
        // <=
        LeOp,
        // <<=
        LeftAssign,

        // >
        Greater,
        // >>
        RightOp,
        // >=
        GeOp,
        // >>=
        RightAssign,

        // +
        Plus,
        // ++
        IncOp,
        // +=
        AddAssign,

        // -
        Minus,
        // --
        DecOp,
        // -=
        SubAssign,
        // ->
        PtrOp,

        // /
        Slash,
        // /=
        DivAssign,

        // *
        Asterisk,
        // *=
        MulAssign,

        // ^
        CircumFlex,
        // ^=
        XorAssign,

        // .
        Dot,
        // ...
        Ellipsis,

        // ~
        Tilde,
        // \
        Backslash,
        // #
        Raute,
        // ,
        Comma,
        // ;
        Semicolon,
        // :
        Colon,
        // ?
        QuestionMark,
        // {
        LeftBrace,
        // }
        RightBrace,
        // [
        LeftBracket,
        // ]
        RightBracket,
        // (
        LeftParen,
        // )
        RightParen,
    }
}
