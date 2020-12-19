using ImGuiNET;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;

namespace LeaderEditor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application app = new Application(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 5),
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
            Application.main.EditorMode = true;

            GameObject editorController = new GameObject("EditorController");
            editorController.AddComponent<EditorController>();
        }
    }
}
