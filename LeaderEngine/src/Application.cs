using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

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
        public static Application main = null;

        public List<GameObject> WorldGameObjects = new List<GameObject>();
        public List<GameObject> WorldGameObjects_Transparent = new List<GameObject>();
        public List<GameObject> GuiGameObjects = new List<GameObject>();
        public bool EditorMode = false;

        public Vector2i ViewportSize;

        private Queue<Action> NextUpdateQueue = new Queue<Action>();

        private Action initCallback;
        public event Action RenderBegin;
        public event Action SceneRender;
        public event Action PostSceneRender;
        public event Action GuiRender;
        public event Action PostGuiRender;
        public event Action FinishRender;
        public event Action<Vector2i> OnViewportResize;

        public Application(GameWindowSettings gws, NativeWindowSettings nws, Action initCallback) : base(gws, nws)
        {
            if (main != null)
                return;

            main = this;
            CursorVisible = true;

            this.initCallback = initCallback;

            GLFW.SwapInterval(1);
        }

        public void ExecuteNextUpdate(Action action)
        {
            NextUpdateQueue.Enqueue(action);
        }

        public override void Run()
        {
            initCallback?.Invoke();

            base.Run();
        }

        protected override void OnLoad()
        {
            Debug.WriteLine("Base Directory: " + AppContext.BaseDirectory);

            Shader.InitDefaults();
            Material.InitDefaults();

            LightingController.Init();

            Input.InputUpdate(KeyboardState, MouseState);

            GL.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);

            ViewportSize = Size;

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

            if (!EditorMode)
            {
                WorldGameObjects.ForEach(go => go.Update());
                WorldGameObjects_Transparent.ForEach(go => go.Update());
                GuiGameObjects.ForEach(go => go.Update());

                WorldGameObjects.ForEach(go => go.LateUpdate());
                WorldGameObjects_Transparent.ForEach(go => go.LateUpdate());
                GuiGameObjects.ForEach(go => go.LateUpdate());
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderBegin?.Invoke();

            GL.Viewport(0, 0, ViewportSize.X, ViewportSize.Y);

            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            RenderingGlobals.CurrentPass = RenderPass.Lighting;
            LightingController.RenderDepth(RenderScene);

            SceneRender?.Invoke();
            RenderingGlobals.CurrentPass = RenderPass.World;
            RenderScene();
            PostSceneRender?.Invoke();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            RenderingGlobals.CurrentPass = RenderPass.Gui;

            GuiRender?.Invoke();
            RenderGui();
            PostGuiRender?.Invoke();

            FinishRender?.Invoke();

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

        protected override void OnClosing(CancelEventArgs e)
        {
            WorldGameObjects.ForEach(go => go.OnClosing());

            base.OnClosing(e);
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
