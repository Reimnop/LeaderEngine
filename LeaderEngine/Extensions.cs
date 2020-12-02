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

        public static System.Numerics.Vector3 ToSystemVector3(this OpenTK.Mathematics.Vector3 vector3)
        {
            return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static OpenTK.Mathematics.Vector3 ToOTKVector3(this System.Numerics.Vector3 vector3)
        {
            return new OpenTK.Mathematics.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}
