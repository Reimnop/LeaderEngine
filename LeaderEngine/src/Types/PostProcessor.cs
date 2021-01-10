using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class PostProcessor : IDisposable
    {
        public Shader PPShader = Shader.PostProcessing;

        private int FBO, depthTexture, gColorSpec, gPosition, gNormal;

        private Mesh mesh;

        public PostProcessor(int width, int height)
        {
            Setup(new Vector2i(width, height));
        }

        public PostProcessor(Vector2i vSize)
        {
            Setup(vSize);
        }

        ~PostProcessor()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Setup(Vector2i size)
        {
            {
                FBO = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

                //GBUFFER SETUP

                //color + specular buffer
                gColorSpec = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, gColorSpec);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gColorSpec, 0);

                //position color buffer
                gPosition = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, gPosition);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gPosition, 0);

                //normals color buffer
                gNormal = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, gNormal);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gNormal, 0);

                //END OF GBUFFER SETUP

                depthTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            {
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
            }
        }

        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gColorSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Render()
        {
            mesh.Use();
            PPShader.Use();

            PPShader.SetInt("depthMap", 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            PPShader.SetInt("albedoSpec", 1);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gColorSpec);

            PPShader.SetInt("position", 2);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gPosition);

            PPShader.SetInt("normal", 3);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gNormal);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(FBO);
            GL.DeleteTexture(depthTexture);

            GL.DeleteTexture(gPosition);
            GL.DeleteTexture(gNormal);
            GL.DeleteTexture(gColorSpec);

            mesh.Dispose();
        }
    }
}
