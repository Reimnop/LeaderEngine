using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor
{
    public class EditorCamera : Component
    {
        public static EditorCamera main;

        public float FOV = 1.04719755f; //60 degrees

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (main == null)
                main = this;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.main.Size.X / (float)Application.main.Size.Y, 0.05f, 500.0f);

            Application.main.Resize += Resize;
            Application.main.UpdateFrame += UpdateFrame;
            Application.main.SceneRender += SceneRender;
        }

        private void Resize(ResizeEventArgs e)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, e.Width / (float)e.Height, 0.1f, 200.0f);
        }

        private void UpdateFrame(FrameEventArgs e)
        {
            if (EditorController.Mode != EditorController.EditorMode.Editor)
                return;

            if (Camera.main != null)
                Camera.main.Enabled = false;
        }

        private void SceneRender()
        {
            if (!Enabled)
                return;

            ViewMatrix = Matrix4.CreateTranslation(-gameObject.transform.position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(-new Vector3(
                    MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.X),
                    MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.Y),
                    MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.Z))
                    ));

            RenderingGlobals.Projection = ProjectionMatrix;
            RenderingGlobals.View = ViewMatrix;
        }
    }
}
