using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace LeaderEditor
{
    internal class GridRenderer : Renderer
    {
        private Mesh mesh;
        private Shader shader = DefaultShaders.SingleColor;

        private UniformData uniforms = new UniformData();

        //init grid mesh
        private void Start()
        {
            mesh = new Mesh("grid");

            List<Vertex> vertices = new List<Vertex>();

            for (int i = -10; i <= 10; i++)
            {
                vertices.Add(new Vertex
                {
                    Position = new Vector3(i, 0.0f, -10.0f),
                    Color = i == 0 ? new Vector3(1.0f, 0.0f, 0.0f) : Vector3.One * 0.75f
                });

                vertices.Add(new Vertex
                {
                    Position = new Vector3(i, 0.0f, 10.0f),
                    Color = i == 0 ? new Vector3(1.0f, 0.0f, 0.0f) : Vector3.One * 0.75f
                });
            }

            for (int i = -10; i <= 10; i++)
            {
                vertices.Add(new Vertex
                {
                    Position = new Vector3(-10.0f, 0.0f, i),
                    Color = i == 0 ? new Vector3(0.0f, 1.0f, 0.0f) : Vector3.One * 0.75f
                });

                vertices.Add(new Vertex
                {
                    Position = new Vector3(10.0f, 0.0f, i),
                    Color = i == 0 ? new Vector3(0.0f, 1.0f, 0.0f) : Vector3.One * 0.75f
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

        public override void Render()
        {
            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                renderer.WorldView
                * renderer.WorldProjection));

            renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = mesh,
                Shader = shader,
                Uniforms = uniforms
            });
        }
    }
}
