using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : Component
    {
        public static DirectionalLight Main;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        public void CalculateViewProjection(out Matrix4 view, out Matrix4 projection, float size)
        {
            if (Camera.Main == null)
            {
                view = Matrix4.Identity;
                projection = Matrix4.Identity;
                return;
            }

            projection = Matrix4.CreateOrthographic(size, size, -size / 2.0f, size / 2.0f);

            view = Matrix4.LookAt(
                    Camera.Main.BaseTransform.Position,
                    Camera.Main.BaseTransform.Position + BaseTransform.Forward,
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
