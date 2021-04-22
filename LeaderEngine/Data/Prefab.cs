using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class PrefabEntity
    {
        public string Name;

        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        public Vector3 OriginOffset = Vector3.Zero;

        public Mesh Mesh;
        public Material Material;

        public List<PrefabEntity> Children { get; } = new List<PrefabEntity>();

        public PrefabEntity(string name)
        {
            Name = name;
        }
    }

    public class Prefab : IDisposable
    {
        public readonly string Name;
        public readonly string ID;

        public readonly PrefabEntity RootPrefabEntity;

        public Prefab(string name, PrefabEntity rootEntity, string id = null)
        {
            Name = name;
            RootPrefabEntity = rootEntity;

            ID = id != null ? id : DataManager.GetUniqueID(x => DataManager.Prefabs.ContainsKey(x));

            DataManager.Prefabs.Add(ID, this);
        }

        public Entity Instantiate(Entity parent = null)
        {
            return RecursivelySpawnEntities(RootPrefabEntity, parent);
        }

        private Entity RecursivelySpawnEntities(PrefabEntity prefabEntity, Entity parent)
        {
            Entity entity = new Entity(prefabEntity.Name, parent: parent);

            entity.Transform.Position = prefabEntity.Position;
            entity.Transform.Rotation = prefabEntity.Rotation;
            entity.Transform.Scale = prefabEntity.Scale;
            entity.Transform.OriginOffset = prefabEntity.OriginOffset;

            if (prefabEntity.Material != null && prefabEntity.Mesh != null)
            {
                var renderer = entity.AddComponent<MeshRenderer>();

                renderer.Mesh = prefabEntity.Mesh;
                renderer.Material = prefabEntity.Material;
            }

            foreach (var child in prefabEntity.Children)
                RecursivelySpawnEntities(child, entity);

            return entity;
        }

        public void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);

            //write entites
            RecursivelySerialize(RootPrefabEntity, writer);
        }

        private void RecursivelySerialize(PrefabEntity entity, BinaryWriter writer)
        {
            //write name
            writer.Write(entity.Name);

            //write transform
            writer.Write(entity.Position.X); 
            writer.Write(entity.Position.Y); 
            writer.Write(entity.Position.Z); //position

            writer.Write(entity.Rotation.X); 
            writer.Write(entity.Rotation.Y); 
            writer.Write(entity.Rotation.Z); 
            writer.Write(entity.Rotation.W); //rotation

            writer.Write(entity.Scale.X);
            writer.Write(entity.Scale.Y);
            writer.Write(entity.Scale.Z); //scale

            writer.Write(entity.OriginOffset.X);
            writer.Write(entity.OriginOffset.Y);
            writer.Write(entity.OriginOffset.Z); //origin

            //write mesh & material
            writer.Write(entity.Mesh != null);
            if (entity.Mesh != null)
                writer.Write(entity.Mesh.ID);

            writer.Write(entity.Material != null);
            if (entity.Material != null)
                writer.Write(entity.Material.ID);

            //write children
            writer.Write(entity.Children.Count); //children count

            foreach (var child in entity.Children)
                RecursivelySerialize(child, writer);
        }

        public static Prefab Deserialize(BinaryReader reader, string id = null)
        {
            //read name
            string name = reader.ReadString();

            //read entities
            PrefabEntity rootEntity = RecursivelyDeserialize(reader);

            return new Prefab(name, rootEntity, id);
        }

        private static PrefabEntity RecursivelyDeserialize(BinaryReader reader)
        {
            //read name and init
            PrefabEntity prefabEntity = new PrefabEntity(reader.ReadString());

            //read transform
            prefabEntity.Position.X = reader.ReadSingle();
            prefabEntity.Position.Y = reader.ReadSingle();
            prefabEntity.Position.Z = reader.ReadSingle(); //position

            prefabEntity.Rotation.X = reader.ReadSingle();
            prefabEntity.Rotation.Y = reader.ReadSingle();
            prefabEntity.Rotation.Z = reader.ReadSingle();
            prefabEntity.Rotation.W = reader.ReadSingle(); //rotation

            prefabEntity.Scale.X = reader.ReadSingle();
            prefabEntity.Scale.Y = reader.ReadSingle();
            prefabEntity.Scale.Z = reader.ReadSingle(); //scale

            prefabEntity.OriginOffset.X = reader.ReadSingle();
            prefabEntity.OriginOffset.Y = reader.ReadSingle();
            prefabEntity.OriginOffset.Z = reader.ReadSingle(); //origin

            //read mesh & material
            if (reader.ReadBoolean())
                prefabEntity.Mesh = DataManager.Meshes[reader.ReadString()]; //mesh

            if (reader.ReadBoolean())
                prefabEntity.Material = DataManager.Materials[reader.ReadString()]; //material

            //read children
            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
                prefabEntity.Children.Add(RecursivelyDeserialize(reader));

            return prefabEntity;
        }

        public void Dispose()
        {
            DataManager.Prefabs.Remove(ID);
        }
    }
}
