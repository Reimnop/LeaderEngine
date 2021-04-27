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
            public T[] Vertices;
            public uint[] Indices;

            public Type VertexType { get; private set; }

            public int VerticesCount => Vertices.Length;
            public int IndicesCount => Indices.Length;

            public VertexArrayGeneric(T[] vertices, uint[] indices)
            {
                VertexType = typeof(T);

                Vertices = vertices;
                Indices = indices;
            }

            public void Serialize(BinaryWriter writer)
            {
                int vSize = Unsafe.SizeOf<T>();

                //write size
                writer.Write(Vertices.Length * vSize);
                //write vertices
                byte[] vertData = new byte[Vertices.Length * vSize];
                GCHandle vHandle = GCHandle.Alloc(Vertices, GCHandleType.Pinned);
                Marshal.Copy(vHandle.AddrOfPinnedObject(), vertData, 0, Vertices.Length * vSize);
                vHandle.Free();
                writer.Write(vertData);

                //write size
                writer.Write(Indices.Length * sizeof(uint));
                //write indices
                byte[] indexData = new byte[Indices.Length * sizeof(uint)];
                GCHandle iHandle = GCHandle.Alloc(Indices, GCHandleType.Pinned);
                Marshal.Copy(iHandle.AddrOfPinnedObject(), indexData, 0, Indices.Length * sizeof(uint));
                iHandle.Free();
                writer.Write(indexData);
            }

            public void Deserialize(BinaryReader reader)
            {
                //read size
                int vSize = reader.ReadInt32();
                //read vertices
                byte[] vertData = reader.ReadBytes(vSize);
                Vertices = new T[vSize / Unsafe.SizeOf<T>()];
                GCHandle vHandle = GCHandle.Alloc(Vertices, GCHandleType.Pinned);
                Marshal.Copy(vertData, 0, vHandle.AddrOfPinnedObject(), vSize);
                vHandle.Free();

                //read size
                int iSize = reader.ReadInt32();
                //read indices
                byte[] indexData = reader.ReadBytes(iSize);
                Indices = new uint[iSize / sizeof(uint)];
                GCHandle iHandle = GCHandle.Alloc(Indices, GCHandleType.Pinned);
                Marshal.Copy(indexData, 0, iHandle.AddrOfPinnedObject(), iSize);
                iHandle.Free();

                VertexType = typeof(T);
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

            ID = id != null ? id : RNG.GetRandomID();

            DataManager.Meshes.Add(ID, this);
        }

        internal void Unlist()
        {
            DataManager.Meshes.Remove(ID);
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

        private void FromVertexArrayInternal<T>(VertexArrayGeneric<T> vertexArray, PrimitiveType primitiveType = PrimitiveType.Triangles) where T : struct
        {
            PrimitiveType = primitiveType;

            //store vertices
            this.vertexArray = vertexArray;

            GL.BindVertexArray(VAO);

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexArray.VerticesCount * Unsafe.SizeOf<T>(), vertexArray.Vertices, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexArray.IndicesCount * sizeof(uint), vertexArray.Indices, BufferUsageHint.StaticDraw);

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

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }

        public void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);
            //write id
            writer.Write(ID);
            //write prim type
            writer.Write((int)PrimitiveType);
            //write type name
            writer.Write(vertexArray.VertexType.AssemblyQualifiedName);
            //write vertex array
            vertexArray.Serialize(writer);
        }

        public static Mesh Deserialize(BinaryReader reader)
        {
            //read name
            string name = reader.ReadString();
            //read id
            string id = reader.ReadString();
            //read prim type
            PrimitiveType primitiveType = (PrimitiveType)reader.ReadInt32();
            //read type
            Type vertexType = Type.GetType(reader.ReadString());
            //read vertex array
            dynamic vertexArray = Activator.CreateInstance(typeof(VertexArrayGeneric<>).MakeGenericType(vertexType));
            vertexArray.Deserialize(reader);

            dynamic mesh = new Mesh(name, id);
            mesh.FromVertexArrayInternal(vertexArray, primitiveType);

            return mesh;
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
