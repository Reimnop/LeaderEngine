using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    public class BloomEffect : PostProcessingEffect
    {
        private int filter, thresholdLoc;
        private int horizontalBlur, sizeLocH, sigmaLocH;
        private int verticalBlur, sizeLocV, sigmaLocV;
        private int overlay, sourceLoc, bloomLoc;

        private const int passCount = 3;

        private int[] fbos = new int[passCount];
        private int[] textures = new int[passCount];

        public float Threshold = 0.8f;

        public int Size = 12;
        public float Sigma = 4f;

        public override void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            filter = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "bloom-filter.frag"));

            thresholdLoc = GL.GetUniformLocation(filter, "threshold");

            horizontalBlur = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "bloom-hblur.frag"));

            sizeLocH = GL.GetUniformLocation(horizontalBlur, "size");
            sigmaLocH = GL.GetUniformLocation(horizontalBlur, "sigma");

            verticalBlur = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "bloom-vblur.frag"));

            sizeLocV = GL.GetUniformLocation(verticalBlur, "size");
            sigmaLocV = GL.GetUniformLocation(verticalBlur, "sigma");

            overlay = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "bloom-overlay.frag"));

            sourceLoc = GL.GetUniformLocation(overlay, "sourceTexture");
            bloomLoc = GL.GetUniformLocation(overlay, "bloomTexture");

            //init fbos
            for (int i = 0; i < passCount; i++)
            {
                int fbo = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                fbos[i] = fbo;
                textures[i] = texture;
            }
        }

        public override void Resize(Vector2i size)
        {
            for (int i = 0; i < passCount; i++)
            {
                GL.BindTexture(TextureTarget.Texture2D, textures[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public override void Render(PostProcessingData postProcessingData)
        {
            //extract bright pixels
            GL.UseProgram(filter);

            GL.Uniform1(thresholdLoc, Threshold);
            GL.BindTextureUnit(0, postProcessingData.ColorTexture);

            FramebufferManager.PushFramebuffer(fbos[0]);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad();
            FramebufferManager.PopFramebuffer();

            //horizontal blur
            GL.UseProgram(horizontalBlur);
            GL.BindTextureUnit(0, textures[0]);

            GL.Uniform1(sizeLocH, Size);
            GL.Uniform1(sigmaLocH, Sigma);

            FramebufferManager.PushFramebuffer(fbos[1]);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad();
            FramebufferManager.PopFramebuffer();

            //vertical blur
            GL.UseProgram(verticalBlur);
            GL.BindTextureUnit(0, textures[1]);

            GL.Uniform1(sizeLocV, Size);
            GL.Uniform1(sigmaLocV, Sigma);

            FramebufferManager.PushFramebuffer(fbos[2]);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad();
            FramebufferManager.PopFramebuffer();

            //overlay
            GL.UseProgram(overlay);

            GL.Uniform1(sourceLoc, 0);
            GL.Uniform1(bloomLoc, 1);

            GL.BindTextureUnit(0, postProcessingData.ColorTexture);
            GL.BindTextureUnit(1, textures[2]);

            DrawQuad();
        }
    }
}
