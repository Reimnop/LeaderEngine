using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeaderEngine
{
    public static class ResourceLoader
    {
        public static Mesh LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();

            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            List<VertexArray> vertexArrays = new List<VertexArray>();

            foreach (var mesh in scene.Meshes)
            {
                List<uint> indices = IntToUint(mesh.GetIndices()).ToList();
                List<float> vertices = new List<float>();

                List<Vector3D> uvs = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0] : null;
                var material = scene.Materials[mesh.MaterialIndex];

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    Vector3D vert = mesh.Vertices[i];
                    Vector3D norm = mesh.Normals[i];
                    Vector3D uv = (uvs != null) ? uvs[i] : new Vector3D(0, 0, 0);

                    vertices.Add(vert.X);
                    vertices.Add(vert.Z);
                    vertices.Add(vert.Y);

                    vertices.Add(material.ColorDiffuse.R);
                    vertices.Add(material.ColorDiffuse.G);
                    vertices.Add(material.ColorDiffuse.B);

                    vertices.Add(norm.X);
                    vertices.Add(norm.Z);
                    vertices.Add(norm.Y);

                    vertices.Add(uv.X);
                    vertices.Add(1.0f - uv.Y);
                }

                VertexArray vertArray = new VertexArray(vertices.ToArray(), indices.ToArray(), new VertexAttrib[]
                {
                    new VertexAttrib { location = 0, size = 3 },
                    new VertexAttrib { location = 1, size = 3 },
                    new VertexAttrib { location = 2, size = 3 },
                    new VertexAttrib { location = 3, size = 2 }
                });

                if (material.HasTextureDiffuse)
                    if (Path.IsPathRooted(material.TextureDiffuse.FilePath))
                        vertArray.SetTexture(new Texture().FromFile(material.TextureDiffuse.FilePath));
                    else
                        vertArray.SetTexture(new Texture().FromFile(Path.Combine(Path.GetDirectoryName(path), material.TextureDiffuse.FilePath)));

                vertexArrays.Add(vertArray);
            }

            return new Mesh(vertexArrays.ToArray());
        }

        private static uint[] IntToUint(int[] ints)
        {
            uint[] uints = new uint[ints.Length];

            for (int i = 0; i < ints.Length; i++)
            {
                uints[i] = (uint)ints[i];
            }

            return uints;
        }
    }
}
