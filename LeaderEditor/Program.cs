using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace LeaderEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 5),
                WindowBorder = WindowBorder.Fixed,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1600, 900),
                Title = "LeaderEditor"
            }, new Program().load);
            app.Run();
        }

        public void load()
        {
            GameObject imguiController = new GameObject("ImGui Controller");
            imguiController.AddComponent<ImGuiController>().OnImGui += Program_OnImGui;
        }

        private void Program_OnImGui()
        {
            ImGui.ShowDemoWindow();
        }
    }
}
