using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class ForwardRenderer : GLRenderer
    {
        private const int defaultSize = 4096;

        private List<CommandBuffer> shadowMapBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> opaqueBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> transparentBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> guiBuffers = new List<CommandBuffer>();

        #region PostProcess
        public float Exposure = 1f;
        #endregion

        private const int shadowMapRes = 4096;
        private const float shadowMapSize = 48f;

        private int shadowMapFBO;
        private int shadowMap;

        private PostProcessor postProcessor;

        private Vector2i lastViewportSize;

        public override void Init()
        {
            //init FBO
            shadowMapFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowMapFBO);

            //init shadowmap
            shadowMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadowMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadowMapRes, shadowMapRes, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //bind texture to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowMap, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //init post processor
            postProcessor = new PostProcessor();

            GL.DepthFunc(DepthFunction.Lequal);

            Logger.Log("Renderer initialized.", true);
        }

        public override void QueueCommandsShadowMap(CommandBuffer commandBuffer)
        {
            shadowMapBuffers.Add(commandBuffer);
        }

        public override void QueueCommandsOpaque(CommandBuffer commandBuffer)
        {
            opaqueBuffers.Add(commandBuffer);
        }

        public override void QueueCommandsTransparent(CommandBuffer commandBuffer)
        {
            transparentBuffers.Add(commandBuffer);
        }

        public override void QueueCommandsGUI(CommandBuffer commandBuffer)
        {
            guiBuffers.Add(commandBuffer);
        }

        public override void Update()
        {
            if (ViewportSize == lastViewportSize)
                return;

            postProcessor.Resize(ViewportSize);
            lastViewportSize = ViewportSize;
        }

        public override void Render()
        {
            RenderStuff();
            RenderPostProcess();
        }

        protected void RenderStuff()
        {
            Camera camera = Camera.Main;

            if (camera == null)
                return;

            //shadow mapping
            Matrix4 lightView = Matrix4.Identity;
            Matrix4 lightProjection = Matrix4.Identity;

            if (DirectionalLight.Main == null)
                goto RenderOpaque;

            DirectionalLight.Main.CalculateViewProjection(out lightView, out lightProjection, shadowMapSize, camera.BaseTransform.Position);

            LightData lightData = new LightData
            {
                View = lightView,
                Projection = lightProjection
            };

            foreach (Entity entity in DataManager.CurrentScene.SceneEntities)
                entity.RenderShadowMap(in lightData);

            foreach (Entity entity in DataManager.UnlistedEntities)
                entity.RenderShadowMap(in lightData);

            GL.Viewport(0, 0, shadowMapRes, shadowMapRes);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            foreach (var buffer in shadowMapBuffers)
                ExecuteCommandBuffer(buffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        //render opaque
        RenderOpaque:
            camera.CalculateViewProjection(out var view, out var projection);

            //call all render funcs
            RenderData renderData = new RenderData
            {
                View = view,
                Projection = projection,
                LightView = lightView,
                LightProjection = lightProjection,
                ShadowMapTexture = shadowMap
            };

            foreach (var entity in DataManager.CurrentScene.SceneEntities)
                entity.Render(renderData);

            foreach (var entity in DataManager.UnlistedEntities)
                entity.Render(renderData);

            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            postProcessor.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            foreach (var buffer in opaqueBuffers)
                ExecuteCommandBuffer(buffer);

            //render transparent
            GL.DepthMask(false);

            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            foreach (var buffer in transparentBuffers)
                ExecuteCommandBuffer(buffer);

            postProcessor.End();

            //clean up
            shadowMapBuffers.Clear();
            opaqueBuffers.Clear();
            transparentBuffers.Clear();
            guiBuffers.Clear();

            CommandProcessor.Reset();
        }

        protected void RenderPostProcess()
        {
            //reset states
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            postProcessor.Render();
        }

        private void ExecuteCommandBuffer(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < commandBuffer.Count; i++)
                CommandProcessor.ExecuteCommand(commandBuffer.Commands[i]);
        }
    }
}
