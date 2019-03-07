using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public static class SymbolCache
    {
        private class Entry
        {
            private readonly Dictionary<string, List<SourceSymbol>> _sources = new Dictionary<string, List<SourceSymbol>>();
            private readonly Dictionary<string, KeyValuePair<string, List<RefrenceSymbol>>> _references = new Dictionary<string, KeyValuePair<string, List<RefrenceSymbol>>>();
            public IEnumerable<KeyValuePair<string, List<RefrenceSymbol>>> References => _references.Values;

            public bool HasSource(string name)
            {
                bool result = _sources.ContainsKey(name);
                return (result);
            }

            public void AddSource(string name, SourceSymbol symbol)
            {
                List<SourceSymbol> set;
                if (_sources.ContainsKey(name))
                    set = _sources[name];
                else
                {
                    set = new List<SourceSymbol>();
                    _sources.Add(name, set);
                }
                set.Add(symbol);
            }

            public void AddReference(string name, RefrenceSymbol symbol)
            {
                KeyValuePair<string, List<RefrenceSymbol>> pair;
                if (_references.ContainsKey(name))
                    pair = _references[name];
                else
                {
                    pair = new KeyValuePair<string, List<RefrenceSymbol>>(name, new List<RefrenceSymbol>());
                    _references.Add(name, pair);
                }
                pair.Value.Add(symbol);
            }

            public void Clear()
            {
                _sources.Clear();
                _references.Clear();
            }
        }
        private readonly static Dictionary<object, Entry> _entries = new Dictionary<object, Entry>();

        public static void Clear(object tag)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag");
            if (_entries.ContainsKey(tag))
                _entries[tag].Clear();

        }

        public static void AddSource(object tag, string name, SourceSymbol symbol)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag");
            if (!_entries.ContainsKey(tag))
                _entries.Add(tag, new Entry());
            _entries[tag].AddSource(name, symbol);
        }

        public static void AddReference(object tag, string name, RefrenceSymbol symbol)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag");
            if (!_entries.ContainsKey(tag))
                _entries.Add(tag, new Entry());
            _entries[tag].AddReference(name, symbol);
        }

        public static bool HasReference(string symbol)
        {
            foreach (var entryPair in _entries)
            {
                var tag = entryPair.Key;
                var entry = entryPair.Value;
                if (entry.HasSource(symbol))
                    return (true);
            }
            return (false);
        }

        public static IEnumerable<KeyValuePair<object, TextError>> Validate()
        {
            List<KeyValuePair<object, TextError>> result = new List<KeyValuePair<object, TextError>>();
            foreach (var entryPair in _entries)
            {
                var tag = entryPair.Key;
                var entry = entryPair.Value;
                foreach (var names in entry.References)
                {
                    string name = names.Key;
                    foreach (var reference in names.Value)
                    {
                        if (!HasReference(name))
                            result.Add(new KeyValuePair<object, TextError>(tag, new TextError(reference.Range.Position, "Symbols", $"Missing symbol '{name}'")));
                    }
                }
            }
            return (result);
        }
    }
}
