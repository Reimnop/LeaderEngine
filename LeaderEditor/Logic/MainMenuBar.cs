using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Emit;
using System.Windows.Forms;
using ImGuiNET;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEditor.Compilation;
using LeaderEngine;

using Application = LeaderEngine.Application;

namespace LeaderEditor
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
                    if (ImGui.MenuItem("Open Project", "Ctrl+O"))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "*.csproj|*.csproj";
                        ofd.Multiselect = false;

                        ofd.ShowDialog();

                        if (!string.IsNullOrEmpty(ofd.FileName))
                        {
                            AssetLoader.LoadProject(ofd.FileName);
                        }
                    }

                    if (ImGui.MenuItem("Exit", "Alt+F4"))
                    {
                        Application.main.Close();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Windows"))
                {
                    ImGui.MenuItem("Console", null, ref DebugConsole.main.isOpen);
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }
    }
}
