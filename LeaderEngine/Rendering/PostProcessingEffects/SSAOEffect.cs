using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    public class SSAOEffect : PostProcessingEffect
    {
        private const int kernelSize = 64;
        private const int noiseSize = 4;

        private int noiseTexture;
        private int ssaoProgram, viewLoc, projectionLoc;

        private int ssaoBlur;

        private int fbo;
        private int texture;

        public override void Init()
        {
            //init fbo and texture
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.Red, 1, 1, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //init shaders
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            ssaoProgram = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "ssao.frag"));

            ssaoBlur = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "ssao-blur.frag"));

            //create kernel
            Random rng = new Random();

            Vector3[] ssaoKernel = new Vector3[kernelSize];
            for (int i = 0; i < kernelSize; i++)
            {
                Vector3 sample = new Vector3(
                    (float)rng.NextDouble() * 2f - 1f,
                    (float)rng.NextDouble() * 2f - 1f,
                    (float)rng.NextDouble());

                sample.Normalize();
                sample *= (float)rng.NextDouble();

                float scale = i / 64f;
                scale = MathHelper.Lerp(0.1f, 10f, scale * scale);
                sample *= scale;

                ssaoKernel[i] = sample;
            }

            //create noise texture
            Vector3[] noiseData = new Vector3[noiseSize * noiseSize];
            for (int i = 0; i < noiseSize * noiseSize; i++)
            {
                noiseData[i] = new Vector3(
                    (float)rng.NextDouble() * 2f - 1f,
                    (float)rng.NextDouble() * 2f - 1f,
                    (float)rng.NextDouble());
            }

            noiseTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, noiseTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, noiseSize, noiseSize, 0, PixelFormat.Rgb, PixelType.Float, noiseData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //upload uniforms
            GL.UseProgram(ssaoProgram);
            int kernelLoc = GL.GetUniformLocation(ssaoProgram, "kernel[0]");
            for (int i = 0; i < kernelSize; i++)
            {
                GL.Uniform3(kernelLoc + i, ssaoKernel[i]);
            }

            GL.Uniform1(GL.GetUniformLocation(ssaoProgram, "gPosition"), 0);
            GL.Uniform1(GL.GetUniformLocation(ssaoProgram, "gNormal"), 1);
            GL.Uniform1(GL.GetUniformLocation(ssaoProgram, "texNoise"), 2);

            viewLoc = GL.GetUniformLocation(ssaoProgram, "view");
            projectionLoc = GL.GetUniformLocation(ssaoProgram, "projection");

            GL.UseProgram(ssaoBlur);
            GL.Uniform1(GL.GetUniformLocation(ssaoBlur, "sourceTexture"), 0);
            GL.Uniform1(GL.GetUniformLocation(ssaoBlur, "ssaoTexture"), 1);
            GL.UseProgram(0);
        }

        public override void Resize(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.Red, size.X, size.Y, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Render(PostProcessingData postProcessingData)
        {
            GL.UseProgram(ssaoProgram);
            GL.UniformMatrix4(viewLoc, true, ref postProcessingData.View);
            GL.UniformMatrix4(projectionLoc, true, ref postProcessingData.Projection);

            GL.BindTextureUnit(0, postProcessingData.PositionTexture);
            GL.BindTextureUnit(1, postProcessingData.NormalTexture);
            GL.BindTextureUnit(2, noiseTexture);

            FramebufferManager.PushFramebuffer(fbo);
            DrawQuad();
            FramebufferManager.PopFramebuffer();

            GL.UseProgram(ssaoBlur);

            GL.BindTextureUnit(0, postProcessingData.ColorTexture);
            GL.BindTextureUnit(1, texture);

            DrawQuad();
        }
    }
}
