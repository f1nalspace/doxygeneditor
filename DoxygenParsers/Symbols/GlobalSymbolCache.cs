using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TSP.DoxygenEditor.TextAnalysis;
using System.Collections;

namespace TSP.DoxygenEditor.Symbols
{
    public static class GlobalSymbolCache
    {
        private readonly static ConcurrentDictionary<object, SymbolTable> _tableMap = new ConcurrentDictionary<object, SymbolTable>();

        public static void Clear(object id)
        {
            if (id == null)
                throw new ArgumentNullException("Id may not be null");
            if (_tableMap.ContainsKey(id))
            {
                var table = _tableMap[id];
                table.Clear();
            }
        }

        public static void Remove(object id)
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

        public static IEnumerable<SourceSymbol> GetSources(object id)
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

        public static IEnumerable<KeyValuePair<object, TextError>> Validate()
        {
            List<KeyValuePair<object, TextError>> result = new List<KeyValuePair<object, TextError>>();
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
                            result.Add(new KeyValuePair<object, TextError>(id, new TextError(reference.Range.Position, "Symbols", $"Missing symbol '{name}'", reference.Kind.ToString(), name) { Tag = reference }));
                    }
                }
            }
            return (result);
        }
    }
}