using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshSerializer : GameAssetSerializer
    {
        public override bool CanSerialize(GameAssetType assetType)
        {
            return assetType == GameAssetType.Mesh;
        }

        public override void WriteToStream(BinaryWriter writer, GameAsset asset)
        {
            Mesh mesh = (Mesh)asset;

            //get arrays
            Vector3[] vertices = mesh.Vertices.ToArray();
            uint[] indices = mesh.Indices.ToArray();

            //get per vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VBO1);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int perVertexBufferSize);

            byte[] perVertexBuffer = new byte[perVertexBufferSize];
            GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, perVertexBufferSize, perVertexBuffer);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            //write to stream
            writer.Write(mesh.Name); //name

            writer.Write(vertices.Length); //vertices count

            byte[] vertexBytesBuffer = Helper.StructArrayToByteArray(vertices); 
            writer.Write(vertexBytesBuffer); //vertices

            writer.Write(indices.Length); //indices count
            byte[] indexBytesBuffer = Helper.StructArrayToByteArray(indices);
            writer.Write(indexBytesBuffer); //indices

            writer.Write(perVertexBufferSize); //per vertex buffer size
            writer.Write(perVertexBuffer); //per vertex buffer

            //vertex attribs
            VertexAttrib[] attribs = mesh.VertexAttribs.ToArray();
            writer.Write(attribs.Length); //attribs count
            for (int i = 0; i < attribs.Length; i++)
            {
                VertexAttrib attrib = attribs[i];

                writer.Write((int)attrib.PointerType); //pointer type
                writer.Write(attrib.Size); //size
                writer.Write(attrib.Normalized); //normalized
            }
        }

        public override GameAsset ReadFromStream(BinaryReader reader)
        {
            string name = reader.ReadString();

            //vertex buffer
            int verticesCount = reader.ReadInt32();
            int vertexBufferSize = Vector3.SizeInBytes * verticesCount;
            byte[] vertexBytesBuffer = reader.ReadBytes(vertexBufferSize);
            Vector3[] vertices = Helper.ByteArrayToStructArray<Vector3>(vertexBytesBuffer);

            //index buffer
            int indicesCount = reader.ReadInt32();
            int indexBufferSize = sizeof(uint) * indicesCount;
            byte[] indexBytesBuffer = reader.ReadBytes(indexBufferSize);
            uint[] indices = Helper.ByteArrayToStructArray<uint>(indexBytesBuffer);

            //per vertex buffer
            int perVertexBufferSize = reader.ReadInt32();
            byte[] perVertexBytesBuffer = reader.ReadBytes(perVertexBufferSize);

            //vertex attribs
            int attribsCount = reader.ReadInt32();
            VertexAttrib[] attribs = new VertexAttrib[attribsCount];
            for (int i = 0; i < attribsCount; i++)
            {
                VertexAttribPointerType pointerType = (VertexAttribPointerType)reader.ReadInt32();
                int size = reader.ReadInt32();
                bool normalized = reader.ReadBoolean();

                attribs[i] = new VertexAttrib(pointerType, size, normalized);
            }

            //init mesh
            Mesh mesh = new Mesh(name);
            mesh.LoadMesh(vertices, indices);
            mesh.SetPerVertexData(perVertexBytesBuffer, attribs);

            return mesh;
        }
    }
}
