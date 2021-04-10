using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using System;
using System.IO;

namespace LeaderEngine
{
    public class TextRenderer : Renderer
    {
        private struct Character
        {
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

        private TextureArray fontTexture;

        private Character[] characters = new Character[128];

        private void Start()
        {
            InitFont(Path.Combine(AppContext.BaseDirectory, "Fonts/Inconsolata.ttf"));

            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public override void Render()
        {
            //throw new NotImplementedException();
        }

        private void UpdateTextMesh()
        {

        }

        private void InitFont(string path)
        {
            const int glyphs = 128;

            int[] width = new int[glyphs];
            int[] height = new int[glyphs];
            IntPtr[] data = new IntPtr[glyphs];

            using (Library ft = new Library())
            {
                using (Face face = new Face(ft, path))
                {
                    face.SetPixelSizes(0, 48);

                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    for (uint i = 0; i < glyphs; i++)
                    {
                        face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                        var bitmap = face.Glyph.Bitmap;

                        width[i] = bitmap.Width;
                        height[i] = bitmap.Rows;
                        data[i] = bitmap.Buffer;

                        characters[i] = new Character
                        {
                            Size = new Vector2i(bitmap.Width, bitmap.Rows),
                            Bearing = new Vector2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                            Advance = face.Glyph.Advance.X.Value
                        };
                    }
                }
            }

            fontTexture = TextureArray.FromPointer("FontTexture", width, height, data,
                (PixelInternalFormat)All.Red,
                PixelFormat.Red,
                PixelType.UnsignedByte);
        }
    }
}
