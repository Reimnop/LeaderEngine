using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
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
        public int VerticesCount { get; private set; }
        public int IndicesCount { get; private set; }
        public PrimitiveType PrimitiveType { get; private set; }

        private int VAO, VBO, EBO;

        public readonly string Name;

        public Mesh(string name)
        {
            Name = name;

            //generate buffers
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();
        }

        public void LoadMesh<T>(T[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles) where T : struct
        {
            PrimitiveType = primitiveType;

            //update vertices and indices counts
            VerticesCount = vertices.Length;
            IndicesCount = indices.Length;

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
        }

        public void UpdateMesh<T>(T[] vertices, uint[] indices) where T : struct
        {
            //upload buffers
            GL.NamedBufferData(VBO, vertices.Length * Unsafe.SizeOf<T>(), vertices, BufferUsageHint.DynamicCopy);
            GL.NamedBufferData(EBO, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicCopy);

            VerticesCount = vertices.Length;
            IndicesCount = indices.Length;
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
        }
    }
}
