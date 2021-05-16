using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera Main;

        public float FOV = 60.0f;

        public float NearPlane = 0.05f;
        public float FarPlane = 500.0f;

        public bool OrthographicProjection = false;
        public float OrthographicScale = 5.0f;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        public virtual void CalculateViewProjection(out Matrix4 view, out Matrix4 projection)
        {
            GLRenderer renderer = Engine.Renderer;

            if (!OrthographicProjection)
            {
                projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), renderer.ViewportSize.X / (float)renderer.ViewportSize.Y, NearPlane, FarPlane);
            }
            else
            {
                var winSize = Engine.MainWindow.ClientSize;
                float aspect = winSize.X / winSize.Y;

                projection = Matrix4.CreateOrthographic(OrthographicScale * 2.0f * aspect, OrthographicScale * 2.0f, NearPlane, FarPlane);
            }

            Vector3 pos = BaseTransform.GlobalTransform.ExtractTranslation();

            view = Matrix4.LookAt(
                    pos,
                    pos + BaseTransform.Forward,
                    BaseTransform.Up
                );
        }

        private void OnRemove()
        {
            if (Main == this)
                Main = null;
        }
    }
}
