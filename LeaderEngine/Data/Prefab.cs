using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class PrefabEntity
    {
        public string Name;
        public List<PrefabEntity> Children { get; } = new List<PrefabEntity>();

        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        public Vector3 OriginOffset = Vector3.Zero;

        public Mesh Mesh;
        public Material Material;

        public PrefabEntity(string name)
        {
            Name = name;
        }
    }

    public class Prefab : IDisposable
    {
        public readonly string Name;
        public readonly PrefabEntity RootPrefabEntity;

        public Prefab(string name, PrefabEntity rootEntity)
        {
            Name = name;
            RootPrefabEntity = rootEntity;

            DataManager.Prefabs.Add(this);
        }

        public Entity Instantiate(Entity parent = null)
        {
            return RecursivelySpawnEntities(RootPrefabEntity, parent);
        }

        private Entity RecursivelySpawnEntities(PrefabEntity prefabEntity, Entity parent)
        {
            Entity entity = new Entity(prefabEntity.Name, parent);

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

        public void Dispose()
        {
            DataManager.Prefabs.Remove(this);
        }
    }
}
