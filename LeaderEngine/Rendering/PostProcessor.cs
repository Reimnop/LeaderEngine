using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class PostProcessor
    {
        public readonly List<Shader> Shaders = new List<Shader>();

        private Vector2i framebufferSize = Vector2i.One;

        private Mesh mesh;

        private Framebuffer framebuffer;

        private Framebuffer stageReadFramebuffer;
        private Framebuffer stageDrawFramebuffer;

        public PostProcessor()
        {
            mesh = new Mesh("post-process-quad");
            mesh.Reserve();

            mesh.LoadMesh(new Vertex[]
            {
                new Vertex { Position = new Vector3(1.0f, 1.0f, 0.0f), UV = new Vector2(1.0f, 1.0f) },
                new Vertex { Position = new Vector3(1.0f, -1.0f, 0.0f), UV = new Vector2(1.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, -1.0f, 0.0f), UV = new Vector2(0.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, 1.0f, 0.0f), UV = new Vector2(0.0f, 1.0f) }
            },
            new uint[]
            {
                0, 1, 3,
                1, 2, 3
            });

            framebuffer = new Framebuffer("post-process-fbo", framebufferSize.X, framebufferSize.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.SrgbAlpha,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                },
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.DepthComponent,
                    PixelFormat = PixelFormat.DepthComponent,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.DepthAttachment,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Nearest },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest }
                    }
                }
            });
            stageReadFramebuffer = new Framebuffer("stage-post-process-fbo", framebufferSize.X, framebufferSize.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                }
            });
            stageDrawFramebuffer = new Framebuffer("stage-post-process-fbo", framebufferSize.X, framebufferSize.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                }
            });
        }

        public void Resize(Vector2i newSize)
        {
            if (newSize == framebufferSize)
                return;

            framebuffer.Resize(newSize.X, newSize.Y);
            stageReadFramebuffer.Resize(newSize.X, newSize.Y);
            stageDrawFramebuffer.Resize(newSize.X, newSize.Y);

            framebufferSize = newSize;
        }

        public void Begin()
        {
            framebuffer.Begin();
        }

        public void End()
        {
            framebuffer.End();
        }

        public void Render()
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                Shader shader = Shaders[i];

                mesh.Use();
                shader.Use();

                shader.SetInt("sourceTexture", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0));

                shader.SetInt("sourceDepth", 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, framebuffer.GetTexture(FramebufferAttachment.DepthAttachment));

                if (i != 0)
                {
                    shader.SetInt("lastStageTexture", 2);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, stageReadFramebuffer.GetTexture(FramebufferAttachment.ColorAttachment0));
                }

                if (i != Shaders.Count - 1)
                {
                    stageDrawFramebuffer.Begin();
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                }

                GL.DrawElements(PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);

                if (i != Shaders.Count - 1)
                {
                    stageDrawFramebuffer.End();
                }

                {
                    Framebuffer temp = stageDrawFramebuffer;
                    stageDrawFramebuffer = stageReadFramebuffer;
                    stageReadFramebuffer = temp;
                }
            }
        }
    }
}
