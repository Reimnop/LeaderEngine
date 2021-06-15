using ImGuiNET;
using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeaderEditor
{
    public class Inspector : Component
    {
        //all serializeable components
        public static Dictionary<Type, Action<Component>> SerializeableComponents = new Dictionary<Type, Action<Component>>()
        {
            { typeof(DirectionalLight), null },
            { typeof(TextRenderer), SerializeFunc.SerializeTextRenderer },
            { typeof(AudioSource), SerializeFunc.SerializeAudioSource },
            { typeof(AudioListener), null },
            { typeof(SpriteRenderer), null },
            { typeof(SkyboxRenderer), null }
        };

        private void Start()
        {
            //register ImGui
            ImGuiController.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            if (ImGui.Begin("Inspector"))
            {
                if (SceneHierachy.SelectedEntity == null)
                {
                    ImGui.Text("No Entity selected.");
                    goto EndMenu;
                }

                ImGui.SetNextItemWidth(160f);
                ImGui.InputText("Name", ref SceneHierachy.SelectedEntity.Name, 255);

                ImGui.SameLine();

                ImGui.SetNextItemWidth(160f);
                ImGui.InputText("Tag", ref SceneHierachy.SelectedEntity.Tag, 255);

                if (ImGui.Button("Add Component"))
                    ImGui.OpenPopup("comp-menu");

                ImGui.SameLine();

                ImGui.Checkbox("Enabled", ref SceneHierachy.SelectedEntity.Active);

                //get all components
                var components = SceneHierachy.SelectedEntity.GetComponents<Component>();

                //add component menu
                if (ImGui.BeginPopup("comp-menu"))
                {
                    foreach (var comp in SerializeableComponents)
                    {
                        if (!components.Any(x => x.GetType() == comp.Key))
                            if (ImGui.Selectable(comp.Key.Name))
                                SceneHierachy.SelectedEntity.AddComponent((Component)Activator.CreateInstance(comp.Key)); //create new component and add
                    }
                    ImGui.EndPopup();
                }

                //render transform editor
                ImGui.Separator();
                ImGui.Text("Transform");

                ImGuiExtension.DragVector3("Position", ref SceneHierachy.SelectedEntity.Transform.Position, Vector3.Zero, 0.1f);

                Vector3 euler = SceneHierachy.SelectedEntity.Transform.EulerAngles;
                ImGuiExtension.DragVector3("Rotation", ref euler, Vector3.Zero);
                SceneHierachy.SelectedEntity.Transform.EulerAngles = euler;

                ImGuiExtension.DragVector3("Scale", ref SceneHierachy.SelectedEntity.Transform.Scale, Vector3.One, 0.1f);

                ImGui.Separator();

                ImGuiExtension.DragVector3("Origin Offset", ref SceneHierachy.SelectedEntity.Transform.OriginOffset, Vector3.Zero, 0.1f);

                //serialize components in an entity
                for (int i = 0; i < components.Count; i++)
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
                        ImGui.Checkbox("Enabled", ref component.Enabled);

                        ImGui.SameLine();

                        //remove component button
                        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 30f);
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