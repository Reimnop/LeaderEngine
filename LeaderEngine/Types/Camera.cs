using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace LeaderEngine
{
    public class Camera : Component
    {
        //TODO: finish camera
        public static Camera main { private set; get; }

        public float VerticalScale = 9.0f;
        public float FOV = 1.04719755f; //60 degrees

        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (main == null)
                main = this;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.main.Size.X / (float)Application.main.Size.Y, 0.05f, 500.0f);

            Application.main.Resize += Instance_Resize;
        }

        public override void LateUpdate()
        {
            ViewMatrix = Matrix4.CreateTranslation(-gameObject.transform.position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(-gameObject.transform.rotationEuler));
        }

        private void Instance_Resize(ResizeEventArgs e)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, e.Width / (float)e.Height, 0.1f, 200.0f);
        }
    }
}
