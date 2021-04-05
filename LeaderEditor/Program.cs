using System;
using OpenTK.Windowing.Desktop;
using LeaderEngine;

namespace LeaderEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 3),
                Size = new OpenTK.Mathematics.Vector2i(1600, 900)
            }, InitEditor);
        }

        private static void InitEditor()
        {
            Entity camera = new Entity("Camera");
            camera.AddComponent<Camera>();

            Engine.Renderer = new EditorRenderer();

            Logger.Log("Editor initialized.");
        }
    }
}
