using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public sealed class Mesh : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Mesh;

        public Span<Vector3> Vertices => new Span<Vector3>(_vertices);
        public Span<uint> Indices => new Span<uint>(_indices);

        public Span<VertexAttrib> VertexAttribs => new Span<VertexAttrib>(_vertexAttribs);

        public int VerticesCount => _vertices.Length;
        public int IndicesCount => _indices.Length;

        public int VAO => _vao;
        public int VBO0 => _vbo0;
        public int VBO1 => _vbo1;
        public int EBO => _ebo;

        private Vector3[] _vertices;
        private uint[] _indices;

        private VertexAttrib[] _vertexAttribs;

        private int _vao;
        private int _vbo0;
        private int _vbo1;
        private int _ebo;

        public Mesh(string name) : base(name)
        {
            //generate buffers
            _vao = GL.GenVertexArray();
            _vbo0 = GL.GenBuffer();
            _vbo1 = GL.GenBuffer();
            _ebo = GL.GenBuffer();
        }

        public void LoadMesh(Vector3[] vertices, uint[] indices)
        {
            _vertices = vertices;
            _indices = indices;

            GL.BindVertexArray(_vao);

            //upload vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo0);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            //upload element buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

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
            //vertex attribs
            FieldInfo[] fields = typeof(T).GetFields();

            VertexAttrib[] vertexAttribs = new VertexAttrib[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                var attribs = fields[i].GetCustomAttributes(typeof(VertexAttribAttribute));
                VertexAttribAttribute attrib = attribs.Count() > 0 ? (VertexAttribAttribute)attribs.First() : throw new ArgumentNullException();
                vertexAttribs[i] = new VertexAttrib(attrib.PointerType, attrib.Size, attrib.Normalized);
            }

            SetPerVertexData(data, vertexAttribs);
        }

        public void SetPerVertexData<T>(T[] data, params VertexAttrib[] vertexAttribs) where T : struct
        {
            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo1);
            GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<T>() * data.Length, data, BufferUsageHint.StaticDraw);

            //calculate stride
            int stride = 0;
            foreach (VertexAttrib attrib in vertexAttribs)
                stride += GetPointerTypeSize(attrib.PointerType) * attrib.Size;

            int offset = 0;
            for (int i = 0; i < vertexAttribs.Length; i++)
            {
                VertexAttrib attrib = vertexAttribs[i];

                GL.VertexAttribPointer(i + 1, attrib.Size, attrib.PointerType, attrib.Normalized, stride, offset);
                GL.EnableVertexAttribArray(i + 1);

                offset += GetPointerTypeSize(attrib.PointerType) * attrib.Size;
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _vbo1, Name.Length + 5, Name + "-VBO1");

            _vertexAttribs = vertexAttribs;
        }

        private int GetPointerTypeSize(VertexAttribPointerType pointerType)
        {
            switch (pointerType)
            {
                case VertexAttribPointerType.Byte:
                    return sizeof(sbyte);
                case VertexAttribPointerType.Double:
                    return sizeof(double);
                case VertexAttribPointerType.Fixed:
                    return 4; //32 bits
                case VertexAttribPointerType.Float:
                    return sizeof(float);
                case VertexAttribPointerType.HalfFloat:
                    return 2; //16 bits
                case VertexAttribPointerType.Int:
                    return sizeof(int);
                case VertexAttribPointerType.Int2101010Rev:
                    return 4; //32 bits
                case VertexAttribPointerType.Short:
                    return sizeof(short);
                case VertexAttribPointerType.UnsignedByte:
                    return sizeof(byte);
                case VertexAttribPointerType.UnsignedInt:
                    return sizeof(uint);
                case VertexAttribPointerType.UnsignedInt10F11F11FRev:
                    return 4; //32 bits
                case VertexAttribPointerType.UnsignedInt2101010Rev:
                    return 4; //32 bits
                case VertexAttribPointerType.UnsignedShort:
                    return sizeof(ushort);
                default:
                    throw new NotImplementedException();
            }
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