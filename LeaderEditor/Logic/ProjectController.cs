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
                    if (ImGui.MenuItem("Save assets as \"Bruh\""))
                    {
                        LeaderEngine.AssetManager.SaveAssetsToFile("bruh.ldrassets");
                    }

                    if (ImGui.MenuItem("Load \"Bruh\" assets"))
                    {
                        LeaderEngine.AssetManager.LoadAssetsFromFile("bruh.ldrassets");
                    }
                    ImGui.EndMenu();
                }
#endif

                ImGui.EndMenu();
            }
        }
    }
}
