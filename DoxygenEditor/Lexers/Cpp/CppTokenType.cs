namespace DoxygenEditor.Lexers.Cpp
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
    }
}
