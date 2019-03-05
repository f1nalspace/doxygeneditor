using System.Collections.Generic;
using TSP.DoxygenEditor.Pools;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Html
{
    public static class HtmlTokenPool
    {
        private static ObjectPool<HtmlToken> _pool = null;
        public static HtmlToken Make(HtmlTokenKind kind, TextRange range, bool isComplete)
        {
            if (_pool == null)
                _pool = new ObjectPool<HtmlToken>(() => new HtmlToken());
            HtmlToken result = _pool.Aquire();
            result.Set(kind, range, isComplete);
            return (result);
        }
        public static void Release(IEnumerable<HtmlToken> tokens)
        {
            _pool?.Release(tokens);
        }
    }
}
