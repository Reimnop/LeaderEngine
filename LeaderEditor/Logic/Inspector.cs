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
        private SceneHierachy sceneHierachy;

        public override void Start()
        {
            sceneHierachy = gameObject.GetComponent<SceneHierachy>();

            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            ImGui.Begin("Inspector");

            if (SceneHierachy.SelectedObject != null)
            {
                if (ImGui.Button("Add Component"))
                {

                }

                Component[] components = SceneHierachy.SelectedObject.GetAllComponents();

                foreach (Component component in components)
                {
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
