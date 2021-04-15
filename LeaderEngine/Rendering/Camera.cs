using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera Main;

        public float FOV = 60.0f;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        public virtual void CalculateViewProjection(out Matrix4 view, out Matrix4 projection)
        {
            GLRenderer renderer = Engine.Renderer;

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), renderer.ViewportSize.X / (float)renderer.ViewportSize.Y, 0.02f, 800.0f);

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
