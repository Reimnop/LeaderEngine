using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LeaderEngine
{
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
