namespace LeaderEditor.Compilation
{
    public static class CodeGenerator
    {
        public static string GenerateCode()
        {
            string output = @"
using System;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
                
class Program {
    static void Main(string[] args) {
        LeaderEngine.Application app = new LeaderEngine.Application(new GameWindowSettings(), new NativeWindowSettings()
        {
            APIVersion = new Version(4, 5),
            WindowBorder = WindowBorder.Resizable,
            API = ContextAPI.OpenGL,
            Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core,
            Size = new Vector2i(1600, 900),
        }, LoadGame);
        app.Run();
    }
    static void LoadGame() {
";

            for (int i = 0; i < SceneHierachy.SceneEntities.Count; i++)
            {
                var en = SceneHierachy.SceneEntities[i];
                output += $"        var en_{i} = new LeaderEngine.Entity(\"{en.Name}\");\n";
                foreach (var comp in en.GetAllComponents())
                {
                    output += $"        en_{i}.AddComponent<{comp.GetType().FullName}>();\n";
                }
                output += "\n";
            }

            output += @"
    }
}";

            return output;
        }
    }
}
