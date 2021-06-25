using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera Main;

        public float FOV = 60f;

        public float NearPlane = 0.05f;
        public float FarPlane = 500f;

        public bool OrthographicProjection = false;
        public float OrthographicScale = 5f;

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
                float aspect = renderer.ViewportSize.X / (float)renderer.ViewportSize.Y;

                projection = Matrix4.CreateOrthographic(OrthographicScale * 2f * aspect, OrthographicScale * 2f, NearPlane, FarPlane);
            }

            Vector3 pos = BaseTransform.GlobalModelMatrix.ExtractTranslation();

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
