using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeaderEngine
{
    public static class GlobalData
    {
        internal static Dictionary<string, Type> ComponentTypes { get; } = new Dictionary<string, Type>();
        internal static List<Entity> UnlistedEntities { get; } = new List<Entity>();

        public static Dictionary<string, Material> Materials { get; } = new Dictionary<string, Material>();
        public static Dictionary<string, Prefab> Prefabs { get; } = new Dictionary<string, Prefab>();
        public static Dictionary<string, Mesh> Meshes { get; } = new Dictionary<string, Mesh>();
        public static Dictionary<string, Texture> Textures { get; } = new Dictionary<string, Texture>();
        public static Dictionary<string, AudioClip> AudioClips { get; } = new Dictionary<string, AudioClip>();
        public static Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

        internal static void Init()
        {
            var kvps =
                AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t))
                .Select(x => KeyValuePair.Create(x.AssemblyQualifiedName, x));

            foreach (var kvp in kvps)
                ComponentTypes.Add(kvp.Key, kvp.Value);

            new Font("Inconsolata", Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Inconsolata.fnt"));
            new Font("Impact", Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Impact.fnt"));
        }

        public static void SaveAssets(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(fileStream);

                //serialize meshes
                writer.Write(Meshes.Count);
                foreach (var mesh in Meshes)
                    mesh.Value.Serialize(writer);

                //serialize textures
                writer.Write(Textures.Count);
                foreach (var tex in Textures)
                    tex.Value.Serialize(writer);

                //serialize materials
                writer.Write(Materials.Count);
                foreach (var mat in Materials)
                    mat.Value.Serialize(writer);

                //serialize prefabs
                writer.Write(Prefabs.Count);
                foreach (var pref in Prefabs)
                    pref.Value.Serialize(writer);
            }
        }

        public static void LoadAssets(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fileStream);

                //deserialize meshes
                int meshCount = reader.ReadInt32();
                for (int i = 0; i < meshCount; i++)
                    Mesh.Deserialize(reader);

                //deserialize textures
                int texCount = reader.ReadInt32();
                for (int i = 0; i < texCount; i++)
                    Texture.Deserialize(reader);

                //deserialize materials
                int matCount = reader.ReadInt32();
                for (int i = 0; i < matCount; i++)
                    Material.Deserialize(reader);

                //deserialize prefabs
                int prefCount = reader.ReadInt32();
                for (int i = 0; i < prefCount; i++)
                    Prefab.Deserialize(reader);
            }
        }
    }
}
