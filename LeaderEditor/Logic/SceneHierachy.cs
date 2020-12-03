using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace LeaderEditor.Logic
{
    public class SceneHierachy : Component
    {
        public static List<GameObject> SceneObjects = new List<GameObject>();
        public static GameObject SelectedObject { 
            get 
            { 
                if (SelectedObjectIndex < SceneObjects.Count && SelectedObjectIndex > -1)
                    return SceneObjects[SelectedObjectIndex];

                return null;
            } 
        }

        private static int SelectedObjectIndex = -1;

        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        public override void Update()
        {
            if (InputManager.GetKeyDown(Keys.Delete) && SelectedObject != null)
            {
                SelectedObject.Destroy();
                SceneObjects.Remove(SelectedObject);

                if (SceneObjects.Count <= SelectedObjectIndex)
                    SelectedObjectIndex = -1;
            }

            if (InputManager.GetKeyDown(Keys.RightControl))
                SelectedObjectIndex = -1;
        }

        private void OnImGui()
        {
            if (ImGui.Begin("Scene Hierachy"))
            {
                if (ImGui.Button("New Object"))
                    CreateNewObject();

                for (int i = 0; i < SceneObjects.Count; i++)
                {
                    var go = SceneObjects[i];

                    ImGui.PushID(go.Name + i);

                    if (ImGui.Selectable(go.Name, i == SelectedObjectIndex, ImGuiSelectableFlags.DontClosePopups))
                        SelectedObjectIndex = i;
                }
            }
            ImGui.End();
        }

        private void CreateNewObject()
        {
            SceneObjects.Add(new GameObject("New GameObject"));
        }
    }
}
