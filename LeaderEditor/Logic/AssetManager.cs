using ImGuiNET;
using LeaderEngine;
using System.Numerics;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace LeaderEditor
{
    public class AssetManager : Component
    {
        public static Prefab SelectedPrefab;
        public static Mesh SelectedMesh;
        public static Texture SelectedTexture;
        public static Material SelectedMaterial;

        private void Start()
        {
            //register ImGui
            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
        {
            if (ImGui.Begin("Asset Manager"))
            {
                if (Input.GetKeyDown(Keys.I) && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
                    SelectedPrefab?.Instantiate();

                if (ImGui.BeginChild("prefabs", new Vector2(210.0f, 0.0f), true))
                {
                    ImGui.Text("Prefabs");
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 25.0f);

                    FilePicker fp = FilePicker.GetFilePicker(this, null);

                    if (fp.Draw("Import Model"))
                    {
                        if (!string.IsNullOrEmpty(fp.SelectedFile))
                            DataManager.LoadModelFromFile(fp.SelectedFile);
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-prefabs-win"))
                    {
                        foreach (var p in DataManager.Prefabs)
                            if (ImGui.Selectable(p.Name, SelectedPrefab == p))
                                SelectedPrefab = p;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("meshes", new Vector2(210.0f, 0.0f), true))
                {
                    ImGui.Text("Meshes");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-meshes-win"))
                    {
                        foreach (var m in DataManager.Meshes)
                            if (ImGui.Selectable(m.Name, SelectedMesh == m))
                                SelectedMesh = m;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("textures", new Vector2(210.0f, 0.0f), true))
                {
                    ImGui.Text("Textures");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-tex-win"))
                    {
                        foreach (var t in DataManager.Textures)
                            if (ImGui.Selectable(t.Name, SelectedTexture == t))
                                SelectedTexture = t;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("materials", new Vector2(210.0f, 0.0f), true))
                {
                    ImGui.Text("Materials");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-mats-win"))
                    {
                        foreach (var m in DataManager.Materials)
                            if (ImGui.Selectable(m.Name, SelectedMaterial == m))
                                SelectedMaterial = m;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.End();
            }
        }
    }
}
