using System;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 0),
                WindowBorder = WindowBorder.Fixed,
                API = ContextAPI.OpenGL,
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Size = new Vector2i(1600, 900),
                Title = "LeaderEngine"
            }, new Program().OnLoad);
            app.Run();
        }

        float[] verts =
        {
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f,
             0.5f,  0.5f, 0.0f,
             0.5f, -0.5f, 0.0f
        };

        uint[] indices =
        {
            0, 1, 3,
            3, 2, 1
        };

        void OnLoad()
        {
            VertexArray vertexArray = new VertexArray(verts, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 }
            });

            GameObject camera = new GameObject("Main Camera");
            camera.AddComponent<Camera>();

            GameObject test = new GameObject("Test GameObject");
            test.AddComponent<MeshFilter>(vertexArray);
            test.AddComponent<MeshRenderer>();
            test.AddComponent<Trans>();
            test.transform.position = new Vector3(0.0f, 0.0f, -3.0f);
        }
    }

    class Trans : Component
    {
        public override void Update()
        {
            gameObject.transform.position = new Vector3(MathF.Sin(Time.time), MathF.Cos(Time.time), MathF.Sin(Time.time) - 3.0f);
            gameObject.transform.rotationEuler.Y = MathF.Sin(Time.time) * MathF.PI;
        }
    }
}
