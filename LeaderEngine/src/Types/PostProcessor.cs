using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class PostProcessor : IDisposable
    {
        public Shader PPShader = Shader.PostProcessing;

        private Framebuffer framebuffer;

        private VertexArray vertexArray;

        public PostProcessor(int width, int height)
        {
            framebuffer = new Framebuffer(width, height);

            Setup();
        }

        public PostProcessor(Vector2i vSize)
        {
            framebuffer = new Framebuffer(vSize.X, vSize.Y);

            Setup();
        }

        ~PostProcessor()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Setup()
        {
            float[] vertices =
            {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
                 1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
                 1.0f, -1.0f, 0.0f, 1.0f, 0.0f
            };

            uint[] indices =
            {
                0, 1, 3,
                1, 2, 3
            };

            vertexArray = new VertexArray(vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 },
                new VertexAttrib { location = 1, size = 2 }
            });
        }

        public void Begin()
        {
            framebuffer.Begin();
        }

        public void End()
        {
            framebuffer.End();
        }

        public void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
        }

        public void Render()
        {
            vertexArray.Use();
            PPShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, framebuffer.GetColorTexture());

            GL.DrawElements(PrimitiveType.Triangles, vertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            vertexArray.Dispose();
        }
    }
}
