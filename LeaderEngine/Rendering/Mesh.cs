using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttrib : Attribute
    {
        public VertexAttribPointerType PointerType;
        public int Location;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int location, int size, bool normalized)
        {
            if (location == 0)
                throw new Exception("Location cannot be 0!");

            PointerType = pointerType;
            Location = location;
            Normalized = normalized;
            Size = size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        [VertexAttrib(VertexAttribPointerType.Float, 1, 3, false)]
        public Vector3 Normal;

        [VertexAttrib(VertexAttribPointerType.Float, 2, 3, false)]
        public Vector3 Color;

        [VertexAttrib(VertexAttribPointerType.Float, 3, 2, false)]
        public Vector2 UV;
    }

    public sealed class Mesh : IDisposable
    {
        private struct VertexAttribData
        {
            public VertexAttribPointerType PointerType;
            public int Location;
            public int Size;
            public int Offset;
            public bool Normalized;
        }

        public readonly string Name;
        public readonly string ID;

        //vertex data
        private int VAO, VBO0, VBO1, EBO;
        private VertexAttribData[] vertexAttribs;

        private Type perVertexDataType;

        public PrimitiveType PrimitiveType { private set; get; }
        public DrawElementsType DrawElementsType { private set; get; }

        public int VerticesCount { private set; get; }
        public int IndicesCount { private set; get; }

        public Mesh(string name, string id = null)
        {
            Name = name;

            //generate buffers
            VAO = GL.GenVertexArray();
            VBO0 = GL.GenBuffer();
            VBO1 = GL.GenBuffer();
            EBO = GL.GenBuffer();

            ID = id != null ? id : RNG.GetRandomID();

            DataManager.Meshes.Add(ID, this);
        }

        internal void Unlist()
        {
            DataManager.Meshes.Remove(ID);
        }

        public void LoadMesh(Vector3[] vertexPositions, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles, DrawElementsType drawElementsType = DrawElementsType.UnsignedInt)
        {
            PrimitiveType = primitiveType;
            DrawElementsType = drawElementsType;

            GL.BindVertexArray(VAO);

            //update array sizes
            VerticesCount = vertexPositions.Length;
            IndicesCount = indices.Length;

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO0);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexPositions.Length * Vector3.SizeInBytes, vertexPositions, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //vertex attrib
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO0, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length, Name);
        }

        public void SetPerVertexData<T>(T[] data) where T : struct
        {
            perVertexDataType = typeof(T);

            //upload buffer to gpu
            int vertexSize = Unsafe.SizeOf<T>();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO1);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * vertexSize, data, BufferUsageHint.StaticDraw);

            //vertex attribs
            FieldInfo[] fields = typeof(T).GetFields();

            vertexAttribs = new VertexAttribData[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                var attribs = fields[i].GetCustomAttributes(typeof(VertexAttrib));
                VertexAttrib attrib = attribs.Count() > 0 ? (VertexAttrib)attribs.First() : throw new ArgumentNullException();

                int offset = Marshal.OffsetOf<T>(fields[i].Name).ToInt32();

                GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.PointerType, attrib.Normalized, vertexSize, offset);
                GL.EnableVertexAttribArray(attrib.Location);

                //store vertex attrib
                vertexAttribs[i] = new VertexAttribData
                {
                    PointerType = attrib.PointerType,
                    Location = attrib.Location,
                    Size = attrib.Size,
                    Offset = offset,
                    Normalized = attrib.Normalized
                };
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO1, Name.Length, Name);
        }

        public void Clear()
        {
            //update array sizes
            VerticesCount = 0;
            IndicesCount = 0;

            //clear buffers
            GL.NamedBufferData(VBO0, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(VBO1, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(EBO, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }

        internal void Serialize(BinaryWriter writer)
        {
            /*//write name
            writer.Write(Name);
            //write id
            writer.Write(ID);
            //write prim type
            writer.Write((int)PrimitiveType);
            //write draw elem type
            writer.Write((int)DrawElementsType);
            //write attribs
            SerializeVertexAttribs(writer);

            //download buffers
            byte[] vertexBuffer = new byte[VerticesCount * vertexSize];
            GL.GetNamedBufferSubData(VBO, IntPtr.Zero, vertexBuffer.Length, vertexBuffer);

            byte[] elementBuffer = new byte[IndicesCount * indexSize];
            GL.GetNamedBufferSubData(EBO, IntPtr.Zero, elementBuffer.Length, elementBuffer);

            //write buffers
            writer.Write(vertexSize);
            writer.Write(VerticesCount);
            writer.Write(vertexBuffer);

            writer.Write(indexSize);
            writer.Write(IndicesCount);
            writer.Write(elementBuffer);*/
        }

        private void SerializeVertexAttribs(BinaryWriter writer)
        {
            //write length
            writer.Write(vertexAttribs.Length);
            //write attribs
            for (int i = 0; i < vertexAttribs.Length; i++)
            {
                VertexAttribData attrib = vertexAttribs[i];

                writer.Write((int)attrib.PointerType);
                writer.Write(attrib.Location);
                writer.Write(attrib.Size);
                writer.Write(attrib.Offset);
                writer.Write(attrib.Normalized);
            }
        }

        public static Mesh Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
            /*//read name
            string name = reader.ReadString();
            //read id
            string id = reader.ReadString();
            //read prim type
            var primType = (PrimitiveType)reader.ReadInt32();
            //read draw elem type
            var drawElemType = (DrawElementsType)reader.ReadInt32();
            //read attribs
            var attribs = DeserializeVertexAttribs(reader);

            //read buffers
            int vertexSize = reader.ReadInt32();
            int verticesCount = reader.ReadInt32();
            byte[] vertexBuffer = reader.ReadBytes(verticesCount * vertexSize);

            int indexSize = reader.ReadInt32();
            int indicesCount = reader.ReadInt32();
            byte[] elementBuffer = reader.ReadBytes(indicesCount * indexSize);

            //upload to GPU
            Mesh mesh = new Mesh(name, id);

            #region MeshInitOverride
            int vao = mesh.VAO;
            int vbo = mesh.VBO;
            int ebo = mesh.EBO;

            mesh.PrimitiveType = primType;
            mesh.DrawElementsType = drawElemType;

            GL.BindVertexArray(vao);

            //update sizes
            mesh.vertexSize = vertexSize;
            mesh.indexSize = indexSize;

            mesh.VerticesCount = verticesCount;
            mesh.IndicesCount = indicesCount;

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length, vertexBuffer, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, elementBuffer.Length, elementBuffer, BufferUsageHint.StaticDraw);

            //vertex attribs
            mesh.vertexAttribs = attribs;

            for (int i = 0; i < attribs.Length; i++)
            {
                var attrib = attribs[i];

                GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.PointerType, attrib.Normalized, vertexSize, attrib.Offset);
                GL.EnableVertexAttribArray(attrib.Location);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, vao, name.Length, name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, vbo, name.Length, name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, ebo, name.Length, name);

            mesh.initialized = true;
            #endregion

            return mesh;*/
        }

        private static VertexAttribData[] DeserializeVertexAttribs(BinaryReader reader)
        {
            int attribCount = reader.ReadInt32();

            VertexAttribData[] attribs = new VertexAttribData[attribCount];

            for (int i = 0; i < attribCount; i++)
            {
                attribs[i].PointerType = (VertexAttribPointerType)reader.ReadInt32();
                attribs[i].Location = reader.ReadInt32();
                attribs[i].Size = reader.ReadInt32();
                attribs[i].Offset = reader.ReadInt32();
                attribs[i].Normalized = reader.ReadBoolean();
            }

            return attribs;
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO0);
            GL.DeleteBuffer(VBO1);
            GL.DeleteBuffer(EBO);

            DataManager.Meshes.Remove(ID);
        }
    }
}