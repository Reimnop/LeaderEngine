using ImGuiNET;
using LeaderEngine;
using System.Numerics;
using System.Windows.Forms;

namespace LeaderEditor
{
    public class ProjectController : Component
    {
        private bool projectConfigMenuOpen = false;
        private int selectedIndex = 0;

        private void Start()
        {
            Logger.Log(":help:"); //TODO: lmao

            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
        {
            if (projectConfigMenuOpen)
            {
                if (ImGui.Begin("Project Configuration", ref projectConfigMenuOpen))
                {
                    if (ImGui.BeginTabBar("prj-config-tabs"))
                    {
                        if (ImGui.BeginTabItem("Assets"))
                        {
                            //asset group list
                            if (ImGui.BeginChild("asset-groups-win", new Vector2(210f, 0f), true))
                            {
                                for (int i = 0; i < Project.Assets.Count; i++)
                                {
                                    ImGui.PushID(i);
                                    if (ImGui.Selectable(Project.Assets[i].Name, i == Project.CurrentAssetGroupIndex))
                                    {
                                        selectedIndex = i;
                                    }
                                    ImGui.PopID();
                                }
                                ImGui.EndChild();
                            }

                            //config props
                            ImGui.SameLine();

                            if (ImGui.BeginChild("asset-config-props"))
                            {
                                if (ImGui.Button("Save Current Asset Group"))
                                {
                                    ProjectManager.SaveAssetGroup();
                                }

                                if (ImGui.Button("New Asset Group"))
                                {
                                    Project.Assets.Add(new AssetGroup { FileName = $"untitled_asset_group_{Project.Assets.Count}.ldrassets", Name = $"Untitled Asset Group {Project.Assets.Count}" });
                                    ProjectManager.SaveAssetGroup(Project.Assets.Count - 1);
                                }

                                if (selectedIndex != Project.CurrentAssetGroupIndex)
                                {
                                    ImGui.SameLine();

                                    if (ImGui.Button("Switch to " + Project.Assets[selectedIndex].Name))
                                    {
                                        ProjectManager.SaveAssetGroup();

                                        Project.CurrentAssetGroupIndex = selectedIndex;
                                        ProjectManager.LoadAssetGroup();
                                    }
                                }

                                ImGui.EndChild();
                            }

                            ImGui.EndTabItem();
                        }

                        ImGui.EndTabBar();
                    }

                    ImGui.End();
                }
            }
        }

        public void DrawFileMenu()
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open Project"))
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.ShowDialog();

                        if (!string.IsNullOrEmpty(fbd.SelectedPath))
                        {
                            if (!ProjectManager.OpenProject(fbd.SelectedPath))
                            {
                                Logger.LogError("Invalid project path: " + fbd.SelectedPath);
                            }
                        }
                    }
                }

                if (ImGui.MenuItem("New Project"))
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.ShowDialog();

                        if (!string.IsNullOrEmpty(fbd.SelectedPath))
                        {
                            ProjectManager.NewProject(fbd.SelectedPath);
                        }
                    }
                }

                if (ImGui.MenuItem("Save Project"))
                {
                    ProjectManager.SaveProject();
                }

                if (ImGui.MenuItem("Save Scene"))
                {
                    ProjectManager.SaveScene();
                }

                if (ImGui.MenuItem("Project Configuration"))
                {
                    projectConfigMenuOpen = true;
                }

                ImGui.EndMenu();
            }
        }
    }
}
