using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    class DoxygenBlockSymbolResolver : BaseSymbolResolver<DoxygenBlockEntity, DoxygenToken>
    {
        public DoxygenBlockSymbolResolver(SymbolTable localSymbolTable) : base(localSymbolTable)
        {
        }

        public override void ResolveTokens(IEnumerable<DoxygenToken> tokens)
        {
            // Properly change "Any" kinds for each reference symbol
            foreach (KeyValuePair<string, List<ReferenceSymbol>> refPair in _localSymbolTable.ReferenceMap)
            {
                string name = refPair.Key;
                List<ReferenceSymbol> referenceSymbols = refPair.Value;
                foreach (ReferenceSymbol referenceSymbol in referenceSymbols)
                {
                    if (referenceSymbol.Kind == ReferenceSymbolKind.Any)
                    {
                        SourceSymbol sourceSymbol = _localSymbolTable.GetSource(name);
                        if (sourceSymbol != null)
                        {
                            if (sourceSymbol.Kind == SourceSymbolKind.DoxygenSection)
                                referenceSymbol.Kind = ReferenceSymbolKind.DoxygenSection;
                            else if (sourceSymbol.Kind == SourceSymbolKind.CppMacro)
                                referenceSymbol.Kind = ReferenceSymbolKind.CppMacroUsage;
                        }
                    }
                }
            }
        }
    }
}
