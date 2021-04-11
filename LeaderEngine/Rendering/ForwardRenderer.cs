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

        const int shadowMapRes = 4096;
        const float shadowMapSize = 32.0f;

        private Framebuffer shadowMapFramebuffer;
        private Framebuffer ppFramebuffer;

        private Mesh ppMesh;
        private Shader ppShader;

        public override void Init()
        {
            shadowMapFramebuffer = new Framebuffer("ShadowMapFBO", shadowMapRes, shadowMapRes, new Attachment[]
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

            ppFramebuffer = new Framebuffer("PostProcessFBO", ViewportSize.X, ViewportSize.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                },
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
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest }
                    }
                }
            });

            ppMesh = new Mesh("PostProcessQuad");
            ppMesh.LoadMesh(new Vertex[] 
            {
                new Vertex { Position = new Vector3(1.0f, 1.0f, 0.0f), UV = new Vector2(1.0f, 1.0f) },
                new Vertex { Position = new Vector3(1.0f, -1.0f, 0.0f), UV = new Vector2(1.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, -1.0f, 0.0f), UV = new Vector2(0.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, 1.0f, 0.0f), UV = new Vector2(0.0f, 1.0f) }
            },
            new uint[]
            {
                0, 1, 3,
                1, 2, 3
            });

            ppShader = Shader.FromSourceFile("PostProcessShader",
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/post-process.vert"),
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/post-process.frag"));

            Logger.Log("Renderer initialized.", true);
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawLists[drawType].Add(drawData);
        }

        public override void Update()
        {
            ppFramebuffer.Resize(ViewportSize.X, ViewportSize.Y);
        }

        public override void Render()
        {
            if (Camera.Main == null)
                return;

            //set matrices
            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());

            Camera.Main.CalculateViewProjection(out Matrix4 view, out Matrix4 projection);

            //call all render funcs
            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.RecursivelyRender(view, projection));

            //shadow mapping
            if (DirectionalLight.Main == null)
                goto RenderOpaque;

            Matrix4 lView; Matrix4 lProjection;
            DirectionalLight.Main.CalculateViewProjection(out lView, out lProjection, shadowMapSize);
            LightingGlobals.LightView = lView;
            LightingGlobals.LightProjection = lProjection;

            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.RecursivelyRenderShadowMap(LightingGlobals.LightView, LightingGlobals.LightProjection));

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
            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            ppFramebuffer.Begin();
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

            ppFramebuffer.End();

            //reset states
            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            //post process
            ppMesh.Use();
            ppShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ppFramebuffer.GetTexture(FramebufferAttachment.ColorAttachment0));

            GL.DrawElements(PrimitiveType.Triangles, ppMesh.IndicesCount, DrawElementsType.UnsignedInt, 0);

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

                GL.DrawElements(mesh.PrimitiveType, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            });
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawLists)
                kvp.Value.Clear();
        }
    }
}
