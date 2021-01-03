using ImGuiNET;
using LeaderEditor.Compilation;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Application = LeaderEngine.Application;

namespace LeaderEditor
{
    public class MainMenuBar : EditorComponent
    {
        public static Dictionary<string, WindowComponent> windows = new Dictionary<string, WindowComponent>();

        private static Dictionary<string, Action> ImGuiMenuDrawers = new Dictionary<string, Action>()
        {
            { "File", DrawFileMenu },
            { "Windows", DrawWindowsMenu }
        };

        public static void RegisterWindow(string name, WindowComponent window)
            => windows.Add(name, window);

        public override void EditorStart()
            => ImGuiController.main.OnImGui += OnImGui;

        private void OnImGui()
        {
            if (ImGui.BeginMainMenuBar())
            {
                foreach (var item in ImGuiMenuDrawers)
                    if (ImGui.BeginMenu(item.Key))
                    {
                        item.Value();
                        ImGui.EndMenu();
                    }

                ImGui.EndMainMenuBar();
            }
        }

        private static void DrawFileMenu()
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

                    string path = Path.Combine(AssetLoader.LoadedProjectDir, "Assets", "scene.ldrscene");

                    if (File.Exists(path))
                        SceneLoad.LoadScene(path);
                }
            }

            if (ImGui.MenuItem("Save Project", "Ctrl+S"))
            {
                if (!string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                    SceneSave.SaveScene(Path.Combine(AssetLoader.LoadedProjectDir, "Assets", "scene.ldrscene"));
            }

#if DEBUG
            if (ImGui.BeginMenu("DEBUG"))
            {
                if (ImGui.MenuItem("Generate Code"))
                {
                    DebugConsole.Log(CodeGenerator.GenerateCode());
                }
                if (ImGui.MenuItem("Recompile shaders"))
                {
                    DebugConsole.Log("Recompiling!");
                    LeaderEngine.Shader.InitDefaults();
                }
                ImGui.EndMenu();
            }
#endif
            if (ImGui.BeginMenu("Import"))
            {
                if (ImGui.MenuItem("Model"))
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "3D Model|*.fbx;*.obj";
                        ofd.Multiselect = false;

                        ofd.ShowDialog();

                        if (!string.IsNullOrEmpty(ofd.FileName))
                        {
                            ResourceLoader.LoadModel(AssetLoader.LoadAsset(ofd.FileName));
                        }
                    }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Exit", "Alt+F4"))
            {
                Application.main.Close();
            }
        }

        private static void DrawWindowsMenu()
        {
            foreach (var window in windows)
                ImGui.MenuItem(window.Key, null, ref window.Value.IsOpen);
        }
    }
}
