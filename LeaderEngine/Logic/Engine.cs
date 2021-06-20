using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;

namespace LeaderEngine
{
    public static class Time
    {
        public static float Elapsed { get; internal set; }
        public static float DeltaTime { get; internal set; }
        public static float UnscaledDeltaTime { get; internal set; }
        public static float TimeScale = 1f;
    }

    public static class Engine
    {
        public static GameWindow MainWindow => _window;
        public static GLRenderer Renderer => _renderer;

        private static GameWindow _window;
        private static GLRenderer _renderer;

        public static void Init(GameWindowSettings gws, NativeWindowSettings nws, GLRenderer renderer = null, Action initCallback = null)
        {
            _window = new GameWindow(gws, nws);

            //log basic info
            Logger.Log("Base Directory: " + AppContext.BaseDirectory, true);
            Logger.Log("Renderer: " + GL.GetString(StringName.Renderer), true);
            Logger.Log("Vendor: " + GL.GetString(StringName.Vendor), true);
            Logger.Log("Version: " + GL.GetString(StringName.Version), true);
            Logger.Log("Shading Language version: " + GL.GetString(StringName.ShadingLanguageVersion), true);

            //subscribe to window events
            MainWindow.UpdateFrame += UpdateFrame;
            MainWindow.RenderFrame += RenderFrame;

            //intialize engine
            Logger.Log("Initializing LeaderEngine...", true);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //init debug callbacks
            GLFW.SetErrorCallback(LogGLFWError);

            DebugProc debugProcCallback = DebugCallback;
            GCHandle debugProcCallbackHandle = GCHandle.Alloc(debugProcCallback);

            GL.DebugMessageCallback(debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            //init modules
            AssetManager.Init();
            DefaultShaders.Init();
            SpriteRenderer.Init();
            SkyboxRenderer.Init();

            _renderer = renderer ?? new ForwardRenderer();
            Renderer.Init();

            AudioManager.Init();

            Input.Init(MainWindow.KeyboardState, MainWindow.MouseState);

            //init main application
            initCallback?.Invoke();

            stopwatch.Stop();
            //print init complete msg
            Logger.Log($"LeaderEngine initialized. ({stopwatch.ElapsedMilliseconds}ms)", true);

            //open window
            MainWindow.Run();
        }

        private static void LogGLFWError(ErrorCode errorCode, string description)
        {
            Logger.LogError("GLFW: " + errorCode.ToString() + ": " + description);
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string messageString = Marshal.PtrToStringAnsi(message, length);

            switch (severity)
            {
                case DebugSeverity.DebugSeverityMedium:
                    Logger.LogWarning($"OpenGL: {messageString}");
                    break;
                case DebugSeverity.DebugSeverityHigh:
                    Logger.LogError($"OpenGL: {messageString}");
#if THROW_ON_GL_ERROR
                    throw new Exception(messageString);
#else
                    break;
#endif
                default:
#if !NO_GL_INFO
                    Logger.Log($"OpenGL: {messageString}");
#endif
                    break;
            }
        }

        private static void UpdateFrame(FrameEventArgs obj)
        {
            //updates time
            float t = (float)obj.Time;

            Time.Elapsed += t;
            Time.DeltaTime = t * Time.TimeScale;
            Time.UnscaledDeltaTime = t;

            //update scene
            DataManager.CurrentScene.UpdateScene();

            //update unlisted entities
            foreach (var entity in DataManager.UnlistedEntities)
                entity.Update();

            //update renderer
            Renderer.Update();
        }

        private static void RenderFrame(FrameEventArgs obj)
        {
            Renderer.Render();

            MainWindow.SwapBuffers();
        }
    }
}
