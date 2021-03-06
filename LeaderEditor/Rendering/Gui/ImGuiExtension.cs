﻿using ImGuiNET;
using OpenTK.Mathematics;

namespace LeaderEditor
{
    public static class ImGuiExtension
    {
        public static void DragVector2(string label, ref Vector2 value, Vector2 defaultValue, float speed = 1f)
        {
            ImGui.PushID(label);

            ImGui.PushID("X-btn");
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("X-drag");
            ImGui.SetNextItemWidth(75f);
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
            ImGui.SetNextItemWidth(75f);
            ImGui.DragFloat(string.Empty, ref value.Y, speed);
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
        }
        public static void DragVector3(string label, ref Vector3 value, Vector3 defaultValue, float speed = 1f)
        {
            ImGui.PushID(label);

            ImGui.PushID("X-btn");
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("X-drag");
            ImGui.SetNextItemWidth(75f);
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
            ImGui.SetNextItemWidth(75f);
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
            ImGui.SetNextItemWidth(75f);
            ImGui.DragFloat(string.Empty, ref value.Z, speed);
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
        }
        public static void DragVector4(string label, ref Vector4 value, Vector4 defaultValue, float speed = 1f)
        {
            ImGui.PushID(label);

            ImGui.PushID("X-btn");
            if (ImGui.Button("X"))
            {
                value.X = defaultValue.X;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("X-drag");
            ImGui.SetNextItemWidth(75f);
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
            ImGui.SetNextItemWidth(75f);
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
            ImGui.SetNextItemWidth(75f);
            ImGui.DragFloat(string.Empty, ref value.Z, speed);
            ImGui.PopID();

            ImGui.SameLine();

            ImGui.PushID("W-btn");
            if (ImGui.Button("W"))
            {
                value.W = defaultValue.W;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("W-drag");
            ImGui.SetNextItemWidth(75f);
            ImGui.DragFloat(string.Empty, ref value.W, speed);
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
        }
        public static void BeginGlobalDocking()
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.SetNextWindowBgAlpha(0f);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.MenuBar;

            windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse;
            windowFlags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
            ImGui.Begin("imgui-docking", windowFlags);
            ImGui.PopStyleVar(3);

            uint dockspaceID = ImGui.GetID("default-dockspace");
            ImGuiDockNodeFlags dockspaceFlags = ImGuiDockNodeFlags.PassthruCentralNode;
            ImGui.DockSpace(dockspaceID, System.Numerics.Vector2.Zero, dockspaceFlags);
        }
    }
}
