using LeaderEngine;
using ImGuiNET;
using LeaderEditor.Gui;
using System;
using System.Collections.Generic;
using System.Text;
using LeaderEditor.Logic.LeaderCompiler;
using System.IO;

namespace LeaderEditor.Logic
{
    public class Inspector : Component
    {
        //all serializeable components
        public Dictionary<Type, Action<Component>> SerializeableComponents = new Dictionary<Type, Action<Component>>()
        {
            { typeof(Transform), SerializeFunc.Transform },
            { typeof(Camera), null },
            { typeof(MeshFilter), SerializeFunc.MeshFilter },
            { typeof(MeshRenderer), null },
            { typeof(Sprite), SerializeFunc.Sprite }
        };

        //is the add component menu open?
        private bool compMenuOpen = false;

        public override void Start()
        {
            //DEBUG CODE - test the compiler
            Compiler compiler = new Compiler();
            Type[] types = compiler.Compile(File.ReadAllText("source.cs"), out _);

            foreach (var t in types)
            {
                SerializeableComponents.Add(t, null);
            }

            //register ImGui
            ImGuiController.main.OnImGui += OnImGui;
        }

        private void OnImGui()
        {
            if (ImGui.Begin("Inspector"))
            {

                if (SceneHierachy.SelectedObject != null)
                {
                    if (ImGui.Button("Add Component"))
                        compMenuOpen = !compMenuOpen;

                    ImGui.SameLine();

                    ImGui.InputText("Name", ref SceneHierachy.SelectedObject.Name, 255);

                    //get all components
                    List<Component> components = SceneHierachy.SelectedObject.GetAllComponents();

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
                                        SceneHierachy.SelectedObject.AddComponent((Component)Activator.CreateInstance(comp.Key));
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
                                SceneHierachy.SelectedObject.RemoveComponent(component);
                            }
                        }
                    }
                }

                ImGui.End();
            }
        }
    }
}
