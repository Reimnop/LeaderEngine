using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public static class LightingController
    {
        public static DirectionalLight DirectionalLight;

        private static Framebuffer depthBuffer;

        private static Matrix4 lightView;
        private static Matrix4 lightProjection;

        public const int ShadowWidth = 4096;
        public const int ShadowHeight = 4096;

        public static Vector3 CameraPos;

        public static Vector3 Ambient = new Vector3(0.4f);
        public static Vector3 LightColor = new Vector3(1.0f);
        public static float Intensity = 1.2f;

        public static void Init()
        {
            depthBuffer = new Framebuffer(ShadowWidth, ShadowHeight, true);
            depthBuffer.SetDepthMinFilter(TextureMinFilter.Linear);
            depthBuffer.SetDepthMagFilter(TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void RenderDepth(Action renderFunc)
        {
            if (DirectionalLight == null)
                return;

            GL.Viewport(0, 0, ShadowWidth, ShadowHeight);

            RenderingGlobals.GlobalPosition = -CameraPos;

            DirectionalLight.GenViewProject(out lightView, out lightProjection);

            RenderingGlobals.View = lightView;
            RenderingGlobals.Projection = lightProjection;

            depthBuffer.Begin();
            GL.Clear(ClearBufferMask.DepthBufferBit);

            renderFunc();
            depthBuffer.End();

            RenderingGlobals.GlobalPosition = Vector3.Zero;
        }

        public static void LightingShaderSetup(Shader shader, Matrix4 model)
        {
            shader.SetVector3("ambient", Ambient);
            shader.SetVector3("lightColor", LightColor);
            shader.SetFloat("intensity", Intensity);

            if (DirectionalLight == null)
            {
                shader.SetInt("shadowMap", 1);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                return;
            }

            Matrix4 lightModel = model * Matrix4.CreateTranslation(-CameraPos);

            var dir = -DirectionalLight.BaseTransform.Forward;

            shader.SetMatrix4("model", lightModel);
            shader.SetMatrix4("lightSpaceMatrix", lightView * lightProjection);

            shader.SetVector3("lightDir", dir);

            shader.SetInt("shadowMap", 1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
        }
    }
}
