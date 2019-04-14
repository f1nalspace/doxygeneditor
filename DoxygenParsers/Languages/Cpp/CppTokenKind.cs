using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public enum CppTokenKind
    {
        // Unknown token
        Unknown = -1,

        // End of stream/file
        Eof = 0,
        // Spacings (Tabs, Empty space, Vertical space, etc.)
        Spacings,
        // End of line
        EndOfLine,

        SingleLineComment,
        SingleLineCommentDoc,
        MultiLineComment,
        MultiLineCommentDoc,

        PreprocessorStart,
        PreprocessorOperator,
        PreprocessorKeyword,
        PreprocessorFunctionSource,
        PreprocessorDefineSource,
        PreprocessorDefineUsage,
        PreprocessorDefineMatch,
        PreprocessorDefineArgument,
        PreprocessorInclude,
        PreprocessorEnd,

        IdentLiteral,
        ReservedKeyword,
        GlobalTypeKeyword,
        FunctionIdent,
        UserTypeIdent,
        MemberIdent,

        StringLiteral,
        CharLiteral,
        IntegerLiteral,
        HexLiteral,
        OctalLiteral,
        BinaryLiteral,
        IntegerFloatLiteral,
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
