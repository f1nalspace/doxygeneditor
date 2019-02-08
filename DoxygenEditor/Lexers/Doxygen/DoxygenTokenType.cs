namespace TSP.DoxygenEditor.Lexers.Doxygen
{
    public enum DoxygenTokenType
    {
        Invalid = -1,
        EOF,
        BlockStart,
        BlockEnd,
        BlockChars,
        GroupStart,
        GroupEnd,
        Command,
        Ident,
        Caption,
        Text,
        CodeType,
        CodeBlock,
    }
}
