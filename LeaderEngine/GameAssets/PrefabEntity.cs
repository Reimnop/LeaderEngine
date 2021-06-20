using System.Collections.Generic;
using OpenTK.Mathematics;

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
}
