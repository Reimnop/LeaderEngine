using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class Prefab : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Prefab;

        public PrefabEntity RootEntity => _rootEntity;

        private PrefabEntity _rootEntity;

        public Prefab(string name, PrefabEntity rootEntity) : base(name)
        {
            _rootEntity = rootEntity;
        }

        public Entity Instantiate(Entity parent = null)
        {
            return RecursivelySpawnEntities(_rootEntity, parent);
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
    }
}
