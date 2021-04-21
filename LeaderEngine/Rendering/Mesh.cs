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

            public void Serialize(Stream stream);
            public void Deserialize(Stream stream);
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

            public void Serialize(Stream stream)
            {
                int vSize = Unsafe.SizeOf<T>();

                //write length
                stream.Write(BitConverter.GetBytes(vertices.Length * vSize));
                IntPtr vPtr = Marshal.AllocHGlobal(vSize);
                byte[] vBuffer = new byte[vSize];
                //write vertices
                for (int i = 0; i < vertices.Length; i++)
                {
                    T vertex = vertices[i];

                    Marshal.StructureToPtr(vertex, vPtr, true);
                    Marshal.Copy(vPtr, vBuffer, 0, vSize);

                    stream.Write(vBuffer, 0, vSize);
                }
                Marshal.FreeHGlobal(vPtr);

                //write length
                stream.Write(BitConverter.GetBytes(indices.Length * sizeof(uint)));
                //write indices
                for (int i = 0; i < indices.Length; i++)
                {
                    stream.Write(BitConverter.GetBytes(indices[i]));
                }
            }
            public void Deserialize(Stream stream)
            {
                //read length
                byte[] buffer = new byte[sizeof(int)];
                stream.Read(buffer, 0, sizeof(int));

                int vBufSize = BitConverter.ToInt32(buffer, 0);
                int vSize = Unsafe.SizeOf<T>();
                int vLength = vBufSize / vSize;

                //read vertices
                vertices = new T[vLength];
                buffer = new byte[vSize];
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                for (int i = 0; i < vLength; i++)
                {
                    stream.Read(buffer, 0, vSize);
                    vertices[i] = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                }
                handle.Free();

                //read length
                buffer = new byte[sizeof(int)];
                stream.Read(buffer, 0, sizeof(int));

                int iBufSize = BitConverter.ToInt32(buffer, 0);
                int iSize = sizeof(uint);
                int iLength = iBufSize / iSize;

                //read indices
                indices = new uint[iLength];
                buffer = new byte[iSize];
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                for (int i = 0; i < iLength; i++)
                {
                    stream.Read(buffer, 0, iSize);
                    indices[i] = BitConverter.ToUInt32(buffer, 0);
                }
                handle.Free();
            }
        }

        //vertex data
        private int VAO, VBO, EBO;
        private IVertexArray vertexArray;
        public int VerticesCount => vertexArray.VerticesCount;
        public int IndicesCount => vertexArray.IndicesCount;

        public PrimitiveType PrimitiveType { get; private set; }

        public readonly string Name;

        public bool Initialized { get; private set; } = false;

        public Mesh(string name)
        {
            Name = name;

            //generate buffers
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            DataManager.Meshes.Add(this);
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

            DataManager.Meshes.Remove(this);
        }
    }
}
