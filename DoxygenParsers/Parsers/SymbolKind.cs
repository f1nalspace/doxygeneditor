namespace TSP.DoxygenEditor.Parsers
{
    public enum SymbolKind
    {
        Unknown = 0,
        DoxygenSection,
        DoxygenPage,
        CppType,
        CppStruct,
        CppFunctionDefinition,
        CppFunctionBody,
        CppEnum,
        CppMember,
        CppDefine,
    }
}
