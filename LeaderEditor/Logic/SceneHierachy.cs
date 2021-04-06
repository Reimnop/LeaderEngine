using ImGuiNET;
using LeaderEngine;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using System.Linq;

namespace LeaderEditor
{
    public class SceneHierachy : Component
    {
        public static Entity SelectedEntity = null;

        private static List<Entity> currentSceneEntities => DataManager.CurrentScene.SceneEntities;

        private void Start()
        {
            ImGuiController.RegisterImGui(ImGuiRenderer);
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

        private void ImGuiRenderer()
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
                for (int i = 0; i < currentSceneEntities.Count; i++)
                {
                    RecursivelyRender(currentSceneEntities[i]);
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
             
            bool nodeOpen = ImGui.TreeNodeEx(en.Name, nodeFlags);

            if (ImGui.IsItemClicked())
                SelectedEntity = en;

            if (ImGui.BeginPopupContextItem("Entity Popup"))
            {
                if (ImGui.MenuItem("New Entity"))
                {
                    var newEntity = new Entity("New Entity");
                    newEntity.Parent = en;
                }

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

            ImGui.PopID();
        }
    }
}