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

        private CameraMove cm;

        private void Start()
        {
            if (Main == null)
                Main = this;

            ERenderer = (EditorRenderer)Engine.Renderer;

            //init editor gui
            ImGuiController.RegisterImGui(ImGuiRenderer);

            BaseEntity.AddComponent<GridRenderer>();
            BaseEntity.AddComponent<SceneHierachy>();
            BaseEntity.AddComponent<Inspector>();
            BaseEntity.AddComponent<AssetManager>();

            editorCamera = new Entity("EditorCamera");
            editorCamera.AddComponent<Camera>();
            cm = editorCamera.AddComponent<CameraMove>();
        }

        private void ImGuiRenderer()
        {
            //dockspace
            ImGui.DockSpaceOverViewport();

            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new System.Numerics.Vector2(160.0f, 90.0f));
            if (ImGui.Begin("Viewport"))
            {
                cm.Focus = ImGui.IsWindowFocused();

                var vSize = ImGui.GetContentRegionAvail();

                ERenderer.ViewportSize = new Vector2i((int)vSize.X, (int)vSize.Y);

                //display framebuffer texture on window
                ImGui.Image(
                    (IntPtr)ERenderer.Framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0),
                    vSize,
                    new System.Numerics.Vector2(0.0f, 1.0f),
                    new System.Numerics.Vector2(1.0f, 0.0f));

                ImGui.End();
            }
            ImGui.PopStyleVar();
        }
    }
}
