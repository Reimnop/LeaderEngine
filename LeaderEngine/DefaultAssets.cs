using System;
using System.IO;

namespace LeaderEngine
{
    public static class DefaultShaders
    {
        public static Shader SingleColor;
        public static Shader Lit;

        internal static void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/");

            SingleColor = Shader.FromSourceFile("single-color",
                Path.Combine(baseDir, "single-color.vert"),
                Path.Combine(baseDir, "single-color.frag"));

            Lit = Shader.FromSourceFile("lit",
                Path.Combine(baseDir, "lit.vert"),
                Path.Combine(baseDir, "lit.frag"));
        }
    }
}
