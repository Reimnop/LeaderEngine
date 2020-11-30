using System;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 0),
                WindowBorder = WindowBorder.Fixed,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1600, 900),
                Title = "LeaderEngine"
            }, new Program().OnLoad);
            app.Run();
        }

        void OnLoad()
        {
            GameObject camera = new GameObject("Main Camera");
            camera.AddComponent<Camera>();

            GameObject test = new GameObject("Test GameObject");
            test.transform.position = new Vector3(0.0f, 0.0f, -3.0f);
        }
    }
}
