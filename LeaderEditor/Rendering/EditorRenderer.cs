using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using LeaderEngine;

namespace LeaderEditor
{
    public class EditorRenderer : ForwardRenderer
    {
        private ImGuiController ImGuiController { get; } = new ImGuiController();

        public Framebuffer Framebuffer;

        public Vector2i ViewportSize;

        public override void Init()
        {
            Framebuffer = new Framebuffer("ViewportFBO", 1600, 900, new Attachment[]
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

            ImGuiController.Init();

            base.Init();
        }

        public override void Update()
        {
            base.Update();

            if (ViewportSize.X * ViewportSize.Y > 0)
                Framebuffer.Resize(ViewportSize.X, ViewportSize.Y);

            ImGuiController.Update(Time.DeltaTime);
        }

        public override void Render()
        {
            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            //begin fbo
            Framebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            base.Render();
            Framebuffer.End();

            //restore viewport
            GL.Viewport(0, 0, Engine.MainWindow.ClientSize.X, Engine.MainWindow.ClientSize.Y);

            ImGuiController.RenderImGui();
        }
    }
}
