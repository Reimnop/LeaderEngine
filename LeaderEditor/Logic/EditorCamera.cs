using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor
{
    public class EditorCamera : Component
    {
        public static EditorCamera main;

        public float FOV = 1.04719755f; //60 degrees

        public float Speed = 2.5f;
        public float Sensitivity = 0.4f;

        private float speedMultiplier = 1.0f;

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (main == null)
                main = this;

            Application.main.UpdateFrame += UpdateFrame;
            Application.main.SceneRender += SceneRender;
        }

        private void UpdateFrame(FrameEventArgs e)
        {
            if (EditorController.Mode != EditorController.EditorMode.Editor)
                return;

            if (Camera.main != null)
                Camera.main.Enabled = false;
        }

        public void UpdateCamMove()
        {
            if (InputManager.GetKey(Keys.LeftShift))
                speedMultiplier = 2.5f;
            else speedMultiplier = 1.0f;

            float moveX = InputManager.GetAxis(Axis.Horizontal);
            float moveZ = InputManager.GetAxis(Axis.Vertical);

            Vector3 move = main.gameObject.Transform.forward * moveZ + gameObject.Transform.right * moveX;
            gameObject.Transform.Position += move * Time.deltaTime * Speed * speedMultiplier;

            if (InputManager.GetMouse(MouseButton.Right))
            {
                Vector2 delta = InputManager.GetMouseDelta() * Sensitivity;
                gameObject.Transform.RotationEuler.X += delta.Y;
                gameObject.Transform.RotationEuler.Y += delta.X;
            }
        }

        private void SceneRender()
        {
            if (!Enabled)
                return;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.main.ViewportSize.X / (float)Application.main.ViewportSize.Y, 0.1f, 200.0f);

            ViewMatrix = Matrix4.CreateTranslation(-gameObject.Transform.Position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.X),
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.Y),
                    MathHelper.DegreesToRadians(gameObject.Transform.RotationEuler.Z))
                    ));

            RenderingGlobals.Projection = ProjectionMatrix;
            RenderingGlobals.View = ViewMatrix;
        }
    }
}
