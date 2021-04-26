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

        public readonly string Name;

        private int fontHeight;

        private Dictionary<int, Character> characters;
        private Texture fontTexture;

        public Font(string name, string path)
        {
            Name = name;

            using (var fnt = new FntParser(path))
            {
                while (fnt.NextToken(out string token, out _))
                {
                    switch (token)
                    {
                        case "info": //parse info
                            LoadFontInfo(fnt);
                            break;
                        case "common": //parse common
                            LoadCommon(fnt);
                            break;
                    }
                }
            }
        }

        private void LoadCommon(FntParser fnt, out int lineHeight)
        {
            lineHeight = 0;

            while (fnt.NextToken(out string token, out _))
            {
                bool eol;
                switch (token)
                {
                    case "lineHeight":
                        {
                            fnt.NextToken(out string tk, out eol);
                            lineHeight = int.Parse(tk);
                        }
                        break;
                    default:
                        fnt.NextToken(out _, out eol);
                        break;
                }

                if (eol)
                    return;
            }
        }

        private void LoadFontInfo(FntParser fnt)
        {
            while (fnt.NextToken(out _, out bool eol))
            {
                if (eol)
                    return;
            }
        }

        public void GenTextMesh(Mesh mesh, string text)
        {
            List<TextVertex> vertices = new List<TextVertex>();
            List<uint> indices = new List<uint>();

            float xOffset = 0;
            float yOffset = 0;
            uint ind = 0;

            float scale = 1.0f / fontHeight;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    xOffset = 0;
                    yOffset -= fontHeight * scale;
                    continue;
                }

                Character ch;
                if (!characters.TryGetValue(text[i], out ch))
                    continue;

                float xpos = xOffset + ch.Bearing.X * scale;
                float ypos = yOffset + (ch.Bearing.Y - ch.Size.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

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
                xOffset += (ch.Advance >> 6) * scale;
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
