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

                    ImGui.End();
                }
        }
    }
}
