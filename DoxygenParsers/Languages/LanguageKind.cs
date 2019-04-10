namespace TSP.DoxygenEditor.Languages
{
    public enum LanguageKind
    {
        None = 0,
        Cpp = 1 << 0,
        DoxygenCode = 1 << 1,
        Doxygen = 1 << 2,
        Html = 1 << 3,
    }
}
