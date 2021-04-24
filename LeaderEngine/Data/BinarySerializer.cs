using System;
using System.IO;

namespace LeaderEngine
{
    public abstract class BinarySerializer
    {
        public abstract bool IsSerializable(Type type);
        public abstract void SerializeObject(BinaryWriter writer, object value);
        public abstract object DeserializeObject(BinaryReader reader);
    }
}
