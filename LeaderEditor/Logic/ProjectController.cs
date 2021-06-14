using ImGuiNET;
using LeaderEngine;

namespace LeaderEditor
{
    public class ProjectController : Component
    {
        private void Start()
        {
            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
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
