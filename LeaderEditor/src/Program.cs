using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LeaderEditor
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main(string[] args)
        {
            AllocConsole();

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
            Application.main.EditorMode = true;

            GameObject editorController = new GameObject("EditorController");
            editorController.AddComponent<EditorController>();
        }
    }
}
