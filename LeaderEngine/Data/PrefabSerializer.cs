using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class PrefabSerializer : GameAssetSerializer
    {
        public override bool CanSerialize(GameAssetType assetType)
        {
            return assetType == GameAssetType.Prefab;
        }

        public override void WriteToStream(BinaryWriter writer, GameAsset asset)
        {
            Prefab prefab = (Prefab)asset;

            writer.Write(prefab.Name);
            RecursivelyWritePrefabEntity(writer, prefab.RootEntity);
        }

        public override GameAsset ReadFromStream(BinaryReader reader)
        {
            string name = reader.ReadString();

            return new Prefab(name, RecursivelyReadPrefabEntity(reader));
        }

        private PrefabEntity RecursivelyReadPrefabEntity(BinaryReader reader)
        {
            string name = reader.ReadString();

            PrefabEntity entity = new PrefabEntity(name);

            entity.Position = ReadStruct<Vector3>(reader);
            entity.Scale = ReadStruct<Vector3>(reader);
            entity.Rotation = ReadStruct<Quaternion>(reader);
            entity.OriginOffset = ReadStruct<Vector3>(reader);

            string meshID = reader.ReadString();
            entity.Mesh = !string.IsNullOrEmpty(meshID) ? (Mesh)AssetManager.Assets[meshID] : null;

            string materialID = reader.ReadString();
            entity.Material = !string.IsNullOrEmpty(materialID) ? (Material)AssetManager.Assets[materialID] : null;

            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
                entity.Children.Add(RecursivelyReadPrefabEntity(reader));

            return entity;
        }

        private void RecursivelyWritePrefabEntity(BinaryWriter writer, PrefabEntity entity)
        {
            writer.Write(entity.Name);

            WriteStruct(writer, entity.Position);
            WriteStruct(writer, entity.Scale);
            WriteStruct(writer, entity.Rotation);
            WriteStruct(writer, entity.OriginOffset);

            writer.Write(entity.Mesh != null ? entity.Mesh.ID : string.Empty);
            writer.Write(entity.Material != null ? entity.Material.ID : string.Empty);

            writer.Write(entity.Children.Count);
            foreach (PrefabEntity child in entity.Children)
                RecursivelyWritePrefabEntity(writer, child);
        }

        private T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] bytes = reader.ReadBytes(size);
            Marshal.Copy(bytes, 0, ptr, size);
            T value = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return value;
        }

        private void WriteStruct<T>(BinaryWriter writer, T value) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            writer.Write(bytes);
        }
    }
}
