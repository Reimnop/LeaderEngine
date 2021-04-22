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

        const int gridSize = 100;

        //init grid mesh
        private void Start()
        {
            mesh = new Mesh("grid");

            List<Vertex> vertices = new List<Vertex>();

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.4f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.8f;

                if (i == 0)
                    color = new Vector3(1.0f, 0.0f, 0.0f);

                vertices.Add(new Vertex
                {
                    Position = new Vector3(i, 0.0f, -gridSize),
                    Color = color
                });

                vertices.Add(new Vertex
                {
                    Position = new Vector3(i, 0.0f, gridSize),
                    Color = color
                });
            }

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 color = Vector3.One * 0.4f;

                if (i % 10 == 0)
                    color = Vector3.One * 0.8f;

                if (i == 0)
                    color = new Vector3(0.0f, 1.0f, 0.0f);

                vertices.Add(new Vertex
                {
                    Position = new Vector3(-gridSize, 0.0f, i),
                    Color = color
                });

                vertices.Add(new Vertex
                {
                    Position = new Vector3(gridSize, 0.0f, i),
                    Color = color
                });
            }

            uint[] indices = new uint[vertices.Count];

            for (uint i = 0; i < vertices.Count; i++)
                indices[i] = i;

            mesh.LoadMesh(vertices.ToArray(), indices, PrimitiveType.Lines);

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
                Mesh = mesh,
                Shader = shader,
                Uniforms = uniforms
            });
        }
    }
}
