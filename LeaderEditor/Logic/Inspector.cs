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
        public Dictionary<Type, Action<Component>> SerializeableComponents = new Dictionary<Type, Action<Component>>()
        {
            { typeof(Transform), ComponentEditorGui.Transform },
            { typeof(MeshFilter), ComponentEditorGui.MeshFilter },
            { typeof(MeshRenderer), null },
            { typeof(Sprite), ComponentEditorGui.Sprite },
            { typeof(Camera), null }
        };

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

                List<Component> components = SceneHierachy.SelectedObject.GetAllComponents();

                if (compMenuOpen)
                {
                    if (ImGui.ListBoxHeader("Components"))
                    {
                        foreach (var comp in SerializeableComponents)
                        {
                            if (components.Find(x => x.GetType() == comp.Key) == null)
                                if (ImGui.Button(comp.Key.Name))
                                {
                                    SceneHierachy.SelectedObject.AddComponent((Component)Activator.CreateInstance(comp.Key));
                                }
                        }
                    }
                    ImGui.ListBoxFooter();
                }

                for (int i = 0; i < components.Count; i++)
                {
                    ImGui.Separator();

                    Component component = components[i];

                    ImGui.PushID(i);
                    if (ImGui.CollapsingHeader(component.GetType().Name))
                    {
                        Action<Component> serializeFunc = null;
                        if (SerializeableComponents.ContainsKey(component.GetType()))
                            serializeFunc = SerializeableComponents[component.GetType()];

                        if (serializeFunc == null)
                            ImGui.Text("No property");
                        else
                            serializeFunc.Invoke(component);

                        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 110.0f);
                        if (ImGui.Button("Remove Component"))
                        {
                            SceneHierachy.SelectedObject.RemoveComponent(component);
                        }
                    }
                }
            }

            ImGui.End();
        }
    }
}
