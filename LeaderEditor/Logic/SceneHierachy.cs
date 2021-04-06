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
                //SelectedEntity.Destroy();
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

        private int index = 0;

        private void RenderTree()
        {
            index = 0;
            if (ImGui.BeginChild("Scene"))
            {
                for (int i = 0; i < currentSceneEntities.Count; i++)
                {
                    var go = currentSceneEntities[i];

                    RecursivelyRender(go);
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
                        //Application.Main.ExecuteNextUpdate(() => CreateNewObject(null));

                    ImGui.EndPopup();
                }

                ImGui.EndChild();
            }
        }

        private void RecursivelyRender(Entity en)
        {
            ImGui.PushID(en.Name + index);

            index++;

            ImGuiTreeNodeFlags nodeFlags = ImGuiTreeNodeFlags.OpenOnArrow;

            if (SelectedEntity == en)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            //if (!en.ActiveSelf)
                //ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 0.8f));

            bool nodeOpen = ImGui.TreeNodeEx(en.Name, nodeFlags);

            //if (!en.ActiveSelf)
                //ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                SelectedEntity = en;

            if (ImGui.BeginPopupContextItem("Entity Popup"))
            {
                //if (ImGui.MenuItem("New Entity"))
                    //Application.Main.ExecuteNextUpdate(() => CreateNewObject(en));

                //if (ImGui.MenuItem("Delete"))
                    //Application.Main.ExecuteNextUpdate(() => en.Destroy());

                ImGui.EndPopup();
            }

            //ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            //ImGui.Text(renderHintText[en.EntityType]);
            ImGui.PopStyleColor();

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