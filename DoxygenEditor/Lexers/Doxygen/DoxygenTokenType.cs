namespace TSP.DoxygenEditor.Lexers.Doxygen
{
    public enum DoxygenTokenType
    {
        Invalid = -1,
        EOF,
        BlockStartSingle,
        BlockStartMulti,
        BlockEnd,
        BlockChars,
        GroupStart,
        GroupEnd,
        Command,
        Name,
        Caption,
        Text,
        CodeType,
        CodeBlock,
    }
}
