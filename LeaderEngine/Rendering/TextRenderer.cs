using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace LeaderEngine
{
    public class TextRenderer : Component, IRenderer
    {
        private string _text = "New Text";
        private Font _font;

        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Bottom;

        private TextMeshGenerator tmg;

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;

                if (_font == null)
                    return;

                var mesh = tmg.GenTextMesh(value);
                UpdateMesh(mesh);
            }
        }
        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value)
                    return;

                _font = value;

                if (_font == null)
                    return;

                tmg = new TextMeshGenerator(value);
                tmg.HorizontalAlignment = _horizontalAlignment;
                tmg.VerticalAlignment = _verticalAlignment;

                var mesh = tmg.GenTextMesh(_text);
                UpdateMesh(mesh);
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment != value)
                {
                    _horizontalAlignment = value;
                    tmg.HorizontalAlignment = value;
                    var mesh = tmg.GenTextMesh(_text);
                    UpdateMesh(mesh);
                }
            }
        }
        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment != value)
                {
                    _verticalAlignment = value;
                    tmg.VerticalAlignment = value;
                    var mesh = tmg.GenTextMesh(_text);
                    UpdateMesh(mesh);
                }
            }
        }

        public float Width = 0.5f;
        public float Edge = 0.01f;

        private int VAO, VBO;
        private int vertCount;

        private CommandBuffer cmd = new CommandBuffer();

        private void Start()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<TextVertex>(), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<TextVertex>(), Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void UpdateMesh(TextVertex[] vertices)
        {
            GL.NamedBufferData(VBO, vertices.Length * Unsafe.SizeOf<TextVertex>(), vertices, BufferUsageHint.DynamicDraw);
            vertCount = vertices.Length;
        }

        private void OnRemove()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
        }

        public void Render(in RenderData renderData)
        {
            if (!Enabled)
                return;

            if (_font == null)
                return;

            var shader = DefaultShaders.Text;

            cmd.Clear();

            cmd.BindShader(shader);
            cmd.SetUniformMatrix4(shader, "mvp", BaseTransform.GlobalModelMatrix * renderData.ViewProjection);
            cmd.SetUniformFloat(shader, "width", Width);
            cmd.SetUniformFloat(shader, "edge", Edge);

            cmd.BindTexture(TextureUnit.Texture0, Font.FontTexture);

            cmd.BindVertexArray(VAO);
            cmd.DrawArrays(PrimitiveType.Triangles, 0, vertCount);

            Engine.Renderer.QueueCommandsTransparent(cmd);
        }
    }
}
