using ImGuiNET;
using LeaderEngine;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace LeaderEditor
{
    public class AssetManager : Component
    {
        public static GameAsset SelectedAsset;

        private List<GameAsset> displayAssets = new List<GameAsset>();

        private string searchStr = string.Empty;
        private GameAssetType filter = GameAssetType.All;

        private GameAssetType[] assetTypes;

        private void Start()
        {
            //register ImGui
            ImGuiController.OnImGui += OnImGui;

            assetTypes = (GameAssetType[])Enum.GetValues(typeof(GameAssetType));

            ApplyFilter(searchStr, filter);
        }

        private void OnImGui()
        {
            if (ImGui.Begin("Asset Manager"))
            {
                if (ImGui.BeginChild("asset-import", new Vector2(140f, 0f), true))
                {
                    {
                        if (ImGui.Button("Import Cubemap"))
                        {
                            var aiw = new AssetImporterWizard("cubemap-importer");
                            aiw.Title = "Import Cubemap";
                        }

                        var assetImporter = AssetImporterWizard.GetAssetImporter("cubemap-importer");

                        if (assetImporter != null)
                        {
                            if (assetImporter.Begin())
                            {
                                string name = assetImporter.InputText("Name");
                                string right = assetImporter.OpenFileDialog("Right", "Image|*.jpg;*.png");
                                string left = assetImporter.OpenFileDialog("Left", "Image|*.jpg;*.png");
                                string top = assetImporter.OpenFileDialog("Top", "Image|*.jpg;*.png");
                                string bottom = assetImporter.OpenFileDialog("Bottom", "Image|*.jpg;*.png");
                                string back = assetImporter.OpenFileDialog("Back", "Image|*.jpg;*.png");
                                string front = assetImporter.OpenFileDialog("Front", "Image|*.jpg;*.png");

                                assetImporter.End();

                                if (assetImporter.Finished())
                                {
                                    Cubemap.FromFile(name, right, left, top, bottom, back, front);

                                    assetImporter.Dispose();
                                }
                            }
                        }
                    }

                    {
                        if (ImGui.Button("Import Audio"))
                        {
                            var aiw = new AssetImporterWizard("audio-importer");
                            aiw.Title = "Import Audio";
                        }

                        var assetImporter = AssetImporterWizard.GetAssetImporter("audio-importer");

                        if (assetImporter != null)
                        {
                            if (assetImporter.Begin())
                            {
                                string name = assetImporter.InputText("Name");
                                string path = assetImporter.OpenFileDialog("Audio Clip", "Audio File|*.wav");

                                assetImporter.End();

                                if (assetImporter.Finished())
                                {
                                    AudioClip.FromFile(name, path);

                                    assetImporter.Dispose();
                                }
                            }
                        }
                    }

                    if (ImGui.Button("Import Model", new Vector2(100f, 0f)))
                    {
                        using (var ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "3D Model|*.fbx;*.obj";

                            ofd.ShowDialog();

                            if (!string.IsNullOrEmpty(ofd.FileName))
                            {
                                AssetImporter.LoadModelFromFile(ofd.FileName);
                            }
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.SameLine();

                if (ImGui.BeginChild("assets", new Vector2(630f, 0f), true))
                {
                    ImGui.Text("Assets");
                    ImGui.SameLine();

                    ImGui.SetNextItemWidth(240f);
                    ImGui.InputText("Search", ref searchStr, 32768);
                    bool editedStr = ImGui.IsItemEdited();

                    ImGui.SameLine();

                    bool editedFilter = false;
                    ImGui.SetNextItemWidth(140f);
                    if (ImGui.BeginCombo("Filter", filter.ToString()))
                    {
                        foreach (GameAssetType assetType in assetTypes)
                        {
                            if (ImGui.Selectable(assetType.ToString(), assetType == filter))
                            {
                                filter = assetType;
                                editedFilter = true;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Refresh") || editedStr || editedFilter)
                    {
                        ApplyFilter(searchStr, filter);
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("assets-sub", Vector2.Zero))
                    {
                        foreach (GameAsset asset in displayAssets)
                        {
                            ImGui.PushID(asset.GetHashCode());

                            if (ImGui.Selectable(asset.Name, asset == SelectedAsset))
                            {
                                SelectedAsset = asset;
                            }

                            if (asset.AssetType == GameAssetType.Prefab) 
                            {
                                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                {
                                    Prefab prefab = (Prefab)asset;
                                    prefab.Instantiate();
                                }
                            }

                            ImGui.SameLine();

                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 0.5f));
                            ImGui.Text($"[{asset.AssetType}]");
                            ImGui.PopStyleColor();

                            ImGui.PopID();
                        }

                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
            }
        }

        private void ApplyFilter(string strFilter, GameAssetType assetTypeFilter)
        {
            displayAssets.Clear();

            foreach (GameAsset asset in LeaderEngine.AssetManager.Assets)
            {
                if (asset.Name.Contains(strFilter, StringComparison.OrdinalIgnoreCase) && assetTypeFilter.HasFlag(asset.AssetType))
                {
                    displayAssets.Add(asset);
                }
            }
        }
    }
}
