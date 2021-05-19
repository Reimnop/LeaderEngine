using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public static class Helper
    {
        public static void SetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryAdd(key, value))
                dictionary[key] = value;
        }

        public static bool NearlyEquals(this float a, float b)
        {
            const float epsilon = 1e-8f;

            if (MathF.Abs(a - b) < epsilon)
                return true;

            return false;
        }

        public static byte[] StructArrayToByteArray<T>(T[] data) where T : struct
        {
            byte[] output = new byte[data.Length * Unsafe.SizeOf<T>()];

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), output, 0, output.Length);
            handle.Free();

            return output;
        }

        public static T[] ByteArrayToStructArray<T>(byte[] data) where T : struct
        {
            int itemSize = Unsafe.SizeOf<T>();

            T[] output = new T[data.Length / itemSize];

            GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
            Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
            handle.Free();

            return output;
        }
    }
}
