﻿namespace TSP.DoxygenEditor.Lexers.Cpp
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
        Binary,
        Float,
        Double,

        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        LeftCurlyBrace,
        RightCurlyBrace,

        Semicolon,
        Comma,
        Dot,

        Colon,
        QuestionMark,
        OpNot,
        OpMod,
        OpNeg,
        OpLess,
        OpGreater,
        OpEquals,
        OpMul,
        OpDiv,
        OpPlus,
        OpMinus,
        OpOr,
        OpAnd,
    }
}
