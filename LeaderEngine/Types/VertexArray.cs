using OpenTK.Graphics.OpenGL4;
using System;

namespace LeaderEngine
{
    public struct VertexAttrib
    {
        public int location;
        public int size;
    }

    public class VertexArray : IDisposable
    {
        private float[] vertices;
        private uint[] indices;

        private int VAO, VBO, EBO;

        private Texture texture;

        public VertexArray(float[] vertices, uint[] indices, VertexAttrib[] attribs)
        {
            this.vertices = vertices;
            this.indices = indices;
            Init(attribs);
        }

        ~VertexArray()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        public Texture GetTexture()
        {
            return texture;
        }

        public void SetTexture(Texture texture)
        {
            this.texture = texture;
        }

        private void Init(VertexAttrib[] attribs)
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            int size = 0;
            foreach (VertexAttrib attrib in attribs)
                size += attrib.size;

            int c = 0;
            for (int i = 0; i < attribs.Length; i++)
            {
                GL.VertexAttribPointer(attribs[i].location, attribs[i].size, VertexAttribPointerType.Float, false, size * sizeof(float), c);
                GL.EnableVertexAttribArray(attribs[i].location);
                c += attribs[i].size * sizeof(float);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public int GetVerticesCount()
        {
            return vertices.Length;
        }

        public int GetIndicesCount()
        {
            return indices.Length;
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
