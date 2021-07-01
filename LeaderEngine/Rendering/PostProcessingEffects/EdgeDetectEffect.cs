using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace LeaderEngine
{
    public class EdgeDetectEffect : PostProcessingEffect
    {
        private int shader;

        public override void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            shader = Helper.CreateShaderProgram(
                Path.Combine(baseDir, "post-process.vert"),
                Path.Combine(baseDir, "edge-detect.frag"));

            GL.UseProgram(shader);
            GL.Uniform1(GL.GetUniformLocation(shader, "sourceTexture"), 0);
            GL.Uniform1(GL.GetUniformLocation(shader, "gPosition"), 1);
            GL.UseProgram(0);
        }

        public override void Resize(Vector2i size)
        {
            
        }

        public override void Render(PostProcessingData postProcessingData)
        {
            GL.UseProgram(shader);

            GL.BindTextureUnit(0, postProcessingData.ColorTexture);
            GL.BindTextureUnit(1, postProcessingData.PositionTexture);

            DrawQuad();
        }
    }
}
