using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace LeaderEngine
{
    public class SpriteRenderer : Component, IRenderer
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SpriteVertexData
        {
            [VertexAttrib(VertexAttribPointerType.Float, 1, 2, false)]
            public Vector2 UV;
        }

        private static Mesh mesh;
        private static Shader shader;

        public Texture Texture;
        public Vector4 Color = Vector4.One;

        private UniformData uniforms = new UniformData();

        internal static void Init()
        {
            mesh = new Mesh("sprite-mesh");

            Vector3[] vertices = new Vector3[]
            {
                new Vector3( 0.5f,  0.5f, 0.0f),
                new Vector3( 0.5f, -0.5f, 0.0f),
                new Vector3(-0.5f, -0.5f, 0.0f),
                new Vector3(-0.5f,  0.5f, 0.0f)
            };

            uint[] indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            mesh.LoadMesh(vertices, indices);
            mesh.SetPerVertexData(new SpriteVertexData[]
            {
                new SpriteVertexData { UV = new Vector2(1.0f, 1.0f) },
                new SpriteVertexData { UV = new Vector2(1.0f, 0.0f) },
                new SpriteVertexData { UV = new Vector2(0.0f, 0.0f) },
                new SpriteVertexData { UV = new Vector2(0.0f, 1.0f) }
            });

            mesh.Unlist();

            string dir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/");

            shader = Shader.FromSourceFile("sprite-shader",
                Path.Combine(dir, "sprite.vert"),
                Path.Combine(dir, "sprite.frag"));
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            if (Texture == null)
                return;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * view * projection));

            uniforms.SetUniform("color", new Uniform(UniformType.Vector4,
                Color));

            uniforms.SetUniform("texture0", new Uniform(UniformType.Texture2D, new TextureData
            {
                TextureUnit = TextureUnit.Texture0,
                TextureHandle = Texture.GetHandle()
            }));

            Engine.Renderer.PushDrawData(DrawType.Transparent, new GLDrawData
            {
                SourceEntity = BaseEntity,
                Mesh = mesh,
                Shader = shader,
                Uniforms = uniforms
            });
        }
    }
}
