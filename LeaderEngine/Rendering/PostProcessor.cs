using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public class PostProcessor
    {
        private int FBO;
        private int colorTexture, gAlbedoTexture, gPositionTexture, gNormalTexture, depthTexture;

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            gAlbedoTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 1, 1, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            gPositionTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPositionTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 1, 1, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            gNormalTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormalTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 1, 1, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 1, 1, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gAlbedoTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gPositionTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, gNormalTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);

            GL.DrawBuffers(4, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 });

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

            GL.BindTexture(TextureTarget.Texture2D, gAlbedoTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, size.X, size.Y, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gPositionTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, size.X, size.Y, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gNormalTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, size.X, size.Y, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, postProcessColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

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

        public void Render(Matrix4 view, Matrix4 projection)
        {
            PostProcessingData postProcessingData = new PostProcessingData
            {
                View = view,
                Projection = projection,
                ColorTexture = colorTexture,
                DepthTexture = depthTexture,
                AlbedoTexture = gAlbedoTexture,
                PositionTexture = gPositionTexture,
                NormalTexture = gNormalTexture
            };

            for (int i = 0; i < postProcessingEffects.Length - 1; i++)
            {
                FramebufferManager.PushFramebuffer(postProcessFBO);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                postProcessingEffects[i].Render(postProcessingData);

                GL.BindTexture(TextureTarget.Texture2D, postProcessReadTexture);
                GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, currentSize.X, currentSize.Y);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                FramebufferManager.PopFramebuffer();

                postProcessingData.ColorTexture = postProcessReadTexture;
            }

            postProcessingEffects[postProcessingEffects.Length - 1].Render(postProcessingData);
        }
    }
}
