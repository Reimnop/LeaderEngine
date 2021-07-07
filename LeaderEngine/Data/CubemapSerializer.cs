using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace LeaderEngine
{
    public class CubemapSerializer : GameAssetSerializer
    {
        public override bool CanSerialize(GameAssetType assetType)
        {
            return assetType == GameAssetType.Cubemap;
        }

        public override void WriteToStream(BinaryWriter writer, GameAsset asset)
        {
            Cubemap cubemap = (Cubemap)asset;
            int handle = cubemap.Handle;

            int width = cubemap.Size.X;
            int height = cubemap.Size.Y;

            PixelInternalFormat internalFormat = cubemap.PixelInternalFormat;
            PixelFormat format = cubemap.PixelFormat;
            PixelType pixelType = cubemap.PixelType;

            int singlePixelSize = TextureHelper.GetSinglePixelSize(internalFormat, pixelType);
            byte[] right = new byte[width * height * singlePixelSize];
            byte[] left = new byte[width * height * singlePixelSize];
            byte[] top = new byte[width * height * singlePixelSize];
            byte[] bottom = new byte[width * height * singlePixelSize];
            byte[] back = new byte[width * height * singlePixelSize];
            byte[] front = new byte[width * height * singlePixelSize];

            GL.BindTexture(TextureTarget.TextureCubeMap, handle);
            GL.GetTexImage(TextureTarget.TextureCubeMapPositiveX, 0, format, pixelType, right);
            GL.GetTexImage(TextureTarget.TextureCubeMapNegativeX, 0, format, pixelType, left);
            GL.GetTexImage(TextureTarget.TextureCubeMapPositiveY, 0, format, pixelType, top);
            GL.GetTexImage(TextureTarget.TextureCubeMapNegativeY, 0, format, pixelType, bottom);
            GL.GetTexImage(TextureTarget.TextureCubeMapPositiveZ, 0, format, pixelType, back);
            GL.GetTexImage(TextureTarget.TextureCubeMapNegativeZ, 0, format, pixelType, front);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            //write data
            writer.Write(cubemap.Name);

            writer.Write(width);
            writer.Write(height);
            writer.Write((int)internalFormat);
            writer.Write((int)format);
            writer.Write((int)pixelType);

            WriteBuffer(right, writer);
            WriteBuffer(left, writer);
            WriteBuffer(top, writer);
            WriteBuffer(bottom, writer);
            WriteBuffer(back, writer);
            WriteBuffer(front, writer);
        }

        public override GameAsset ReadFromStream(BinaryReader reader)
        {
            string name = reader.ReadString();

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            PixelInternalFormat internalFormat = (PixelInternalFormat)reader.ReadInt32();
            PixelFormat format = (PixelFormat)reader.ReadInt32();
            PixelType pixelType = (PixelType)reader.ReadInt32();

            byte[] right = ReadBuffer(reader);
            byte[] left = ReadBuffer(reader);
            byte[] top = ReadBuffer(reader);
            byte[] bottom = ReadBuffer(reader);
            byte[] back = ReadBuffer(reader);
            byte[] front = ReadBuffer(reader);

            Cubemap cubemap = Cubemap.FromArrays(
                name,
                width, height,
                right, left, top, bottom, back, front,
                internalFormat, format, pixelType);

            return cubemap;
        }

        private void WriteBuffer(byte[] buffer, BinaryWriter writer)
        {
            writer.Write(buffer.Length);
            writer.Write(buffer);
        }

        private byte[] ReadBuffer(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }
    }
}
