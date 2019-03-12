using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public static class SymbolCache
    {
        private class Entry
        {
            private readonly Dictionary<string, List<SourceSymbol>> _sources = new Dictionary<string, List<SourceSymbol>>();
            private readonly Dictionary<string, KeyValuePair<string, List<ReferenceSymbol>>> _references = new Dictionary<string, KeyValuePair<string, List<ReferenceSymbol>>>();
            public IEnumerable<KeyValuePair<string, List<ReferenceSymbol>>> References => _references.Values;
            public IEnumerable<KeyValuePair<string, List<SourceSymbol>>> Sources
            {
                get
                {
                    List<KeyValuePair<string, List<SourceSymbol>>> result = new List<KeyValuePair<string, List<SourceSymbol>>>();
                    foreach (var pair in _sources)
                    {
                        var k = new KeyValuePair<string, List<SourceSymbol>>(pair.Key, pair.Value);
                        result.Add(k);
                    }
                    return (result);
                }
            }

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

            public void AddReference(string name, ReferenceSymbol symbol)
            {
                KeyValuePair<string, List<ReferenceSymbol>> pair;
                if (_references.ContainsKey(name))
                    pair = _references[name];
                else
                {
                    pair = new KeyValuePair<string, List<ReferenceSymbol>>(name, new List<ReferenceSymbol>());
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

        public static void Remove(object tag)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag");
            if (_entries.ContainsKey(tag))
            {
                _entries[tag].Clear();
                _entries.Remove(tag);
            }
        }

        public static void AddSource(object tag, string name, SourceSymbol symbol)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag");
            if (!_entries.ContainsKey(tag))
                _entries.Add(tag, new Entry());
            _entries[tag].AddSource(name, symbol);
        }

        public static void AddReference(object tag, string name, ReferenceSymbol symbol)
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

        public static IEnumerable<KeyValuePair<string, SourceSymbol>> GetSources(object tag)
        {
            List<KeyValuePair<string, SourceSymbol>> result = new List<KeyValuePair<string, SourceSymbol>>();
            var entry = _entries.ContainsKey(tag) ? _entries[tag] : null;
            if (entry != null)
            {
                var sources = entry.Sources;
                foreach (var source in sources)
                {
                    foreach (var symbol in source.Value)
                    {
                        result.Add(new KeyValuePair<string, SourceSymbol>(source.Key, symbol));
                    }
                }
            }
            return (result);
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
                            result.Add(new KeyValuePair<object, TextError>(tag, new TextError(reference.Range.Position, "Symbols", $"Missing symbol '{name}'", reference.Kind.ToString(), name) { Tag = reference }));
                    }
                }
            }
            return (result);
        }
    }
}
