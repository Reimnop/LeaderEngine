using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        public List<GameObject> GameObjects = new List<GameObject>();
        public bool EditorMode = false;

        private Queue<Action> NextUpdateQueue = new Queue<Action>();

        private Action initCallback;
        public event Action SceneRender;
        public event Action PostSceneRender;
        public event Action GuiRender;
        public event Action FinishRender;

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
            Shader.InitDefaults();
            Material.InitDefaults();

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

            if (!EditorMode)
            {
                GameObjects.ForEach(go => go.Update());
                GameObjects.ForEach(go => go.LateUpdate());
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            SceneRender?.Invoke();
            if (RenderingGlobals.RenderingEnabled)
                GameObjects.ForEach(go => go.Render());
            PostSceneRender?.Invoke();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GuiRender?.Invoke();
            if (RenderingGlobals.RenderingEnabled)
                GameObjects.ForEach(go => go.RenderGui());

            FinishRender?.Invoke();

            SwapBuffers();

            Time.deltaTimeUnscaled = (float)GLFW.GetTime() - Time.time;
            Time.deltaTime = Time.deltaTimeUnscaled * Time.timeScale;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            GameObjects.ForEach(go => go.OnClosing());

            base.OnClosing(e);
        }
    }
}
