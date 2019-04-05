namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public enum DoxygenTokenKind
    {
        Invalid = -1,
        EOF,
        EndOfLine,
        EmptyLine,
        DoxyBlockStartSingle,
        DoxyBlockStartMulti,
        DoxyBlockEnd,
        DoxyBlockChars,
        GroupStart,
        GroupEnd,
        Command,
        CommandStart,
        CommandEnd,
        InvalidCommand,
        ArgumentIdent,
        ArgumentText,
        ArgumentCaption,
        ArgumentFile,
        TextStart,
        TextEnd,
        Text,
        Code,
    }
}
