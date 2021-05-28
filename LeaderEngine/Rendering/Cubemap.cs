using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public class Cubemap : IDisposable
    {
        public readonly string Name;

        private int handle;

        private Cubemap(string name, string id = null)
        {
            Name = name;

            //TODO: implement id
        }

        public static Cubemap FromPointers(
            string name,
            int width, int height,
            IntPtr right, IntPtr left, IntPtr top, IntPtr bottom, IntPtr back, IntPtr front, 
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte,
            string id = null)
        {
            Cubemap cubemap = new Cubemap(name, id);
            cubemap.handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap.handle);

            //upload
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, internalFormat, width, height, 0, format, pixelType, right);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, internalFormat, width, height, 0, format, pixelType, left);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, internalFormat, width, height, 0, format, pixelType, top);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, internalFormat, width, height, 0, format, pixelType, bottom);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, internalFormat, width, height, 0, format, pixelType, back);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, internalFormat, width, height, 0, format, pixelType, front);

            //params
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, cubemap.handle, name.Length, name);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return cubemap;
        }

        public unsafe static Cubemap FromFile(
            string name, 
            string rightPath, string leftPath, string topPath, string bottomPath, string backPath, string frontPath,
            string id = null)
        {
            var right = Helper.LoadImageFromFile(rightPath, out int width, out int height);
            var left = Helper.LoadImageFromFile(leftPath, out _, out _);
            var top = Helper.LoadImageFromFile(topPath, out _, out _);
            var bottom = Helper.LoadImageFromFile(bottomPath, out _, out _);
            var back = Helper.LoadImageFromFile(backPath, out _, out _);
            var front = Helper.LoadImageFromFile(frontPath, out _, out _);

            return FromPointers(
                name,
                width, height,
                (IntPtr)Unsafe.AsPointer(ref right[0]),
                (IntPtr)Unsafe.AsPointer(ref left[0]),
                (IntPtr)Unsafe.AsPointer(ref top[0]),
                (IntPtr)Unsafe.AsPointer(ref bottom[0]),
                (IntPtr)Unsafe.AsPointer(ref back[0]),
                (IntPtr)Unsafe.AsPointer(ref front[0]),
                id: id);
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
