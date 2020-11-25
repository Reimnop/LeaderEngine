using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public static class Time
    {
        public static float deltaTime = 0.16f;
        public static float deltaTimeUnscaled = 0.16f;
        public static float timeScale = 1f;
    }

    public class Application : GameWindow
    {
        public static Application instance;

        public static List<GameObject> GameObjects = new List<GameObject>();

        private Action initCallback;

        public Application(GameWindowSettings gws, NativeWindowSettings nws, Action initCallback) : base(gws, nws)
        {
            if (instance != null)
                return;

            instance = this;
            CursorVisible = false;

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

            base.OnLoad();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SwapBuffers();

            GL.Finish();
            base.OnRenderFrame(e);
        }
    }
}
