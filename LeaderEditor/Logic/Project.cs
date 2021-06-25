using System.Collections.Generic;

namespace LeaderEditor
{
    public static class Project
    {
        public static string Path = string.Empty;

        public static string Name = "Untitled Project";

        public static string AssetsPackage = "assets.ldrassets";

        public static int CurrentSceneIndex = 0;
        public static List<string> SceneFiles = new List<string>() { "scene.ldrscene" };
    }
}
