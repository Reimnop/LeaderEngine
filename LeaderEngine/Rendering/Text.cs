using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using System;

namespace LeaderEngine
{
    public class TextRenderer : Renderer
    {
        private struct Character
        {
            public Texture Texture;
            public Vector2i Size;
            public Vector2i Bearing;
            public int Advance;
        }

        private string _text;
        public string Text 
        { 
            get => _text; 
            set
            {
                if (_text == value)
                    return;

                _text = value;
                UpdateTextMesh();
            }
        }

        private Character[] characters = new Character[128];

        private void Start()
        {
            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }

        private void UpdateTextMesh()
        {

        }

        private void InitFont(string path)
        {
            using (Library ft = new Library())
            {
                using (Face face = new Face(ft, path))
                {
                    face.SetPixelSizes(0, 48);

                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    for (uint i = 0; i < 128; i++)
                    {
                        face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                        var bitmap = face.Glyph.Bitmap;

                        Texture texture = Texture.FromPointer("char-" + (char)i,
                            bitmap.Width,
                            bitmap.Rows,
                            bitmap.Buffer,
                            (PixelInternalFormat)All.Red,
                            PixelFormat.Red,
                            PixelType.UnsignedByte);

                        characters[i] = new Character
                        {
                            Texture = texture,
                            Size = new Vector2i(bitmap.Width, bitmap.Rows),
                            Bearing = new Vector2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                            Advance = face.Glyph.Advance.X.Value
                        };
                    }
                }
            }
        }
    }
}
