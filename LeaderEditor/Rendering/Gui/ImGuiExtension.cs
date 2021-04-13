using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace LeaderEditor
{
    public class FilePicker
    {
        private static readonly Dictionary<object, FilePicker> s_filePickers = new Dictionary<object, FilePicker>();
        private static readonly System.Numerics.Vector2 DefaultFilePickerSize = new System.Numerics.Vector2(600, 400);

        public string Title = "Select File";
        public string SelectedFile = null;

        private string currentFolder = "/";

        public static FilePicker GetFilePicker(object o, string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = AppContext.BaseDirectory;
            }

            if (!s_filePickers.TryGetValue(o, out FilePicker fp))
            {
                fp = new FilePicker();
                fp.currentFolder = startingPath;
                s_filePickers.Add(o, fp);
            }

            return fp;
        }

        public void Open()
        {
            ImGui.OpenPopup(Title);
        }

        public bool Draw()
        {
            bool result = false;
            ImGui.SetNextWindowSize(DefaultFilePickerSize, ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal(Title))
            {
                result = DrawFolder();
                ImGui.EndPopup();
            }

            return result;
        }

        private bool TryGetFileInfo(string path, out FileInfo info)
        {
            if (File.Exists(path))
            {
                info = new FileInfo(path);
                return true;
            }
            info = null;
            return false;
        }

        private bool DrawFolder()
        {
            bool result = false;
            
            DirectoryInfo di = new DirectoryInfo(currentFolder);
            if (di.Exists)
            {
                float availY = ImGui.GetContentRegionAvail().Y;
                if (ImGui.BeginChildFrame(1, new System.Numerics.Vector2(0, availY - 30.0f)))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                    if (di.Parent != null)
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                            currentFolder = di.Parent.FullName;

                    foreach (var f in Directory.GetDirectories(di.FullName))
                    {
                        string name = Path.GetFileName(f);
                        
                        if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                            currentFolder = f;
                    }
                    ImGui.PopStyleColor();

                    foreach (var f in Directory.GetFiles(di.FullName))
                    {
                        string name = Path.GetFileName(f);

                        if (ImGui.Selectable(name, SelectedFile == f, ImGuiSelectableFlags.DontClosePopups))
                            SelectedFile = f;
                    }
                    ImGui.EndChildFrame();
                }
            }


            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
            }

            if (!string.IsNullOrEmpty(SelectedFile))
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    ImGui.CloseCurrentPopup();
                }
            }

            return result;
        }
    }

    public static class ImGuiExtension
    {
        public static void DragVector2(string label, ref Vector2 value, Vector2 defaultValue, float speed = 1.0f)
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
            ImGui.Text(label);

            ImGui.PopID();
        }
        public static void DragVector3(string label, ref Vector3 value, Vector3 defaultValue, float speed = 1.0f)
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

            ImGui.PopID();
        }
        public static void DragVector4(string label, ref Vector4 value, Vector4 defaultValue, float speed = 1.0f)
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

            ImGui.PushID("W-btn");
            if (ImGui.Button("W"))
            {
                value.W = defaultValue.W;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.PushID("W-drag");
            ImGui.SetNextItemWidth(75.0f);
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
            ImGui.SetNextWindowBgAlpha(0.0f);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.MenuBar;

            windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse;
            windowFlags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
            ImGui.Begin("imgui-docking", windowFlags);
            ImGui.PopStyleVar(3);

            uint dockspaceID = ImGui.GetID("default-dockspace");
            ImGuiDockNodeFlags dockspaceFlags = ImGuiDockNodeFlags.PassthruCentralNode;
            ImGui.DockSpace(dockspaceID, System.Numerics.Vector2.Zero, dockspaceFlags);
        }
    }
}
