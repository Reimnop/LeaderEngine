using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace LeaderEditor
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 5),
                WindowBorder = WindowBorder.Resizable,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1440, 810),
                Title = "LeaderEditor"
            }, InitEditor, new EditorRenderer());
        }

        private static void InitEditor()
        {
            Engine.MainWindow.VSync = VSyncMode.Off;

            Entity editorScripts = new Entity("EditorEntity");
            editorScripts.AddComponent<EditorController>();

            editorScripts.Unlist();

            Logger.Log("Editor initialized.", true);
        }
    }
}
