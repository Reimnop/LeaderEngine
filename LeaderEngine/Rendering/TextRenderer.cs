using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class TextRenderer : Component, IRenderer
    {
        public string Text = "New Text";

        private string _text;
        private string internalText
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;
                Font.GenTextMesh(textMesh, value);
            }
        }

        public Font Font = DefaultFonts.Inconsolata;

        private Mesh textMesh;

        private UniformData uniforms = new UniformData();
        private Material textMaterial = new Material("text material");

        private void Start()
        {
            textMesh = new Mesh("text mesh");

            BaseEntity.Renderers.Add(this);
        }

        private void Update()
        {
            internalText = Text;
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            if (!Enabled)
                return;

            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * view * projection));

            textMaterial.SetTexture2D(TextureUnit.Texture0, Font.GetTexture());

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
