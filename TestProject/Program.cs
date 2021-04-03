using LeaderEngine;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new System.Version(4, 3)
            }, Init);
        }

        static void Init()
        {
            Shader shader = Shader.FromSourceFile(
                Path.Combine(AppContext.BaseDirectory, "shader.vert"),
                Path.Combine(AppContext.BaseDirectory, "shader.frag"));

            Mesh mesh = new Mesh();
            mesh.LoadMesh(new Vertex[]
            {
                new Vertex { Position = new Vector3(1.0f, 1.0f, 0.0f), Color = new Vector3(1.0f, 0.0f, 0.0f) },
                new Vertex { Position = new Vector3(1.0f, -1.0f, 0.0f), Color = new Vector3(0.0f, 1.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, -1.0f, 0.0f), Color = new Vector3(0.0f, 0.0f, 1.0f) },
                new Vertex { Position = new Vector3(-1.0f, 1.0f, 0.0f), Color = new Vector3(1.0f, 1.0f, 0.0f) },
            }, 
            new uint[] 
            {
                0, 1, 3,
                1, 2, 3
            });

            MeshRenderer meshRenderer = new MeshRenderer();
            meshRenderer.Shader = shader;
            meshRenderer.Mesh = mesh;

            Entity entity = new Entity("bruh");
            entity.AddComponent(meshRenderer);
        }
    }
}
