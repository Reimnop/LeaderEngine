using OpenTK.Graphics.OpenGL4;
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

        public static void CheckForErrors()
        {
            ErrorCode error;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Logger.LogError("ERROR: " + error.ToString());
            }
        }
    }
}
