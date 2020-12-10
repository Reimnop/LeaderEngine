using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using OpenTK.Windowing.Common;
using System.Collections.Generic;
using System.Text;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEditor
{
    public class Viewport : WindowComponent
    {
        private Framebuffer framebuffer;

        public Vector2 ViewportSize;

        public override void Start()
        {
            framebuffer = new Framebuffer(1280, 720);

            Application.main.RenderBegin += RenderBegin;
            Application.main.PostGuiRender += PostGuiRender;

            ImGuiController.main.OnImGui += OnImGui;

            MainMenuBar.RegisterWindow("Viewport", this);
        }

        private void RenderBegin()
        {
            //resize viewport
            Application.main.ResizeViewport((int)ViewportSize.X, (int)ViewportSize.Y);

            //resize framebuffer to match viewport
            framebuffer.Resize((int)ViewportSize.X, (int)ViewportSize.Y);

            //render scene to a framebuffer
            framebuffer.Begin();
        }

        private void PostGuiRender()
        {
            //end the render
            framebuffer.End();

            Application.main.ResizeViewport(Application.main.Size);
        }


        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Viewport", ref IsOpen, ImGuiWindowFlags.NoCollapse))
                {
                    if (ImGui.IsWindowFocused())
                    {
                        if (InputManager.GetKeyDown(Keys.P))
                            if (EditorController.Mode == EditorController.EditorMode.Editor)
                                EditorController.Mode = EditorController.EditorMode.Play;
                            else EditorController.Mode = EditorController.EditorMode.Editor;

                        if (EditorController.Mode == EditorController.EditorMode.Editor)
                            EditorCamera.main.UpdateCamMove();
                    }

                    //display to framebuffer texture on gui
                    ImGui.Image((IntPtr)framebuffer.GetColorTexture(), ViewportSize = ImGui.GetContentRegionAvail(), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
                    ImGui.End();
                }
        }
    }
}