using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using ImGuiNET;

namespace LeaderEditor.Gui
{
    public static class ImGuiExtension
    {
        public static void DragVector3(string label, ref Vector3 value, Vector3 defaultValue, float speed = 1.0f)
        {
            ImGui.PushID("X-btn");
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("X-drag");
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.X, speed);
            ImGui.PopID();

            ImGui.SameLine();

            ImGui.PushID("X-btn");
            if (ImGui.Button("Y"))
            {
                value.Y = defaultValue.Y;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("Y-drag");
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.Y, speed);
            ImGui.PopID();

            ImGui.SameLine();

            ImGui.PushID("Z-btn");
            if (ImGui.Button("Z"))
            {
                value.Z = defaultValue.Z;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("Z-drag");
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.Z, speed);
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Text(label);
        }
    }
}
