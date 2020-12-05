using System;
using System.Collections.Generic;
using System.IO;
using LeaderEditor.Compilation;
using Microsoft.CodeAnalysis.Emit;
using System.Text;
using LeaderEngine;

namespace LeaderEditor.Data
{
    public static class AssetLoader
    {
        private static List<Type> loadedTypes = new List<Type>();
        private static string loadedProjectDir;

        public static void LoadProject(string prjPath)
        {
            SceneHierachy.SceneObjects.ForEach(x => x.Destroy());

            loadedTypes.ForEach(x => Inspector.SerializeableComponents.Remove(x));
            loadedTypes.Clear();

            string[] sourcePaths = Directory.GetFiles(loadedProjectDir = Path.GetDirectoryName(prjPath), "*.cs", SearchOption.AllDirectories);
            string[] sources = new string[sourcePaths.Length];

            for (int i = 0; i < sourcePaths.Length; i++)
                sources[i] = File.ReadAllText(sourcePaths[i]);

            EmitResult compilationResult;

            Compiler compiler = new Compiler();
            Type[] types = compiler.Compile(sources, out compilationResult);

            if (compilationResult.Success)
            {
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Component)))
                    {
                        loadedTypes.Add(type);
                        Inspector.SerializeableComponents.Add(type, null);
                    }
                }
            }
        }
    }
}
