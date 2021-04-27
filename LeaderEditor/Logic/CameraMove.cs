using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEditor
{
    public class CameraMove : Component
    {
        public float Speed = 2.0f;
        public float Sensitivity = 0.4f;

        public bool Focus = false;

        private float speedMultiplier = 1.0f;

        private void Start()
        {
            BaseTransform.Position.Y = 2.0f;
        }

        private void Update()
        {
            if (!Focus)
                return;

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