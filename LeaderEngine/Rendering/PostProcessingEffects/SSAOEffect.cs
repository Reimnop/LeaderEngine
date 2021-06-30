using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    public class SSAOEffect : PostProcessingEffect
    {
        private const int kernelSize = 64;
        private const int noiseSize = 8;

        private int noiseTexture;
        private int ssaoShader, viewLoc, projectionLoc;

        public override void Init()
        {
            //init shader
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            ssaoShader = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "ssao.frag"));

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
            GL.UseProgram(ssaoShader);
            int kernelLoc = GL.GetUniformLocation(ssaoShader, "kernel[0]");
            for (int i = 0; i < kernelSize; i++)
            {
                GL.Uniform3(kernelLoc + i, ssaoKernel[i]);
            }

            GL.Uniform1(GL.GetUniformLocation(ssaoShader, "sourceTexture"), 0);
            GL.Uniform1(GL.GetUniformLocation(ssaoShader, "gPosition"), 1);
            GL.Uniform1(GL.GetUniformLocation(ssaoShader, "gNormal"), 2);
            GL.Uniform1(GL.GetUniformLocation(ssaoShader, "texNoise"), 3);

            viewLoc = GL.GetUniformLocation(ssaoShader, "view");
            projectionLoc = GL.GetUniformLocation(ssaoShader, "projection");

            GL.UseProgram(0);
        }

        public override void Resize(Vector2i size)
        {
            
        }

        public override void Render(PostProcessingData postProcessingData)
        {
            GL.UseProgram(ssaoShader);
            GL.UniformMatrix4(viewLoc, true, ref postProcessingData.View);
            GL.UniformMatrix4(projectionLoc, true, ref postProcessingData.Projection);

            GL.BindTextureUnit(0, postProcessingData.ColorTexture);
            GL.BindTextureUnit(1, postProcessingData.PositionTexture);
            GL.BindTextureUnit(2, postProcessingData.NormalTexture);
            GL.BindTextureUnit(3, noiseTexture);

            DrawQuad();
        }
    }
}
