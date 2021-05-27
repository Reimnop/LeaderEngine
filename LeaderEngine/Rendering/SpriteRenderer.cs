using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Runtime.InteropServices;

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

        private CommandBuffer cmd = new CommandBuffer();

        internal static void Init()
        {
            mesh = new Mesh("sprite-mesh");

            Vector3[] vertices = new Vector3[]
            {
                new Vector3( 0.5f,  0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f)
            };

            uint[] indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            mesh.LoadMesh(vertices, indices);
            mesh.SetPerVertexData(new SpriteVertexData[]
            {
                new SpriteVertexData { UV = new Vector2(1f, 1f) },
                new SpriteVertexData { UV = new Vector2(1f, 0f) },
                new SpriteVertexData { UV = new Vector2(0f, 0f) },
                new SpriteVertexData { UV = new Vector2(0f, 1f) }
            });

            mesh.Unlist();

            string dir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/");

            shader = Shader.FromSourceFile("sprite-shader",
                Path.Combine(dir, "sprite.vert"),
                Path.Combine(dir, "sprite.frag"));
        }

        public void Render(in RenderData renderData)
        {
            if (Texture == null)
                return;

            cmd.Clear();

            cmd.BindShader(shader);
            cmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);
            cmd.SetUniformVector4(shader, "color", Color);

            cmd.BindTexture(TextureUnit.Texture0, Texture);

            cmd.BindMesh(mesh);
            cmd.DrawMesh(mesh);

            Engine.Renderer.QueueCommandsOpaque(cmd);
        }
    }
}
