using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class SSAOBlur : IDisposable
    {
        private Shader BlurShader = Shader.SSAOBlur;

        private int FBO, ssaoTexture;

        private Vector2 currentSize;

        private Mesh mesh;

        public int BlurSamples = 4;

        public SSAOBlur(Vector2i vSize)
        {
            Setup(vSize);
        }

        ~SSAOBlur()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Setup(Vector2i size)
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            ssaoTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ssaoTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ssaoTexture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            #region QUAD_SETUP
            float[] vertices =
            {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
                 1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
                 1.0f, -1.0f, 0.0f, 1.0f, 0.0f
            };

            uint[] indices =
            {
                0, 1, 3,
                1, 2, 3
            };

            mesh = new Mesh("PPMesh", vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 },
                new VertexAttrib { location = 1, size = 2 }
            });
            #endregion
        }

        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Resize(int width, int height)
        {
            Update(new Vector2i(width, height));
            currentSize = new Vector2(width, height);
        }

        private void Update(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, ssaoTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Render()
        {
            mesh.Use();
            BlurShader.Use();

            BlurShader.SetInt("ssaoTexture", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ssaoTexture);

            BlurShader.SetVector2("vSize", currentSize);
            BlurShader.SetInt("blurSamples", BlurSamples);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(FBO);

            GL.DeleteTexture(ssaoTexture);

            mesh.Dispose();
        }
    }
}
