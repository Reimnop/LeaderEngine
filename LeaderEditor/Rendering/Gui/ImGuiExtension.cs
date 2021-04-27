using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeaderEditor
{
    public class FilePicker
    {
        private static readonly Dictionary<string, FilePicker> filePickers = new Dictionary<string, FilePicker>();

        private static readonly System.Numerics.Vector2 defaultFilePickerSize = new System.Numerics.Vector2(600.0f, 400.0f);
        private static readonly System.Numerics.Vector2 minFilePickerSize = new System.Numerics.Vector2(400.0f, 300.0f);

        public string Title = "Select File";

        public string Filter = null;
        public string SelectedFile = null;

        private string currentFolder = "/";

        private string[] displayFolders;
        private string[] displayFiles;

        private FilePicker() { }

        public static FilePicker GetFilePicker(string id, string startingPath, string filter)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = AppContext.BaseDirectory;
            }

            if (!filePickers.TryGetValue(id, out FilePicker fp))
            {
                fp = new FilePicker();
                fp.currentFolder = startingPath;
                fp.Filter = filter;
                fp.UpdateDisplay(startingPath);
                filePickers.Add(id, fp);
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
            ImGui.SetNextWindowSize(defaultFilePickerSize, ImGuiCond.FirstUseEver);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, minFilePickerSize);
            if (ImGui.BeginPopupModal(Title))
            {
                result = DrawFolder();
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();

            return result;
        }

        private bool CheckFilter(string path)
        {
            string[] filters = Filter.Split(';');
            string ext = Path.GetExtension(path);
            return filters.Contains(ext);
        }

        private void UpdateDisplay(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            displayFolders = Directory.GetDirectories(directoryInfo.FullName).Where(x => !new DirectoryInfo(x).Attributes.HasFlag(FileAttributes.System)).ToArray();

            string[] files = Directory.GetFiles(directoryInfo.FullName);

            if (!string.IsNullOrEmpty(Filter))
            {
                displayFiles = files.Where(x => CheckFilter(x)).ToArray();
                return;
            }

            displayFiles = files;
        }

        private bool DrawFolder()
        {
            bool result = false;

            DirectoryInfo di = new DirectoryInfo(currentFolder);
            if (di.Exists)
            {
                float availY = ImGui.GetContentRegionAvail().Y;
                if (ImGui.BeginChildFrame(1, new System.Numerics.Vector2(160.0f, availY - 30.0f)))
                {
                    var drives = DriveInfo.GetDrives();

                    ImGui.Text("Drives");
                    ImGui.Separator();
                    foreach (var drive in drives)
                    {
                        if (ImGui.Selectable(drive.Name + $" ({drive.VolumeLabel})"))
                        {
                            SelectedFile = null;
                            currentFolder = drive.Name;
                            UpdateDisplay(drive.Name);
                        }
                    }

                    ImGui.EndChildFrame();
                }

                ImGui.SameLine();
                if (ImGui.BeginChildFrame(2, new System.Numerics.Vector2(0, availY - 30.0f)))
                {
                    //file picker
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                    if (di.Parent != null && ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        currentFolder = di.Parent.FullName;
                        SelectedFile = null;
                        UpdateDisplay(di.Parent.FullName);
                    }

                    foreach (var f in displayFolders)
                    {
                        string name = Path.GetFileName(f);
                        if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                        {
                            currentFolder = f;
                            SelectedFile = null;
                            UpdateDisplay(f);
                        }
                    }

                    ImGui.PopStyleColor();

                    foreach (var f in displayFiles)
                    {
                        string name = Path.GetFileName(f);
                        if (ImGui.Selectable(name, SelectedFile == f, ImGuiSelectableFlags.DontClosePopups))
                            SelectedFile = f;
                    }
                    ImGui.EndChildFrame();
                }
            }

            float availX = ImGui.GetContentRegionMax().X;

            ImGui.SetNextItemWidth(availX - 144.0f);

            string dis = string.IsNullOrEmpty(SelectedFile) ? currentFolder : SelectedFile;
            ImGui.InputText(string.Empty, ref dis, 32786);

            if (ImGui.IsKeyDown(ImGui.GetKeyIndex(ImGuiKey.Enter)))
            {
                if (Directory.Exists(dis))
                {
                    currentFolder = dis;
                    UpdateDisplay(dis);
                }
                else if (File.Exists(dis))
                {
                    SelectedFile = dis;
                    currentFolder = Path.GetDirectoryName(dis);
                    UpdateDisplay(currentFolder);
                }
                else
                {
                    ImGui.OpenPopup("path-invalid");
                }
            }

            if (ImGui.BeginPopup("path-invalid", ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.Text("Specified path does not exist!");
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Open", new System.Numerics.Vector2(60.0f, 0.0f)) && !string.IsNullOrEmpty(SelectedFile) && File.Exists(SelectedFile))
            {
                result = true;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(60.0f, 0.0f)))
            {
                result = false;
                ImGui.CloseCurrentPopup();
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
