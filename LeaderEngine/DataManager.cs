using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;
using Assimp;

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
            Material[] materials = new Material[aiMaterials.Count];

            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var aiMaterial = aiMaterials[i];

                Material mat = new Material(null); //TODO: replace with shader

                materials[i] = mat;

                //reset values
                mat.SetInt("useTexture", 0);

                if (aiMaterial.HasTextureDiffuse)
                {
                    TextureSlot aiTexture = aiMaterial.TextureDiffuse;

                    //load texture
                    string texPath = aiTexture.FilePath;

                    if (!Path.IsPathRooted(texPath))
                        texPath = Path.Combine(Path.GetDirectoryName(path), texPath);

                    Texture texture = Texture.FromFile(aiMaterial.Name + "-Diffuse", texPath);

                    texture.SetWrapS((TextureWrapMode)aiTexture.WrapModeU);
                    texture.SetWrapT((TextureWrapMode)aiTexture.WrapModeV);

                    mat.SetInt("useTexture", 1);
                }
            }

            //load model
            RecursivelyLoadAssimpNode(aiMeshes, aiMaterials, scene.RootNode);
        }

        private static void RecursivelyLoadAssimpNode(List<Assimp.Mesh> meshes, List<Assimp.Material> materials, Node node)
        {
            if (!node.HasMeshes)
                goto LoadChildren;

            int rIndex = 0;
            foreach (int ind in node.MeshIndices)
            {
                var aiMesh = meshes[ind];

                Vertex[] vertices = new Vertex[aiMesh.VertexCount];

                //load each vertex
                for (int i = 0; i < aiMesh.VertexCount; i++)
                {
                    var aiVert = aiMesh.Vertices[i];

                    vertices[i] = new Vertex
                    {
                        Position = new Vector3(aiVert.X, aiVert.Y, aiVert.Z),

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
            }

            //load children
            LoadChildren:
            foreach (var child in node.Children)
                RecursivelyLoadAssimpNode(meshes, materials, child);
        }
    }
}
