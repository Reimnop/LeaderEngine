using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace LeaderEngine
{
    public class TextureSerializer : GameAssetSerializer
    {
        public override bool CanSerialize(GameAssetType assetType)
        {
            return assetType == GameAssetType.Texture;
        }

        public override void WriteToStream(BinaryWriter writer, GameAsset asset)
        {
            Texture texture = (Texture)asset;

            int sizeInBytes = TextureHelper.GetSinglePixelSize(texture.PixelInternalFormat, texture.PixelType) * texture.Size.X * texture.Size.Y;

            GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

            byte[] textureData = new byte[sizeInBytes];
            GL.GetTexImage(TextureTarget.Texture2D, 0, texture.PixelFormat, texture.PixelType, textureData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //write to stream
            writer.Write(texture.Name);

            writer.Write((int)texture.MinFilter);
            writer.Write((int)texture.MagFilter);
            writer.Write((int)texture.WrapModeS);
            writer.Write((int)texture.WrapModeT);

            writer.Write((int)texture.PixelInternalFormat);
            writer.Write((int)texture.PixelFormat);
            writer.Write((int)texture.PixelType);

            writer.Write(texture.Size.X);
            writer.Write(texture.Size.Y);

            writer.Write(texture.IsResident);

            //buffer
            writer.Write(sizeInBytes);
            writer.Write(textureData);
        }

        public override GameAsset ReadFromStream(BinaryReader reader)
        {
            string name = reader.ReadString();

            TextureMinFilter minFilter = (TextureMinFilter)reader.ReadInt32();
            TextureMagFilter magFilter = (TextureMagFilter)reader.ReadInt32();
            TextureWrapMode wrapModeS = (TextureWrapMode)reader.ReadInt32();
            TextureWrapMode wrapModeT = (TextureWrapMode)reader.ReadInt32();

            PixelInternalFormat internalFormat = (PixelInternalFormat)reader.ReadInt32();
            PixelFormat pixelFormat = (PixelFormat)reader.ReadInt32();
            PixelType pixelType = (PixelType)reader.ReadInt32();

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            bool isResident = reader.ReadBoolean();

            int bufferSize = reader.ReadInt32();
            byte[] buffer = reader.ReadBytes(bufferSize);

            Texture texture = Texture.FromArray(name, width, height, buffer, internalFormat, pixelFormat, pixelType);
            texture.SetMinFilter(minFilter);
            texture.SetMagFilter(magFilter);
            texture.SetWrapModeS(wrapModeS);
            texture.SetWrapModeT(wrapModeT);

            if (isResident)
            {
                texture.MakeImmutable();
                texture.MakeResident();
            }

            return texture;
        }
    }
}
