using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;
using Shader = LeaderEngine.Shader;

namespace LeaderEditor
{
    public class Viewport : WindowComponent
    {
        private Framebuffer framebuffer;

        public Vector2 ViewportSize;

        private float[] vertices =
        {
            -1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,
             1.0f, -1.0f, 0.0f
        };

        private uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private Mesh gridMesh;
        private Shader gridShader;

        private int viewportTex;

        public override void EditorStart()
        {
            framebuffer = new Framebuffer(1280, 720, new Attachment[] 
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                },
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.DepthComponent,
                    PixelFormat = PixelFormat.DepthComponent,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.DepthAttachment,
                    TextureParams = new TextureParam[]
                    {
                        new TextureParam { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Nearest },
                        new TextureParam { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest }
                    }
                }
            });

            viewportTex = framebuffer.GetTexture(FramebufferAttachment.ColorAttachment0);

            Application.Main.SceneRender += SceneRender;
            Application.Main.PostPostProcess += PostPostProcess;
            Application.Main.PostGuiRender += PostGuiRender;

            ImGuiController.RegisterImGui(OnImGui);

            //setup grid rendering
            gridMesh = new Mesh("Grids", vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 }
            });

            gridShader = Shader.FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/Editor/grid-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/Editor/grid-fs.glsl");
            //end setup

            MainMenuBar.RegisterWindow("Viewport", this);
        }

        private void SceneRender()
        {
            //resize viewport and postprocessor
            if (ViewportSize.X > 0.0f && ViewportSize.Y > 0.0f)
            {
                Application.Main.ResizeViewport((int)ViewportSize.X, (int)ViewportSize.Y);
                framebuffer.Resize((int)ViewportSize.X, (int)ViewportSize.Y);
            }

            //render scene to a framebuffer
            framebuffer.Begin();

            //clear buffers
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void PostPostProcess()
        {
            if (EditorController.Mode != EditorController.EditorMode.Editor)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            gridShader.SetMatrix4("v", RenderingGlobals.View);
            gridShader.SetMatrix4("p", RenderingGlobals.Projection);

            gridShader.Use();
            gridMesh.Use();

            GL.DrawElements(PrimitiveType.Triangles, gridMesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.Blend);
        }

        private void PostGuiRender()
        {
            //end the render
            framebuffer.End();

            GL.Viewport(0, 0, Application.Main.Size.X, Application.Main.Size.Y);
        }


        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Viewport", ref IsOpen, ImGuiWindowFlags.NoCollapse))
                {
                    if (ImGui.IsWindowFocused())
                    {
                        if (Input.GetKeyDown(Keys.P))
                            if (EditorController.Mode == EditorController.EditorMode.Editor)
                                EditorController.Mode = EditorController.EditorMode.Play;
                            else EditorController.Mode = EditorController.EditorMode.Editor;

                        if (EditorController.Mode == EditorController.EditorMode.Editor)
                            EditorCamera.Main.UpdateCamMove();
                    }

                    //display to framebuffer texture on gui
                    ImGui.Image((IntPtr)viewportTex, ViewportSize = ImGui.GetContentRegionAvail(), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
                    ImGui.End();
                }
        }
    }
}