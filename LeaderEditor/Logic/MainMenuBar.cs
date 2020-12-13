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
        public static Dictionary<string, WindowComponent> windows = new Dictionary<string, WindowComponent>();

        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        public static void RegisterWindow(string name, WindowComponent window)
        {
            windows.Add(name, window);
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

                    if (ImGui.MenuItem("Save Project", "Ctrl+S"))
                    {
                        if (!string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                            SceneSerializer.SaveScene(Path.Combine(AssetLoader.LoadedProjectDir, "Assets", "scene.ldrscene"));
                    }

                    //TODO: DEBUG CODE
                    if (ImGui.MenuItem("Generate Code"))
                    {
                        DebugConsole.Log(CodeGenerator.GenerateCode());
                    }

                    if (ImGui.MenuItem("Load 100 Textures"))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "*.png|*.png";
                        ofd.Multiselect = false;

                        ofd.ShowDialog();

                        for (int i = 0; i < 100; i++)
                        {
                            new LeaderEngine.Texture().FromFile(ofd.FileName);
                        }

                        DebugConsole.Log("Loaded 100 textures");
                    }

                    if (ImGui.MenuItem("Run GC"))
                    {
                        GC.Collect();
                    }
                    //END OF DEBUG

                    if (ImGui.MenuItem("Exit", "Alt+F4"))
                    {
                        Application.main.Close();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    foreach (var window in windows)
                        ImGui.MenuItem(window.Key, null, ref window.Value.IsOpen);

                    ImGui.EndMenu();
                }
            }
            ImGui.EndMainMenuBar();
        }
    }
}
