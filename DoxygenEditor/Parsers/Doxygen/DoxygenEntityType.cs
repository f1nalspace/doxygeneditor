namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    enum DoxygenEntityType
    {
        None = 0,

        BlockMulti = 1,
        BlockSingle = 2,
        Group = 3,

        Page = 50,

        Section = 100,
        SubSection,
        SubSubSection,

        SourceDeclaration = 300,

        SubPage = 1000,
        Ref,
        Brief,
        Foreign,
    }
}
