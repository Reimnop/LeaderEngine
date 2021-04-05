using ImGuiNET;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEditor
{
    public class EditorController : Component
    {
        public static EditorController Main;
        public static EditorRenderer ERenderer;

        private Entity editorCamera;

        private void Start()
        {
            if (Main == null)
                Main = this;

            ERenderer = (EditorRenderer)Engine.Renderer;

            //init editor gui
            ImGuiController.RegisterImGui(ImGuiRenderer);

            editorCamera = new Entity("EditorCamera");
            editorCamera.AddComponent<Camera>();
            editorCamera.AddComponent<CameraMove>();
        }

        private void ImGuiRenderer()
        {
            if (ImGui.Begin("Viewport"))
            {
                var vSize = ImGui.GetContentRegionAvail();

                ERenderer.ViewportSize = new Vector2i((int)vSize.X, (int)vSize.Y);

                //display framebuffer texture on gui
                ImGui.Image(
                    (IntPtr)ERenderer.Framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0),
                    vSize, 
                    new System.Numerics.Vector2(0.0f, 1.0f), 
                    new System.Numerics.Vector2(1.0f, 0.0f));

                ImGui.End();
            }
        }
    }
}
