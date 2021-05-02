using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public sealed class Texture : IDisposable
    {
        private static class TextureHelper
        {
            private readonly static Dictionary<PixelInternalFormat, int> internalFormatSize = new Dictionary<PixelInternalFormat, int>()
            {
                { (PixelInternalFormat)All.Red, 1 },
                { PixelInternalFormat.Rgba, 4 }
            };

            private readonly static Dictionary<PixelType, int> typeSize = new Dictionary<PixelType, int>()
            {
                { PixelType.UnsignedByte, 1 },
                { PixelType.Float, sizeof(float) }
            };

            public static int GetSinglePixelSize(PixelInternalFormat pixelInternalFormat, PixelType pixelType)
            {
                int iSize, tSize;

                if (!internalFormatSize.TryGetValue(pixelInternalFormat, out iSize))
                    throw new NotImplementedException();

                if (!typeSize.TryGetValue(pixelType, out tSize))
                    throw new NotImplementedException();

                return iSize * tSize;
            }
        }

        public readonly string Name;
        public readonly string ID;

        private int handle;

        public Vector2i Size;

        private PixelInternalFormat pixelInternalFormat;
        private PixelFormat pixelFormat;
        private PixelType pixelType;

        private TextureMinFilter minFilter = TextureMinFilter.Linear;
        private TextureMagFilter magFilter = TextureMagFilter.Linear;
        private TextureWrapMode wrapS = TextureWrapMode.ClampToEdge;
        private TextureWrapMode wrapT = TextureWrapMode.ClampToEdge;

        private Texture(string name, string id = null)
        {
            Name = name;

            ID = id != null ? id : RNG.GetRandomID();

            DataManager.Textures.Add(ID, this);
        }

        internal void Unlist()
        {
            DataManager.Textures.Remove(ID);
        }

        public static Texture FromFile(string name, string path, string id = null)
        {
            using (var image = Image.Load<Rgba32>(path))
                return FromImage(name, image, id);
        }

        public unsafe static Texture FromImage(string name, Image<Rgba32> image, string id = null)
        {
            Span<Rgba32> pixelSpan;
            if (!image.TryGetSinglePixelSpan(out pixelSpan))
            {
                Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
                for (int i = 0; i < image.Height; i++)
                {
                    var row = image.GetPixelRowSpan(i);
                    for (int j = 0; j < image.Width; j++)
                    {
                        pixelArray[i * image.Height + j] = row[j];
                    }
                }
                pixelSpan = new Span<Rgba32>(pixelArray);
            }

            return FromPointer(name, image.Width, image.Height, (IntPtr)Unsafe.AsPointer(ref pixelSpan[0]), id: id);
        }

        public static Texture FromPointer(string name, int width, int height, IntPtr data,
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte,
            string id = null)
        {
            Texture texture = new Texture(name, id);

            //copy pixel array
            texture.pixelInternalFormat = internalFormat;
            texture.pixelFormat = format;
            texture.pixelType = pixelType;

            texture.handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture.handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, texture.handle, name.Length, name);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            texture.Size = new Vector2i(width, height);

            return texture;
        }

        public static Texture FromArray<T>(string name, int width, int height, T[] data,
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte,
            string id = null) where T : struct
        {
            Texture texture = new Texture(name, id);

            //copy pixel array
            texture.pixelInternalFormat = internalFormat;
            texture.pixelFormat = format;
            texture.pixelType = pixelType;

            texture.handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture.handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, texture.handle, name.Length, name);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            texture.Size = new Vector2i(width, height);

            return texture;
        }

        public void SetMinFilter(TextureMinFilter textureMinFilter)
        {
            minFilter = textureMinFilter;

            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMinFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetMagFilter(TextureMagFilter textureMagFilter)
        {
            magFilter = textureMagFilter;

            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMagFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapS(TextureWrapMode textureWrapMode)
        {
            wrapS = textureWrapMode;

            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapT(TextureWrapMode textureWrapMode)
        {
            wrapT = textureWrapMode;

            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);
            //write id
            writer.Write(ID);

            //write texture info
            writer.Write((int)minFilter);
            writer.Write((int)magFilter);
            writer.Write((int)wrapS);
            writer.Write((int)wrapT);

            //write pixel info
            writer.Write((int)pixelInternalFormat);
            writer.Write((int)pixelFormat);
            writer.Write((int)pixelType);

            //write width and height
            writer.Write(Size.X);
            writer.Write(Size.Y);

            //write pixels
            byte[] rawData = new byte[Size.X * Size.Y * TextureHelper.GetSinglePixelSize(pixelInternalFormat, pixelType)];

            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.GetTexImage(TextureTarget.Texture2D, 0, pixelFormat, pixelType, rawData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            writer.Write(rawData);
        }

        public static Texture Deserialize(BinaryReader reader)
        {
            //read name
            string name = reader.ReadString();
            //read id
            string id = reader.ReadString();

            //read texture info
            TextureMinFilter minFilter = (TextureMinFilter)reader.ReadInt32();
            TextureMagFilter magFilter = (TextureMagFilter)reader.ReadInt32();
            TextureWrapMode wrapS = (TextureWrapMode)reader.ReadInt32();
            TextureWrapMode wrapT = (TextureWrapMode)reader.ReadInt32();

            //read pixel info
            PixelInternalFormat pixelInternalFormat = (PixelInternalFormat)reader.ReadInt32();
            PixelFormat pixelFormat = (PixelFormat)reader.ReadInt32();
            PixelType pixelType = (PixelType)reader.ReadInt32();

            //read width and height
            Vector2i size = new Vector2i(reader.ReadInt32(), reader.ReadInt32());

            //read pixels
            byte[] data = reader.ReadBytes(size.X * size.Y * TextureHelper.GetSinglePixelSize(pixelInternalFormat, pixelType));

            Texture tex = FromArray(name,
                size.X, size.Y,
                data,
                pixelInternalFormat, pixelFormat,
                pixelType, id: id);

            tex.SetMinFilter(minFilter);
            tex.SetMagFilter(magFilter);
            tex.SetWrapS(wrapS);
            tex.SetWrapT(wrapT);

            return tex;
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

            DataManager.Textures.Remove(ID);
        }
    }
}
