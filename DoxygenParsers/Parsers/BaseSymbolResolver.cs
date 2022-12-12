using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseSymbolResolver<TEntity, TToken> where TEntity : BaseEntity where TToken : BaseToken
    {
        protected readonly SymbolTable _localSymbolTable;

        public BaseSymbolResolver(SymbolTable localSymbolTable)
        {
            _localSymbolTable = localSymbolTable;
        }

        public virtual void ResolveTokens(IEnumerable<TToken> tokens) { }
    }
}
