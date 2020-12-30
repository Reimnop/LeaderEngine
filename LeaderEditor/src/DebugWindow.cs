using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor
{
    public class DebugWindow : WindowComponent
    {
        public override void Start()
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
                    ImGui.Checkbox("Pause Physics", ref PhysicsController.PausePhysics);
                    ImGui.End();
                }
        }
    }
}
