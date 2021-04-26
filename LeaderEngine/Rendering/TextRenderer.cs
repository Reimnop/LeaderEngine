using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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

                if (Font == null)
                    return;

                _text = value;
                Font.GenTextMesh(textMesh, value);
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
                Font.GenTextMesh(textMesh, _text);
            }
        }

        private Mesh textMesh;

        private UniformData uniforms = new UniformData();
        private Material textMaterial = new Material("text material");

        private void Start()
        {
            textMesh = new Mesh("text mesh");

            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            if (!Enabled)
                return;

            if (_font == null)
                return;

            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * view * projection));

            textMaterial.SetTexture2D(TextureUnit.Texture0, _font.GetTexture());

            renderer.PushDrawData(DrawType.Transparent, new GLDrawData
            {
                Mesh = textMesh,
                Shader = DefaultShaders.Text,
                Material = textMaterial,
                Uniforms = uniforms
            });
        }
    }
}
