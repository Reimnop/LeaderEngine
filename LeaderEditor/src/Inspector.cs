using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using System;
using System.Collections.Generic;

namespace LeaderEditor
{
    public class Inspector : WindowComponent
    {
        //all serializeable components
        public static Dictionary<Type, Action<Component>> SerializeableComponents = new Dictionary<Type, Action<Component>>()
        {
            { typeof(Transform), SerializeFunc.Transform },
            { typeof(Camera), null },
            { typeof(MeshFilter), SerializeFunc.MeshFilter },
            { typeof(MeshRenderer), null },
            { typeof(AudioSource), null },
            { typeof(AudioListener), null },
            { typeof(BoxCollider), null },
            { typeof(Rigidbody), null },
            { typeof(Staticbody), null },
            { typeof(Skybox), SerializeFunc.Skybox },
            { typeof(Sprite), SerializeFunc.Sprite },
            { typeof(DirectionalLight), null },
            { typeof(UIText), null }
        };

        //is the add component menu open?
        private bool compMenuOpen = false;

        public override void EditorStart()
        {
            //register ImGui
            ImGuiController.RegisterImGui(OnImGui);

            MainMenuBar.RegisterWindow("Inspector", this);
        }

        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Inspector", ref IsOpen))
                {
                    if (SceneHierachy.SelectedEntity != null)
                    {
                        if (ImGui.Button("Add Component"))
                            compMenuOpen = !compMenuOpen;

                        ImGui.SameLine();

                        ImGui.SetNextItemWidth(127.5f);
                        ImGui.InputText("Name", ref SceneHierachy.SelectedEntity.Name, 255);

                        //get all components
                        List<Component> components = SceneHierachy.SelectedEntity.GetAllComponents();

                        //add component menu
                        if (compMenuOpen)
                        {
                            if (ImGui.ListBoxHeader("Components"))
                            {
                                foreach (var comp in SerializeableComponents)
                                {
                                    if (components.Find(x => x.GetType() == comp.Key) == null)
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
                        for (int i = 0; i < components.Count; i++)
                        {
                            ImGui.Separator();

                            Component component = components[i];

                            ImGui.PushID(i);
                            if (ImGui.CollapsingHeader(component.GetType().Name))
                            {
                                //get serialize function in dictionary
                                Action<Component> serializeFunc = null;
                                if (SerializeableComponents.ContainsKey(component.GetType()))
                                    serializeFunc = SerializeableComponents[component.GetType()];

                                //if none was found, use the default serialize function
                                if (serializeFunc == null)
                                    SerializeFunc.DefaultSerializeFunc(component);
                                else
                                    serializeFunc.Invoke(component);

                                //remove component button
                                ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 120.0f);
                                if (ImGui.Button("Remove Component"))
                                {
                                    SceneHierachy.SelectedEntity.RemoveComponent(component);
                                }
                            }
                            ImGui.PopID();
                        }
                    }

                    ImGui.End();
                }
        }
    }
}
