using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public sealed class Texture : IDisposable
    {
        public int handle;

        public Vector2 Size;

        public readonly string Name;

        internal Texture(string name)
        {
            Name = name;
        }

        public static Texture FromFile(string name, string path)
        {
            return FromImage(name, (Image<Rgba32>)Image.Load(path));
        }

        public static Texture FromImage(string name, Image<Rgba32> image)
        {
            Span<Rgba32> pixelSpan;
            if (!image.TryGetSinglePixelSpan(out pixelSpan))
                return null;

            GCHandle handle = GCHandle.Alloc(pixelSpan.ToArray(), GCHandleType.Pinned);
            Texture tex = FromPointer(name, image.Width, image.Height, handle.AddrOfPinnedObject());
            handle.Free();

            return tex;
        }

        public static Texture FromPointer(string name, int width, int height, IntPtr data, PixelInternalFormat internalFormat = PixelInternalFormat.SrgbAlpha, PixelFormat format = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
        {
            Texture texture = new Texture(name);

            texture.handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture.handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, texture.handle, name.Length, name);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            texture.Size = new Vector2(width, height);

            return texture;
        }

        public void SetMinFilter(TextureMinFilter textureMinFilter)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMinFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetMagFilter(TextureMagFilter textureMagFilter)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMagFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapS(TextureWrapMode textureWrapMode)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapT(TextureWrapMode textureWrapMode)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Use(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
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
