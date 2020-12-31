using ImGuiNET;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

namespace LeaderEditor
{
    public class SceneHierachy : WindowComponent
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

        private static readonly Dictionary<RenderHint, string> renderHintText = new Dictionary<RenderHint, string>()
        {
            { RenderHint.World, "[World]" },
            { RenderHint.Transparent, "[World, Transparent]" },
            { RenderHint.Gui, "[Gui]" }
        };

        private static int SelectedObjectIndex = -1;

        private string[] objectTypes = { "World", "Transparent", "Gui" };
        private string currentType = "World";

        public override void EditorStart()
        {
            ImGuiController.main.OnImGui += OnImGui;

            MainMenuBar.RegisterWindow("Scene Hierachy", this);
        }

        public override void EditorUpdate()
        {
            //delete object
            if (Input.GetKeyDown(Keys.Delete) && SelectedObject != null)
            {
                SelectedObject.Destroy();
                SceneObjects.Remove(SelectedObject);

                if (SceneObjects.Count <= SelectedObjectIndex)
                    SelectedObjectIndex = -1;
            }

            //press right ctrl to deselect
            if (Input.GetKeyDown(Keys.RightControl))
                SelectedObjectIndex = -1;
        }

        private void OnImGui()
        {
            //render scene hierachy gui
            if (IsOpen)
                if (ImGui.Begin("Scene Hierachy", ref IsOpen))
                {
                    //select
                    ImGui.SetNextItemWidth(120.0f);

                    if (ImGui.BeginCombo("##combo", currentType))
                    {
                        foreach (string typeStr in objectTypes)
                        {
                            if (ImGui.Selectable(typeStr, currentType == typeStr))
                                currentType = typeStr;
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();

                    //new object button
                    if (ImGui.Button("New Object") && !string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                        CreateNewObject();

                    if (SelectedObject != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Go to {SelectedObject.Name}"))
                        {
                            EditorCamera.main.LookAt(SelectedObject.transform.Position);
                        }
                    }

                    //draw all objects
                    for (int i = 0; i < SceneObjects.Count; i++)
                    {
                        var go = SceneObjects[i];

                        ImGui.PushID(go.Name + i);

                        if (ImGui.Selectable(go.Name, i == SelectedObjectIndex, ImGuiSelectableFlags.DontClosePopups))
                            SelectedObjectIndex = i;
                        ImGui.SameLine();

                        ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));
                        ImGui.Text(renderHintText[go.RenderHint]);
                        ImGui.PopStyleColor();

                        ImGui.PopID();
                    }
                    ImGui.End();
                }
        }

        //new object function
        private void CreateNewObject()
        {
            RenderHint renderHint = RenderHint.World;

            switch (currentType)
            {
                case "World":
                    renderHint = RenderHint.World;
                    break;
                case "Transparent":
                    renderHint = RenderHint.Transparent;
                    break;
                case "Gui":
                    renderHint = RenderHint.Gui;
                    break;
            }

            SceneObjects.Add(new GameObject("New GameObject", renderHint));
        }
    }
}
