using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttrib : Attribute
    {
        public VertexAttribPointerType PointerType;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int size, bool normalized)
        {
            PointerType = pointerType;
            Normalized = normalized;
            Size = size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Position;

        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Normal;

        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Color;

        [VertexAttrib(VertexAttribPointerType.Float, 2, false)]
        public Vector2 UV;
    }

    public sealed class Mesh : IDisposable
    {
        private interface IVertexArray
        {
            public Type VertexType { get; }

            public int VerticesCount { get; }
            public int IndicesCount { get; }

            public void Serialize(BinaryWriter writer);
            public void Deserialize(BinaryReader reader);
        }

        private struct VertexArrayGeneric<T> : IVertexArray where T : struct
        {
            private T[] vertices;
            private uint[] indices;

            public Type VertexType { get; private set; }

            public int VerticesCount => vertices.Length;
            public int IndicesCount => indices.Length;

            public VertexArrayGeneric(T[] vertices, uint[] indices)
            {
                VertexType = typeof(T);

                this.vertices = vertices;
                this.indices = indices;
            }

            public void Serialize(BinaryWriter writer)
            {
                int vSize = Unsafe.SizeOf<T>();

                //write size
                writer.Write(vertices.Length * vSize);
                //write vertices
                byte[] vertData = new byte[vertices.Length * vSize];
                GCHandle vHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                Marshal.Copy(vHandle.AddrOfPinnedObject(), vertData, 0, vertices.Length * vSize);
                vHandle.Free();
                writer.Write(vertData);

                //write size
                writer.Write(indices.Length * sizeof(uint));
                //write indices
                byte[] indexData = new byte[indices.Length * sizeof(uint)];
                GCHandle iHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
                Marshal.Copy(iHandle.AddrOfPinnedObject(), indexData, 0, indices.Length * sizeof(uint));
                iHandle.Free();
                writer.Write(indexData);
            }

            public void Deserialize(BinaryReader reader)
            {
                //read size
                int vSize = reader.ReadInt32();
                //read vertices
                byte[] vertData = reader.ReadBytes(vSize);
                vertices = new T[vSize / Unsafe.SizeOf<T>()];
                GCHandle vHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                Marshal.Copy(vertData, 0, vHandle.AddrOfPinnedObject(), vSize);
                vHandle.Free();

                //read size
                int iSize = reader.ReadInt32();
                //read indices
                byte[] indexData = reader.ReadBytes(iSize);
                indices = new uint[iSize / sizeof(uint)];
                GCHandle iHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
                Marshal.Copy(indexData, 0, iHandle.AddrOfPinnedObject(), iSize);
                iHandle.Free();
            }
        }

        //vertex data
        private int VAO, VBO, EBO;
        private IVertexArray vertexArray;
        public int VerticesCount => vertexArray.VerticesCount;
        public int IndicesCount => vertexArray.IndicesCount;

        public PrimitiveType PrimitiveType { get; private set; }

        public readonly string Name;
        public readonly string ID;

        public bool Initialized { get; private set; } = false;

        public Mesh(string name, string id = null)
        {
            Name = name;

            //generate buffers
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            ID = id != null ? id : DataManager.GetUniqueID(x => DataManager.Meshes.ContainsKey(x));

            DataManager.Meshes.Add(ID, this);
        }

        public void LoadMesh<T>(T[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles) where T : struct
        {
            PrimitiveType = primitiveType;

            //store vertices
            vertexArray = new VertexArrayGeneric<T>(vertices, indices);

            GL.BindVertexArray(VAO);

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Unsafe.SizeOf<T>(), vertices, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //vertex attribs
            FieldInfo[] fields = typeof(T).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                VertexAttrib attrib = ((VertexAttrib[])fields[i].GetCustomAttributes(typeof(VertexAttrib)))[0];

                GL.VertexAttribPointer(i, attrib.Size, attrib.PointerType, attrib.Normalized, Unsafe.SizeOf<T>(), Marshal.OffsetOf<T>(fields[i].Name));
                GL.EnableVertexAttribArray(i);
            }

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length, Name);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            Initialized = true;
        }

        public void UpdateMesh<T>(T[] vertices, uint[] indices) where T : struct
        {
            //store vertices
            vertexArray = new VertexArrayGeneric<T>(vertices, indices);

            //upload buffers
            GL.NamedBufferData(VBO, vertices.Length * Unsafe.SizeOf<T>(), vertices, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(EBO, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicCopy);
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(EBO);

            DataManager.Meshes.Remove(ID);
        }
    }
}
