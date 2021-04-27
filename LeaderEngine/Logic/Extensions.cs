using System.Collections.Generic;

namespace LeaderEngine
{
    public static class Extensions
    {
        public static void SetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryAdd(key, value))
                dictionary[key] = value;
        }
    }
}
