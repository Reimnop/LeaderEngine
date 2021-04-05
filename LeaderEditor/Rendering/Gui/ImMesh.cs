using OpenTK.Graphics.OpenGL4;
using System;

namespace LeaderEditor
{
    public struct VertexAttrib
    {
        public int Location;
        public int Size;
        public VertexAttribPointerType PointerType;
        public int SizeOfType;
        public bool Normalized;

        public VertexAttrib(int location, int size, bool normalized = false)
        {
            Location = location;
            Size = size;

            PointerType = VertexAttribPointerType.Float;
            SizeOfType = sizeof(float);

            Normalized = normalized;
        }

        public VertexAttrib(int location, int size, VertexAttribPointerType pointerType, int sizeOfType, bool normalized = false)
        {
            Location = location;
            Size = size;
            PointerType = pointerType;
            SizeOfType = sizeOfType;
            Normalized = normalized;
        }
    }

    public sealed class ImMesh : IDisposable
    {
        public string Name;

        private int oldVertexSize;
        private int oldIndexSize;

        public int VAO, VBO, EBO;

        public ImMesh(string name, float[] vertices, uint[] indices, VertexAttrib[] attribs, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticDraw)
        {
            Name = name;
            Init(attribs, vertices, indices, bufferUsageHint);
        }

        public ImMesh(string name, IntPtr vertices, IntPtr indices, int vertexSize, int indexSize, VertexAttrib[] attribs, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticDraw)
        {
            Name = name;
            Init(attribs, vertices, indices, vertexSize, indexSize, bufferUsageHint);
        }

        private void Init(VertexAttrib[] attribs, IntPtr vertices, IntPtr indices, int vertexSize, int indexSize, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticDraw)
        {
            oldVertexSize = vertexSize;
            oldIndexSize = indexSize;

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexSize, vertices, bufferUsageHint);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexSize, indices, bufferUsageHint);

            int size = 0;
            foreach (VertexAttrib attrib in attribs)
                size += attrib.Size * attrib.SizeOfType;

            int c = 0;
            for (int i = 0; i < attribs.Length; i++)
            {
                GL.VertexAttribPointer(attribs[i].Location, attribs[i].Size, attribs[i].PointerType, attribs[i].Normalized, size, c);
                GL.EnableVertexAttribArray(attribs[i].Location);
                c += attribs[i].Size * attribs[i].SizeOfType;
            }

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length, Name);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void Init(VertexAttrib[] attribs, float[] vertices, uint[] indices, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticDraw)
        {
            int vertexSize = vertices.Length * sizeof(float);
            int indexSize = indices.Length * sizeof(uint);

            oldVertexSize = vertexSize;
            oldIndexSize = indexSize;

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexSize, vertices, bufferUsageHint);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexSize, indices, bufferUsageHint);

            int size = 0;
            foreach (VertexAttrib attrib in attribs)
                size += attrib.Size * attrib.SizeOfType;

            int c = 0;
            for (int i = 0; i < attribs.Length; i++)
            {
                GL.VertexAttribPointer(attribs[i].Location, attribs[i].Size, attribs[i].PointerType, attribs[i].Normalized, size, c);
                GL.EnableVertexAttribArray(attribs[i].Location);
                c += attribs[i].Size * attribs[i].SizeOfType;
            }

            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VAO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VBO, Name.Length, Name);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, EBO, Name.Length, Name);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void UpdateMeshData(float[] vertices, uint[] indices)
        {
            int vertexSize = vertices.Length * sizeof(float);
            int indexSize = indices.Length * sizeof(uint);

            if (vertexSize > oldVertexSize)
                GL.NamedBufferData(VBO, vertexSize, vertices, BufferUsageHint.DynamicDraw);
            else
                GL.NamedBufferSubData(VBO, IntPtr.Zero, vertexSize, vertices);

            if (indexSize > oldIndexSize)
                GL.NamedBufferData(EBO, indexSize, indices, BufferUsageHint.DynamicDraw);
            else
                GL.NamedBufferSubData(EBO, IntPtr.Zero, indexSize, indices);

            oldVertexSize = vertexSize;
            oldIndexSize = indexSize;
        }

        public void UpdateMeshData(IntPtr vertices, IntPtr indices, int vertexSize, int indexSize)
        {
            if (vertexSize > oldVertexSize)
                GL.NamedBufferData(VBO, vertexSize, vertices, BufferUsageHint.DynamicDraw);
            else
                GL.NamedBufferSubData(VBO, IntPtr.Zero, vertexSize, vertices);

            if (indexSize > oldIndexSize)
                GL.NamedBufferData(EBO, indexSize, indices, BufferUsageHint.DynamicDraw);
            else
                GL.NamedBufferSubData(EBO, IntPtr.Zero, indexSize, indices);

            oldVertexSize = vertexSize;
            oldIndexSize = indexSize;
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

            GC.SuppressFinalize(this);
        }
    }
}
