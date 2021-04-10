using ImGuiNET;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Vector2 = System.Numerics.Vector2;

namespace LeaderEditor
{
    public class ImGuiController
    {
        public static ImGuiController Main;

        private bool frameBegun;

        private ImMesh mesh;

        private Texture fontTexture;
        private Shader shader;
        
        private int windowWidth => Engine.MainWindow.ClientSize.X;
        private int windowHeight => Engine.MainWindow.ClientSize.Y;

        private static List<Action> ImGuiFuncs = new List<Action>();

        public static void RegisterImGui(Action action)
        {
            ImGuiFuncs.Add(action);
        }

        public static void UnregisterImGui(Action action)
        {
            ImGuiFuncs.Remove(action);
        }

        internal void Init()
        {
            Main = this;

            //Application.Main.CursorVisible = false;

            Engine.MainWindow.TextInput += TextInput;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontFromFileTTF(Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Inconsolata.ttf"), 16);

            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigWindowsResizeFromEdges = true;

            ImGui.StyleColorsDark();

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(0.166666f);

            ImGui.NewFrame();
            frameBegun = true;
        }

        private void TextInput(TextInputEventArgs obj)
        {
            PressChar((char)obj.Unicode);
        }

        private void CreateDeviceResources()
        {
            mesh = new ImMesh("ImGui Mesh", IntPtr.Zero, IntPtr.Zero, 0, 0, new VertexAttrib[] 
            {
                new VertexAttrib(0, 2),
                new VertexAttrib(1, 2),
                new VertexAttrib(2, 4, VertexAttribPointerType.UnsignedByte, sizeof(byte), true)
            }, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = 
@"#version 430 core

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

uniform mat4 projection_matrix;

out vec4 color;
out vec2 texCoord;

void main()
{
    color = in_color;
    texCoord = in_texCoord;
    gl_Position = vec4(in_position, 0.0, 1.0) * projection_matrix;
}";
            string FragmentSource = 
@"#version 430 core

layout(location = 0) out vec4 fragColor;

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

void main()
{
    fragColor = texture(in_fontTexture, texCoord) * color;
}";

            shader = new Shader("ImGui Shader", VertexSource, FragmentSource);
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        private void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

            fontTexture = Texture.FromPointer("ImGui Font Texture", width, height, pixels,
                PixelInternalFormat.Rgba,
                PixelFormat.Rgba,
                PixelType.UnsignedByte);

            fontTexture.SetMagFilter(TextureMagFilter.Linear);
            fontTexture.SetMinFilter(TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)fontTexture.GetHandle());

            io.Fonts.ClearTexData();
        }

        internal void RenderImGui()
        {
            ImGuiFuncs.ForEach(x => x.Invoke());

            if (frameBegun)
            {
                frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        internal void Update(float dt)
        {
            if (frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(dt);
            UpdateImGuiInput();

            frameBegun = true;
            ImGui.NewFrame();
        }

        private void SetPerFrameImGuiData(float dt)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(windowWidth, windowHeight);
            io.DisplayFramebufferScale = Vector2.One;
            io.DeltaTime = dt;
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = Engine.MainWindow.MouseState;
            KeyboardState KeyboardState = Engine.MainWindow.KeyboardState;

            io.MouseDown[0] = MouseState.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = MouseState.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = MouseState.IsButtonDown(MouseButton.Middle);

            var point = MouseState.Position;
            io.MousePos = new Vector2(point.X, point.Y);

            io.MouseWheel = MouseState.ScrollDelta.Y;
            io.MouseWheelH = MouseState.ScrollDelta.X;
            
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key != Keys.Unknown)
                    io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
                return;

            //Update Mesh
            int totalVBSize = draw_data.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>();
            int totalIBSize = draw_data.TotalIdxCount * sizeof(ushort);

            mesh.UpdateMeshData(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    totalVBSize,
                    totalIBSize);

            int vertexOffsetInVertices = 0;
            int indexOffsetInElements = 0;

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                GL.NamedBufferSubData(mesh.VBO, (IntPtr)(vertexOffsetInVertices * Unsafe.SizeOf<ImDrawVert>()), cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);

                GL.NamedBufferSubData(mesh.EBO, (IntPtr)(indexOffsetInElements * sizeof(ushort)), cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                vertexOffsetInVertices += cmd_list.VtxBuffer.Size;
                indexOffsetInElements += cmd_list.IdxBuffer.Size;
            }

            //Enable render states
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f, io.DisplaySize.X, //width
                io.DisplaySize.Y, 0.0f, //height
                -1.0f, 1.0f); //near and far plane

            shader.Use();
            shader.SetMatrix4("projection_matrix", mvp);
            shader.SetInt("in_fontTexture", 0);

            mesh.Use();

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            GL.BindVertexArray(0);
        }
    }
}
