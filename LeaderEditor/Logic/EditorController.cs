using LeaderEditor.Gui;
using OpenTK.Graphics.OpenGL4;
using LeaderEngine;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;

using Texture = LeaderEngine.Texture;


namespace LeaderEditor
{
    public class EditorController : Component
    {
        GameObject ImGuiController;

        public override void Start()
        {
            //add gui controller
            ImGuiController = new GameObject("Controller");
            ImGuiController.AddComponent<ImGuiController>().OnImGui += OnImGui;

            //add all components
            gameObject.AddComponents(
                new Component[] {
                    new InputManager(),
                    new MainMenuBar(),
                    new Viewport(),
                    new SceneHierachy(),
                    new Inspector(),
                    new DebugConsole()
                });
        }

        private void OnImGui()
        {
            //the dockspace
            ImGui.DockSpaceOverViewport();
        }
    }
}
