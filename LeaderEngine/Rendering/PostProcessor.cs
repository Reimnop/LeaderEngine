using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class PostProcessor
    {
        public readonly Shader[] shaders;

        private Vector2i framebufferSize = Vector2i.One;

        private Mesh mesh;

        private Framebuffer framebuffer;

        private Framebuffer stageReadFramebuffer;
        private Framebuffer stageDrawFramebuffer;

        public PostProcessor(Shader[] shaders)
        {
            this.shaders = shaders;

            mesh = new Mesh("post-process-quad");
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
                    PixelInternalFormat = PixelInternalFormat.Rgba,
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
            for (int i = 0; i < shaders.Length; i++)
            {
                Shader shader = shaders[i];

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

                if (i != shaders.Length - 1)
                {
                    stageDrawFramebuffer.Begin();
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                }

                GL.DrawElements(PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);

                if (i != shaders.Length - 1)
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
