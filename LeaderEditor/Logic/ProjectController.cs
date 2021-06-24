using ImGuiNET;
using LeaderEngine;

namespace LeaderEditor
{
    public class ProjectController : Component
    {
        private void Start()
        {
            ImGuiController.OnImGui += OnImGui;
        }

        private void OnImGui()
        {

        }

        public void DrawFileMenu()
        {
            if (ImGui.BeginMenu("File"))
            {
#if DEBUG
                if (ImGui.BeginMenu("DEBUG"))
                {
                    if (ImGui.MenuItem("Save as \"Test\""))
                    {
                        LeaderEngine.AssetManager.SaveAssetsToFile("test.ldrassets");
                        DataManager.SaveSceneToFile("test.ldrscene");
                    }

                    if (ImGui.MenuItem("Load \"Test\""))
                    {
                        LeaderEngine.AssetManager.LoadAssetsFromFile("test.ldrassets");
                        DataManager.LoadSceneFromFile("test.ldrscene");
                    }
                    ImGui.EndMenu();
                }
#endif

                ImGui.EndMenu();
            }
        }
    }
}
