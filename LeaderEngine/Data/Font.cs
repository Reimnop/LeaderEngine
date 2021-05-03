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
        private struct TextVertexData
        {
            [VertexAttrib(VertexAttribPointerType.Float, 1, 2, false)]
            public Vector2 UV;
        }

        public readonly string Name;
        public readonly string ID;

        private int fontHeight;
        private Texture fontTexture;

        private Dictionary<int, Character> characters = new Dictionary<int, Character>();
        private Dictionary<(int, int), int> kernings = new Dictionary<(int, int), int>();

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

            kernings = parser.Kernings;

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

            ID = id ?? RNG.GetRandomID();

            DataManager.Fonts.Add(ID, this);
        }

        public void GenTextMesh(Mesh mesh, string text)
        {
            //calculate render char count size
            int charCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                charCount += text[i] != '\n' && characters.ContainsKey(text[i]) ? 1 : 0;
            }

            //init vertex arrays
            Vector3[] vertices = new Vector3[charCount * 4];
            TextVertexData[] perVertexData = new TextVertexData[charCount * 4];
            uint[] indices = new uint[charCount * 6];

            //init variables
            float xOffset = 0;
            float yOffset = 0;

            float scale = 1.0f / fontHeight;

            int spaceWidth = paddingTop + paddingBottom;

            //indices
            uint vertOffset = 0;
            uint indOffset = 0;

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

                //set vertices
                vertices[vertOffset + 0] = new Vector3(xmaxproper, ymaxproper, 0.0f);
                vertices[vertOffset + 1] = new Vector3(xmaxproper, yproper, 0.0f);
                vertices[vertOffset + 2] = new Vector3(xproper, yproper, 0.0f);
                vertices[vertOffset + 3] = new Vector3(xproper, ymaxproper, 0.0f);

                perVertexData[vertOffset + 0] = new TextVertexData { UV = new Vector2(ch.End.X, ch.End.Y) };
                perVertexData[vertOffset + 1] = new TextVertexData { UV = new Vector2(ch.End.X, ch.Start.Y) };
                perVertexData[vertOffset + 2] = new TextVertexData { UV = new Vector2(ch.Start.X, ch.Start.Y) };
                perVertexData[vertOffset + 3] = new TextVertexData { UV = new Vector2(ch.Start.X, ch.End.Y) };

                //set indices
                uint ind = indOffset / 6 * 4;
                indices[indOffset + 0] = ind + 0;
                indices[indOffset + 1] = ind + 1;
                indices[indOffset + 2] = ind + 3;
                indices[indOffset + 3] = ind + 1;
                indices[indOffset + 4] = ind + 2;
                indices[indOffset + 5] = ind + 3;

                //update offset
                vertOffset += 4;
                indOffset += 6;

                xOffset += ch.Advance * scale;

                if (i < text.Length - 1)
                {
                    int first = text[i];
                    int second = text[i + 1];

                    if (kernings.TryGetValue((first, second), out int amount))
                    {
                        xOffset += amount * scale;
                    }
                }
            }

            mesh.LoadMesh(vertices, indices);
            mesh.SetPerVertexData(perVertexData);
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
