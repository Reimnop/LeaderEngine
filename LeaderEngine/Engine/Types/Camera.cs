using System;
using OpenTK.Windowing.Common;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera main;

        public float VerticalScale = 9.0f;
        public float FOV = MathF.PI / 2.0f;

        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (main != null)
            {
                gameObject.RemoveComponent<Camera>();
                return;
            }
            main = this;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.instance.Size.X / (float)Application.instance.Size.Y, 0.05f, 500.0f);

            Application.instance.Resize += Instance_Resize;
        }

        public override void LateUpdate()
        {
            ViewMatrix = Matrix4.LookAt(gameObject.transform.position,
                                        gameObject.transform.position + new Vector3(0.0f, 0.0f, 1.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f));
        }

        private void Instance_Resize(ResizeEventArgs e)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, e.Width / (float)e.Height, 0.05f, 200.0f);
        }
    }
}
