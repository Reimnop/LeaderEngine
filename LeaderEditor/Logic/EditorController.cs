using LeaderEditor.Gui;
using OpenTK.Graphics.OpenGL4;
using LeaderEngine;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace LeaderEditor
{
    public class EditorController : Component
    {
        public enum EditorMode
        {
            Editor,
            Play
        }

        private static EditorMode _mode;
        public static EditorMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                UpdateMode();
            }
        }

        private GameObject ImGuiController;

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
                    new EditorCamera(),
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

        private static void UpdateMode()
        {
            switch (_mode)
            {
                case EditorMode.Editor:
                    SetupEditorMode();
                    break;
                case EditorMode.Play:
                    SetupPlayMode();
                    break;
            }
        }

        private static void SetupEditorMode()
        {
            DebugConsole.Log("Entering Editor Mode");

            EditorCamera.main.Enabled = true;
            if (Camera.main != null)
                Camera.main.Enabled = false;
            RenderingGlobals.RenderingEnabled = true;
            Application.main.EditorMode = true;
        }

        private static void SetupPlayMode()
        {
            DebugConsole.Log("Entering Play Mode");

            EditorCamera.main.Enabled = false;
            if (Camera.main != null)
                Camera.main.Enabled = true;
            else RenderingGlobals.RenderingEnabled = false;
            Application.main.EditorMode = false;
        }
    }
}
