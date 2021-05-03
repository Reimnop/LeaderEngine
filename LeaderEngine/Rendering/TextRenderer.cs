﻿using OpenTK.Graphics.OpenGL4;
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

        private UniformData uniforms = new UniformData();

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

            uniforms.SetUniform("fontAtlas", new Uniform(UniformType.Texture2D,
                new TextureData(TextureUnit.Texture0, _font.GetTexture().GetHandle())));

            uniforms.SetUniform("width", new Uniform(UniformType.Float, Width));
            uniforms.SetUniform("edge", new Uniform(UniformType.Float, Edge));

            renderer.PushDrawData(DrawType.Transparent, new GLDrawData
            {
                SourceEntity = BaseEntity,
                Mesh = textMesh,
                Shader = DefaultShaders.Text,
                Uniforms = uniforms
            });
        }
    }
}
