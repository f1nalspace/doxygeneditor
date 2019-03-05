using System.Collections.Generic;
using TSP.DoxygenEditor.Pools;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public static class DoxygenTokenPool
    {
        private static ObjectPool<DoxygenToken> _pool = null;
        public static DoxygenToken Make(DoxygenTokenKind kind, TextRange range, bool isComplete)
        {
            if (_pool == null)
                _pool = new ObjectPool<DoxygenToken>(() => new DoxygenToken());
            DoxygenToken result = _pool.Aquire();
            result.Set(kind, range, isComplete);
            return (result);
        }
        public static void Release(IEnumerable<DoxygenToken> list)
        {
            _pool?.Release(list);
        }
    }
}
