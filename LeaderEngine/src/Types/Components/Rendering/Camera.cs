using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera Main;

        public float FOV = 1.04719755f; //60 degrees

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (Main == null)
                Main = this;

            Application.Main.SceneRender += SceneRender;
        }

        private void SceneRender()
        {
            if (!Enabled)
                return;

            Vector3 pos = BaseTransform.Position;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.Main.ViewportSize.X / (float)Application.Main.ViewportSize.Y, 0.02f, 512.0f);

            ViewMatrix = Matrix4.LookAt(
                    pos,
                    pos + BaseTransform.Forward,
                    BaseTransform.Up
                );

            RenderingGlobals.Projection = ProjectionMatrix;
            RenderingGlobals.View = ViewMatrix;
        }

        public override void LateUpdate()
        {
            LightingController.CameraPos = BaseTransform.LocalPosition;
        }

        public override void OnRemove()
        {
            Main = null;

            Application.Main.SceneRender -= SceneRender;
        }
    }
}
