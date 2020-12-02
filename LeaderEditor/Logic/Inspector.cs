using LeaderEngine;
using ImGuiNET;
using LeaderEditor.Gui;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor.Logic
{
    public class Inspector : Component
    {
        private bool compMenuOpen = false;

        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            ImGui.Begin("Inspector");

            if (SceneHierachy.SelectedObject != null)
            {
                if (ImGui.Button("Add Component"))
                    compMenuOpen = !compMenuOpen;

                ImGui.SameLine();

                ImGui.InputText("Name", ref SceneHierachy.SelectedObject.Name, 255);

                if (compMenuOpen)
                {
                    if (ImGui.ListBoxHeader("Components"))
                    {
                        ImGui.Button("Foo");
                        ImGui.Button("Bar");
                    }
                    ImGui.ListBoxFooter();
                }

                Component[] components = SceneHierachy.SelectedObject.GetAllComponents();

                foreach (Component component in components)
                {
                    ImGui.Separator();
                    if (ImGui.CollapsingHeader(component.GetType().Name))
                    {
                        component.OnEditorGui();
                    }
                    ImGui.TreePop();
                }
            }

            ImGui.End();
        }
    }
}
