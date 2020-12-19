using System;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Framebuffer : IDisposable
    {
        private int handle;
        private int colorTexture, depthTexture;

        public Framebuffer(int width, int height)
        {
            handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

            colorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero); 
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, depthTexture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        ~Framebuffer()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        public void Resize(int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public int GetHandle()
        {
            return handle;
        }

        public int GetColorTexture()
        {
            return colorTexture;
        }

        public int GetDepthTexture()
        {
            return depthTexture;
        }

        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(handle);
            GL.DeleteTexture(depthTexture);
            GL.DeleteTexture(colorTexture);

            GC.SuppressFinalize(this);
        }
    }
}
