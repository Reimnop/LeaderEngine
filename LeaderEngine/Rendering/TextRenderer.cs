using OpenTK.Graphics.OpenGL4;

namespace LeaderEngine
{
    public class TextRenderer : Component, IRenderer
    {
        private string _text = "New Text";
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;
                _font?.GenTextMesh(textMesh, value);
            }
        }

        private Font _font;
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

                _font.GenTextMesh(textMesh, _text);
            }
        }

        public float Width = 0.5f;
        public float Edge = 0.01f;

        private Mesh textMesh;

        private CommandBuffer cmd = new CommandBuffer() { DrawType = DrawType.Transparent };

        private void Start()
        {
            textMesh = new Mesh("text-mesh");
            textMesh.Unlist();

            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
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
            cmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);
            cmd.SetUniformFloat(shader, "width", Width);
            cmd.SetUniformFloat(shader, "edge", Edge);

            cmd.BindTexture(TextureUnit.Texture0, Font.GetTexture());

            cmd.BindMesh(textMesh);
            cmd.DrawMesh(textMesh);

            Engine.Renderer.QueueCommands(cmd);
        }
    }
}
