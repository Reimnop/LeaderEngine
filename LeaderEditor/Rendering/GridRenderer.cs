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

        private UniformData uniforms = new UniformData();

        const int gridSize = 80;

        //init grid mesh
        private void Start()
        {
            mesh = new Mesh("grid");
            mesh.Unlist();

            List<Vector3> vertexPositions = new List<Vector3>();
            List<VertexData> vertices = new List<VertexData>();

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.2f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.6f;

                if (i == 0)
                    color = new Vector3(0.0f, 0.0f, 1.0f);

                vertexPositions.Add(new Vector3(i, 0.0f, -gridSize));
                vertexPositions.Add(new Vector3(i, 0.0f, gridSize));

                vertices.Add(new VertexData
                {
                    Color = color
                });

                vertices.Add(new VertexData
                {
                    Color = color
                });
            }

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.2f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.6f;

                if (i == 0)
                    color = new Vector3(1.0f, 0.0f, 0.0f);

                vertexPositions.Add(new Vector3(-gridSize, 0.0f, i));
                vertexPositions.Add(new Vector3(gridSize, 0.0f, i));

                vertices.Add(new VertexData
                {
                    Color = color
                });

                vertices.Add(new VertexData
                {
                    Color = color
                });
            }

            uint[] indices = new uint[vertices.Count];

            for (uint i = 0; i < vertices.Count; i++)
                indices[i] = i;

            mesh.LoadMesh(vertexPositions.ToArray(), indices, PrimitiveType.Lines);
            mesh.SetPerVertexData(vertices.ToArray());

            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                view * projection));

            renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                SourceEntity = BaseEntity,
                Mesh = mesh,
                Shader = shader,
                Uniforms = uniforms
            });
        }
    }
}
