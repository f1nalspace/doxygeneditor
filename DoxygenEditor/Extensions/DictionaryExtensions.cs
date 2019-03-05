using System.Collections.Generic;

namespace TSP.DoxygenEditor.Extensions
{
    static class DictionaryExtensions
    {
        public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey,TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return default(TValue);
        }
    }
}
