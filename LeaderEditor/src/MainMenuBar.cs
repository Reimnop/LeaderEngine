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
        private static class Theme
        {
            public static string Light = "Light";
            public static string Dark = "Dark";
        }

        private static Dictionary<string, Action> ImGuiMenuDrawers = new Dictionary<string, Action>()
        {
            { "File", DrawFileMenu },
            { "Windows", DrawWindowsMenu }
        };

        private string theme = Theme.Dark;

        public static Dictionary<string, WindowComponent> windows = new Dictionary<string, WindowComponent>();

        public static void RegisterWindow(string name, WindowComponent window)
            => windows.Add(name, window);

        public override void EditorStart()
            => ImGuiController.AddImGuiFunc(OnImGui);

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

                ImGui.SetNextItemWidth(120.0f);
                if (ImGui.BeginCombo("Theme", theme))
                {
                    if (ImGui.Selectable(Theme.Dark, theme == Theme.Dark))
                    {
                        theme = Theme.Dark;
                        ImGui.StyleColorsDark();
                    }
                    if (ImGui.Selectable(Theme.Light, theme == Theme.Light))
                    {
                        theme = Theme.Light;
                        ImGui.StyleColorsLight();
                    }
                    ImGui.EndCombo();
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
                    if (!string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "3D Model|*.fbx;*.obj|All files (*.*)|*.*";
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
                Application.Main.Close();
            }
        }

        private static void DrawWindowsMenu()
        {
            foreach (var window in windows)
                ImGui.MenuItem(window.Key, null, ref window.Value.IsOpen);
        }
    }
}
