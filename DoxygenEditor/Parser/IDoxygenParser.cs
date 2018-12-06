namespace DoxygenEditor.Parser
{
    public interface IDoxygenParser
    {
        ParseState Parse(string sourceText);
    }
}
