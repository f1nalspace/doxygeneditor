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

        private readonly Dictionary<string, List<SourceSymbol>> _sources = new Dictionary<string, List<SourceSymbol>>();
        public IEnumerable<KeyValuePair<string, List<SourceSymbol>>> SourceMap => _sources;
        public int SourceCount => _sources.Count;

        private readonly Dictionary<string, List<ReferenceSymbol>> _references = new Dictionary<string, List<ReferenceSymbol>>();
        public IEnumerable<KeyValuePair<string, List<ReferenceSymbol>>> ReferenceMap => _references;
        public int ReferenceCount => _references.Count;

        private readonly Dictionary<TextRange, BaseSymbol> _rangeToSymbolMap = new Dictionary<TextRange, BaseSymbol>();

        public bool HasSource(string name)
        {
            bool result = _sources.ContainsKey(name);
            return (result);
        }

        public SourceSymbol GetSource(string name)
        {
            if (_sources.ContainsKey(name))
            {
                List<SourceSymbol> list = _sources[name];
                if (list.Count > 0)
                {
                    SourceSymbol result = list[0];
                    return (result);
                }
            }
            return (null);
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

        public void AddSource(SourceSymbol source)
        {
            List<SourceSymbol> list;
            if (_sources.ContainsKey(source.Name))
                list = _sources[source.Name];
            else
            {
                list = new List<SourceSymbol>();
                _sources.Add(source.Name, list);
            }
            list.Add(source);
            _rangeToSymbolMap[source.Range] = source;
        }

        public void AddReference(ReferenceSymbol reference)
        {
            List<ReferenceSymbol> list;
            if (_references.ContainsKey(reference.Name))
                list = _references[reference.Name];
            else
            {
                list = new List<ReferenceSymbol>();
                _references.Add(reference.Name, list);
            }
            list.Add(reference);
            _rangeToSymbolMap[reference.Range] = reference;
        }

        public void AddTable(SymbolTable table)
        {
            foreach (var sourcePair in table.SourceMap)
            {
                foreach (var source in sourcePair.Value)
                    AddSource(source);
            }
            foreach (var referencePair in table.ReferenceMap)
            {
                foreach (var reference in referencePair.Value)
                    AddReference(reference);
            }
        }

        public void Clear()
        {
            _sources.Clear();
            _references.Clear();
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