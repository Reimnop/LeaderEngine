using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor.Logic
{
    public class MainMenuBar : Component
    {
        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }
    }
}
