using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class ForwardRenderer : GLRenderer
    {
        const int defaultSize = 4096;

        private List<CommandBuffer> shadowMapBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> opaqueBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> transparentBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> guiBuffers = new List<CommandBuffer>();

        #region PostProcess
        public float Exposure = 1f;
        #endregion

        const int shadowMapRes = 4096;
        const float shadowMapSize = 48f;

        private Framebuffer shadowMapFramebuffer;

        private PostProcessor postProcessor;

        private PostProcessingEffect hdrEffect;

        public override void Init()
        {
            shadowMapFramebuffer = new Framebuffer("shadowmap-fbo", shadowMapRes, shadowMapRes, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.DepthComponent,
                    PixelFormat = PixelFormat.DepthComponent,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.DepthAttachment,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Nearest },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest },
                        new TextureParamInt { ParamName = TextureParameterName.TextureWrapS, Param = (int)TextureWrapMode.ClampToBorder },
                        new TextureParamInt { ParamName = TextureParameterName.TextureWrapT, Param = (int)TextureWrapMode.ClampToBorder }
                    },
                    TextureParamsFloatArr = new TextureParamFloatArr[]
                    {
                        new TextureParamFloatArr { ParamName = TextureParameterName.TextureBorderColor, Params = new float[] { 1f, 1f, 1f, 1f } }
                    }
                }
            });

            #region PostProcess
            string postProcessPath = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            hdrEffect = new PostProcessingEffect
            {
                Uniforms = new UniformData(),
                Shader = Shader.FromSourceFile("hdr",
                    Path.Combine(postProcessPath, "post-process.vert"),
                    Path.Combine(postProcessPath, "hdr.frag"))
            };

            postProcessor = new PostProcessor();
            postProcessor.Effects.AddRange(new PostProcessingEffect[] {
                hdrEffect
            });
            #endregion

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
            postProcessor.Resize(ViewportSize);
        }

        public override void Render()
        {
            if (Camera.Main == null)
                return;

            foreach (var entity in DataManager.CurrentScene.SceneRootEntities)
                entity.Transform.CalculateModelMatrixRecursively();

            foreach (var entity in DataManager.UnlistedEntities)
                entity.Transform.CalculateModelMatrixRecursively();

            //shadow mapping
            Matrix4 lightView = Matrix4.Identity;
            Matrix4 lightProjection = Matrix4.Identity;

            if (DirectionalLight.Main == null)
                goto RenderOpaque;

            DirectionalLight.Main.CalculateViewProjection(out lightView, out lightProjection, shadowMapSize, Camera.Main.BaseTransform.Position);

            LightData lightData = new LightData
            {
                View = lightView,
                Projection = lightProjection
            };

            foreach (var entity in DataManager.CurrentScene.SceneRootEntities)
                entity.RecursivelyRenderShadowMap(in lightData);

            foreach (var entity in DataManager.UnlistedEntities)
                entity.RecursivelyRenderShadowMap(in lightData);

            GL.Viewport(0, 0, shadowMapRes, shadowMapRes);

            shadowMapFramebuffer.Begin();
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            foreach (var buffer in shadowMapBuffers)
                ExecuteCommandBuffer(buffer);

            shadowMapFramebuffer.End();

        //render opaque
        RenderOpaque:
            Camera.Main.CalculateViewProjection(out var view, out var projection);

            //call all render funcs
            RenderData renderData = new RenderData
            {
                View = view,
                Projection = projection,
                LightView = lightView,
                LightProjection = lightProjection,
                ShadowMapTexture = shadowMapFramebuffer.GetTexture(FramebufferAttachment.DepthAttachment)
            };

            foreach (var entity in DataManager.CurrentScene.SceneRootEntities)
                entity.RecursivelyRender(renderData);

            foreach (var entity in DataManager.UnlistedEntities)
                entity.RecursivelyRender(renderData);

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

            //reset states
            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            //post processing
            hdrEffect.Uniforms.SetUniform("exposure", new Uniform(UniformType.Float, Exposure));

            postProcessor.Render();

            //clean up
            shadowMapBuffers.Clear();
            opaqueBuffers.Clear();
            transparentBuffers.Clear();
            guiBuffers.Clear();

            CommandProcessor.Reset();
        }

        private void ExecuteCommandBuffer(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < commandBuffer.Count; i++)
                CommandProcessor.ExecuteCommand(commandBuffer.Commands[i]);
        }
    }
}
