namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    enum DoxygenEntityType
    {
        None = 0,

        Page = 10,

        Section = 50,
        SubSection,
        SubSubSection,

        GroupDefinition = 100,
        GroupBegin,
        GroupEnd,

        SourceDeclaration,

        SubPage = 500,
        Ref,
    }
}
