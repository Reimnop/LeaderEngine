using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera Main;

        public float FOV = 60f;

        public float NearPlane = 0.05f;
        public float FarPlane = 280f;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        public virtual void GetViewProjectionMatrices(out Matrix4 view, out Matrix4 projection)
        {
            GLRenderer renderer = Engine.Renderer;

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), renderer.ViewportSize.X / (float)renderer.ViewportSize.Y, NearPlane, FarPlane);
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
