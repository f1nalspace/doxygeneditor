using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Styles
{
    class StyleEntries : IEnumerable<StyleEntry>
    {
        private readonly List<StyleEntry> _items = new List<StyleEntry>(4096);

        public IEnumerable<StyleEntry> Items => _items;

        public int Count => _items.Count;

        public void Clear() => _items.Clear();

#if DEBUG
        public void Add(LanguageKind lang, int index, int length, int style, string value = null)
            => _items.Add(new StyleEntry(lang, index, length, style, value));
#else
        public void Add(LanguageKind lang, int index, int length, int style)
            => _items.Add(new StyleEntry(lang, index, length, style));
#endif

#if DEBUG
        public void Add(LanguageKind lang, IBaseToken token, int style, string value = null)
            => _items.Add(new StyleEntry(lang, token, style, value));
#else
        public void Add(LanguageKind lang, IBaseToken token, int style)
            => _items.Add(new StyleEntry(lang, token, style));
#endif

        public StyleEntry Find(int position) => _items.FirstOrDefault((e) => position >= e.Index && position <= e.End);

        public IEnumerator<StyleEntry> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
