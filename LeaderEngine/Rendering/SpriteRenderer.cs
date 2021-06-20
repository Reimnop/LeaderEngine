using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class SpriteRenderer : Component, IRenderer
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SpriteVertex
        {
            public Vector3 Position;
            public Vector2 UV;

            public SpriteVertex(Vector3 position, Vector2 uv)
            {
                Position = position;
                UV = uv;
            }
        }

        private static int meshVAO;
        private static int meshVBO;
        private static int meshEBO;

        private static Shader shader;

        public Texture Texture;
        public Vector4 Color = Vector4.One;

        private CommandBuffer cmd = new CommandBuffer();

        internal static void Init()
        {
            SpriteVertex[] vertices = new SpriteVertex[]
            {
                new SpriteVertex(new Vector3( 0.5f,  0.5f, 0f), new Vector2(1f, 1f)),
                new SpriteVertex(new Vector3( 0.5f, -0.5f, 0f), new Vector2(1f, 0f)),
                new SpriteVertex(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0f, 0f)),
                new SpriteVertex(new Vector3(-0.5f,  0.5f, 0f), new Vector2(0f, 1f))
            };

            uint[] indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            meshVAO = GL.GenVertexArray();
            GL.BindVertexArray(meshVAO);

            meshVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<SpriteVertex>() * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            meshEBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, meshEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

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

            cmd.BindVertexArray(meshVAO);
            cmd.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            Engine.Renderer.QueueCommandsOpaque(cmd);
        }
    }
}
