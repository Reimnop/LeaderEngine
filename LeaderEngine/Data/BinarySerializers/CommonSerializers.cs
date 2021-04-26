using System;
using System.IO;

namespace LeaderEngine
{
    public class IntSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(int);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write((int)value);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
    }

    public class FloatSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(float);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write((float)value);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return reader.ReadSingle();
        }
    }

    public class StringSerializer : BinarySerializer
    {
        public override bool IsSerializable(Type type)
        {
            return type == typeof(string);
        }

        public override void SerializeObject(BinaryWriter writer, object value)
        {
            writer.Write((string)value);
        }

        public override object DeserializeObject(BinaryReader reader)
        {
            return reader.ReadString();
        }
    }
}
