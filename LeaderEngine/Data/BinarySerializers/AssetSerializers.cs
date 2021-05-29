using System;
using System.IO;

namespace LeaderEngine
{
    public class MeshSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(Mesh);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write(((Mesh)value).ID);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return GlobalData.Meshes[reader.ReadString()];
        }
    }

    public class MaterialSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(Material);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write(((Material)value).ID);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return GlobalData.Materials[reader.ReadString()];
        }
    }

    public class TextureSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(Texture);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write(((Texture)value).ID);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return GlobalData.Textures[reader.ReadString()];
        }
    }
}
