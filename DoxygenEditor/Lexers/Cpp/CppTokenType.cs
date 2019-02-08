namespace TSP.DoxygenEditor.Lexers.Cpp
{
    public enum CppTokenType
    {
        Invalid = -1,
        EOF,
        MultiLineComment,
        MultiLineCommentDoc,
        SingleLineComment,
        SingleLineCommentDoc,
        Preprocessor,
        Identifier,
        ReservedKeyword,
        TypeKeyword,
        String,
        Integer,
        Hex,
        Octal,
        Decimal,

        [TokenText("typedef")]
        Typedef,
        [TokenText("struct")]
        Struct,
        [TokenText("union")]
        Union,
        [TokenText("enum")]
        Enum,
        [TokenText("class")]
        Class,
        [TokenText("namespace")]
        Namespace,
        [TokenText("template")]
        Template,

        FunctionDeclaration,

        [TokenText("(")]
        LeftParenthsis,
        [TokenText(")")]
        RightParenthsis,
        [TokenText("[")]
        LeftSquareBracket,
        [TokenText("]")]
        RightSquareBracket,
        [TokenText("{")]
        LeftCurlyBrace,
        [TokenText("}")]
        RightCurlyBrace,

        [TokenText(";")]
        Semicolon,
        [TokenText(",")]
        Comma,
        [TokenText(":")]
        OpTernaryElse,
        [TokenText("?")]
        OpTernaryIf,

        [TokenText("~")]
        OpNot,
        [TokenText("%")]
        OpMod,
        [TokenText("!")]
        OpNeg,
        [TokenText("<")]
        OpLess,
        [TokenText(">")]
        OpGreater,
        [TokenText("=")]
        OpEquals,
        [TokenText("*")]
        OpMul,
        [TokenText("/")]
        OpDiv,
        [TokenText("+")]
        OpPlus,
        [TokenText("-")]
        OpMinus,
        [TokenText("|")]
        OpOr,
        [TokenText("&")]
        OpAnd,
    }
}
