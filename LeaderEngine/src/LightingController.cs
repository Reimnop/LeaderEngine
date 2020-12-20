using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LeaderEngine
{
    public static class LightingController
    {
        public static DirectionalLight DirectionalLight;

        private static Framebuffer depthBuffer;

        public static void Init()
        {
            depthBuffer = new Framebuffer(1024, 1024, true);
        }

        public static void RenderDepth(Action renderFunc)
        {
            if (DirectionalLight == null)
                return;

            GL.Viewport(0, 0, 1024, 1024);

            Shader shader = RenderingGlobals.ForcedShader;

            RenderingGlobals.ForcedShader = Shader.DepthOnly;

            DirectionalLight.GenViewProject(out RenderingGlobals.View, out RenderingGlobals.Projection);

            depthBuffer.Begin();
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            renderFunc();
            depthBuffer.End();

            RenderingGlobals.ForcedShader = shader;
        }

        public static void LightingShaderSetup(Shader shader, Matrix4 model)
        {
            if (DirectionalLight == null)
                return;

            Matrix4 view;
            Matrix4 proj;

            DirectionalLight.GenViewProject(out view, out proj);

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("lightSpaceMatrix", view * proj);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthBuffer.GetDepthTexture());
        }
    }
}
