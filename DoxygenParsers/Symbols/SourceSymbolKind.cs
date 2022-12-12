namespace TSP.DoxygenEditor.Symbols
{
    public enum SourceSymbolKind
    {
        Unknown = 0,
        DoxygenSection,
        DoxygenPage,
        DoxygenConfigValue,
        CppType,
        CppStruct,
        CppClass,
        CppEnum,
        CppFunctionDefinition,
        CppFunctionBody,
        CppMember,
        CppMacro,
    }
}
