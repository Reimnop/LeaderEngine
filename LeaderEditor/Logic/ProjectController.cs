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


                ImGui.EndMenu();
            }
        }
    }
}
