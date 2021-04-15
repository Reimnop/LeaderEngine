using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Quaternion = OpenTK.Mathematics.Quaternion;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace LeaderEngine
{
    public class Scene
    {
        public string Name;

        public List<Entity> SceneRootEntities { get; } = new List<Entity>();

        public Scene(string name)
        {
            Name = name;
        }

        internal void UpdateSceneHierachy()
        {
            for (int i = 0; i < SceneRootEntities.Count; i++)
                if (SceneRootEntities[i].Parent == null)
                    SceneRootEntities[i].RecursivelyUpdate();
        }
    }

    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");

        public static List<Material> Materials { get; } = new List<Material>();
        public static List<Prefab> Prefabs { get; } = new List<Prefab>();
        public static List<Mesh> Meshes { get; } = new List<Mesh>();
        public static List<Texture> Textures { get; } = new List<Texture>();
        public static List<AudioClip> AudioClips { get; } = new List<AudioClip>();

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
                                GCHandle handle = GCHandle.Alloc(embedTexture.NonCompressedData, GCHandleType.Pinned);

                                texture = Texture.FromPointer(
                                    texName,
                                    embedTexture.Width,
                                    embedTexture.Height,
                                    handle.AddrOfPinnedObject());

                                handle.Free();
                            }
                        }

                        texture.SetWrapS(ConvertWrapModeToOTK(aiTexture.WrapModeU));
                        texture.SetWrapT(ConvertWrapModeToOTK(aiTexture.WrapModeV));

                        mat.SetInt("hasDiffuse", 1);

                        mat.SetInt("diffuse", 0);
                        mat.SetTexture2D(TextureUnit.Texture0, texture);
                    }
                    catch
                    {
                        Logger.LogError($"Could not load texture {texName}!");
                    }
                }
            }

            //load model
            PrefabEntity rootEntity = RecursivelyLoadAssimpNode(aiMeshes, materials, scene.RootNode);

            return new Prefab(Path.GetFileNameWithoutExtension(path), rootEntity);
        }

        private static PrefabEntity RecursivelyLoadAssimpNode(List<Assimp.Mesh> aiMeshes, Material[] materials, Node node)
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

            int rIndex = 0;
            foreach (int ind in node.MeshIndices)
            {
                var aiMesh = aiMeshes[ind];

                Vertex[] vertices = new Vertex[aiMesh.VertexCount];

                //load each vertex
                for (int i = 0; i < aiMesh.VertexCount; i++)
                {
                    var aiVert = aiMesh.Vertices[i];
                    var aiNormal = aiMesh.Normals[i];

                    vertices[i] = new Vertex
                    {
                        Position = new Vector3(aiVert.X, aiVert.Y, aiVert.Z),

                        Normal = new Vector3(aiNormal.X, aiNormal.Y, aiNormal.Z),

                        Color = aiMesh.HasVertexColors(0) ? new Vector3(
                            aiMesh.VertexColorChannels[0][i].R,
                            aiMesh.VertexColorChannels[0][i].G,
                            aiMesh.VertexColorChannels[0][i].B) : Vector3.One,

                        UV = aiMesh.HasTextureCoords(0) ? new Vector2(
                            aiMesh.TextureCoordinateChannels[0][i].X,
                            aiMesh.TextureCoordinateChannels[0][i].Y) : Vector2.Zero
                    };
                }

                //create mesh
                Mesh mesh = new Mesh(node.Name + "-" + rIndex);
                mesh.LoadMesh(vertices, aiMesh.GetUnsignedIndices());

                //create entity
                PrefabEntity mEntity = new PrefabEntity(node.Name + "-" + rIndex);
                mEntity.Mesh = mesh;
                mEntity.Material = materials[aiMesh.MaterialIndex];

                entity.Children.Add(mEntity);
            }

        //load children
        LoadChildren:
            foreach (var child in node.Children)
                entity.Children.Add(RecursivelyLoadAssimpNode(aiMeshes, materials, child));

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
