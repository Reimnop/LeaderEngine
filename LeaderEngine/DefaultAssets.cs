using System;
using System.IO;

namespace LeaderEngine
{
    public static class DefaultShaders
    {
        public static Shader SingleColor;
        public static Shader Text;
        public static Shader Lit;

        internal static void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/");

            SingleColor = Shader.FromSourceFile("single-color",
                Path.Combine(baseDir, "single-color.vert"),
                Path.Combine(baseDir, "single-color.frag"));

            Text = Shader.FromSourceFile("text",
                Path.Combine(baseDir, "text.vert"),
                Path.Combine(baseDir, "text.frag"));

            Lit = Shader.FromSourceFile("lit",
                Path.Combine(baseDir, "lit.vert"),
                Path.Combine(baseDir, "lit.frag"));
        }
    }

    public static class DefaultFonts
    {
        public static Font Inconsolata;

        public static void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/");

            Inconsolata = new Font("Inconsolata", 
                Path.Combine(baseDir, "Inconsolata.ttf"));
        }
    }
}
