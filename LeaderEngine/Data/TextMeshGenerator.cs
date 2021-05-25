using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LeaderEngine
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextVertex
    {
        public Vector3 Position;
        public Vector2 UV;

        public TextVertex(Vector3 position, Vector2 uv)
        {
            Position = position;
            UV = uv;
        }
    }

    public class TextMeshGenerator
    {
        public HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment = VerticalAlignment.Bottom;

        private Font font;

        public TextMeshGenerator(Font font)
        {
            this.font = font;
        }

        public TextVertex[] GenTextMesh(string text)
        {
            string[] lines = Regex.Split(text, "\r\n|\r|\n");

            TextVertex[][] vertexArrays = new TextVertex[lines.Length][];

            float scale = 1f / font.FontHeight;
            int spaceWidth = font.PaddingTop + font.PaddingBottom;

            float yOffset = 0f;
            if (VerticalAlignment == VerticalAlignment.Center)
            {
                yOffset = -(font.FontHeight - spaceWidth) * scale * lines.Length * 0.5f;
            }
            else if (VerticalAlignment == VerticalAlignment.Top)
            {
                yOffset = -(font.FontHeight - spaceWidth) * scale * lines.Length;
            }

            int vertCount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                float xOffset = 0f;
                if (HorizontalAlignment == HorizontalAlignment.Center)
                {
                    xOffset = -GetLineWidth(lines[i]) * 0.5f;
                }
                else if (HorizontalAlignment == HorizontalAlignment.Right)
                {
                    xOffset = -GetLineWidth(lines[i]);
                }
                
                yOffset += (font.FontHeight - spaceWidth) * scale;

                vertexArrays[i] = GenSingleLineMesh(lines[i], xOffset, yOffset);
                vertCount += vertexArrays[i].Length;
            }

            //combine
            TextVertex[] vertices = new TextVertex[vertCount];
            int offset = 0;
            for (int i = 0; i < vertexArrays.Length; i++)
            {
                for (int j = 0; j < vertexArrays[i].Length; j++)
                {
                    vertices[offset] = vertexArrays[i][j];
                    offset++;
                }
            }

            return vertices;
        }

        private TextVertex[] GenSingleLineMesh(string text, float xOffset, float yOffset)
        {
            //init
            int vertCount = text.Count(c => font.Characters.ContainsKey(c)) * 6;
            TextVertex[] vertices = new TextVertex[vertCount];

            float scale = 1f / font.FontHeight;
            int spaceWidth = font.PaddingTop + font.PaddingBottom;

            int offset = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n' || text[i] == '\r')
                    throw new Exception("How did you even get here???");

                Character ch;
                if (!font.Characters.TryGetValue(text[i], out ch))
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
                vertices[offset + 0] = new TextVertex(new Vector3(xmaxproper, ymaxproper, 0f), new Vector2(ch.End.X, ch.End.Y));
                vertices[offset + 1] = new TextVertex(new Vector3(xmaxproper, yproper, 0f), new Vector2(ch.End.X, ch.Start.Y));
                vertices[offset + 2] = new TextVertex(new Vector3(xproper, ymaxproper, 0f), new Vector2(ch.Start.X, ch.End.Y));
                vertices[offset + 3] = new TextVertex(new Vector3(xmaxproper, yproper, 0f), new Vector2(ch.End.X, ch.Start.Y));
                vertices[offset + 4] = new TextVertex(new Vector3(xproper, yproper, 0f), new Vector2(ch.Start.X, ch.Start.Y));
                vertices[offset + 5] = new TextVertex(new Vector3(xproper, ymaxproper, 0f), new Vector2(ch.Start.X, ch.End.Y));

                //update offset
                offset += 6;

                xOffset += ch.Advance * scale;

                //kerning
                if (i < text.Length - 1)
                {
                    int first = text[i];
                    int second = text[i + 1];

                    if (font.Kernings.TryGetValue((first, second), out int amount))
                    {
                        xOffset += amount * scale;
                    }
                }
            }

            return vertices;
        }

        private float GetLineWidth(string text)
        {
            float result = 0f;
            float scale = 1f / font.FontHeight;

            for (int i = 0; i < text.Length; i++)
            {
                Character ch;
                if (!font.Characters.TryGetValue(text[i], out ch))
                    continue;

                result += ch.Advance * scale;

                //kerning
                if (i < text.Length - 1)
                {
                    int first = text[i];
                    int second = text[i + 1];

                    if (font.Kernings.TryGetValue((first, second), out int amount))
                    {
                        result += amount * scale;
                    }
                }
            }

            return result;
        }
    }
}
