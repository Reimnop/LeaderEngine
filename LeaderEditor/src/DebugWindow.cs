using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using System;

namespace LeaderEditor
{
    public class DebugWindow : WindowComponent
    {
        public override void EditorStart()
        {
            ImGuiController.main.OnImGui += OnImGui;

            MainMenuBar.RegisterWindow("Debug Window", this);
        }

        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Debug"))
                {
                    ImGui.Text("FPS: " + MathF.Round(100.0f / Time.deltaTime) / 100.0f);
                    ImGui.Text("Frametime: " + MathF.Round(Time.deltaTime * 100000.0f) / 100.0f + "ms");
                    ImGui.End();
                }
        }
    }
}
