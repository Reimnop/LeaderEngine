using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using LeaderEditor.Gui;
using ImGuiNET;
using LeaderEngine;

namespace LeaderEditor.Logic
{
    public class DebugConsole : Component
    {
        public static DebugConsole main { private set; get; }

        private string text = string.Empty;

        public bool isOpen = true;

        public override void Start()
        {
            if (main == null)
                main = this;

            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            if (isOpen)
            {
                if (ImGui.Begin("Console", ref isOpen))
                {
                    if (ImGui.Button("Clear"))
                    {
                        Clear();
                    }

                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 0.4f));
                    if (ImGui.BeginChild("console-scrollview", new Vector2(0.0f, 0.0f), false, ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        ImGui.TextUnformatted(text);
                        ImGui.EndChild();
                    }
                    ImGui.PopStyleColor();

                    ImGui.End();
                }
            }
        }

        public void WriteLine(string value)
        {
            text += value + Environment.NewLine;

            isOpen = true;
        }

        public void Write(string value)
        {
            text += value;

            isOpen = true;
        }

        public void Clear()
        {
            text = string.Empty;
        }
    }
}
