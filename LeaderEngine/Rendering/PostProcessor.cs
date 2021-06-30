using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class PostProcessor
    {
        private int FBO;
        private int colorTexture;
        private int depthBuffer;

        private int postProcessFBO;
        private int postProcessColorTexture;
        private int postProcessReadTexture;

        private Vector2i currentSize = Vector2i.One;

        private PostProcessingEffect[] postProcessingEffects;

        public PostProcessor(params PostProcessingEffect[] effects)
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

            depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, 1, 1);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //init post process resources
            postProcessFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, postProcessFBO);

            postProcessColorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, postProcessColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, postProcessColorTexture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            postProcessReadTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, postProcessReadTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            foreach (PostProcessingEffect effect in effects)
                effect.Init();

            postProcessingEffects = effects;
        }

        public void Resize(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, size.X, size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.BindTexture(TextureTarget.Texture2D, postProcessColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindTexture(TextureTarget.Texture2D, postProcessReadTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            foreach (PostProcessingEffect effect in postProcessingEffects)
                effect.Resize(size);

            currentSize = size;
        }

        public void Begin()
        {
            FramebufferManager.PushFramebuffer(FBO);
        }

        public void End()
        {
            FramebufferManager.PopFramebuffer();
        }

        public void Render()
        {
            int texture = colorTexture;

            for (int i = 0; i < postProcessingEffects.Length - 1; i++)
            {
                FramebufferManager.PushFramebuffer(postProcessFBO);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                postProcessingEffects[i].Render(texture);

                GL.BindTexture(TextureTarget.Texture2D, postProcessReadTexture);
                GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, currentSize.X, currentSize.Y);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                FramebufferManager.PopFramebuffer();

                texture = postProcessReadTexture;
            }

            postProcessingEffects[postProcessingEffects.Length - 1].Render(texture);
        }
    }
}
