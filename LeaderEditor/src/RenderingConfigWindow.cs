using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor
{
    public class RenderingConfigWindow : WindowComponent
    {
        public override void EditorStart()
        {
            ImGuiController.RegisterImGui(OnImGui);

            MainMenuBar.RegisterWindow("Rendering Configuration", this);
        }

        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Rendering"))
                {
                    ImGui.Text("Lighting");

                    var amColor = new System.Numerics.Vector3(LightingController.Ambient.X, LightingController.Ambient.Y, LightingController.Ambient.Z);
                    ImGui.ColorEdit3("Ambient Color", ref amColor);
                    LightingController.Ambient = new OpenTK.Mathematics.Vector3(amColor.X, amColor.Y, amColor.Z);

                    var liColor = new System.Numerics.Vector3(LightingController.LightColor.X, LightingController.LightColor.Y, LightingController.LightColor.Z);
                    ImGui.ColorEdit3("Light Color", ref liColor);
                    LightingController.LightColor = new OpenTK.Mathematics.Vector3(liColor.X, liColor.Y, liColor.Z);

                    ImGui.DragFloat("Intensity", ref LightingController.Intensity);

                    ImGui.End();
                }
        }
    }
}
