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

        public static void Init()
        {
            depthBuffer = new Framebuffer(ShadowWidth, ShadowHeight, true);
            depthBuffer.SetDepthMinFilter(TextureMinFilter.Linear);
            depthBuffer.SetDepthMagFilter(TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void RenderDepth()
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

            Application.Main.RenderOpaque();
            Application.Main.RenderTransparent();

            depthBuffer.End();

            RenderingGlobals.GlobalPosition = Vector3.Zero;
        }

        public static void LightingShaderSetup(Shader shader)
        {
            if (DirectionalLight == null)
            {
                shader.SetInt("shadowMap", 5);

                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                return;
            }

            var dir = DirectionalLight.Transform.Forward;

            shader.SetMatrix4("lightSpaceMatrix", Matrix4.CreateTranslation(-CameraPos) * lightView * lightProjection);

            shader.SetVector3("lightDir", dir);

            shader.SetFloat("intensity", DirectionalLight.Intensity);

            shader.SetInt("shadowMap", 5);

            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
        }
    }
}
