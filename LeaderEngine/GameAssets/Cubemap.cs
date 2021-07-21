using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class Cubemap : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Cubemap;

        public int Handle => _handle;
        public Vector2i Size => _size;

        public PixelInternalFormat PixelInternalFormat => _pixelInternalFormat;
        public PixelFormat PixelFormat => _pixelFormat;
        public PixelType PixelType => _pixelType;

        private int _handle;
        private Vector2i _size;

        private PixelInternalFormat _pixelInternalFormat;
        private PixelFormat _pixelFormat;
        private PixelType _pixelType;

        private Cubemap(string name) : base(name)
        {
        }

        public static Cubemap FromPointers(
            string name,
            int width, int height,
            IntPtr right, IntPtr left, IntPtr top, IntPtr bottom, IntPtr back, IntPtr front, 
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            Cubemap cubemap = new Cubemap(name);
            cubemap._size = new Vector2i(width, height);
            cubemap._pixelInternalFormat = internalFormat;
            cubemap._pixelFormat = format;
            cubemap._pixelType = pixelType;

            cubemap._handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap._handle);

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

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, cubemap._handle, name.Length, name);

            return cubemap;
        }

        public static Cubemap FromArrays<T>(
            string name,
            int width, int height,
            T[] right, T[] left, T[] top, T[] bottom, T[] back, T[] front,
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
            PixelFormat format = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte) where T : struct
        {
            Cubemap cubemap = new Cubemap(name);
            cubemap._size = new Vector2i(width, height);
            cubemap._pixelInternalFormat = internalFormat;
            cubemap._pixelFormat = format;
            cubemap._pixelType = pixelType;

            cubemap._handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap._handle);

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

            GL.ObjectLabel(ObjectLabelIdentifier.Texture, cubemap._handle, name.Length, name);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return cubemap;
        }

        public static Cubemap FromFile(
            string name, 
            string rightPath, string leftPath, string topPath, string bottomPath, string backPath, string frontPath)
        {
            int[] widths = new int[6];
            int[] heights = new int[6];

            byte[] right = Helper.LoadImageFromFile(rightPath, out widths[0], out heights[0]);
            byte[] left = Helper.LoadImageFromFile(leftPath, out widths[1], out heights[1]);
            byte[] top = Helper.LoadImageFromFile(topPath, out widths[2], out heights[2]);
            byte[] bottom = Helper.LoadImageFromFile(bottomPath, out widths[3], out heights[3]);
            byte[] back = Helper.LoadImageFromFile(backPath, out widths[4], out heights[4]);
            byte[] front = Helper.LoadImageFromFile(frontPath, out widths[5], out heights[5]);

            int width;
            int height;

            if (!Helper.EnsureEqual(widths, out width) || !Helper.EnsureEqual(heights, out height))
            {
                Logger.Log("Error whilst loading Cubemap: Images do not have equal sizes!");
                return null;
            }

            return FromArrays(
                name,
                width, height,
                right, left, top, bottom, back, front,
                PixelInternalFormat.Rgba,
                PixelFormat.Bgra);
        }

        public int GetHandle()
        {
            return _handle;
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteTexture(_handle);
        }
    }
}
