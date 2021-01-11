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

        public List<GameObject> WorldGameObjects = new List<GameObject>();
        public List<GameObject> WorldGameObjects_Transparent = new List<GameObject>();
        public List<GameObject> GuiGameObjects = new List<GameObject>();

        private GameObject[] allObjects
        {
            get
            {
                List<GameObject> all = new List<GameObject>();
                all.AddRange(WorldGameObjects);
                all.AddRange(WorldGameObjects_Transparent);
                all.AddRange(GuiGameObjects);
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

        internal SSAO SSAOProcessor;
        internal SSAOBlur SSAOBlur;

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

            Main = this;
            CursorVisible = true;

            this.initCallback = initCallback;

            Logger.Log("Starting " + nws.Title);

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
                WorldGameObjects.ForEach(go => go.StartAll());
                WorldGameObjects_Transparent.ForEach(go => go.StartAll());
                GuiGameObjects.ForEach(go => go.StartAll());
            }
            else
            {
                WorldGameObjects.ForEach(go => go.RemoveAll());
                WorldGameObjects_Transparent.ForEach(go => go.RemoveAll());
                GuiGameObjects.ForEach(go => go.RemoveAll());
            }
        }

        protected override void OnLoad()
        {
            Logger.Log("Base Directory: " + AppContext.BaseDirectory);

            Logger.Log("Initializing...");

            GL.ClearColor(0.005f, 0.005f, 0.005f, 1.0f);

            Shader.InitDefaults();
            Material.InitDefaults();

            LightingController.Init();
            IM.Init();
            PhysicsController.Init();
            AudioSource.Init();

            Input.InputUpdate(KeyboardState, MouseState);

            SSAOProcessor = new SSAO(Size);
            SSAOBlur = new SSAOBlur(SSAOProcessor.Albedo, Size);
            //PostProcessor = new PostProcessor(Size);
            //TODO: fix post processing
            ViewportSize = Size;

            initCallback?.Invoke();

            Logger.Log("Done initializing");

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

            GameObject[] gameObjects = allObjects;

            for (int i = 0; i < gameObjects.Length; i++)
                if (gameObjects[i] != null)
                    if (gameObjects[i].Parent == null)
                        gameObjects[i].Update();

            for (int i = 0; i < gameObjects.Length; i++)
                if (gameObjects[i] != null)
                    if (gameObjects[i].Parent == null)
                        gameObjects[i].LateUpdate();

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
            SSAOBlur.Resize(ViewportSize.X, ViewportSize.Y);
            SSAOProcessor.Begin();

            Skybox.Main?.Render();
            RenderScene();

            PostSceneRender?.Invoke();
            SSAOProcessor.End();

            SSAOBlur.Begin();
            SSAOProcessor.Render();
            SSAOBlur.End();

            PostProcess?.Invoke();
            SSAOBlur.Render(); //TODO: post process here
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

            WorldGameObjects.ForEach(go => go.Render());
            WorldGameObjects_Transparent.ForEach(go => go.Render());
        }

        public void RenderGui()
        {
            if (RenderingGlobals.RenderingEnabled)
                GuiGameObjects.ForEach(go => go.RenderGui());
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
