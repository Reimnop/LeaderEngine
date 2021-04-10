using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public class Font : IDisposable
    {
        private struct CharacterData
        {
            public int Width;
            public int Height;
            public Vector2i Bearing;
            public int Advance;
            public byte[] BufferData;
        }

        private struct Character
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2i Size;
            public Vector2i Bearing;
            public int Advance;
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

        public readonly string Name;

        private Character[] characters = new Character[glyphs];
        private Texture fontTexture;

        public Font(string name, string path)
        {
            Name = name;

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
            fontTexture = Texture.FromPointer(name + "-font",
                atlasSize.X,
                atlasSize.Y,
                handle.AddrOfPinnedObject(),
                (PixelInternalFormat)All.Red,
                PixelFormat.Red,
                PixelType.UnsignedByte);
            handle.Free();
        }

        public void GenTextMesh(Mesh mesh, string text)
        {
            List<TextVertex> vertices = new List<TextVertex>();
            List<uint> indices = new List<uint>();

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

            if (mesh.Initialized)
                mesh.UpdateMesh(vertices.ToArray(), indices.ToArray());
            else
                mesh.LoadMesh(vertices.ToArray(), indices.ToArray());
        }

        public Texture GetTexture()
            => fontTexture;

        public void Dispose()
        {
            fontTexture.Dispose();
        }
    }
}
