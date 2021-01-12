using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;

namespace LeaderEditor
{
    public class RenderingConfigWindow : WindowComponent
    {
        public override void EditorStart()
        {
            ImGuiController.AddImGuiFunc(OnImGui);

            MainMenuBar.RegisterWindow("Rendering Configuration", this);
        }

        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Rendering"))
                {
                    ImGui.Text("Screen Space Ambient Occlusion");

                    ImGui.DragFloat("Power", ref Application.Main.SSAOProcessor.Power);
                    ImGui.DragFloat("Radius", ref Application.Main.SSAOProcessor.Radius);
                    ImGui.DragFloat("Bias", ref Application.Main.SSAOProcessor.Bias);

                    ImGui.DragInt("Blur Samples", ref Application.Main.SSAOProcessor.SSAOBlur.BlurSamples);

                    ImGui.Text("Lighting");

                    var defProcessor = Application.Main.SSAOProcessor.DeferredProcessor;
                    var amColor = new System.Numerics.Vector3(defProcessor.AmbientColor.X, defProcessor.AmbientColor.Y, defProcessor.AmbientColor.Z);
                    ImGui.ColorEdit3("Ambient Color", ref amColor);
                    defProcessor.AmbientColor = new OpenTK.Mathematics.Vector3(amColor.X, amColor.Y, amColor.Z);

                    var liColor = new System.Numerics.Vector3(defProcessor.LightColor.X, defProcessor.LightColor.Y, defProcessor.LightColor.Z);
                    ImGui.ColorEdit3("Light Color", ref liColor);
                    defProcessor.LightColor = new OpenTK.Mathematics.Vector3(liColor.X, liColor.Y, liColor.Z);

                    ImGui.End();
                }
        }
    }
}
