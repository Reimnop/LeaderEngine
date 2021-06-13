using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public struct Character
    {
        public Vector2 Start;
        public Vector2 End;
        public Vector2i Size;
        public Vector2i Offset;
        public int Advance;
    }

    public class Font : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Font;

        public readonly Dictionary<int, Character> Characters;
        public readonly Dictionary<(int, int), int> Kernings;

        public readonly int FontHeight;
        public readonly int PaddingTop;
        public readonly int PaddingLeft;
        public readonly int PaddingBottom;
        public readonly int PaddingRight;

        public Texture FontTexture => _fontTexture;

        private Texture _fontTexture;

        public Font(string name, string path) : base(name)
        {
            var parser = new FntParser(path);

            _fontTexture = parser.FontTexture;
            FontHeight = parser.LineHeight;

            PaddingTop = parser.PaddingTop;
            PaddingLeft = parser.PaddingLeft;
            PaddingBottom = parser.PaddingBottom;
            PaddingRight = parser.PaddingRight;

            Kernings = parser.Kernings;

            Characters = new Dictionary<int, Character>();

            foreach (var c in parser.Characters)
            {
                var fChar = c.Value;

                Vector2i start = fChar.Position;
                Vector2i end = fChar.Position + fChar.Size;

                Characters.Add(c.Key, new Character
                {
                    Start = new Vector2(start.X / (float)parser.TextureSize.X, start.Y / (float)parser.TextureSize.Y),
                    End = new Vector2(end.X / (float)parser.TextureSize.X, end.Y / (float)parser.TextureSize.Y),
                    Size = fChar.Size,
                    Offset = fChar.Offset,
                    Advance = fChar.Advance - parser.PaddingLeft - parser.PaddingRight
                });
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _fontTexture.Dispose();
        }
    }
}
