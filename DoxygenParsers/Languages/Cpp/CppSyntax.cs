using System.Collections.Generic;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public static class CppSyntax
    {
        public static readonly HashSet<string> StorageClassIdentifiers = new HashSet<string>() {
            "static",
            "inline",
            "extern",
        };
    }
}
