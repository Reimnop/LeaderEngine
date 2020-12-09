using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using LeaderEditor.Gui;
using ImGuiNET;
using LeaderEngine;

namespace LeaderEditor
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class DebugConsole : WindowComponent
    {
        public static DebugConsole main { private set; get; }

        private static bool scrollNextFrame = false;

        private bool autoScroll = true;
        private static string text = string.Empty;

        private static Dictionary<LogType, string> logTypeStr = new Dictionary<LogType, string>()
        {
            { LogType.Info, "[INFO] " },
            { LogType.Warning, "[WARNING] " },
            { LogType.Error, "[ERROR] " }
        };

        public override void Start()
        {
            if (main == null)
                main = this;

            ImGuiController.main.OnImGui += OnImGui;

            MainMenuBar.RegisterWindow("Console", this);
        }

        private void OnImGui()
        {
            if (IsOpen)
            {
                if (ImGui.Begin("Console", ref IsOpen))
                {
                    if (ImGui.Button("Clear"))
                    {
                        Clear();
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("AutoScroll", ref autoScroll);

                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 0.4f));
                    if (ImGui.BeginChild("console-scrollview", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        ImGui.TextUnformatted(text);

                        if (autoScroll && scrollNextFrame)
                            ImGui.SetScrollHereY(1.0f);

                        scrollNextFrame = false;
                    }
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                }
                ImGui.End();
            }
        }

        public static void Log(string msg, LogType logType = LogType.Info)
        {
            text += logTypeStr[logType] + msg + Environment.NewLine;

            scrollNextFrame = true;
        }

        public static void Clear()
        {
            text = string.Empty;
        }
    }
}
