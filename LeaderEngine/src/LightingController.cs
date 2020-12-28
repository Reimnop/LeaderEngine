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

        public const int ShadowWidth = 16384;
        public const int ShadowHeight = 16384;

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

        public static void RenderDepth(Action renderFunc)
        {
            if (DirectionalLight == null)
                return;

            GL.Viewport(0, 0, ShadowWidth, ShadowHeight);

            RenderingGlobals.GlobalPosition = -CameraPos;

            Shader shader = RenderingGlobals.ForcedShader;

            RenderingGlobals.ForcedShader = Shader.DepthOnly;

            DirectionalLight.GenViewProject(out lightView, out lightProjection);

            RenderingGlobals.View = lightView;
            RenderingGlobals.Projection = lightProjection;

            depthBuffer.Begin();
            GL.Clear(ClearBufferMask.DepthBufferBit);

            renderFunc();
            depthBuffer.End();

            RenderingGlobals.GlobalPosition = Vector3.Zero;
            RenderingGlobals.ForcedShader = shader;
        }

        public static void LightingShaderSetup(Shader shader, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (DirectionalLight == null)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                return;
            }

            Matrix4 model = Matrix4.CreateScale(scale)
                 * Matrix4.CreateFromQuaternion(rotation)
                 * Matrix4.CreateTranslation(position - CameraPos);

            var dir = DirectionalLight.transform.Forward;
            dir.Z = -dir.Z;

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("lightSpaceMatrix", lightView * lightProjection);

            shader.SetVector3("lightDir", dir);

            shader.SetInt("shadowMap", 1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
        }
    }
}
