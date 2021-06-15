using ImGuiNET;
using ImGuizmoNET;
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
        public static event Action OnImGui;

        private string vertShaderPath => Path.Combine(AppContext.BaseDirectory, "EditorAssets/Shaders/imgui.vert");
        private string fragShaderPath => Path.Combine(AppContext.BaseDirectory, "EditorAssets/Shaders/imgui.frag");

        private int windowWidth => Engine.MainWindow.ClientSize.X;
        private int windowHeight => Engine.MainWindow.ClientSize.Y;

        private bool frameBegun;

        private int meshVAO;
        private int meshVBO;
        private int meshEBO;

        private int lastVBSize;
        private int lastIBSize;

        private int fontTexture;

        private int shader;
        private int projectionLoc;
        private int textureLoc;

        private List<char> PressedChars = new List<char>();

        internal void Init()
        {
            Main = this;

            Engine.MainWindow.TextInput += TextInput;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            ImGuizmo.SetImGuiContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontFromFileTTF(Path.Combine(AppContext.BaseDirectory, "EditorAssets/Fonts/Inconsolata.ttf"), 16);

            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigWindowsResizeFromEdges = true;

            ImGui.StyleColorsDark();

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(0.166666f);

            ImGui.NewFrame();
            ImGuizmo.BeginFrame();
            frameBegun = true;
        }

        private void TextInput(TextInputEventArgs obj)
        {
            PressChar((char)obj.Unicode);
        }

        private void CreateDeviceResources()
        {
            //generate imgui mesh
            meshVAO = GL.GenVertexArray();
            GL.BindVertexArray(meshVAO);

            //generate vbo
            meshVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            //generate ebo
            meshEBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, meshEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            //attribs
            int stride = sizeof(float) * 4 + sizeof(uint);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0); //aPosition
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, sizeof(float) * 2); //aTexCoord
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof(float) * 4); //aColor
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //compile imgui shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertShaderPath));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragShaderPath));
            GL.CompileShader(fragmentShader);

            shader = GL.CreateProgram();
            GL.AttachShader(shader, vertexShader);
            GL.AttachShader(shader, fragmentShader);
            GL.LinkProgram(shader);

            //cleanup shader
            GL.DetachShader(shader, vertexShader);
            GL.DetachShader(shader, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //get uniform locations
            projectionLoc = GL.GetUniformLocation(shader, "projection");
            textureLoc = GL.GetUniformLocation(shader, "inTexture");

            RecreateFontDeviceTexture();
        }

        private void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

            fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            io.Fonts.SetTexID((IntPtr)fontTexture);

            io.Fonts.ClearTexData();
        }

        internal void RenderImGui()
        {
            OnImGui?.Invoke();       

            if (frameBegun)
            {
                frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

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
            ImGuizmo.BeginFrame();
        }

        private void SetPerFrameImGuiData(float dt)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(windowWidth, windowHeight);
            io.DisplayFramebufferScale = Vector2.One;
            io.DeltaTime = dt;
        }

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

        private void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0)
                return;

            //calculate sizes
            int totalVBSize = drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>();
            int totalIBSize = drawData.TotalIdxCount * sizeof(ushort);

            //resize buffers
            if (totalVBSize > lastVBSize)
            {
                GL.NamedBufferData(meshVBO, totalVBSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                lastVBSize = totalVBSize;
            }

            if (totalIBSize > lastIBSize)
            {
                GL.NamedBufferData(meshEBO, totalIBSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                lastIBSize = totalIBSize;
            }

            int vertexOffsetInVertices = 0;
            int indexOffsetInElements = 0;
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];

                GL.NamedBufferSubData(meshVBO, (IntPtr)(vertexOffsetInVertices * Unsafe.SizeOf<ImDrawVert>()), cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data);
                GL.NamedBufferSubData(meshEBO, (IntPtr)(indexOffsetInElements * sizeof(ushort)), cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data);

                vertexOffsetInVertices += cmdList.VtxBuffer.Size;
                indexOffsetInElements += cmdList.IdxBuffer.Size;
            }

            //enable states
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0f, io.DisplaySize.X, //width
                io.DisplaySize.Y, 0f, //height
                -1f, 1f); //near and far plane
            
            GL.UseProgram(shader);
            GL.UniformMatrix4(projectionLoc, true, ref mvp); //projection
            GL.Uniform1(textureLoc, 0);

            GL.BindVertexArray(meshVAO);

            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            //render command lists
            int vtxOffset = 0;
            int idxOffset = 0;
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];
                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[i];

                    if (pcmd.UserCallback != IntPtr.Zero)
                        throw new NotImplementedException();

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                    var clip = pcmd.ClipRect;
                    GL.Scissor((int)clip.X, windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idxOffset * sizeof(ushort)), vtxOffset);

                    idxOffset += (int)pcmd.ElemCount;
                }
                vtxOffset += cmdList.VtxBuffer.Size;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            GL.BindVertexArray(0);
        }
    }
}
