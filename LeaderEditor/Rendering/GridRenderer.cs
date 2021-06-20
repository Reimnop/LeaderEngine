using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEditor
{
    internal class GridRenderer : Component, IRenderer
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct GridVertex
        {
            public Vector3 Position;
            public Vector3 Color;

            public GridVertex(Vector3 position, Vector3 color)
            {
                Position = position;
                Color = color;
            }
        }

        private int VAO, VBO;
        private Shader shader = DefaultShaders.SingleColor;

        private CommandBuffer cmd = new CommandBuffer();
        private const int gridSize = 80;
        private const int vertCount = gridSize * 8 + 4;

        //init grid mesh
        private void Start()
        {
            #region GenGridMesh
            GridVertex[] vertices = new GridVertex[vertCount];

            int offset = 0;

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.03f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.3f;

                if (i == 0)
                    color = new Vector3(0f, 0f, 1f);

                vertices[offset + 0] = new GridVertex(new Vector3(i, 0f, -gridSize), color);
                vertices[offset + 1] = new GridVertex(new Vector3(i, 0f,  gridSize), color);

                offset += 2;
            }

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.03f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.3f;

                if (i == 0)
                    color = new Vector3(1f, 0f, 0f);

                vertices[offset + 0] = new GridVertex(new Vector3(-gridSize, 0f, i), color);
                vertices[offset + 1] = new GridVertex(new Vector3( gridSize, 0f, i), color);

                offset += 2;
            }
            #endregion

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Unsafe.SizeOf<GridVertex>(), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<GridVertex>(), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<GridVertex>(), Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Render(in RenderData renderData)
        {
            cmd.Clear();

            cmd.BindShader(shader);
            cmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);

            cmd.BindVertexArray(VAO);
            cmd.DrawArrays(PrimitiveType.Lines, 0, vertCount);

            Engine.Renderer.QueueCommandsOpaque(cmd);
        }
    }
}
