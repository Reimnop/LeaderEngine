using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using OpenTK.Windowing.Common;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor.Logic
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
            framebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        private void PostSceneRender()
        {
            framebuffer.End();
        }


        private void OnImGui()
        {
            ImGui.Begin("Viewport", ImGuiWindowFlags.NoCollapse);
            ImGui.Image((IntPtr)framebuffer.GetTexture(), new Vector2(width, height) / 2.0f);
            ImGui.End();
        }
    }
}