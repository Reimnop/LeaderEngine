using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class Font : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Font;

        public readonly Dictionary<int, Character> Characters = new Dictionary<int, Character>();
        public readonly Dictionary<CharsPair, int> Kernings = new Dictionary<CharsPair, int>();

        public readonly int LineHeight;

        public readonly int PaddingTop;
        public readonly int PaddingLeft;
        public readonly int PaddingBottom;
        public readonly int PaddingRight;

        public readonly int TextureWidth;
        public readonly int TextureHeight;

        public readonly int FontTexture;

        public Font(string name, string path) : base(name)
        {
            FntParser parser = new FntParser(path);

            LineHeight = int.Parse(parser.RootNode["common"][0]["lineHeight"][0].Value);

            string[] paddings = parser.RootNode["info"][0]["padding"][0].Value.Split(',');

            PaddingTop = int.Parse(paddings[0]);
            PaddingLeft = int.Parse(paddings[1]);
            PaddingBottom = int.Parse(paddings[2]);
            PaddingRight = int.Parse(paddings[3]);

            TextureWidth = int.Parse(parser.RootNode["common"][0]["scaleW"][0].Value);
            TextureHeight = int.Parse(parser.RootNode["common"][0]["scaleH"][0].Value);

            int kerningsCount = int.Parse(parser.RootNode["kernings"][0]["count"][0].Value);
            for (int i = 0; i < kerningsCount; i++)
            {
                FntNode node = parser.RootNode["kerning"][i];

                CharsPair pair = new CharsPair(
                    (char)int.Parse(node["first"][0].Value),
                    (char)int.Parse(node["second"][0].Value));

                Kernings.Add(pair, int.Parse(node["amount"][0].Value));
            }

            int charsCount = int.Parse(parser.RootNode["chars"][0]["count"][0].Value);
            for (int i = 0; i < charsCount; i++)
            {
                FntNode node = parser.RootNode["char"][i];

                char c = (char)int.Parse(node["id"][0].Value);

                Vector2i size = new Vector2i(
                    int.Parse(node["width"][0].Value),
                    int.Parse(node["height"][0].Value));

                Vector2i offset = new Vector2i(
                    int.Parse(node["xoffset"][0].Value),
                    int.Parse(node["yoffset"][0].Value));

                Vector2i start = new Vector2i(
                    int.Parse(node["x"][0].Value),
                    int.Parse(node["y"][0].Value));

                Vector2i end = start + size;

                Characters.Add(c, new Character
                {
                    Start = new Vector2(start.X / (float)TextureWidth, start.Y / (float)TextureHeight),
                    End = new Vector2(end.X / (float)TextureWidth, end.Y / (float)TextureHeight),
                    Size = size,
                    Offset = offset,
                    Advance = int.Parse(node["xadvance"][0].Value) - PaddingLeft - PaddingRight
                });
            }

            string texPath = Path.Combine(Path.GetDirectoryName(path), parser.RootNode["page"][0]["file"][0].Value);

            Rgba32[] pixels = Helper.LoadImageFromFile(texPath, out _, out _);

            FontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FontTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureWidth, TextureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteTexture(FontTexture);
        }
    }
}
