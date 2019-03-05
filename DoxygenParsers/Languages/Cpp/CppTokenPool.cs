using System.Collections.Generic;
using TSP.DoxygenEditor.Pools;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public static class CppTokenPool
    {
        private static ObjectPool<CppToken> _pool = null;
        public static CppToken Make(CppTokenKind kind, TextRange range, bool isComplete)
        {
            if (_pool == null)
                _pool = new ObjectPool<CppToken>(() => new CppToken());
            CppToken result = _pool.Aquire();
            result.Set(kind, range, isComplete);
            return (result);
        }
        public static void Release(IEnumerable<CppToken> tokens)
        {
            _pool?.Release(tokens);
        }
    }
}
