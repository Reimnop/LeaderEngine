using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class TextRenderer : Renderer
    {
        private string _text;
        public string Text 
        { 
            get => _text; 
            set
            {
                if (_text == value)
                    return;

                _text = value;
                font.GenTextMesh(textMesh, value);
            }
        }

        private Font font;
        
        private Mesh textMesh;

        private UniformData uniforms = new UniformData();
        private Material textMaterial = new Material("text material");

        private void Start()
        {
            font = new Font("Inconsolata", Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Inconsolata.ttf"));

            textMesh = new Mesh("Inconsolate-mesh");

            BaseEntity.Renderers.Add(this);
        }

        private void Update()
        {
            Text = "According to all known laws of aviation, there is no way a bee should be able to fly.";
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

            textMaterial.SetTexture2D(TextureUnit.Texture0, font.GetTexture());

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
