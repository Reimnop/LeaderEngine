using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public class Font : IDisposable
    {
        private struct Character
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2i Size;
            public Vector2i Offset;
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
        public readonly string ID;

        private int fontHeight;

        private Dictionary<int, Character> characters = new Dictionary<int, Character>();
        private Texture fontTexture;

        private int paddingTop;
        private int paddingLeft;
        private int paddingBottom;
        private int paddingRight;

        public Font(string name, string path, string id = null)
        {
            Name = name;

            var parser = new FntParser(path);

            fontTexture = parser.FontTexture;
            fontHeight = parser.LineHeight;

            paddingTop = parser.PaddingTop;
            paddingLeft = parser.PaddingLeft;
            paddingBottom = parser.PaddingBottom;
            paddingRight = parser.PaddingRight;

            foreach (var c in parser.Characters)
            {
                var fChar = c.Value;

                Vector2i start = fChar.Position;
                Vector2i end = fChar.Position + fChar.Size;

                characters.Add(c.Key, new Character
                {
                    Start = new Vector2(start.X / (float)parser.TextureSize.X, start.Y / (float)parser.TextureSize.Y),
                    End = new Vector2(end.X / (float)parser.TextureSize.X, end.Y / (float)parser.TextureSize.Y),
                    Size = fChar.Size,
                    Offset = fChar.Offset,
                    Advance = fChar.Advance - parser.PaddingLeft - parser.PaddingRight
                });
            }

            ID = id != null ? id : RNG.GetRandomID();

            DataManager.Fonts.Add(ID, this);
        }

        public void GenTextMesh(Mesh mesh, string text)
        {
            List<TextVertex> vertices = new List<TextVertex>();
            List<uint> indices = new List<uint>();

            float xOffset = 0;
            float yOffset = 0;
            uint ind = 0;

            float scale = 1.0f / fontHeight;

            int spaceWidth = paddingTop + paddingBottom;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    xOffset = 0;
                    yOffset += (fontHeight - spaceWidth) * scale;
                    continue;
                }

                Character ch;
                if (!characters.TryGetValue(text[i], out ch))
                    continue;

                float xpos = xOffset + ch.Offset.X * scale;
                float ypos = yOffset + ch.Offset.Y * scale;

                float xmax = xpos + ch.Size.X * scale;
                float ymax = ypos + ch.Size.Y * scale;

                float xproper = xpos;
                float yproper = -ypos + spaceWidth * scale + 0.5f;

                float xmaxproper = xmax;
                float ymaxproper = -ymax + spaceWidth * scale + 0.5f;

                vertices.AddRange(
                    new TextVertex[] {
                        new TextVertex { Position = new Vector3(xmaxproper, ymaxproper, 0.0f), UV = new Vector2(ch.End.X,   ch.End.Y  ) },
                        new TextVertex { Position = new Vector3(xmaxproper, yproper,    0.0f), UV = new Vector2(ch.End.X,   ch.Start.Y) },
                        new TextVertex { Position = new Vector3(xproper,    yproper,    0.0f), UV = new Vector2(ch.Start.X, ch.Start.Y) },
                        new TextVertex { Position = new Vector3(xproper,    ymaxproper, 0.0f), UV = new Vector2(ch.Start.X, ch.End.Y  ) }
                    });

                indices.AddRange(new uint[]
                {
                    ind + 0, ind + 1, ind + 3,
                    ind + 1, ind + 2, ind + 3
                });

                ind += 4;
                xOffset += ch.Advance * scale;
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

            DataManager.Fonts.Remove(ID);
        }
    }
}
