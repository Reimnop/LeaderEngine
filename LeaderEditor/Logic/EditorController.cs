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

        Framebuffer framebuffer;

        float[] vertices =
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };

        uint[] indices =
        {
            0, 1, 2
        };

        GameObject tdhf;

        public override void Start()
        {
            GameObject camera = new GameObject("cam");
            camera.AddComponent<Camera>();

            VertexArray vertexArray = new VertexArray(vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { size = 3, location = 0 }
            });

            tdhf = new GameObject("dsd");
            tdhf.AddComponent<MeshFilter>(vertexArray);
            tdhf.AddComponent<MeshRenderer>();
            tdhf.transform.position.Z = -5.0f;

            framebuffer = new Framebuffer(Application.instance.ClientSize.X, Application.instance.ClientSize.Y);

            Application.instance.SceneRender += SceneRender;
            Application.instance.PostSceneRender += PostSceneRender;

            ImGuiController = new GameObject("ImGuiController");
            ImGuiController.AddComponent<ImGuiController>().OnImGui += OnImGui;
        }

        private void PostSceneRender()
        {
            framebuffer.End();
        }

        private void SceneRender()
        {
            framebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        private void OnImGui()
        {
            ImGui.DockSpaceOverViewport();

            ImGui.Begin("Viewport");
            ImGui.Image((IntPtr)framebuffer.GetTexture(), new Vector2(Application.instance.ClientSize.X, Application.instance.ClientSize.Y) / 2.0f);
            ImGui.End();

            ImGui.Begin("Transform");
            ImGui.DragFloat("X", ref tdhf.transform.position.X, 0.1f);
            ImGui.DragFloat("Y", ref tdhf.transform.position.Y, 0.1f);
            ImGui.DragFloat("Z", ref tdhf.transform.position.Z, 0.1f);
            ImGui.End();
        }
    }
}
