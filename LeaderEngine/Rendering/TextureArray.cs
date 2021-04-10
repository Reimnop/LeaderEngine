using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public sealed class TextureArray : IDisposable
    {
        public readonly string Name;

        public int handle;

        private TextureArray(string name)
        {
            Name = name;
        }

        public static TextureArray FromPointer(string name, int[] width, int[] height, IntPtr[] data, PixelInternalFormat internalFormat = PixelInternalFormat.SrgbAlpha, PixelFormat format = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
        {
            TextureArray textureArray = new TextureArray(name);

            textureArray.handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, textureArray.handle);

            for (int i = 0; i < data.Length; i++)
            {
                GL.TexImage3D(TextureTarget.Texture2DArray, i, internalFormat, width[i], height[i], data.Length, 0, format, pixelType, data[i]);
            }

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, textureArray.handle, name.Length, name);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return textureArray;
        }

        public void Use(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2DArray, handle);
        }

        public int GetHandle()
        {
            return handle;
        }

        public void Dispose()
        {
            GL.DeleteTexture(handle);
        }
    }
}
