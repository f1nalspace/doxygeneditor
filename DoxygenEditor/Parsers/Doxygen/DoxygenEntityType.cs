namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    enum DoxygenEntityType
    {
        None = 0,

        Block = 1,
        Group = 2,

        Page = 50,

        Section = 100,
        SubSection,
        SubSubSection,

        SourceDeclaration = 300,

        SubPage = 1000,
        Ref,
    }
}
