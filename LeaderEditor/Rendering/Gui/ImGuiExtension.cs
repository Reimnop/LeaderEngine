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
        private const string FilePickerID = "###FilePicker";
        private static readonly Dictionary<object, FilePicker> s_filePickers = new Dictionary<object, FilePicker>();
        private static readonly System.Numerics.Vector2 DefaultFilePickerSize = new System.Numerics.Vector2(600, 400);

        public string CurrentFolder { get; set; }
        public string SelectedFile { get; set; }

        private string selected = string.Empty;

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
                fp.CurrentFolder = startingPath;
                s_filePickers.Add(o, fp);
            }

            return fp;
        }

        public bool Draw(string label)
        {
            if (ImGui.Button(label))
            {
                ImGui.OpenPopup(FilePickerID);
            }

            bool result = false;
            bool a = true;
            ImGui.SetNextWindowSize(DefaultFilePickerSize, ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal(FilePickerID, ref a, ImGuiWindowFlags.NoTitleBar))
            {
                result = DrawFolder(ref selected, false);
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

        private bool DrawFolder(ref string selected, bool returnOnSelection = false)
        {
            ImGui.Text("Current Folder: " + CurrentFolder);
            bool result = false;

            float availY = ImGui.GetContentRegionAvail().Y;
            if (ImGui.BeginChildFrame(1, new System.Numerics.Vector2(0, availY - 30.0f)))
            {
                DirectoryInfo di = new DirectoryInfo(CurrentFolder);
                if (di.Exists)
                {
                    if (di.Parent != null)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                        {
                            CurrentFolder = di.Parent.FullName;
                        }
                        ImGui.PopStyleColor();
                    }
                    foreach (var fse in Directory.EnumerateFileSystemEntries(di.FullName))
                    {
                        if (Directory.Exists(fse))
                        {
                            string name = Path.GetFileName(fse);
                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                            if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                            {
                                CurrentFolder = fse;
                            }
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            string name = Path.GetFileName(fse);
                            bool isSelected = SelectedFile == fse;
                            if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                            {
                                SelectedFile = fse;
                                if (returnOnSelection)
                                {
                                    result = true;
                                    selected = SelectedFile;
                                }
                            }
                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                result = true;
                                selected = SelectedFile;
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                }

            }
            ImGui.EndChildFrame();


            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
            }

            if (SelectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    selected = SelectedFile;
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
