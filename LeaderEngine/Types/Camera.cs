using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;

namespace LeaderEngine
{
    public class Camera : Component
    {
        public static Camera main;

        public float FOV = 1.04719755f; //60 degrees

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        public override void Start()
        {
            if (main == null)
                main = this;

            Application.main.SceneRender += SceneRender;
        }

        private void SceneRender()
        {
            if (!Enabled)
                return;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Application.main.ViewportSize.X / (float)Application.main.ViewportSize.Y, 0.1f, 200.0f);

            ViewMatrix = Matrix4.CreateTranslation(-gameObject.transform.position) *
                Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(new Vector3(
                    MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.X),
                    MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.Y),
                    -MathHelper.DegreesToRadians(gameObject.transform.rotationEuler.Z))
                    ));

            RenderingGlobals.Projection = ProjectionMatrix;
            RenderingGlobals.View = ViewMatrix;
        }

        public override void OnRemove()
        {
            main = null;

            Application.main.SceneRender -= SceneRender;
        }
    }
}
