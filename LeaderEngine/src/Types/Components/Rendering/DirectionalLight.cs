using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class DirectionalLight : EditorComponent
    {
        public override void EditorStart()
        {
            LightingController.DirectionalLight = this;
        }

        public void GenViewProject(out Matrix4 view, out Matrix4 proj)
        {
            proj = Matrix4.CreateOrthographic(16.0f, 16.0f, -8.0f, 8.0f);

            view = Matrix4.LookAt(Vector3.Zero, BaseTransform.Forward, BaseTransform.Up);
        }

        public override void EditorRemove()
        {
            LightingController.DirectionalLight = null;
        }
    }
}
