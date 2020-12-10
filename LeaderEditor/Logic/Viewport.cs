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

            MainMenuBar.RegisterWindow("Viewport", this);
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
                        {
                            float moveX = InputManager.GetAxis(Axis.Horizontal);
                            float moveZ = InputManager.GetAxis(Axis.Vertical);

                            OpenTK.Mathematics.Vector3 move = EditorCamera.main.gameObject.transform.forward * moveZ + EditorCamera.main.gameObject.transform.right * moveX;
                            EditorCamera.main.gameObject.transform.position += move * Time.deltaTime * 2.0f;

                            if (InputManager.GetMouse(MouseButton.Right))
                            {
                                OpenTK.Mathematics.Vector2 delta = InputManager.GetMouseDelta() / 2.0f;
                                EditorCamera.main.gameObject.transform.rotationEuler.X -= delta.Y;
                                EditorCamera.main.gameObject.transform.rotationEuler.Y -= delta.X;
                            }
                        }
                    }

                    //display to framebuffer texture on gui
                    ImGui.Image((IntPtr)framebuffer.GetColorTexture(), new Vector2(width, height) / 2.0f, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
                    ImGui.End();
                }
        }
    }
}