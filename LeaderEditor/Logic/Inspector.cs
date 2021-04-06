using ImGuiNET;
using LeaderEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LeaderEditor
{
    public class Inspector : Component
    {
        //all serializeable components
        public static Dictionary<Type, Action<Component>> SerializeableComponents = new Dictionary<Type, Action<Component>>()
        {

        };

        //is the add component menu open?
        private bool compMenuOpen = false;

        private void Start()
        {
            //register ImGui
            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
        {
            if (ImGui.Begin("Inspector"))
            {
                if (SceneHierachy.SelectedEntity == null)
                {
                    ImGui.Text("No Entity selected.");
                    goto EndMenu;
                }

                if (ImGui.Button("Add Component"))
                    compMenuOpen = !compMenuOpen;

                ImGui.SameLine();

                ImGui.SetNextItemWidth(150.0f);
                ImGui.InputText("Name", ref SceneHierachy.SelectedEntity.Name, 255);

                //ImGui.SameLine();

                //bool active = SceneHierachy.SelectedEntity.ActiveSelf;
                //ImGui.Checkbox("Enabled", ref active);
                //SceneHierachy.SelectedEntity.SetActive(active);

                //get all components
                Component[] components = SceneHierachy.SelectedEntity.GetComponents<Component>();

                //add component menu
                if (compMenuOpen)
                {
                    if (ImGui.ListBoxHeader("Components"))
                    {
                        foreach (var comp in SerializeableComponents)
                        {
                            if (!components.Any(x => x.GetType() == comp.Key))
                                if (ImGui.Button(comp.Key.Name))
                                {
                                    //create new component and add
                                    SceneHierachy.SelectedEntity.AddComponent((Component)Activator.CreateInstance(comp.Key));
                                }
                        }
                        ImGui.ListBoxFooter();
                    }
                }

                //serialize components in an object
                for (int i = 0; i < components.Length; i++)
                {
                    ImGui.Separator();

                    Component component = components[i];

                    ImGui.PushID(i);
                    if (ImGui.CollapsingHeader(component.GetType().Name))
                    {
                        //get serialize function in dictionary
                        SerializeableComponents.TryGetValue(component.GetType(), out Action<Component> serializeFunc);

                        //if none was found, use the default serialize function
                        if (serializeFunc == null)
                            SerializeFunc.DefaultSerializeFunc(component);
                        else
                            serializeFunc.Invoke(component);

                        //enable/disable button
                        //ImGui.Checkbox("Enabled", ref component.Enabled);

                        //ImGui.SameLine();

                        //remove component button
                        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 125.0f);
                        if (ImGui.Button("Remove Component"))
                        {
                            SceneHierachy.SelectedEntity.RemoveComponent(component);
                        }
                    }
                    ImGui.PopID();
                }

                EndMenu:
                ImGui.End();
            }
        }
    }
}