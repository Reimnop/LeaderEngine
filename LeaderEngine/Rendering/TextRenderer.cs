using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace LeaderEngine
{
    public class TextRenderer : Renderer
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

        public override void Render()
        {
            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * renderer.WorldView
                * renderer.WorldProjection));

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
