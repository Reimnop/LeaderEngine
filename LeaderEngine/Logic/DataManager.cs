using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaternion = OpenTK.Mathematics.Quaternion;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace LeaderEngine
{
    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");

        internal static Dictionary<string, Type> ComponentTypes { get; } = new Dictionary<string, Type>();
        internal static List<Entity> EngineReservedEntities { get; } = new List<Entity>();

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

        public static void SaveGameAssets(string path)
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

        public static void LoadGameAssets(string path)
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

        public static Prefab LoadModelFromFile(string path)
        {
            //import file
            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFile(path,
                PostProcessSteps.Triangulate |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.CalculateTangentSpace);

            var aiMeshes = scene.Meshes;
            var aiMaterials = scene.Materials;

            //load materials
            Material[] materials = new Material[aiMaterials.Count];

            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var aiMaterial = aiMaterials[i];

                materials[i] = new Material(aiMaterial.Name);
                Material mat = materials[i];

                mat.SetVector3("color", new Vector3(aiMaterial.ColorDiffuse.R, aiMaterial.ColorDiffuse.G, aiMaterial.ColorDiffuse.B));

                //reset values
                mat.SetInt("hasDiffuse", 0);

                if (aiMaterial.HasTextureDiffuse)
                {
                    TextureSlot aiTexture = aiMaterial.TextureDiffuse;

                    //load texture
                    string texPath = aiTexture.FilePath;
                    string texName = aiMaterial.Name + "-Diffuse";

                    try
                    {
                        Texture texture;
                        if (!texPath.StartsWith('*'))
                        {
                            if (!Path.IsPathRooted(texPath))
                                texPath = Path.Combine(Path.GetDirectoryName(path), texPath);

                            texture = Texture.FromFile(texName, texPath);
                        }
                        else
                        {
                            int index = int.Parse(texPath.Substring(1));
                            var embedTexture = scene.Textures[index];

                            if (embedTexture.IsCompressed)
                            {
                                texture = Texture.FromImage(texName, Image.Load<Rgba32>(embedTexture.CompressedData));
                            }
                            else
                            {
                                texture = Texture.FromArray(
                                    texName,
                                    embedTexture.Width,
                                    embedTexture.Height,
                                    embedTexture.NonCompressedData);
                            }
                        }

                        texture.SetWrapS(ConvertWrapModeToOTK(aiTexture.WrapModeU));
                        texture.SetWrapT(ConvertWrapModeToOTK(aiTexture.WrapModeV));

                        mat.SetInt("hasDiffuse", 1);

                        mat.SetInt("diffuse", 0);
                        mat.SetTexture2D(TextureUnit.Texture0, texture);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Could not load texture {texName}!");

#if DEBUG
                        Logger.LogError($"Exception: {e}");
#endif
                    }
                }
            }

            //load meshes
            (Mesh mesh, int matIndex)[] meshes = new (Mesh mesh, int matIndex)[scene.Meshes.Count];

            //load each vertex
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                var aiMesh = aiMeshes[i];

                Vertex[] vertices = new Vertex[aiMesh.VertexCount];

                for (int j = 0; j < aiMesh.VertexCount; j++)
                {
                    var aiVert = aiMesh.Vertices[j];
                    var aiNormal = aiMesh.Normals[j];

                    vertices[j] = new Vertex
                    {
                        Position = new Vector3(aiVert.X, aiVert.Y, aiVert.Z),

                        Normal = new Vector3(aiNormal.X, aiNormal.Y, aiNormal.Z),

                        Color = aiMesh.HasVertexColors(0) ? new Vector3(
                            aiMesh.VertexColorChannels[0][j].R,
                            aiMesh.VertexColorChannels[0][j].G,
                            aiMesh.VertexColorChannels[0][j].B) : Vector3.One,

                        UV = aiMesh.HasTextureCoords(0) ? new Vector2(
                            aiMesh.TextureCoordinateChannels[0][j].X,
                            aiMesh.TextureCoordinateChannels[0][j].Y) : Vector2.Zero
                    };
                }

                //create mesh
                Mesh mesh = new Mesh(aiMesh.Name);
                mesh.LoadMesh(vertices, aiMesh.GetUnsignedIndices());

                meshes[i] = (mesh, aiMesh.MaterialIndex);
            }

            //load model
            PrefabEntity rootEntity = RecursivelyLoadAssimpNode(meshes, materials, scene.RootNode);

            return new Prefab(Path.GetFileNameWithoutExtension(path), rootEntity);
        }

        private static PrefabEntity RecursivelyLoadAssimpNode((Mesh Mesh, int MatIndex)[] meshes, Material[] materials, Node node)
        {
            //create Entity
            PrefabEntity entity = new PrefabEntity(node.Name);

            if (!node.HasMeshes)
                goto LoadChildren;

            //set transform
            node.Transform.Decompose(out var scale, out var rotation, out var position);

            entity.Position = new Vector3(position.X, position.Y, position.Z);
            entity.Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            entity.Scale = new Vector3(scale.X, scale.Y, scale.Z);

            for (int i = 0; i < node.MeshIndices.Count; i++)
            {
                int ind = node.MeshIndices[i];

                var mesh = meshes[ind];

                //create entity
                PrefabEntity mEntity = new PrefabEntity(node.Name + "-" + i);

                mEntity.Mesh = mesh.Mesh;
                mEntity.Material = materials[mesh.MatIndex];

                entity.Children.Add(mEntity);
            }

        //load children
        LoadChildren:
            foreach (var child in node.Children)
                entity.Children.Add(RecursivelyLoadAssimpNode(meshes, materials, child));

            return entity;
        }

        private static TextureWrapMode ConvertWrapModeToOTK(Assimp.TextureWrapMode textureWrapMode)
        {
            return textureWrapMode switch
            {
                Assimp.TextureWrapMode.Clamp => TextureWrapMode.ClampToEdge,
                Assimp.TextureWrapMode.Decal => TextureWrapMode.ClampToBorder,
                Assimp.TextureWrapMode.Mirror => TextureWrapMode.MirroredRepeat,
                Assimp.TextureWrapMode.Wrap => TextureWrapMode.Repeat,
                _ => TextureWrapMode.ClampToBorder,
            };
        }
    }
}
