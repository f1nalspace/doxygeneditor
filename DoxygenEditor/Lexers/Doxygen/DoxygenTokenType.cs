namespace DoxygenEditor.Lexers.Doxygen
{
    public enum DoxygenTokenType
    {
        Invalid = -1,
        EOF,
        BlockStart,
        BlockEnd,
        Command,
        Ident,
        Caption,
        Text,
        CodeType,
        CodeBlock,
    }
}
