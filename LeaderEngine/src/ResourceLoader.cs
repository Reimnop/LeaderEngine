using Assimp;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public static class ResourceLoader
    {
        public static Dictionary<string, Mesh> LoadedMeshes = new Dictionary<string, Mesh>();
        public static Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();

        public static void LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder);

            Assimp.Mesh[] meshes = scene.Meshes.ToArray();
            Assimp.Material[] materials = scene.Materials.ToArray();

            RecursivelyLoad(scene.RootNode, null);

            void RecursivelyLoad(Node node, GameObject parent)
            {
                GameObject go = new GameObject(node.Name);

                if (node.HasMeshes)
                {
                    Assimp.Mesh aiMesh = meshes[node.MeshIndices[0]];

                    float[] vertices = LoadMesh(aiMesh);

                    MakeGameObject(node.Name, vertices, aiMesh, go);

                    for (int i = 1; i < node.MeshIndices.Count; i++)
                    {
                        Assimp.Mesh subMesh = meshes[node.MeshIndices[i]];
                        float[] subVertices = LoadMesh(subMesh);

                        string name = node.Name + "." + i;

                        GameObject subGo = new GameObject(name);

                        MakeGameObject(name, subVertices, subMesh, subGo);

                        subGo.Parent = go;
                    }
                }

                Vector3D position;
                Assimp.Quaternion rotation;
                Vector3D scale;

                node.Transform.Decompose(out scale, out rotation, out position);

                go.transform.LocalPosition = new Vector3(position.X, position.Y, position.Z);
                go.transform.Rotation = new OpenTK.Mathematics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
                go.transform.Scale = new Vector3(scale.X, scale.Y, scale.Z);

                go.Parent = parent;

                foreach (var item in node.Children)
                    RecursivelyLoad(item, go);
            }

            float[] LoadMesh(Assimp.Mesh aiMesh)
            {
                float[] vertices = new float[aiMesh.Vertices.Count * 11];

                for (int i = 0; i < aiMesh.Vertices.Count; i++)
                {
                    int vOffset = i * 11;

                    //vertex position
                    vertices[vOffset + 0] = aiMesh.Vertices[i].X;
                    vertices[vOffset + 1] = aiMesh.Vertices[i].Y;
                    vertices[vOffset + 2] = aiMesh.Vertices[i].Z;

                    //vertex colors
                    vertices[vOffset + 3] = 1.0f;
                    vertices[vOffset + 4] = 1.0f;
                    vertices[vOffset + 5] = 1.0f;

                    if (aiMesh.HasVertexColors(0))
                    {
                        vertices[vOffset + 3] = aiMesh.VertexColorChannels[0][i].R;
                        vertices[vOffset + 4] = aiMesh.VertexColorChannels[0][i].G;
                        vertices[vOffset + 5] = aiMesh.VertexColorChannels[0][i].B;
                    }

                    //normals
                    vertices[vOffset + 6] = aiMesh.Normals[i].X;
                    vertices[vOffset + 7] = aiMesh.Normals[i].Y;
                    vertices[vOffset + 8] = aiMesh.Normals[i].Z;

                    //uvs
                    vertices[vOffset + 9] = 0.0f;
                    vertices[vOffset + 10] = 0.0f;

                    if (aiMesh.HasTextureCoords(0))
                    {
                        vertices[vOffset + 9] = aiMesh.TextureCoordinateChannels[0][i].X;
                        vertices[vOffset + 10] = aiMesh.TextureCoordinateChannels[0][i].Y;
                    }
                }

                return vertices;
            }

            void MakeGameObject(string meshName, float[] vertices, Assimp.Mesh aiMesh, GameObject go)
            {
                Mesh mesh = new Mesh(meshName, vertices, aiMesh.GetUnsignedIndices(), new VertexAttrib[]
                {
                    new VertexAttrib { location = 0, size = 3 },
                    new VertexAttrib { location = 1, size = 3 },
                    new VertexAttrib { location = 2, size = 3 },
                    new VertexAttrib { location = 3, size = 2 }
                });

                if (!LoadedMeshes.ContainsKey(meshName))
                    LoadedMeshes.Add(meshName, mesh);

                Assimp.Material aiMaterial = materials[aiMesh.MaterialIndex];

                Material mat = Material.Lit.Clone();

                mat.SetInt("useTexture", 0);

                if (aiMaterial.HasTextureDiffuse)
                {
                    TextureSlot aiTexture = aiMaterial.TextureDiffuse;

                    string texPath = aiTexture.FilePath;

                    Texture texture;
                    if (Path.IsPathRooted(texPath))
                        texture = new Texture().FromFile(texPath);
                    else
                        texture = new Texture().FromFile(Path.Combine(Path.GetDirectoryName(path), texPath));

                    texture.SetWrapS(ConvertToOTK(aiTexture.WrapModeU));
                    texture.SetWrapT(ConvertToOTK(aiTexture.WrapModeV));

                    mat.SetInt("texture0", 0);

                    mat.SetInt("useTexture", 1);
                    mat.SetTexture2D(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0, texture);
                }

                Color4D color = aiMaterial.ColorDiffuse;

                mat.SetVector4("color", new OpenTK.Mathematics.Vector4(color.R, color.G, color.B, color.A));

                go.AddComponent<MeshFilter>(mesh);
                go.AddComponent<MeshRenderer>().Material = mat;
            }
        }

        private static OpenTK.Graphics.OpenGL4.TextureWrapMode ConvertToOTK(TextureWrapMode textureWrapMode)
        {
            switch (textureWrapMode)
            {
                case TextureWrapMode.Clamp:
                    return OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder;
                case TextureWrapMode.Decal:
                    return OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToEdge;
                case TextureWrapMode.Mirror:
                    return OpenTK.Graphics.OpenGL4.TextureWrapMode.MirroredRepeat;
                case TextureWrapMode.Wrap:
                    return OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                default:
                    return OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder;
            }
        }
    }
}
