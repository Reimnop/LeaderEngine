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
        public static GameObject SelectedObject;

        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
        }

        public override void Update()
        {
            if (InputManager.GetKey(Keys.Delete) && SelectedObject != null)
            {
                SceneObjects.Remove(SelectedObject);
                SelectedObject.Destroy();
                SelectedObject = null;
            }

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

                    if (ImGui.Selectable(go.Name, go == SelectedObject, ImGuiSelectableFlags.DontClosePopups))
                        SelectedObject = go;
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
