using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        public static byte[] LoadImageFromFile(string path, out int width, out int height)
        {
            using (Bitmap bmp = new Bitmap(path))
            {
                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                width = bmp.Width;
                height = bmp.Height;

                byte[] pixArr = new byte[width * height * 4]; // Each pixel takes 4 bytes
                Marshal.Copy(data.Scan0, pixArr, 0, width * height * 4);

                bmp.UnlockBits(data);

                return pixArr;
            }
        }

        public static bool EnsureEqual<T>(T[] values, out T value)
        {
            value = default;

            for (int i = 1; i < values.Length; i++) 
            {
                if (!values[i].Equals(values[i - 1]))
                {
                    return false;
                }
            }

            value = values[0];
            return true;
        }

        public static int CreateShaderProgram(string vertPath, string fragPath)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertPath));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragPath));
            GL.CompileShader(fragmentShader);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.DetachShader(shaderProgram, vertexShader);
            GL.DetachShader(shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }
    }
}
