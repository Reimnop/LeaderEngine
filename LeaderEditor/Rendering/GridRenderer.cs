using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LeaderEditor
{
    internal class GridRenderer : Component, IRenderer
    {
        private Mesh mesh;
        private Shader shader = DefaultShaders.SingleColor;

        private CommandBuffer cmd = new CommandBuffer();

        const int gridSize = 80;

        //init grid mesh
        private void Start()
        {
            mesh = new Mesh("grid");
            mesh.Unlist();

            List<Vector3> vertices = new List<Vector3>();
            List<VertexData> perVertexData = new List<VertexData>();

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.03f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.3f;

                if (i == 0)
                    color = new Vector3(0f, 0f, 1f);

                vertices.Add(new Vector3(i, 0f, -gridSize));
                vertices.Add(new Vector3(i, 0f, gridSize));

                perVertexData.Add(new VertexData
                {
                    Color = color
                });

                perVertexData.Add(new VertexData
                {
                    Color = color
                });
            }

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.03f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.3f;

                if (i == 0)
                    color = new Vector3(1f, 0f, 0f);

                vertices.Add(new Vector3(-gridSize, 0f, i));
                vertices.Add(new Vector3(gridSize, 0f, i));

                perVertexData.Add(new VertexData
                {
                    Color = color
                });

                perVertexData.Add(new VertexData
                {
                    Color = color
                });
            }

            uint[] indices = new uint[perVertexData.Count];

            for (uint i = 0; i < perVertexData.Count; i++)
                indices[i] = i;

            mesh.LoadMesh(vertices.ToArray(), indices, PrimitiveType.Lines);
            mesh.SetPerVertexData(perVertexData.ToArray());
        }

        public void Render(in RenderData renderData)
        {
            cmd.Clear();

            cmd.BindShader(shader);
            cmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);

            cmd.BindMesh(mesh);
            cmd.DrawMesh(mesh);

            Engine.Renderer.QueueCommands(cmd);
        }
    }
}
