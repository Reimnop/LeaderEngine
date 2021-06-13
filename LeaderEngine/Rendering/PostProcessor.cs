using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Runtime.CompilerServices;
using System.IO;

namespace LeaderEngine
{
    public class PostProcessor
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

        private int FBO;
        private int colorTexture, depthTexture;

        private int VAO;
        private int VBO;

        private Shader shader;

        public PostProcessor()
        {
            //init fbo
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            //init textures
            colorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 1, 1, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //init mesh
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

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSize * 6, vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertSize, Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            shader = Shader.FromSourceFile("post-process",
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess/post-process.vert"),
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess/hdr.frag"));
        }

        public void Resize(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Render()
        {
            GL.UseProgram(shader.Handle);

            GL.BindVertexArray(VAO);

            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.ActiveTexture(TextureUnit.Texture2);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}
