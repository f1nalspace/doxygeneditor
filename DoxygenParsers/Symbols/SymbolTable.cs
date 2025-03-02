using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Symbols
{
    public class SymbolTable
    {
        public ISymbolTableId Id { get; set; }
        public bool IsValid { get; set; }

        public SymbolTable(ISymbolTableId id)
        {
            Id = id;
            IsValid = false;
        }
        public SymbolTable(SymbolTable other)
        {
            Id = other.Id;
            IsValid = other.IsValid;
            AddTable(other);
        }

        public IEnumerable<KeyValuePair<string, List<SourceSymbol>>> SourceMap => _sources;
        private readonly Dictionary<string, List<SourceSymbol>> _sources = new Dictionary<string, List<SourceSymbol>>();
        public int SourceCount => _sources.Count;

        public IEnumerable<KeyValuePair<string, List<ReferenceSymbol>>> ReferenceMap => _references;
        private readonly Dictionary<string, List<ReferenceSymbol>> _references = new Dictionary<string, List<ReferenceSymbol>>();
        public int ReferenceCount => _references.Count;

        public IEnumerable<KeyValuePair<string, SystemSymbol>> SystemSymbols => _systemSymbols;
        private readonly Dictionary<string, SystemSymbol> _systemSymbols = new Dictionary<string, SystemSymbol>();
        public int SystemSymbolCount => _systemSymbols.Count;

        private readonly Dictionary<TextRange, BaseSymbol> _rangeToSymbolMap = new Dictionary<TextRange, BaseSymbol>();

        public bool HasSource(string name)
        {
            bool result = _sources.ContainsKey(name);
            return (result);
        }

        public bool HasSystemSymbol(string name)
        {
            bool result = _systemSymbols.ContainsKey(name);
            return (result);
        }

        public SourceSymbol GetSource(string name)
        {
            SourceSymbol result = null;
            if (_sources.ContainsKey(name))
            {
                List<SourceSymbol> list = _sources[name];
                if (list.Count > 0)
                {
                    foreach (SourceSymbol item in list)
                    {
                        if (result == null || item.Lang < result.Lang)
                            result = list[0];
                    }
                }
            }
            return (result);
        }

        public IEnumerable<ReferenceSymbol> GetReferences(string name)
        {
            if (_references.ContainsKey(name))
            {
                List<ReferenceSymbol> list = _references[name];
                return (list);
            }
            return (new ReferenceSymbol[0]);
        }

        public IEnumerable<SourceSymbol> GetSources(string name)
        {
            if (_sources.ContainsKey(name))
            {
                List<SourceSymbol> list = _sources[name];
                return (list);
            }
            return (null);
        }

        public SystemSymbol GetSystemSymbol(string name)
        {
            if (_systemSymbols.TryGetValue(name, out SystemSymbol symbol))
                return (symbol);
            return (null);
        }

        public void AddSymbol(SourceSymbol source)
        {
            List<SourceSymbol> list;
            if (!_sources.TryGetValue(source.Name, out list))
            {
                list = new List<SourceSymbol>();
                _sources.Add(source.Name, list);
            }
            list.Add(source);
            _rangeToSymbolMap[source.Range] = source;
        }

        public void AddSymbol(ReferenceSymbol reference)
        {
            List<ReferenceSymbol> list;
            if (!_references.TryGetValue(reference.Name, out list))
            {
                list = new List<ReferenceSymbol>();
                _references.Add(reference.Name, list);
            }
            list.Add(reference);
            _rangeToSymbolMap[reference.Range] = reference;
        }

        public void AddSymbol(SystemSymbol systemSym)
        {
            _systemSymbols[systemSym.Name] = systemSym;
        }

        public void AddSymbol(SystemSymbolDescription symbolDesc)
        {
            AddSymbol(new SystemSymbol(symbolDesc.Language, symbolDesc.Kind, symbolDesc.Name));
        }

        public void AddSymbols(IEnumerable<SystemSymbolDescription> descriptions)
        {
            foreach (SystemSymbolDescription symbol in descriptions) 
                AddSymbol(symbol);
        }

        public void AddTable(SymbolTable table)
        {
            foreach (KeyValuePair<string, List<SourceSymbol>> sourcePair in table.SourceMap)
            {
                foreach (SourceSymbol source in sourcePair.Value)
                    AddSymbol(source);
            }
            foreach (KeyValuePair<string, List<ReferenceSymbol>> referencePair in table.ReferenceMap)
            {
                foreach (ReferenceSymbol reference in referencePair.Value)
                    AddSymbol(reference);
            }
            foreach (KeyValuePair<string, SystemSymbol> systemPair in table.SystemSymbols)
                AddSymbol(systemPair.Value);
        }

        public void Clear()
        {
            _sources.Clear();
            _references.Clear();
            _systemSymbols.Clear();
            _rangeToSymbolMap.Clear();
        }

        public BaseSymbol FindSymbolFromRange(TextRange range)
        {
            if (_rangeToSymbolMap.ContainsKey(range))
                return (_rangeToSymbolMap[range]);
            return (null);
        }
    }
}