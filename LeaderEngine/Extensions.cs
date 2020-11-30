using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public static class Extensions
    {
        public static void SetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
