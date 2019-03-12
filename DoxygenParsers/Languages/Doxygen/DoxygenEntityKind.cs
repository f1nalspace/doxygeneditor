namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public enum DoxygenEntityKind
    {
        None = 0,

        // Blocks
        BlockMulti = 1,
        BlockSingle = 2,

        // Groups
        Group = 3,

        // Page or section
        Page = 50,
        Section,
        SubSection,
        SubSubSection,

        // Block commands
        BlockCommand = 100,

        // Paragraphs
        Paragraph = 500,
        Brief,
        See,

        // Visual
        VisualEnhancement = 1000,

        // Basic
        Basic = 10000,
        Reference,
        Text,
    }
}
