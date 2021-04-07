using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using Quaternion = OpenTK.Mathematics.Quaternion;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace LeaderEngine
{
    public class Scene
    {
        public string Name;

        public List<Material> SceneMaterials { get; } = new List<Material>();
        public List<Texture> SceneTextures { get; } = new List<Texture>();
        public List<Mesh> SceneMeshes { get; } = new List<Mesh>();

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

        public static void LoadModelFromFile(string path)
        {
            //import file
            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFile(path, 
                PostProcessSteps.Triangulate | 
                PostProcessSteps.FlipUVs | 
                PostProcessSteps.FlipWindingOrder | 
                PostProcessSteps.CalculateTangentSpace);

            var aiMeshes = scene.Meshes;
            var aiMaterials = scene.Materials;

            //load materials
            //TODO: remove shader
            Shader shader = Shader.FromSourceFile("Lit",
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/lit.vert"),
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/lit.frag"));

            Material[] materials = new Material[aiMaterials.Count];

            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var aiMaterial = aiMaterials[i];

                Material mat = new Material(shader); //TODO: replace with shader

                materials[i] = mat;

                mat.SetVector3("color", new Vector3(aiMaterial.ColorDiffuse.R, aiMaterial.ColorDiffuse.G, aiMaterial.ColorDiffuse.B));

                //reset values
                mat.SetInt("hasDiffuse", 0);

                if (aiMaterial.HasTextureDiffuse)
                {
                    TextureSlot aiTexture = aiMaterial.TextureDiffuse;

                    //load texture
                    string texPath = aiTexture.FilePath;

                    if (!Path.IsPathRooted(texPath))
                        texPath = Path.Combine(Path.GetDirectoryName(path), texPath);

                    Texture texture = Texture.FromFile(aiMaterial.Name + "-Diffuse", texPath);

                    texture.SetWrapS(ConvertWrapModeToOTK(aiTexture.WrapModeU));
                    texture.SetWrapT(ConvertWrapModeToOTK(aiTexture.WrapModeV));

                    mat.SetInt("hasDiffuse", 1);
                    mat.SetTexture2D(TextureUnit.Texture0, texture);
                }
            }

            //load model
            RecursivelyLoadAssimpNode(aiMeshes, materials, scene.RootNode, null);
        }

        private static void RecursivelyLoadAssimpNode(List<Assimp.Mesh> aiMeshes, Material[] materials, Node node, Entity parent)
        {
            //create Entity
            //TODO: switch to prefabs
            Entity entity = new Entity(node.Name);

            if (parent != null)
                entity.Parent = parent;

            if (!node.HasMeshes)
                goto LoadChildren;

            //set transform
            node.Transform.Decompose(out var scale, out var rotation, out var position);

            entity.Transform.Position = new Vector3(position.X, position.Y, position.Z);
            entity.Transform.Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            entity.Transform.Scale = new Vector3(scale.X, scale.Y, scale.Z);

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
                Entity mEntity = new Entity(node.Name + "-" + rIndex);
                mEntity.Parent = entity;

                var mr = mEntity.AddComponent<MeshRenderer>();

                mr.Mesh = mesh;
                mr.Material = materials[aiMesh.MaterialIndex];
            }

            //load children
            LoadChildren:
            foreach (var child in node.Children)
                RecursivelyLoadAssimpNode(aiMeshes, materials, child, entity);
        }

        private static TextureWrapMode ConvertWrapModeToOTK(Assimp.TextureWrapMode textureWrapMode)
        {
            switch (textureWrapMode)
            {
                case Assimp.TextureWrapMode.Clamp:
                    return TextureWrapMode.ClampToBorder;
                case Assimp.TextureWrapMode.Decal:
                    return TextureWrapMode.ClampToEdge;
                case Assimp.TextureWrapMode.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                case Assimp.TextureWrapMode.Wrap:
                    return TextureWrapMode.Repeat;
                default:
                    return TextureWrapMode.ClampToBorder;
            }
        }
    }
}
