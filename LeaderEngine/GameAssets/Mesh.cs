using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
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

    public sealed class Mesh : GameAsset
    {
        private struct VertexAttribData
        {
            public VertexAttribPointerType PointerType;
            public int Location;
            public int Size;
            public int Offset;
            public bool Normalized;
        }

        public override GameAssetType AssetType => GameAssetType.Mesh;

        public Span<Vector3> Vertices => new Span<Vector3>(_vertices);
        public Span<uint> Indices => new Span<uint>(_indices);

        public int VerticesCount => _vertices.Length;
        public int IndicesCount => _indices.Length;

        public int VAO => _vao;
        public int VBO0 => _vbo0;
        public int VBO1 => _vbo1;
        public int EBO => _ebo;

        private Vector3[] _vertices;
        private uint[] _indices;

        private int _vao;
        private int _vbo0;
        private int _vbo1;
        private int _ebo;

        private VertexAttribData[] vertexAttribs;

        public PrimitiveType PrimitiveType { private set; get; }
        public DrawElementsType DrawElementsType { private set; get; }

        public Mesh(string name) : base(name)
        {
            //generate buffers
            _vao = GL.GenVertexArray();
            _vbo0 = GL.GenBuffer();
            _vbo1 = GL.GenBuffer();
            _ebo = GL.GenBuffer();
        }

        public void LoadMesh(
            Vector3[] vertices, uint[] indices, 
            PrimitiveType primitiveType = PrimitiveType.Triangles, 
            DrawElementsType drawElementsType = DrawElementsType.UnsignedInt)
        {
            PrimitiveType = primitiveType;
            DrawElementsType = drawElementsType;

            _vertices = vertices;
            _indices = indices;

            GL.BindVertexArray(_vao);

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo0);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //vertex attrib
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, _vao, Name.Length + 4, Name + "-VAO");
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _vbo0, Name.Length + 5, Name + "-VBO0");
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _ebo, Name.Length + 4, Name + "-EBO");
        }

        public void SetPerVertexData<T>(T[] data) where T : struct
        {
            //upload buffer to gpu
            int vertexSize = Unsafe.SizeOf<T>();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo1);
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

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _vbo1, Name.Length + 5, Name + "-VBO1");
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo0);
            GL.DeleteBuffer(_vbo1);
            GL.DeleteBuffer(_ebo);
        }
    }
}