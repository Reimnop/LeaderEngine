using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public static class LightingGlobals
    {
        public static Matrix4 LightView { get; internal set; }
        public static Matrix4 LightProjection { get; internal set; }
        public static int ShadowMap { get; internal set; }
    }

    public class ForwardRenderer : GLRenderer
    {
        private Dictionary<DrawType, List<GLDrawData>> drawLists = new Dictionary<DrawType, List<GLDrawData>>()
        {
            { DrawType.ShadowMap, new List<GLDrawData>() },
            { DrawType.Opaque, new List<GLDrawData>() },
            { DrawType.Transparent, new List<GLDrawData>() },
            { DrawType.GUI, new List<GLDrawData>() }
        };

        const int shadowMapRes = 2048;
        const float shadowMapSize = 16.0f;

        private Framebuffer shadowMapFramebuffer;

        private PostProcessor postProcessor;

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
                        new TextureParamFloatArr { ParamName = TextureParameterName.TextureBorderColor, Params = new float[] { 1.0f, 1.0f, 1.0f, 1.0f } }
                    }
                }
            });

            string postProcessPath = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/PostProcess");

            postProcessor = new PostProcessor();
            postProcessor.Shaders.AddRange(new Shader[] {
                Shader.FromSourceFile("post-process",
                    Path.Combine(postProcessPath, "post-process.vert"),
                    Path.Combine(postProcessPath, "empty.frag"))
            });

            Logger.Log("Renderer initialized.", true);
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawLists[drawType].Add(drawData);
        }

        public override void Update()
        {
            postProcessor.Resize(ViewportSize);
        }

        public override void Render()
        {
            if (Camera.Main == null)
                return;

            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());
            DataManager.EngineReservedEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());

            //shadow mapping
            if (DirectionalLight.Main == null)
                goto RenderOpaque;

            Matrix4 lView;
            Matrix4 lProjection;
            DirectionalLight.Main.CalculateViewProjection(out lView, out lProjection, shadowMapSize, Camera.Main.BaseTransform.Position);
            LightingGlobals.LightView = lView;
            LightingGlobals.LightProjection = lProjection;

            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.RecursivelyRenderShadowMap(LightingGlobals.LightView, LightingGlobals.LightProjection));
            DataManager.EngineReservedEntities.ForEach(en => en.RecursivelyRenderShadowMap(LightingGlobals.LightView, LightingGlobals.LightProjection));

            GL.Viewport(0, 0, shadowMapRes, shadowMapRes);

            shadowMapFramebuffer.Begin();
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            DrawDrawList(drawLists[DrawType.ShadowMap]);

            shadowMapFramebuffer.End();

            LightingGlobals.ShadowMap = shadowMapFramebuffer.GetTexture(FramebufferAttachment.DepthAttachment);

        //render opaque
        RenderOpaque:
            Camera.Main.CalculateViewProjection(out Matrix4 view, out Matrix4 projection);

            //call all render funcs
            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.RecursivelyRender(view, projection));
            DataManager.EngineReservedEntities.ForEach(en => en.RecursivelyRender(view, projection));

            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            postProcessor.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            DrawDrawList(drawLists[DrawType.Opaque]);

            //render transparent
            GL.DepthMask(false);

            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            DrawDrawList(drawLists[DrawType.Transparent]);
            postProcessor.End();

            //reset states
            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            postProcessor.Render();

            ClearDrawList();
        }

        private void DrawDrawList(List<GLDrawData> drawDatas)
        {
            Mesh oldMesh = null;
            Shader oldShader = null;

            drawDatas.ForEach(drawData =>
            {
                Mesh mesh = drawData.Mesh;
                Shader shader = drawData.Shader;
                Material material = drawData.Material;
                UniformData uniforms = drawData.Uniforms;

                if (mesh == null || shader == null || uniforms == null)
                    return;

                if (mesh != oldMesh)
                    mesh.Use();

                if (shader != oldShader)
                    shader.Use();

                oldMesh = mesh;
                oldShader = shader;

                material?.Use(shader);
                uniforms.Use(shader);

                GL.DrawElements(mesh.PrimitiveType, mesh.IndicesCount, mesh.DrawElementsType, 0);
            });
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawLists)
                kvp.Value.Clear();
        }
    }
}
