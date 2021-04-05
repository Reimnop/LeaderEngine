using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;

using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;

namespace LeaderEngine
{
    public static class Time
    {
        public static float Elapsed { get; internal set; }
        public static float DeltaTime { get; internal set; }
        public static float UnscaledDeltaTime { get; internal set; }
        public static float TimeScale = 1.0f;
    }

    public static class Engine
    {
        public static GameWindow MainWindow { get; private set; }

        public static GLRenderer Renderer = new ForwardRenderer();

        public static void Init(GameWindowSettings gws, NativeWindowSettings nws, Action initCallback = null)
        {
            MainWindow = new GameWindow(gws, nws);

            //log basic info
            Logger.Log("Base Directory: " + AppContext.BaseDirectory);
            Logger.Log("Renderer: " + GL.GetString(StringName.Renderer));
            Logger.Log("Vendor: " + GL.GetString(StringName.Vendor));
            Logger.Log("Version: " + GL.GetString(StringName.Version));
            Logger.Log("Shading Language version: " + GL.GetString(StringName.ShadingLanguageVersion));

            //subscribe to window events
            MainWindow.UpdateFrame += UpdateFrame;
            MainWindow.RenderFrame += RenderFrame;

            //intialize engine
            Logger.Log("Initializing LeaderEngine...");
            Stopwatch stopwatch = new Stopwatch();

            initCallback?.Invoke();

            Renderer.Init();
            Input.Init(MainWindow.KeyboardState, MainWindow.MouseState);

            GLFW.SetErrorCallback(LogGLError);

            stopwatch.Stop();
            //print init complete msg
            Logger.Log($"LeaderEngine initialized. ({stopwatch.ElapsedMilliseconds}ms)");

            //open window
            MainWindow.Run();
        }

        private static void LogGLError(ErrorCode errorCode, string description)
        {
            Logger.LogError("OpenGL: " + errorCode.ToString() + ": " + description);
        }

        private static void UpdateFrame(FrameEventArgs obj)
        {
            //updates time
            float t = (float)obj.Time;

            Time.Elapsed += t;
            Time.DeltaTime = t * Time.TimeScale;
            Time.UnscaledDeltaTime = t;

            //update scene
            DataManager.CurrentScene.UpdateSceneHierachy();

            //update renderer
            Renderer.Update();
        }

        private static void RenderFrame(FrameEventArgs obj)
        {
            GL.Viewport(0, 0, MainWindow.ClientSize.X, MainWindow.ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Renderer.Render();

            MainWindow.SwapBuffers();
        }
    }
}
