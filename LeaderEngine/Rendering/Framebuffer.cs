using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeaderEngine
{
    public struct TextureParamInt
    {
        public TextureParameterName ParamName;
        public int Param;
    }

    public struct TextureParamIntArr
    {
        public TextureParameterName ParamName;
        public int[] Params;
    }

    public struct TextureParamFloat
    {
        public TextureParameterName ParamName;
        public float Param;
    }

    public struct TextureParamFloatArr
    {
        public TextureParameterName ParamName;
        public float[] Params;
    }

    public struct Attachment
    {
        public PixelInternalFormat PixelInternalFormat;
        public PixelFormat PixelFormat;
        public PixelType PixelType;
        public TextureParamInt[] TextureParamsInt;
        public TextureParamIntArr[] TextureParamsIntArr;
        public TextureParamFloat[] TextureParamsFloat;
        public TextureParamFloatArr[] TextureParamsFloatArr;
        public FramebufferAttachment FramebufferAttachment;
        public bool Draw;
    }

    public sealed class Framebuffer : IDisposable
    {
        private static int currentlyBoundHandle;

        private int handle, oldBoundHandle;

        private Dictionary<Attachment, int> framebufferAttachments = new Dictionary<Attachment, int>();

        private int oldWidth, oldHeight;

        public readonly string Name;

        public Framebuffer(string name, int width, int height, Attachment[] attachments)
        {
            Name = name;

            oldWidth = width;
            oldHeight = height;

            handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

            foreach (var att in attachments)
            {
                int tex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.TexImage2D(TextureTarget.Texture2D, 0, att.PixelInternalFormat, width, height, 0, att.PixelFormat, att.PixelType, IntPtr.Zero);

                if (att.TextureParamsInt != null)
                    foreach (var param in att.TextureParamsInt)
                        GL.TexParameter(TextureTarget.Texture2D, param.ParamName, param.Param);

                if (att.TextureParamsIntArr != null)
                    foreach (var param in att.TextureParamsIntArr)
                        GL.TexParameter(TextureTarget.Texture2D, param.ParamName, param.Params);

                if (att.TextureParamsFloat != null)
                    foreach (var param in att.TextureParamsFloat)
                        GL.TexParameter(TextureTarget.Texture2D, param.ParamName, param.Param);

                if (att.TextureParamsFloatArr != null)
                    foreach (var param in att.TextureParamsFloatArr)
                        GL.TexParameter(TextureTarget.Texture2D, param.ParamName, param.Params);

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
                Logger.LogError("FRAMEBUFFER: " + Name + ": " + fboStatus);

            GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, handle, name.Length, name);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Resize(int width, int height)
        {
            if (width == oldWidth && height == oldHeight)
                return;

            oldWidth = width;
            oldHeight = height;

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
            oldBoundHandle = currentlyBoundHandle;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
            currentlyBoundHandle = handle;
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, oldBoundHandle);
            currentlyBoundHandle = oldBoundHandle;
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(handle);

            foreach (var att in framebufferAttachments)
                GL.DeleteTexture(att.Value);
        }
    }
}
