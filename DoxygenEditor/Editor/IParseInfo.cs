using System.Collections.Generic;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Editor
{
    interface IParseInfo
    {
        IEnumerable<TextError> Errors { get; }
        IEnumerable<PerformanceItemModel> PerformanceItems { get; }
        IBaseNode DoxyBlockTree { get; }
        IBaseNode DoxyConfigTree { get; }
        IBaseNode CppTree { get; }
        SymbolTable LocalSymbolTable { get; }
    }
}
