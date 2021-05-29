using System;
using System.IO;

namespace LeaderEngine
{
    public static class DefaultShaders
    {
        public static Shader SingleColor { get; private set; }
        public static Shader ShadowMap { get; private set; }
        public static Shader Text { get; private set; }
        public static Shader Lit { get; private set; }
        public static Shader Skybox { get; private set; }

        internal static void Init()
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/");

            SingleColor = Shader.FromSourceFile("single-color",
                Path.Combine(baseDir, "single-color.vert"),
                Path.Combine(baseDir, "single-color.frag"));

            ShadowMap = Shader.FromSourceFile("shadowmap",
                Path.Combine(baseDir, "shadowmap.vert"),
                Path.Combine(baseDir, "shadowmap.frag"));

            Text = Shader.FromSourceFile("text",
                Path.Combine(baseDir, "text.vert"),
                Path.Combine(baseDir, "text.frag"));

            Lit = Shader.FromSourceFile("lit",
                Path.Combine(baseDir, "lit.vert"),
                Path.Combine(baseDir, "lit.frag"));

            Skybox = Shader.FromSourceFile("skybox",
                Path.Combine(baseDir, "skybox.vert"),
                Path.Combine(baseDir, "skybox.frag"));
        }
    }
}
