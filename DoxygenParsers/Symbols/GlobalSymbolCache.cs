using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TSP.DoxygenEditor.TextAnalysis;
using System.Collections;

namespace TSP.DoxygenEditor.Symbols
{
    public static class GlobalSymbolCache
    {
        private readonly static ConcurrentDictionary<ISymbolTableId, SymbolTable> _tableMap = new ConcurrentDictionary<ISymbolTableId, SymbolTable>();

        public static void Clear(ISymbolTableId id)
        {
            if (id == null)
                throw new ArgumentNullException("Id may not be null");
            if (_tableMap.ContainsKey(id))
            {
                var table = _tableMap[id];
                table.Clear();
            }
        }

        public static SymbolTable GetTable(ISymbolTableId id)
        {
            if (id == null)
                throw new ArgumentNullException("Id may not be null");
            if (_tableMap.ContainsKey(id))
            {
                var table = _tableMap[id];
                return (table);
            }
            return (null);
        }

        public static void Remove(ISymbolTableId id)
        {
            if (id == null)
                throw new ArgumentNullException("Id may not be null");
            if (_tableMap.ContainsKey(id))
            {
                var table = _tableMap[id];
                ((IDictionary)_tableMap).Remove(id);
                table.Clear();
            }
        }

        public static void AddOrReplaceTable(SymbolTable table)
        {
            if (table == null)
                throw new ArgumentNullException("Table may not be null");
            Remove(table.Id);
            _tableMap.AddOrUpdate(table.Id, table, (key, existingValue) =>
            {
                if (table != existingValue)
                    throw new ArgumentException($"Duplicate table id '{table.Id}' are not allowed");
                return (existingValue);
            });
        }

        public static bool HasReference(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            foreach (var entryPair in _tableMap)
            {
                var id = entryPair.Key;
                var table = entryPair.Value;
                if (table.HasSource(symbol))
                    return (true);
            }
            return (false);
        }

        public static Tuple<SourceSymbol, ISymbolTableId> FindSource(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            foreach (var entryPair in _tableMap)
            {
                var id = entryPair.Key;
                var table = entryPair.Value;
                SourceSymbol result = table.GetSource(symbol);
                if (result != null)
                    return new Tuple<SourceSymbol, ISymbolTableId>(result, id);
            }
            return (null);
        }

        public static BaseSymbol FindSymbolFromRange(TextRange range)
        {
            foreach (var entryPair in _tableMap)
            {
                var id = entryPair.Key;
                var table = entryPair.Value;
                var result = table.FindSymbolFromRange(range);
                if (result != null)
                    return (result);
            }
            return (null);
        }

        public static IEnumerable<SourceSymbol> GetSources(ISymbolTableId id)
        {
            var table = _tableMap.ContainsKey(id) ? _tableMap[id] : null;
            if (table != null)
            {
                var sources = table.SourceMap;
                foreach (var source in sources)
                {
                    foreach (SourceSymbol symbol in source.Value)
                        yield return symbol;
                }
            }
        }

        public static IEnumerable<KeyValuePair<ISymbolTableId, TextError>> Validate()
        {
            List<KeyValuePair<ISymbolTableId, TextError>> result = new List<KeyValuePair<ISymbolTableId, TextError>>();
            foreach (var tablePair in _tableMap)
            {
                var id = tablePair.Key;
                var table = tablePair.Value;
                foreach (var names in table.ReferenceMap)
                {
                    string name = names.Key;
                    foreach (var reference in names.Value)
                    {
                        if (!HasReference(name))
                            result.Add(new KeyValuePair<ISymbolTableId, TextError>(id, new TextError(reference.Range.Position, "Symbols", $"Missing symbol '{name}'", reference.Kind.ToString(), name) { Tag = reference }));
                    }
                }
            }
            return (result);
        }
    }
}