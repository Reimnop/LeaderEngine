using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
        public static Application instance = null;

        public List<GameObject> GameObjects = new List<GameObject>();

        private Action initCallback;

        public Application(GameWindowSettings gws, NativeWindowSettings nws, Action initCallback) : base(gws, nws)
        {
            if (instance != null)
                return;

            instance = this;
            CursorVisible = true;

            this.initCallback = initCallback;

            GLFW.SwapInterval(0);
        }

        public override void Run()
        {
            initCallback?.Invoke();

            base.Run();
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);

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

            GameObjects.ForEach(go => go.Update());
            GameObjects.ForEach(go => go.LateUpdate());

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GameObjects.ForEach(go => go.Render());

            SwapBuffers();

            base.OnRenderFrame(e);

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
