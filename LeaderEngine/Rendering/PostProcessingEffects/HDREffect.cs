using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    public class HDREffect : PostProcessingEffect
    {
        public float Exposure = 1.0f;

        private int shader;
        private int exposureLoc;

        public override void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            shader = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "hdr.frag"));

            GL.GetUniformLocation(shader, "exposure");
        }

        public override void Resize(Vector2i size)
        {
            
        }

        public override void Render(PostProcessingData postProcessingData)
        {
            GL.UseProgram(shader);
            GL.Uniform1(exposureLoc, Exposure);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, postProcessingData.ColorTexture);

            DrawQuad();
        }
    }
}
