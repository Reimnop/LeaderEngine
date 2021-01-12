using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : EditorComponent
    {
        public float Intensity = 1.0f;

        public override void EditorStart()
        {
            LightingController.DirectionalLight = this;
        }

        public void GenViewProject(out Matrix4 view, out Matrix4 proj)
        {
            proj = Matrix4.CreateOrthographic(32.0f, 32.0f, -16.0f, 16.0f);

            view = Matrix4.LookAt(Vector3.Zero, transform.Forward, transform.Up);
        }

        public override void EditorRemove()
        {
            LightingController.DirectionalLight = null;
        }
    }
}
