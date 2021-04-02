using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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

        public static void Init(GameWindowSettings gws, NativeWindowSettings nws)
        {
            MainWindow = new GameWindow(gws, nws);

            //subscribe to window events
            MainWindow.UpdateFrame += UpdateFrame;
            MainWindow.RenderFrame += RenderFrame;

            //open window
            MainWindow.Run();
        }

        private static void UpdateFrame(FrameEventArgs obj)
        {
            //updates time
            float t = (float)obj.Time;

            Time.Elapsed += t;
            Time.DeltaTime = t * Time.TimeScale;
            Time.UnscaledDeltaTime = t;

            //updates entire scene hierachy
            DataManager.CurrentScene.RootEntity.Update();
        }

        private static void RenderFrame(FrameEventArgs obj)
        {
            GL.Viewport(0, 0, MainWindow.ClientSize.X, MainWindow.ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);



            MainWindow.SwapBuffers();
        }
    }
}
