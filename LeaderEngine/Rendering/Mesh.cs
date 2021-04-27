using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Linq;
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
        public int Location;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int location, int size, bool normalized)
        {
            PointerType = pointerType;
            Location = location;
            Normalized = normalized;
            Size = size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        [VertexAttrib(VertexAttribPointerType.Float, 0, 3, false)]
        public Vector3 Position;

        [VertexAttrib(VertexAttribPointerType.Float, 1, 3, false)]
        public Vector3 Normal;

        [VertexAttrib(VertexAttribPointerType.Float, 2, 3, false)]
        public Vector3 Color;

        [VertexAttrib(VertexAttribPointerType.Float, 3, 2, false)]
        public Vector2 UV;
    }

    public sealed class Mesh : IDisposable
    {
        //vertex data
        private int VAO, VBO, EBO;

        public int VerticesCount { private set; get; }
        public int IndicesCount { private set; get; }

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

            GL.BindVertexArray(VAO);

            //update array sizes
            VerticesCount = vertices.Length;
            IndicesCount = indices.Length;

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
                var attribs = fields[i].GetCustomAttributes(typeof(VertexAttrib));
                VertexAttrib attrib = attribs.Count() > 0 ? (VertexAttrib)attribs.First() : throw new ArgumentNullException();

                GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.PointerType, attrib.Normalized, Unsafe.SizeOf<T>(), Marshal.OffsetOf<T>(fields[i].Name));
                GL.EnableVertexAttribArray(attrib.Location);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length, Name);

            Initialized = true;
        }

        public void UpdateMesh<T>(T[] vertices, uint[] indices) where T : struct
        {
            if (!Initialized)
            {
                LoadMesh(vertices, indices);
                return;
            }

            //update array sizes
            VerticesCount = vertices.Length;
            IndicesCount = indices.Length;

            //upload buffers
            GL.NamedBufferData(VBO, vertices.Length * Unsafe.SizeOf<T>(), vertices, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(EBO, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicCopy);
        }

        public void Clear()
        {
            //update array sizes
            VerticesCount = 0;
            IndicesCount = 0;

            //clear buffers
            GL.NamedBufferData(VBO, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(EBO, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }

        public void Serialize(BinaryWriter writer)
        {
            //write name
            /*writer.Write(Name);
            //write id
            writer.Write(ID);
            //write prim type
            writer.Write((int)PrimitiveType);
            //write type name
            writer.Write(vertexArray.VertexType.AssemblyQualifiedName);
            //write vertex array
            vertexArray.Serialize(writer);*/
        }

        public static Mesh Deserialize(BinaryReader reader)
        {
            //read name
            /*string name = reader.ReadString();
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
            mesh.FromVertexArrayInternal(vertexArray, primitiveType);*/

            throw new NotImplementedException();
            //return mesh;
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
