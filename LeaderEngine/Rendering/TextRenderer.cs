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
        private struct Character
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2i Size;
            public Vector2i Bearing;
            public int Advance;
        }

        private struct CharacterData
        {
            public int Width;
            public int Height;
            public Vector2i Bearing;
            public int Advance;
            public byte[] BufferData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TextVertex
        {
            [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
            public Vector3 Position;

            [VertexAttrib(VertexAttribPointerType.Float, 2, false)]
            public Vector2 UV;
        }

        const int fontHeight = 96;
        const int glyphs = 256;
        
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

        private Character[] characters = new Character[glyphs];

        private Texture fontTexture;
        private Mesh textMesh;

        private UniformData uniforms = new UniformData();
        private Material textMaterial = new Material("text material");

        private void Start()
        {
            InitFont(Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Inconsolata.ttf"));

            textMesh = new Mesh("text mesh");
            textMesh.LoadMesh(new TextVertex[0], new uint[0]);

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

            textMaterial.SetTexture2D(TextureUnit.Texture0, fontTexture);

            renderer.PushDrawData(DrawType.Transparent, new GLDrawData
            {
                Mesh = textMesh,
                Shader = DefaultShaders.Text,
                Material = textMaterial,
                Uniforms = uniforms
            });
        }

        private void UpdateTextMesh()
        {
            List<TextVertex> vertices = new List<TextVertex>();
            List<uint> indices = new List<uint>();

            string text = _text;

            int pos = 0;
            uint ind = 0;
            for (int i = 0; i < text.Length; i++)
            {
                Character ch = characters[text[i]];

                float xpos = pos + ch.Bearing.X;
                float ypos = ch.Bearing.Y - ch.Size.Y;

                float w = ch.Size.X;
                float h = ch.Size.Y;

                vertices.AddRange(
                    new TextVertex[] {
                        new TextVertex { Position = new Vector3(xpos + w, ypos + h, 0.0f), UV = new Vector2(ch.End.X, ch.Start.Y) },
                        new TextVertex { Position = new Vector3(xpos + w, ypos, 0.0f),     UV = new Vector2(ch.End.X, ch.End.Y) },
                        new TextVertex { Position = new Vector3(xpos, ypos, 0.0f),         UV = new Vector2(ch.Start.X, ch.End.Y) },
                        new TextVertex { Position = new Vector3(xpos, ypos + h, 0.0f),     UV = new Vector2(ch.Start.X, ch.Start.Y) }
                    });

                indices.AddRange(new uint[]
                {
                    ind + 0, ind + 1, ind + 3,
                    ind + 1, ind + 2, ind + 3
                });

                ind += 4;
                pos += ch.Advance >> 6;
            }

            textMesh.UpdateMesh(vertices.ToArray(), indices.ToArray());
        }

        private void InitFont(string path)
        {
            CharacterData[] characterDatas = new CharacterData[glyphs];

            //cache all characters into array
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            using (Library ft = new Library())
            {
                using (Face face = new Face(ft, path))
                {
                    face.SetPixelSizes(0, fontHeight);

                    for (uint i = 0; i < glyphs; i++)
                    {
                        face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                        var bitmap = face.Glyph.Bitmap;

                        characterDatas[i] = new CharacterData
                        {
                            Width = bitmap.Width,
                            Height = bitmap.Rows,
                            Bearing = new Vector2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                            Advance = face.Glyph.Advance.X.Value,
                            BufferData = bitmap.Buffer == IntPtr.Zero ? new byte[0] : bitmap.BufferData
                        };
                    }
                }
            }

            //calculate atlas size
            Vector2i atlasSize = new Vector2i(0, fontHeight);

            for (int i = 0; i < characterDatas.Length; i++)
            {
                atlasSize.X += characterDatas[i].Width + 1;
            }

            //calculate buffer
            byte[,] buffer = new byte[atlasSize.Y, atlasSize.X];

            int pos = 0;
            for (int i = 0; i < glyphs; i++)
            {
                CharacterData c = characterDatas[i];

                //populate buffer
                for (int x = 0; x < c.Height; x++)
                {
                    for (int y = 0; y < c.Width; y++)
                    {
                        byte b = c.BufferData[x * c.Width + y];

                        buffer[x, pos + y] = b;
                    }
                }

                characters[i] = new Character
                {
                    Start = new Vector2(pos / (float)atlasSize.X, 0.0f),
                    End = new Vector2((pos + c.Width) / (float)atlasSize.X, c.Height / (float)atlasSize.Y),
                    Size = new Vector2i(c.Width, c.Height),
                    Bearing = c.Bearing,
                    Advance = c.Advance
                };

                pos += c.Width + 1;
            }

            //generate texture
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            fontTexture = Texture.FromPointer("font texture",
                atlasSize.X,
                atlasSize.Y,
                handle.AddrOfPinnedObject(),
                (PixelInternalFormat)All.Red,
                PixelFormat.Red,
                PixelType.UnsignedByte);
            handle.Free();
        }
    }
}
