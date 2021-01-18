using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class PostProcessor : IDisposable
    {
        public Shader TransparentComposite = Shader.TransparentComposite;
        public Shader PPShader = Shader.PostProcessing;

        private int colorTexture, accumulation, revealage, depthTexture;
        private int ppColorTexture;

        private Framebuffer framebuffer;
        private Framebuffer ppFramebuffer;

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
            framebuffer = new Framebuffer(size.X, size.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = true,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToEdge },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToEdge }
                    }
                },
                new Attachment
                {
                    Draw = true,
                    PixelInternalFormat = PixelInternalFormat.Rgba16f,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment1,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToEdge },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToEdge }
                    }
                },
                new Attachment
                {
                    Draw = true,
                    PixelInternalFormat = PixelInternalFormat.R16f,
                    PixelFormat = PixelFormat.Red,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment2,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToEdge },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToEdge }
                    }
                },
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.DepthComponent,
                    PixelFormat = PixelFormat.DepthComponent,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.DepthAttachment,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Nearest },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToEdge },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToEdge }
                    }
                }
            });

            colorTexture = framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0);
            accumulation = framebuffer.GetTexture(FramebufferAttachment.ColorAttachment1);
            revealage = framebuffer.GetTexture(FramebufferAttachment.ColorAttachment2);
            depthTexture = framebuffer.GetTexture(FramebufferAttachment.DepthAttachment);

            ppFramebuffer = new Framebuffer(size.X, size.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToEdge },
                        new TextureParam { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToEdge }
                    }
                }
            });

            ppColorTexture = ppFramebuffer.GetTexture(FramebufferAttachment.ColorAttachment0);

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

        float[] clearAccumulation = { 0.0f, 0.0f, 0.0f, 1.0f };
        float[] clearRevealage = { 1.0f };

        public void Begin()
        {
            framebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ClearBuffer(ClearBuffer.Color, 1, clearAccumulation);
            GL.ClearBuffer(ClearBuffer.Color, 2, clearRevealage);
        }

        public void End()
        {
            framebuffer.End();
        }

        public void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
            ppFramebuffer.Resize(width, height);
        }

        public void Render()
        {
            GL.DepthFunc(DepthFunction.Always);

            ppFramebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            RenderStage1();
            ppFramebuffer.End();

            RenderStage2();

            GL.DepthFunc(DepthFunction.Less);
        }

        private void RenderStage1()
        {
            mesh.Use();
            TransparentComposite.Use();

            TransparentComposite.SetInt("colorTexture", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);

            TransparentComposite.SetInt("accumTex", 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, accumulation);

            TransparentComposite.SetInt("revealTex", 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, revealage);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        private void RenderStage2()
        {
            mesh.Use();
            PPShader.Use();

            PPShader.SetInt("colorTexture", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ppColorTexture);

            PPShader.SetInt("depthTexture", 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            framebuffer.Dispose();
            mesh.Dispose();
        }
    }
}
