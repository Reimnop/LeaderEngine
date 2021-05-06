using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : Component
    {
        public static DirectionalLight Main;

        public float Intensity = 1.0f;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        public void CalculateViewProjection(out Matrix4 view, out Matrix4 projection, float size, Vector3 cameraPos)
        {
            projection = Matrix4.CreateOrthographic(size, size, -size / 2.0f, size / 2.0f);

            view = Matrix4.LookAt(
                    cameraPos,
                    cameraPos + BaseTransform.Forward,
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
