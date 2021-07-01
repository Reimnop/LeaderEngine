using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class ForwardRenderer : GLRenderer
    {
        private List<Entity> renderableEntities = new List<Entity>();

        private List<CommandBuffer> shadowMapBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> opaqueBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> transparentBuffers = new List<CommandBuffer>();
        private List<CommandBuffer> guiBuffers = new List<CommandBuffer>();

        private const int cascadeCount = 4;
        private const float shadowMapSizeOffet = 0f;
        private const float shadowMapDepth = 480f;
        private const float cascadeSplitLogFactor = 1.2f;
        private const float shadowMapMaxDistance = 60f;
        private const int shadowMapRes = 2048;

        private float[] cascadeDepths = new float[cascadeCount + 1];

        private int[] cascadeFramebuffers = new int[cascadeCount];
        private int[] cascadeShadowMaps = new int[cascadeCount];

        private Matrix4[] cascadeViewProjectionMatrices = new Matrix4[cascadeCount];

        private PostProcessor postProcessor;

        private Vector2i lastViewportSize;

        public override void Init()
        {
            //init shadowmap resources
            for (int i = 0; i < cascadeCount; i++)
            {
                int shadowMapFramebuffer = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowMapFramebuffer);

                int shadowMapTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, shadowMapTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadowMapRes, shadowMapRes, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowMapTexture, 0);

                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                cascadeFramebuffers[i] = shadowMapFramebuffer;
                cascadeShadowMaps[i] = shadowMapTexture;
            }
            GL.DepthFunc(DepthFunction.Lequal);

            //init post processor
            PostProcessingEffect.InitResources();
            postProcessor = new PostProcessor(
                new SSAOEffect(),
                new BloomEffect(),
                new HDREffect(),
                new EdgeDetectEffect());
            
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
            Camera camera = Camera.Main;

            if (camera == null)
                return;

            //cache renderables
            foreach (Entity entity in DataManager.CurrentScene.SceneEntities)
            {
                if (entity.Renderable)
                {
                    renderableEntities.Add(entity);
                }
            }

            foreach (Entity entity in DataManager.UnlistedEntities)
            {
                if (entity.Renderable)
                {
                    renderableEntities.Add(entity);
                }
            }

            //init render resources on all renderables
            foreach (Entity entity in renderableEntities)
                entity.InitRenderResources();


            camera.GetViewProjectionMatrices(out Matrix4 view, out Matrix4 projection);

            //shadow mapping
            if (DirectionalLight.Main == null)
                goto RenderOpaque;

            DirectionalLight light = DirectionalLight.Main;

            //calculate cascade depths
            float camNear = camera.NearPlane;
            float camFar = MathF.Min(shadowMapMaxDistance, camera.FarPlane);

            for (int i = 0; i < cascadeCount; i++)
            {
                cascadeDepths[i] = MathHelper.Lerp(
                    camNear + (i / (float)cascadeCount) * (camFar - camNear),
                    camNear * MathF.Pow(camFar / camNear, (float)i / cascadeCount),
                    cascadeSplitLogFactor);
            }

            cascadeDepths[cascadeCount] = camFar;

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, shadowMapRes, shadowMapRes);

            //render shadowmaps cascades
            for (int i = 0; i < cascadeCount; i++)
            {
                Matrix4 cascadeProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FOV), ViewportSize.X / (float)ViewportSize.Y, cascadeDepths[i], cascadeDepths[i + 1]);
                FrustumCorners corners = CalculateFrustum(view * cascadeProj);

                Vector3 lightPos = Vector3.Zero;
                lightPos += corners.NearTopLeft;
                lightPos += corners.NearTopRight;
                lightPos += corners.NearBottomLeft;
                lightPos += corners.NearBottomRight;
                lightPos += corners.FarTopLeft;
                lightPos += corners.FarTopRight;
                lightPos += corners.FarBottomLeft;
                lightPos += corners.FarBottomRight;
                lightPos /= 8f;

                Matrix4 lightViewProjection = CalculateLightViewProjection(lightPos, light.BaseTransform.Forward, light.BaseTransform.Up, Vector3.Distance(corners.FarTopLeft, corners.FarTopRight) + shadowMapSizeOffet, shadowMapDepth);

                cascadeViewProjectionMatrices[i] = lightViewProjection;

                LightData lightData = new LightData
                {
                    ViewProjection = lightViewProjection
                };

                foreach (Entity entity in renderableEntities)
                    entity.RenderShadowMap(in lightData);

                FramebufferManager.PushFramebuffer(cascadeFramebuffers[i]);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                foreach (var buffer in shadowMapBuffers)
                    ExecuteCommandBuffer(buffer);

                FramebufferManager.PopFramebuffer();
            }

        //render opaque
        RenderOpaque:
            //call all render funcs
            RenderData renderData = new RenderData
            {
                View = view,
                Projection = projection,
                ViewProjection = view * projection,
                CascadeCount = cascadeCount,
                CascadeDepths = cascadeDepths,
                CascadeShadowMaps = cascadeShadowMaps,
                CascadeViewProjections = cascadeViewProjectionMatrices
            };

            foreach (Entity entity in renderableEntities)
                entity.Render(renderData);

            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            postProcessor.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

            //post process
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            postProcessor.Render(view, projection);

            //clean up
            shadowMapBuffers.Clear();
            opaqueBuffers.Clear();
            transparentBuffers.Clear();
            guiBuffers.Clear();

            renderableEntities.Clear();

            CommandProcessor.Reset();
        }

        private void ExecuteCommandBuffer(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < commandBuffer.Count; i++)
                CommandProcessor.ExecuteCommand(commandBuffer.Commands[i]);
        }

        private Matrix4 CalculateLightViewProjection(Vector3 position, Vector3 direction, Vector3 up, float size, float maxDepth)
        {
            Matrix4 view = Matrix4.LookAt(
                position,
                position + direction,
                up);

            Matrix4 projection = Matrix4.CreateOrthographic(size, size, -maxDepth, maxDepth);

            return view * projection;
        }

        private FrustumCorners CalculateFrustum(Matrix4 projection)
        {
            Vector4 nearTopLeft     = new Vector4(-1f,  1f, -1f, 1f);
            Vector4 nearTopRight    = new Vector4( 1f,  1f, -1f, 1f);
            Vector4 nearBottomLeft  = new Vector4(-1f, -1f, -1f, 1f);
            Vector4 nearBottomRight = new Vector4( 1f, -1f, -1f, 1f);
            Vector4 farTopLeft      = new Vector4(-1f,  1f,  1f, 1f);
            Vector4 farTopRight     = new Vector4( 1f,  1f,  1f, 1f);
            Vector4 farBottomLeft   = new Vector4(-1f, -1f,  1f, 1f);
            Vector4 farBottomRight  = new Vector4( 1f, -1f,  1f, 1f);

            Matrix4 inverseProjection = Matrix4.Invert(projection);

            nearTopLeft     *= inverseProjection;
            nearTopRight    *= inverseProjection;
            nearBottomLeft  *= inverseProjection;
            nearBottomRight *= inverseProjection;
            farTopLeft      *= inverseProjection;
            farTopRight     *= inverseProjection;
            farBottomLeft   *= inverseProjection;
            farBottomRight  *= inverseProjection;

            return new FrustumCorners
            {
                NearTopLeft = nearTopLeft.Xyz / nearTopLeft.W,
                NearTopRight = nearBottomRight.Xyz / nearTopRight.W,
                NearBottomLeft = nearBottomLeft.Xyz / nearBottomLeft.W,
                NearBottomRight = nearBottomRight.Xyz / nearBottomRight.W,
                FarTopLeft = farTopLeft.Xyz / farTopLeft.W,
                FarTopRight = farBottomRight.Xyz / farTopRight.W,
                FarBottomLeft = farBottomLeft.Xyz / farBottomLeft.W,
                FarBottomRight = farBottomRight.Xyz / farBottomRight.W,
            };
        }
    }
}
