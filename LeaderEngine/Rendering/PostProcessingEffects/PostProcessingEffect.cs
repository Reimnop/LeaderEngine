using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public abstract class PostProcessingEffect
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PostProcessorVertex
        {
            public Vector3 Position;
            public Vector2 UV;

            public PostProcessorVertex(Vector3 position, Vector2 uv)
            {
                Position = position;
                UV = uv;
            }
        }

        private static int _meshVAO;
        private static int _meshVBO;

        public static void InitResources()
        {
            PostProcessorVertex[] vertices = new PostProcessorVertex[]
            {
                new PostProcessorVertex(new Vector3(1f, 1f, 0f), new Vector2(1f, 1f)),
                new PostProcessorVertex(new Vector3(1f, -1f, 0f), new Vector2(1f, 0f)),
                new PostProcessorVertex(new Vector3(-1f, 1f, 0f), new Vector2(0f, 1f)),
                new PostProcessorVertex(new Vector3(1f, -1f, 0f), new Vector2(1f, 0f)),
                new PostProcessorVertex(new Vector3(-1f, -1f, 0f), new Vector2(0f, 0f)),
                new PostProcessorVertex(new Vector3(-1f, 1f, 0f), new Vector2(0f, 1f))
            };

            int vertSize = Unsafe.SizeOf<PostProcessorVertex>();

            _meshVAO = GL.GenVertexArray();
            GL.BindVertexArray(_meshVAO);

            _meshVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _meshVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSize * 6, vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertSize, Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected void DrawQuad()
        {
            GL.BindVertexArray(_meshVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public abstract void Init();
        public abstract void Resize(Vector2i size);
        public abstract void Render(PostProcessingData postProcessingData);
    }
}
