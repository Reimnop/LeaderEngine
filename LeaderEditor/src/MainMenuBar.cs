using ImGuiNET;
using LeaderEditor.Compilation;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEngine;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Application = LeaderEngine.Application;

namespace LeaderEditor
{
    public class MainMenuBar : EditorComponent
    {
        public static Dictionary<string, WindowComponent> windows = new Dictionary<string, WindowComponent>();

        private List<LeaderEngine.Texture> textures = new List<LeaderEngine.Texture>();

        public override void EditorStart()
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

                    //TODO: DEBUG CODE
                    if (ImGui.MenuItem("Generate Code"))
                    {
                        DebugConsole.Log(CodeGenerator.GenerateCode());
                    }
                    if (ImGui.MenuItem("Recompile shaders"))
                    {
                        DebugConsole.Log("Recompiling!");
                        LeaderEngine.Shader.InitDefaults();
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
