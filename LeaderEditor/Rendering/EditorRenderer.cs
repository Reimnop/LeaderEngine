using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEditor
{
    public class EditorRenderer : ForwardRenderer
    {
        public int FramebufferTexture => framebufferTexture;

        private ImGuiController ImGuiController = new ImGuiController();

        private Vector2i lastViewportSize;

        private int framebufferTexture;
        private int framebuffer;

        public override void Init()
        {
            ImGuiController.Init();

            //init FBO
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            //init shadowmap
            framebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //bind texture to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebufferTexture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            base.Init();
        }

        public override void Update()
        {
            base.Update();

            ImGuiController.Update(Time.DeltaTime);

            if (ViewportSize == lastViewportSize || ViewportSize.X * ViewportSize.Y == 0)
                return;

            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, ViewportSize.X, ViewportSize.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            lastViewportSize = ViewportSize;
        }

        public override void Render()
        {
            //capture whatever is rendered
            FramebufferManager.PushFramebuffer(framebuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            base.Render();

            FramebufferManager.PopFramebuffer();

            //restore viewport
            GL.Viewport(0, 0, Engine.MainWindow.ClientSize.X, Engine.MainWindow.ClientSize.Y);

            ImGuiController.RenderImGui();
        }
    }
}
