using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class DeferredProcessor : IDisposable
    {
        public Shader DefShader = Shader.Deferred;

        private int FBO, blurredSSAO, gAlbedoSpec, gPosition, gNormal, depthTexture;

        public Vector3 AmbientColor = new Vector3(0.5f);
        public Vector3 LightColor = new Vector3(1.0f);

        private Mesh mesh;

        public DeferredProcessor(int width, int height, int gAlbedoSpec, int gPosition, int gNormal, int depthTexture)
        {
            Setup(new Vector2i(width, height));

            this.gAlbedoSpec = gAlbedoSpec;
            this.gPosition = gPosition;
            this.gNormal = gNormal;
            this.depthTexture = depthTexture;
        }

        public DeferredProcessor(Vector2i vSize, int gAlbedoSpec, int gPosition, int gNormal, int depthTexture)
        {
            Setup(vSize);

            this.gAlbedoSpec = gAlbedoSpec;
            this.gPosition = gPosition;
            this.gNormal = gNormal;
            this.depthTexture = depthTexture;
        }

        ~DeferredProcessor()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Setup(Vector2i size)
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            blurredSSAO = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, blurredSSAO);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, blurredSSAO, 0);

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
        }

        private void Update(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, blurredSSAO);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Render()
        {
            mesh.Use();
            DefShader.Use();

            DefShader.SetInt("blurredSSAO", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, blurredSSAO);

            DefShader.SetInt("gAlbedoSpec", 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);

            DefShader.SetInt("gNormal", 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gNormal);

            DefShader.SetInt("gPosition", 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, gPosition);

            DefShader.SetInt("depthTexture", 4);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            LightingController.LightingShaderSetup(DefShader);

            DefShader.SetVector3("ambientColor", AmbientColor);
            DefShader.SetVector3("lightColor", LightColor);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(FBO);
            GL.DeleteTexture(blurredSSAO);
            mesh.Dispose();
        }
    }
}
