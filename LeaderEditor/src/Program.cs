using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace LeaderEditor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Application app = new Application(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 0),
                WindowBorder = WindowBorder.Resizable,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1600, 900),
                Title = "LeaderEditor"
            }, LoadEditor);
            app.Run();
        }

        static void LoadEditor()
        {
            Logger.Log("Starting Editor");

            Application.Main.EditorMode = true;

            Entity editorController = new Entity("EditorController");
            editorController.AddComponent<EditorController>();

            Logger.Log("Ready");
        }
    }
}
