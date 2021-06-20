using System;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LeaderEngine
{
    public sealed class Texture : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Texture;
        
        public int Handle => _handle;
        public long LongHandle => _longHandle;
        public Vector2i Size => _size;

        public bool IsResident => _isResident;

        public PixelInternalFormat PixelInternalFormat => _pixelInternalFormat;
        public PixelFormat PixelFormat => _pixelFormat;
        public PixelType PixelType => _pixelType;

        public TextureMinFilter MinFilter => _minFilter;
        public TextureMagFilter MagFilter => _magFilter;
        public TextureWrapMode WrapModeT => _wrapModeT;
        public TextureWrapMode WrapModeS => _wrapModeS;

        private int _handle;
        private long _longHandle;

        private Vector2i _size;

        private bool _isResident = false;

        private PixelInternalFormat _pixelInternalFormat;
        private PixelFormat _pixelFormat;
        private PixelType _pixelType;

        private TextureMinFilter _minFilter = TextureMinFilter.Linear;
        private TextureMagFilter _magFilter = TextureMagFilter.Linear;
        private TextureWrapMode _wrapModeT = TextureWrapMode.ClampToEdge;
        private TextureWrapMode _wrapModeS = TextureWrapMode.ClampToEdge;

        private Texture(string name) : base(name)
        {
        }

        public static Texture FromFile(string name, string path)
        {
            using (var image = Image.Load<Rgba32>(path))
                return FromImage(name, image);
        }

        public unsafe static Texture FromImage(string name, Image<Rgba32> image)
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

            return FromPointer(name, image.Width, image.Height, (IntPtr)Unsafe.AsPointer(ref pixelSpan[0]), PixelInternalFormat.SrgbAlpha);
        }

        public static Texture FromPointer(string name, int width, int height, IntPtr data,
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            Texture texture = new Texture(name);

            //copy pixel array
            texture._pixelInternalFormat = internalFormat;
            texture._pixelFormat = format;
            texture._pixelType = pixelType;

            texture._handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture._handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, texture._handle, name.Length, name);

            texture._size = new Vector2i(width, height);

            return texture;
        }

        public static Texture FromArray<T>(string name, int width, int height, T[] data,
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte) where T : struct
        {
            Texture texture = new Texture(name);

            //copy pixel array
            texture._pixelInternalFormat = internalFormat;
            texture._pixelFormat = format;
            texture._pixelType = pixelType;

            texture._handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture._handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, texture._handle, name.Length, name);

            texture._size = new Vector2i(width, height);

            return texture;
        }

        public void MakeImmutable()
        {
            _longHandle = GL.Arb.GetTextureHandle(_handle);
        }

        public void MakeResident()
        {
            GL.Arb.MakeTextureHandleResident(_longHandle);
            _isResident = true;
        }

        public void MakeNonResident()
        {
            GL.Arb.MakeTextureHandleNonResident(_longHandle);
            _isResident = false;
        }

        public void SetMinFilter(TextureMinFilter textureMinFilter)
        {
            _minFilter = textureMinFilter;

            GL.BindTexture(TextureTarget.Texture2D, _handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMinFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetMagFilter(TextureMagFilter textureMagFilter)
        {
            _magFilter = textureMagFilter;

            GL.BindTexture(TextureTarget.Texture2D, _handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)textureMagFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapModeT(TextureWrapMode textureWrapMode)
        {
            _wrapModeT = textureWrapMode;

            GL.BindTexture(TextureTarget.Texture2D, _handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapModeS(TextureWrapMode textureWrapMode)
        {
            _wrapModeS = textureWrapMode;

            GL.BindTexture(TextureTarget.Texture2D, _handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)textureWrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteTexture(_handle);
        }
    }
}
