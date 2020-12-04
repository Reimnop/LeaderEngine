using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using ImGuiNET;

namespace LeaderEditor.Gui
{
    public static class ImGuiExtension
    {
        public static void DragVector2(string label, ref Vector2 value, Vector2 defaultValue, float speed = 1.0f)
        {
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.DragFloat(string.Empty, ref value.X, speed);

            if (ImGui.Button("Y"))
            {
                value.Y = defaultValue.Y;
            }
            ImGui.DragFloat(string.Empty, ref value.Y, speed);

            ImGui.Text(label);
        }

        public static void DragVector3(string label, ref Vector3 value, Vector3 defaultValue, float speed = 1.0f)
        {
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.SameLine();
            ImGui.PushID("X-drag-" + label);
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.X, speed);
            ImGui.PopID();

            ImGui.SameLine();
            if (ImGui.Button("Y"))
            {
                value.Y = defaultValue.Y;
            }
            ImGui.SameLine();
            ImGui.PushID("Y-drag-" + label);
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.Y, speed);
            ImGui.PopID();

            ImGui.SameLine();
            if (ImGui.Button("Z"))
            {
                value.Z = defaultValue.Z;
            }
            ImGui.SameLine();
            ImGui.PushID("Z-drag" + label);
            ImGui.SetNextItemWidth(75.0f);
            ImGui.DragFloat(string.Empty, ref value.Z, speed);
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Text(label);
        }
    }
}
