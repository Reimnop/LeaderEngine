using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace LeaderEngine
{
    public static class Time
    {
        public static float deltaTime = 0.16f;
        public static float deltaTimeUnscaled = 0.16f;
        public static float timeScale = 1.0f;
        public static float time = 0.0f;
    }

    public class Application : GameWindow
    {
        public static Application Main = null;

        public List<Entity> WorldEntities = new List<Entity>();
        public List<Entity> WorldEntities_Transparent = new List<Entity>();
        public List<Entity> GuiEntities = new List<Entity>();

        private Entity[] allEntities
        {
            get
            {
                List<Entity> all = new List<Entity>();
                all.AddRange(WorldEntities);
                all.AddRange(WorldEntities_Transparent);
                all.AddRange(GuiEntities);
                return all.ToArray();
            }
        }

        public bool EditorMode
        {
            get => _editorMode;
            set
            {
                UpdateMode(value);
                _editorMode = value;
            }
        }
        private bool _editorMode = false;

        public SSAO SSAOProcessor;

        [Obsolete] //TODO: Fix post processing
        public PostProcessor PostProcessor;

        public Vector2i ViewportSize;

        private Queue<Action> NextUpdateQueue = new Queue<Action>();

        private Action initCallback;
        public event Action RenderBegin;
        public event Action SceneRender;
        public event Action PostSceneRender;
        public event Action PostProcess;
        public event Action PostPostProcess;
        public event Action GuiRender;
        public event Action PostGuiRender;
        public event Action FinishRender;
        public event Action<Vector2i> OnViewportResize;

        public Application(GameWindowSettings gws, NativeWindowSettings nws, Action initCallback) : base(gws, nws)
        {
            if (Main != null)
                return;

            Logger.Log("Starting " + nws.Title);

            Main = this;
            CursorVisible = true;

            this.initCallback = initCallback;

            ViewportSize = nws.Size;

            GLFW.SwapInterval(1);
        }

        public void ExecuteNextUpdate(Action action)
        {
            NextUpdateQueue.Enqueue(action);
        }

        private void UpdateMode(bool editorMode)
        {
            if (!editorMode)
            {
                WorldEntities.ForEach(en => en.StartAll());
                WorldEntities_Transparent.ForEach(en => en.StartAll());
                GuiEntities.ForEach(en => en.StartAll());
            }
            else
            {
                WorldEntities.ForEach(en => en.RemoveAll());
                WorldEntities_Transparent.ForEach(en => en.RemoveAll());
                GuiEntities.ForEach(en => en.RemoveAll());
            }
        }

        protected override void OnLoad()
        {
            Logger.Log("Base Directory: " + AppContext.BaseDirectory);

            Logger.Log("Renderer: " + GL.GetString(StringName.Renderer));
            Logger.Log("Vendor: " + GL.GetString(StringName.Vendor));
            Logger.Log("Version: " + GL.GetString(StringName.Version) ?? "none");
            Logger.Log("Extensions: " + GL.GetString(StringName.Extensions));
            Logger.Log("Shading Language version: " + GL.GetString(StringName.ShadingLanguageVersion));

            Logger.Log("Initializing...");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            GL.ClearColor(0.005f, 0.005f, 0.005f, 1.0f);

            Shader.InitDefaults();
            Material.InitDefaults();

            LightingController.Init();
            IM.Init();
            PhysicsController.Init();
            AudioSource.Init();

            Input.InputUpdate(KeyboardState, MouseState);

            //PostProcessor = new PostProcessor(Size);
            //TODO: fix post processing
            Size = ViewportSize;

            SSAOProcessor = new SSAO(ViewportSize);

            initCallback?.Invoke();

            stopwatch.Stop();
            Logger.Log($"Done initializing ({stopwatch.ElapsedMilliseconds}ms)");

            base.OnLoad();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Time.time = (float)GLFW.GetTime();

            while (NextUpdateQueue.Count > 0)
                NextUpdateQueue.Dequeue().Invoke();

            base.OnUpdateFrame(e);

            ThreadManager.ExecuteAll();

            Entity[] entities = allEntities;

            for (int i = 0; i < entities.Length; i++)
                if (entities[i] != null)
                    if (entities[i].Parent == null)
                        entities[i].Update();

            for (int i = 0; i < entities.Length; i++)
                if (entities[i] != null)
                    if (entities[i].Parent == null)
                        entities[i].LateUpdate();

            PhysicsController.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderBegin?.Invoke();

            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            RenderingGlobals.CurrentPass = RenderPass.Lighting;
            LightingController.RenderDepth(RenderScene);

            SceneRender?.Invoke();
            RenderingGlobals.CurrentPass = RenderPass.World;

            SSAOProcessor.Resize(ViewportSize.X, ViewportSize.Y);
            SSAOProcessor.Begin();

            RenderScene();

            PostSceneRender?.Invoke();
            SSAOProcessor.End();

            SSAOProcessor.Render();
            SSAOProcessor.RenderBlurPass();

            PostProcess?.Invoke();

            SSAOProcessor.RenderLightPass();
            Skybox.Main?.Render();
            //TODO: post process here

            PostPostProcess?.Invoke();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            RenderingGlobals.CurrentPass = RenderPass.Gui;

            GuiRender?.Invoke();
            RenderGui();

            PostGuiRender?.Invoke();

            FinishRender?.Invoke();

            ErrorCode error = GL.GetError();

            while (error != ErrorCode.NoError)
            {
                Logger.Log("ERROR: " + error.ToString());
                error = GL.GetError();
            }

            SwapBuffers();

            Time.deltaTimeUnscaled = (float)GLFW.GetTime() - Time.time;
            Time.deltaTime = Time.deltaTimeUnscaled * Time.timeScale;
        }

        public void RenderScene()
        {
            if (!RenderingGlobals.RenderingEnabled)
                return;

            WorldEntities.ForEach(en => en.Render());
            WorldEntities_Transparent.ForEach(en => en.Render());
        }

        public void RenderGui()
        {
            if (RenderingGlobals.RenderingEnabled)
                GuiEntities.ForEach(en => en.RenderGui());
        }

        public void ResizeViewport(Vector2i newSize)
        {
            ViewportSize = newSize;
            GL.Viewport(0, 0, newSize.X, newSize.Y);

            OnViewportResize?.Invoke(newSize);
        }

        public void ResizeViewport(int width, int height)
        {
            Vector2i newSize = new Vector2i(width, height);

            ViewportSize = newSize;
            GL.Viewport(0, 0, width, height);

            OnViewportResize?.Invoke(newSize);
        }
    }
}
