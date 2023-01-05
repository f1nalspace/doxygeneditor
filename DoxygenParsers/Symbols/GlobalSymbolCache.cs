using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TSP.DoxygenEditor.TextAnalysis;
using System.Collections;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;

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
                SymbolTable table = _tableMap[id];
                table.Clear();
            }
        }

        public static SymbolTable GetTable(ISymbolTableId id)
        {
            if (id == null)
                throw new ArgumentNullException("Id may not be null");
            if (_tableMap.ContainsKey(id))
            {
                SymbolTable table = _tableMap[id];
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
                SymbolTable table = _tableMap[id];
                table.Clear();
                ((IDictionary)_tableMap).Remove(id);
            }
        }

        public static void AddOrReplaceTable(SymbolTable table)
        {
            if (table == null)
                throw new ArgumentNullException("Table may not be null");
            Remove(table.Id);
            SymbolTable copy = new SymbolTable(table);
            _tableMap.AddOrUpdate(copy.Id, copy, (key, existingValue) =>
            {
                if (copy != existingValue)
                    throw new ArgumentException($"Duplicate table id '{copy.Id}' are not allowed");
                return (existingValue);
            });
        }

        public static bool HasSystemSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> entryPair in _tableMap)
            {
                ISymbolTableId id = entryPair.Key;
                SymbolTable table = entryPair.Value;
                if (table.HasSystemSymbol(symbol))
                    return (true);
            }
            return (false);
        }

        public static bool HasReference(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> entryPair in _tableMap)
            {
                ISymbolTableId id = entryPair.Key;
                SymbolTable table = entryPair.Value;
                if (table.HasSource(symbol))
                    return (true);
            }
            return (false);
        }

        public static Tuple<SourceSymbol, ISymbolTableId> FindSource(string symbol, Func<ISymbolTableId, bool> tableFilter = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            Tuple<SourceSymbol, ISymbolTableId> bestSource = null;
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> entryPair in _tableMap)
            {
                ISymbolTableId id = entryPair.Key;
                SymbolTable table = entryPair.Value;
                if (tableFilter != null && !tableFilter(id))
                    continue;
                SourceSymbol source = table.GetSource(symbol);
                if (source != null)
                {
                    if (bestSource == null || source.Lang < bestSource.Item1.Lang)
                        bestSource = new Tuple<SourceSymbol, ISymbolTableId>(source, id);
                }
            }
            return bestSource;
        }

        public static IEnumerable<Tuple<SourceSymbol, ISymbolTableId>> FindSources(string symbol, Func<ISymbolTableId, bool> tableFilter = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException("Symbol may not be null or empty");
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> entryPair in _tableMap)
            {
                ISymbolTableId id = entryPair.Key;
                SymbolTable table = entryPair.Value;
                if (tableFilter != null && !tableFilter(id))
                    continue;
                SourceSymbol result = table.GetSource(symbol);
                if (result != null)
                    yield return new Tuple<SourceSymbol, ISymbolTableId>(result, id);
            }
        }

        public static BaseSymbol FindSymbolFromRange(TextRange range)
        {
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> entryPair in _tableMap)
            {
                ISymbolTableId id = entryPair.Key;
                SymbolTable table = entryPair.Value;
                BaseSymbol result = table.FindSymbolFromRange(range);
                if (result != null)
                    return (result);
            }
            return (null);
        }

        public static IEnumerable<SourceSymbol> GetSources(ISymbolTableId id)
        {
            SymbolTable table = _tableMap.ContainsKey(id) ? _tableMap[id] : null;
            if (table != null)
            {
                IEnumerable<KeyValuePair<string, List<SourceSymbol>>> sources = table.SourceMap;
                foreach (KeyValuePair<string, List<SourceSymbol>> source in sources)
                {
                    foreach (SourceSymbol symbol in source.Value)
                        yield return symbol;
                }
            }
        }

        public class ValidationConfigration
        {
            public bool ExcludeCppPreprocessorMatch { get; set; }
            public bool ExcludeCppPreprocessorUsage { get; set; }
        }

        public static IEnumerable<KeyValuePair<ISymbolTableId, TextError>> Validate(ValidationConfigration config)
        {
            List<KeyValuePair<ISymbolTableId, TextError>> result = new List<KeyValuePair<ISymbolTableId, TextError>>();
            foreach (KeyValuePair<ISymbolTableId, SymbolTable> tablePair in _tableMap)
            {
                ISymbolTableId id = tablePair.Key;
                SymbolTable table = tablePair.Value;
                foreach (KeyValuePair<string, List<ReferenceSymbol>> names in table.ReferenceMap)
                {
                    string name = names.Key;
                    foreach (ReferenceSymbol reference in names.Value)
                    {
                        if (config.ExcludeCppPreprocessorMatch)
                        {
                            if (reference.Kind == ReferenceSymbolKind.CppMacroMatch)
                                continue;
                        }
                        if (config.ExcludeCppPreprocessorUsage)
                        {
                            if (reference.Kind == ReferenceSymbolKind.CppMacroUsage)
                                continue;
                        }
                        if (!HasReference(name) && !HasSystemSymbol(name))
                        {
                            LanguageKind lang;
                            if (reference.Node is CppNode)
                                lang = LanguageKind.Cpp;
                            else if (reference.Node is DoxygenBlockNode)
                                lang = LanguageKind.DoxygenCode;
                            else if (reference.Node is DoxygenConfigNode)
                                lang = LanguageKind.DoxygenConfig;
                            else
                                lang = reference.Lang;
                            result.Add(new KeyValuePair<ISymbolTableId, TextError>(id, new TextError(lang, reference.Range.Position, "Symbols", $"Missing symbol '{name}'", reference.Kind.ToString(), name)));
                        }
                    }
                }
            }
            return (result);
        }
    }
}