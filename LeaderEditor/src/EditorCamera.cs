using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEditor
{
    public class EditorCamera : EditorComponent
    {
        public static EditorCamera main;

        public float FOV = 1.04719755f; //60 degrees

        public float Speed = 4.0f;
        public float Sensitivity = 0.4f;

        private float speedMultiplier = 1.0f;

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        public override void EditorStart()
        {
            if (main == null)
                main = this;

            Application.main.SceneRender += SceneRender;
            Application.main.GuiRender += GuiRender;
        }

        public override void EditorUpdate()
        {
            if (EditorController.Mode != EditorController.EditorMode.Editor)
                return;

            if (Camera.Main != null)
                Camera.Main.Enabled = false;
        }

        public void LookAt(Vector3 position)
        {
            Vector3 newPos = position + new Vector3(2.0f, 2.0f, 2.0f);
            transform.Position = newPos;
            transform.RotationEuler = new Vector3(30.0f, -45.0f, 0.0f);
        }

        public void UpdateCamMove()
        {
            if (Input.GetKey(Keys.LeftShift))
                speedMultiplier = 2.5f;
            else speedMultiplier = 1.0f;

            float moveX = Input.GetAxis(Axis.Horizontal);
            float moveZ = Input.GetAxis(Axis.Vertical);

            Vector3 move = transform.Forward * moveZ + transform.Right * moveX;
            transform.Position += move * Time.deltaTime * Speed * speedMultiplier;

            if (Input.GetMouse(MouseButton.Right))
            {
                Vector2 delta = Input.GetMouseDelta() * Sensitivity;
                transform.RotationEuler.X += delta.Y;
                transform.RotationEuler.Y += delta.X;
            }

            LightingController.CameraPos = transform.Position;
        }

        private void SceneRender()
        {
            if (!Enabled)
                return;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.main.ViewportSize.X / (float)Application.main.ViewportSize.Y, 0.02f, 1000.0f);

            ViewMatrix = Matrix4.CreateTranslation(-transform.Position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(transform.RotationEuler.X),
                    MathHelper.DegreesToRadians(transform.RotationEuler.Y),
                    MathHelper.DegreesToRadians(transform.RotationEuler.Z))
                    ));

            RenderingGlobals.Projection = ProjectionMatrix;
            RenderingGlobals.View = ViewMatrix;
        }

        private void GuiRender()
        {
            RenderingGlobals.Projection = Matrix4.CreateOrthographic(Application.main.ViewportSize.X, Application.main.ViewportSize.Y, 0.0f, 100.0f);
            RenderingGlobals.View = Matrix4.Identity;
        }
    }
}
