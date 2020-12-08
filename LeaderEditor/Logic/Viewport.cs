using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using OpenTK.Windowing.Common;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor
{
    public class Viewport : Component
    {
        Framebuffer framebuffer;

        int width { get { return Application.main.Size.X; } }
        int height { get { return Application.main.Size.Y; } }

        public override void Start()
        {
            framebuffer = new Framebuffer(width, height);

            Application.main.SceneRender += SceneRender;
            Application.main.PostSceneRender += PostSceneRender;
            Application.main.Resize += Resize;

            ImGuiController.main.OnImGui += OnImGui;
        }

        private void Resize(ResizeEventArgs e)
        {
            framebuffer.Resize(e.Width, e.Height);
        }

        private void SceneRender()
        {
            //render scene to a framebuffer
            framebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        private void PostSceneRender()
        {
            //end the render
            framebuffer.End();
        }


        private void OnImGui()
        {
            if (ImGui.Begin("Viewport", ImGuiWindowFlags.NoCollapse))
            {
                //display to framebuffer texture on gui
                ImGui.Image((IntPtr)framebuffer.GetColorTexture(), new Vector2(width, height) / 2.0f, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
                ImGui.End();
            }
        }
    }
}