using LeaderEngine;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.IO;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                APIVersion = new Version(4, 3)
            }, Init);
        }

        static void Init()
        {
            Shader shader = Shader.FromSourceFile(
                Path.Combine(AppContext.BaseDirectory, "shader.vert"),
                Path.Combine(AppContext.BaseDirectory, "shader.frag"));

            Texture tex = Texture.FromFile("tex.png");

            Material material = new Material(shader);
            material.SetTexture2D(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0, tex);

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

            Entity camera = new Entity("Camera");
            camera.AddComponent<CameraMove>();
            camera.AddComponent<Camera>();

            for (int i = 0; i < 4000; i++)
            {
                Entity entity = new Entity("bruh");
                entity.Transform.Position.X = i / 2.0f;
                entity.Transform.Position.Y = i / 2.0f;
                entity.Transform.Position.Z = i;

                var mr = entity.AddComponent<MeshRenderer>();

                mr.Material = material.Clone();
                mr.Mesh = mesh;
            }
        }
    }

    public class CameraMove : Component
    {
        public float Speed = 2.0f;
        public float Sensitivity = 0.4f;

        private float speedMultiplier = 1.0f;

        void Update()
        {
            if (Input.GetKey(Keys.LeftShift))
                speedMultiplier = 3.0f;
            else speedMultiplier = 1.0f;

            float moveX = Input.GetAxis(Axis.Horizontal);
            float moveZ = Input.GetAxis(Axis.Vertical);

            Vector3 move = BaseTransform.Forward * moveZ + BaseTransform.Right * moveX;
            BaseTransform.Position += move * Time.DeltaTime * Speed * speedMultiplier;

            if (Input.GetMouse(MouseButton.Right))
            {
                Vector2 delta = Input.GetMouseDelta() * Sensitivity;
                BaseTransform.EulerAngles += new Vector3(delta.Y, delta.X, 0.0f);
            }
        }
    }
}
