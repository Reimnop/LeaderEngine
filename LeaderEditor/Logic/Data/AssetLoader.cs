using System;
using System.Collections.Generic;
using System.IO;
using LeaderEditor.Compilation;
using Microsoft.CodeAnalysis.Emit;
using System.Text;
using LeaderEngine;
using Assimp;

namespace LeaderEditor.Data
{
    public static class AssetLoader
    {
        private static List<Type> loadedTypes = new List<Type>();
        public static string LoadedProjectDir;

        public static void LoadProject(string prjPath)
        {
            SceneHierachy.SceneObjects.ForEach(x => x.Destroy());
            SceneHierachy.SceneObjects.Clear();

            loadedTypes.ForEach(x => Inspector.SerializeableComponents.Remove(x));
            loadedTypes.Clear();

            LoadedProjectDir = Path.GetDirectoryName(prjPath);

            string scriptsDir = Path.Combine(LoadedProjectDir, "Scripts");
            Directory.CreateDirectory(scriptsDir);

            string[] sourcePaths = Directory.GetFiles(scriptsDir, "*.cs", SearchOption.AllDirectories);

            if (sourcePaths.Length == 0)
                return;

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

        public static string LoadAsset(string path)
        {
            if (!string.IsNullOrEmpty(LoadedProjectDir))
            {
                string fileName = Path.GetFileName(path);
                Directory.CreateDirectory(Path.Combine(LoadedProjectDir, "Assets"));
                string newPath = Path.Combine(LoadedProjectDir, "Assets", fileName);

                if (!File.Exists(newPath))
                    File.Copy(path, newPath);

                return newPath;
            }
            return null;
        }

        public static VertexArray LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();

            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);
            Mesh mesh = scene.Meshes[0];

            uint[] indices = IntToUint(mesh.GetIndices());
            float[] vertices = Vector3ToFloat(mesh.Vertices);

            return new VertexArray(vertices, indices, new VertexAttrib[] 
            {
                new VertexAttrib { location = 0, size = 3 }
            });
        }

        private static float[] Vector3ToFloat(List<Vector3D> vector3Ds)
        {
            List<float> vertices = new List<float>();

            foreach (var v3d in vector3Ds)
            {
                vertices.Add(v3d.X);
                vertices.Add(v3d.Y);
                vertices.Add(v3d.Z);
            }

            return vertices.ToArray();
        }

        private static uint[] IntToUint(int[] ints)
        {
            uint[] uints = new uint[ints.Length];

            for (int i = 0; i < ints.Length; i++)
            {
                uints[i] = (uint)ints[i];
            }

            return uints;
        }
    }
}
