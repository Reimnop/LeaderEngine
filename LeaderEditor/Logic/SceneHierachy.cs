using ImGuiNET;
using LeaderEngine;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

namespace LeaderEditor
{
    public class SceneHierachy : Component
    {
        public static Entity SelectedEntity = null;

        private static List<Entity> currentSceneEntities => DataManager.CurrentScene.SceneEntities;

        private void Start()
        {
            ImGuiController.OnImGui += OnImGui;
        }

        private void Update()
        {
            //delete object
            if (Input.GetKeyDown(Keys.Delete) && SelectedEntity != null)
            {
                SelectedEntity.Destroy();
                SelectedEntity = null;
            }
        }

        private void OnImGui()
        {
            //render scene hierachy gui
            if (ImGui.Begin("Scene Hierachy"))
            {
                //select
                RenderTree();
                ImGui.End();
            }
        }

        private void RenderTree()
        {
            if (ImGui.BeginChild("Scene"))
            {
                foreach (Entity entity in currentSceneEntities)
                {
                    if (entity.Parent == null)
                    {
                        RecursivelyRender(entity);
                    }
                }

                if (!ImGui.IsAnyItemHovered() && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                        ImGui.OpenPopup("Entity Menu");

                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        SelectedEntity = null;
                }

                if (ImGui.BeginPopup("Entity Menu"))
                {
                    if (ImGui.MenuItem("New Entity"))
                        _ = new Entity("New Entity");

                    ImGui.EndPopup();
                }

                ImGui.EndChild();
            }
        }

        private void RecursivelyRender(Entity en)
        {
            ImGui.PushID(en.GetHashCode());

            ImGuiTreeNodeFlags nodeFlags = ImGuiTreeNodeFlags.OpenOnArrow;

            if (SelectedEntity == en)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            if (!en.Active)
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1f));

            bool nodeOpen = ImGui.TreeNodeEx(en.Name, nodeFlags);

            if (ImGui.IsItemClicked())
                SelectedEntity = en;

            //new entity delete menu
            if (ImGui.BeginPopupContextItem("Entity Popup"))
            {
                if (ImGui.MenuItem("New Entity"))
                    _ = new Entity("New Entity", parent: en);

                if (ImGui.MenuItem("Delete"))
                    en.Destroy();

                ImGui.EndPopup();
            }

            if (nodeOpen)
            {
                for (int i = 0; i < en.Children.Count; i++)
                    RecursivelyRender(en.Children[i]);

                ImGui.TreePop();
            }

            if (!en.Active)
                ImGui.PopStyleColor();

            ImGui.PopID();
        }
    }
}