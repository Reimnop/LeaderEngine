using OpenTK.Graphics.OpenGL4;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LeaderEngine
{
    public struct TextureParam
    {
        public TextureParameterName ParamName;
        public int Param;
    }

    public struct Attachment
    {
        public PixelInternalFormat PixelInternalFormat;
        public PixelFormat PixelFormat;
        public PixelType PixelType;
        public TextureParam[] TextureParams;
        public FramebufferAttachment FramebufferAttachment;
        public bool Draw;
    }

    public class Framebuffer : IDisposable
    {
        private int handle, currentlyBoundHandle;

        private Dictionary<Attachment, int> framebufferAttachments = new Dictionary<Attachment, int>();

        public Framebuffer(int width, int height, Attachment[] attachments)
        {
            handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

            foreach (var att in attachments)
            {
                int tex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.TexImage2D(TextureTarget.Texture2D, 0, att.PixelInternalFormat, width, height, 0, att.PixelFormat, att.PixelType, IntPtr.Zero);

                foreach (var param in att.TextureParams)
                    GL.TexParameter(TextureTarget.Texture2D, param.ParamName, param.Param);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, att.FramebufferAttachment, TextureTarget.Texture2D, tex, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                framebufferAttachments.Add(att, tex);
            }

            DrawBuffersEnum[] drawBuffersEnums = attachments
                .Where(x => x.Draw)
                .Select(x => (DrawBuffersEnum)x.FramebufferAttachment)
                .ToArray();

            if (drawBuffersEnums.Length > 0)
                GL.DrawBuffers(drawBuffersEnums.Length, drawBuffersEnums);

            var fboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (fboStatus != FramebufferErrorCode.FramebufferComplete)
                Logger.LogError("FRAMEBUFFER: " + fboStatus);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        ~Framebuffer()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        public void Resize(int width, int height)
        {
            foreach (var att in framebufferAttachments)
            {
                GL.BindTexture(TextureTarget.Texture2D, att.Value);
                GL.TexImage2D(TextureTarget.Texture2D, 0, att.Key.PixelInternalFormat, width, height, 0, att.Key.PixelFormat, att.Key.PixelType, IntPtr.Zero);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public int GetTexture(FramebufferAttachment framebufferAttachment)
        {
            foreach (var att in framebufferAttachments)
                if (att.Key.FramebufferAttachment == framebufferAttachment)
                    return att.Value;

            throw new Exception($"Attachment {framebufferAttachment} not found!");
        }

        public int GetHandle()
        {
            return handle;
        }

        public void Begin()
        {
            currentlyBoundHandle = GL.GetInteger(GetPName.FramebufferBinding);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentlyBoundHandle);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(handle);

            foreach (var att in framebufferAttachments)
                GL.DeleteTexture(att.Value);

            GC.SuppressFinalize(this);
        }
    }
}
